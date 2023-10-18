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
                byte[] data = Receive(socket);
                Packet newPacket = new Packet();
                newPacket.Deserialize(data);
                Debug.Log(newPacket.GetMessage().ToString());
                Debug.Log(newPacket.GetTimeStamp());
                Debug.Log(newPacket.GetLatency());

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

    public void Send(Socket target, byte[] data)
    {
        if (target is null || !target.Connected) return;
        try
        {
            target.Send(data);
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return;
        }
    }
    
    public void Redistribute(Socket sender, byte[] data)
    {
        foreach (Socket socket in clientSockets)
        {
            if (socket == sender) 
                continue;

            Send(socket, data);
        }
    }

    public byte[] Receive(Socket source)
    {
        if (source is null || !source.Connected) return null;
        try
        {
            byte[] bytes = new byte[source.Available];
            int byteRec = source.Receive(bytes);

            return bytes;
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
