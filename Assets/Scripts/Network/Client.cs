using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Security;
using UnityEngine;

public class Client : MonoBehaviour
{
    [SerializeField] public string serverIP   = "10.2.103.130";
    [SerializeField] public int    serverPort = 11000;
    
    private Socket    clientSocket = null;
    private IPAddress ipAddress = null;
    public  bool      isConnected => clientSocket is not null && clientSocket.Connected;
    public  Action<string> receiveCallback = null;

    void Start()
    {
        receiveCallback = Debug.Log;
    }

    private void Update()
    {
        if (!isConnected) return;
        
        if (clientSocket.Poll(100000, SelectMode.SelectRead))
            Receive();
    }

    public void Connect()
    {
        if (isConnected) Close();
        else clientSocket?.Close();
        
        ipAddress    = IPAddress.Parse(serverIP);
        clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect(ipAddress, serverPort);
    }

    private void OnDestroy()
    {
        Close();
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
            string data = Encoding.ASCII.GetString(msg, 0, byteCount);
            if (receiveCallback is not null)
                receiveCallback(data);
            return data;
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
