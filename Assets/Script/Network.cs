using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;

public class Network : MonoBehaviour
{
    bool bServer = false;
    bool bConnect = false;

    Socket socketListen = null; //서버가 쓰는 소켓
    Socket socket = null;  //서버 클라이언트가 쓰는 소켓

    Thread thread = null;
    bool bThreadBegin = false;

    Buffer bufferSend;
    Buffer bufferReceive;

    public string name;

    void Start()
    {
        bufferSend = new Buffer();
        bufferReceive = new Buffer();
    }

    public void ServerStart(int port, int backlog)
    {
	   // 서버 시작
        socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //리스닝 소켓 생성
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
        socketListen.Bind(ep);
        socketListen.Listen(backlog);

        bServer = true;
        Debug.Log("Server Start");

        StartThread(); 
    }

    public bool IsServer()
    {
        return bServer;
    }

    bool StartThread()
    {
        ThreadStart threadDelegate = new ThreadStart(ThreadProc);
        thread = new Thread(threadDelegate);
        thread.Start();

        bThreadBegin = true;

        return true;
    }

    public void ThreadProc()
    {
        while (bThreadBegin)
        {
            AcceptClient();

            if (socket != null && bConnect == true)
            {
                // 네트워크 업데이트
                SendUpdate();
                ReceiveUpdate();
            }

            Thread.Sleep(10);
        }
    }

    public void ClientStart(string address, int port)
    {
        // 클라이언트 시작
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //리스닝 소켓 생성
        socket.Connect(address, port); //address-연결하고자 하는 주소
        
        bConnect = true;
        Debug.Log("ClientStart");

        StartThread();

    }

    void AcceptClient()
    {
        if (socketListen != null && socketListen.Poll(0, SelectMode.SelectRead))
        {
            socket = socketListen.Accept();
            bConnect = true;

            Debug.Log("Client Connect");
        }
    }

    public bool IsConnect()
    {
        return bConnect;
    }

    public int Send(byte[] bytes, int length) //Sendbuffer에 넣는 것
    {
        return bufferSend.Write(bytes, length);
    }

    public int Receive(ref byte[] bytes, int length)
    {
        return bufferReceive.Read(ref bytes, length);
    }

    void SendUpdate()
    {
        if (socket.Poll(0, SelectMode.SelectWrite))
        {
            byte[] bytes = new byte[1024];

            int length = bufferSend.Read(ref bytes, bytes.Length);
            while (length > 0)
            {
                socket.Send(bytes, length, SocketFlags.None);
                length = bufferSend.Read(ref bytes, bytes.Length);
            }
        }
    }

    void ReceiveUpdate()
    {
        while (socket.Poll(0, SelectMode.SelectRead))
        {
            byte[] bytes = new byte[1024];

            int length = socket.Receive(bytes, bytes.Length, SocketFlags.None);
            if (length > 0)
            {
                bufferReceive.Write(bytes, length);
            }
        }
    }
}
