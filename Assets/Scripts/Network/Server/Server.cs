using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    private Socket listenSocket;

    private List<Socket> clientSockets = new List<Socket>();

    [SerializeField] private string serverIP = "127.0.0.1";
    [SerializeField] private int serverPort = 11000;

    public void Listen()
    {
        listenSocket.Listen(10);
    }

    public Socket Accept()
    {
        Socket newConnection = listenSocket.Accept();
        newConnection.Blocking = false;
        clientSockets.Add(newConnection);

        return newConnection;
    }

    public void SendToAll(string data)
    {
        byte[] msg = Encoding.ASCII.GetBytes(data);

        foreach (var socket in clientSockets)
        {
            socket.Send(msg);
        }
    }

    public string Receive(int id)
    {
        byte[] bytes = new byte[clientSockets[id].Available];
        int byteRec = clientSockets[id].Receive(bytes);

        return Encoding.ASCII.GetString(bytes, 0, byteRec);
    }

    public void Close()
    {
        listenSocket.Shutdown(SocketShutdown.Both);
        listenSocket.Close();
    }

    void Awake()
    {
        IPAddress ipAddress = IPAddress.Parse(serverIP);
        listenSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Blocking = false;
        listenSocket.Bind(new IPEndPoint(ipAddress, serverPort));
        listenSocket.Listen(10);
    }

    void Start()
    {

    }

    void Update()
    {
        if (listenSocket.Poll(100000, SelectMode.SelectRead))
        {
            Socket newConnection = Accept();
            SendToAll("Hello");
        }
    }
}