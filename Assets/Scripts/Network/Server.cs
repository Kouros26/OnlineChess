using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    [SerializeField] private int serverPort = 11000;
    private IPAddress serverIP;
    
    private Socket       listenSocket = null;
    private List<Socket> clientSockets = new();
    
    public int connectionCount => clientSockets.Count;

    public int    GetPort()    { return serverPort; }
    public string GetAddress() { return serverIP.ToString(); }
    
    void Awake()
    {
        // Find an available LAN IP address.
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                serverIP = ip;
            }
        }
        
        // Open the server on that address.
        listenSocket = new Socket(serverIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Bind(new IPEndPoint(serverIP, serverPort));
        listenSocket.Listen(10);
    }

    void Update()
    {
        if (listenSocket.Poll(100000, SelectMode.SelectRead))
            Accept();

        foreach (Socket socket in clientSockets)
        {
            if (socket.Poll(100000, SelectMode.SelectRead))
            {
                string data = Receive(socket);
                Debug.Log(data);
                Redistribute(socket, data);
            }
        }
    }

    private void OnDestroy()
    {
        Close();
    }

    public Socket Accept()
    {
        try {
            Socket newSocket = listenSocket.Accept();
            newSocket.Blocking = false;
            clientSockets.Add(newSocket);
            return newSocket;
        }
        catch (SocketException) {
            return null;
        }
    }

    public void Send(Socket target, string data)
    {
        if (target is null || !target.Connected) return;
        try
        {
            byte[] msg = Encoding.ASCII.GetBytes(data);
            target.Send(msg);
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return;
        }
    }
    
    public void Redistribute(Socket sender, string data)
    {
        foreach (Socket socket in clientSockets)
        {
            if (socket == sender) continue;
            Send(socket, data);
        }
    }

    public string? Receive(Socket source)
    {
        if (source is null || !source.Connected) return null;
        try
        {
            byte[] bytes = new byte[1024];
            int byteRec = source.Receive(bytes);
            return Encoding.ASCII.GetString(bytes, 0, byteRec);
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public void Close()
    {
        listenSocket?.Close();
        listenSocket = null;
    }
}
