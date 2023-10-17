using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public Socket clientSocket { get; private set; }

    [SerializeField] private string serverIP = "192.168.56.1";
    [SerializeField] private int serverPort = 11000;

    public void Send(string data)
    {
        if (clientSocket is null) return;
        byte[] msg = Encoding.ASCII.GetBytes(data);
        clientSocket.Send(msg);
    }

    public string Receive()
    {
        if (clientSocket is null) return "";
        byte[] msg = new byte[1024];
        int byteCount = clientSocket.Receive(msg);
        return Encoding.ASCII.GetString(msg, 0, byteCount);
    }

    public void Close()
    {
        if (clientSocket is null) return;
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }

    void Start()
    {
        IPAddress ipAddress = IPAddress.Parse(serverIP);
        clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Blocking = false;
        clientSocket.Connect(ipAddress, serverPort);
    }

    void Update()
    {
        while (!clientSocket.Connected)
        {
            if (clientSocket.Poll(100000, SelectMode.SelectWrite))
            {
                // The client socket is connected to the server
                break;
            }
        }

        if (clientSocket.Poll(100000, SelectMode.SelectRead))
        {
            Debug.Log(Receive());
        }
    }
}