using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Purchasing;

public class Server : MonoBehaviour
{
    [SerializeField] private int serverPort = 11000;
    private IPAddress serverIP;
    
    private Socket       listenSocket = null;
    private Socket       discoverySocket = null;
    private List<Socket> clientSockets = new();
    
    public int connectionCount => clientSockets.Count;

    public int    GetPort()    { return serverPort; }
    public string GetAddress() { return serverIP.ToString(); }
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
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
        listenSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
        listenSocket.Listen(10);

        discoverySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        discoverySocket.Bind(new IPEndPoint(IPAddress.Any, serverPort + 1));
    }

    void Update()
    {
        if (listenSocket.Poll(100, SelectMode.SelectRead))
            Accept();

        foreach (Socket socket in clientSockets)
        {
            if (socket.Poll(100, SelectMode.SelectRead))
            {
                if (socket.Available <= 0) {
                    socket.Close();
                    clientSockets.Remove(socket);
                    return;
                }
                
                byte[] data = Receive(socket);
                Redistribute(socket, data);
            }
        }

        if (discoverySocket.Poll(100, SelectMode.SelectRead))
        {
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = new byte[discoverySocket.Available];
            int bytesRead = discoverySocket.ReceiveFrom(data, ref remoteEndPoint);
            Packet packet = new Packet();
            packet.Deserialize(data);
            Debug.Log(packet.DataAsString());
            SendServerInfo(remoteEndPoint);
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

    public void SendServerInfo(EndPoint sender)
    {
        Packet infoPacket = new Packet(Packet.Type.Command, serverIP + " " + serverPort);
        discoverySocket.SendTo(infoPacket.Serialize(), sender);
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
            source.Receive(bytes);
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

        discoverySocket?.Close();
        discoverySocket = null;
    }
}
