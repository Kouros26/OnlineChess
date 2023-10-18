using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Security;
using UnityEngine;

public class Client : MonoBehaviour
{
    [SerializeField] private string serverIP   = "10.2.103.130";
    [SerializeField] private int    serverPort = 11000;
    
    private Socket     clientSocket = null;
    public  bool       isConnected => clientSocket is not null && clientSocket.Connected;

    void Start()
    {
        IPAddress ipAddress = IPAddress.Parse(serverIP);
        clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect(ipAddress, serverPort);

        Packet newPacket = new Packet("Hello there !");
        byte[] data = newPacket.Serialize();
        Send(newPacket);
    }

    private void Update()
    {
        if (!isConnected) return;

        if (clientSocket.Poll(100000, SelectMode.SelectRead))
        {
            Debug.Log(Receive());
        }
    }

    public void Send(Packet packet)
    {
        if (!isConnected) return;
        try
        {
            clientSocket.Send(packet.Serialize());
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return;
        }
    }

    public Packet Receive()
    {
        if (!isConnected) return null;
        try
        {
            byte[] data = new byte[clientSocket.Available];
            Packet newPacket = new Packet();
            int byteCount = clientSocket.Receive(data);
            newPacket.Deserialize(data);

            return newPacket;
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public void Close()
    {
        clientSocket?.Shutdown(SocketShutdown.Both);
        clientSocket?.Close();
        clientSocket = null;
    }
}
