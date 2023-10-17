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
    }

    private void Update()
    {
        while (!isConnected) {
            if (clientSocket.Poll(100000, SelectMode.SelectWrite)) {
                // The client was connected to the server.
                break;
            }
        }
        
        if (clientSocket.Poll(100000, SelectMode.SelectRead))
        {
            Debug.Log(Receive());
        }
    }

    public void Send(string data)
    {
        if (!isConnected) return;
        try
        {
            byte[] msg = Encoding.ASCII.GetBytes(data);
            clientSocket.Send(msg);
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return;
        }
    }

    public string? Receive()
    {
        if (!isConnected) return null;
        try
        {
            byte[] msg = new byte[1024];
            int byteCount = clientSocket.Receive(msg);
            return Encoding.ASCII.GetString(msg, 0, byteCount);
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
