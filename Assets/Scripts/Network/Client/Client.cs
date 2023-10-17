using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Security;
using UnityEngine;

public class RemoteClient
{
    protected Server server = null;
    public    Socket serverSocket { get; protected set; } = null;
    public    bool   isConnected => serverSocket is not null;

    public RemoteClient(Server _server)
    {
        server = _server;
    }

    public bool Connect()
    {
        return isConnected || (serverSocket = server.Accept()) is not null;
    }

    public void Close()
    {
        serverSocket?.Close();
        serverSocket = null;
    }
}

public class LocalClient
{
    private Socket     clientSocket = null;
    private IPEndPoint endPoint     = null;
    public  bool       isConnected => clientSocket is not null && clientSocket.Connected;

    public LocalClient(byte[] serverIP, int serverPort)
    {
        endPoint     = new IPEndPoint(new IPAddress(serverIP), serverPort);
        clientSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Blocking = false;
    }

    public bool Connect()
    {
        if (isConnected) return true;
        try
        {
            clientSocket.Connect(endPoint);
            return clientSocket.Connected;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (SecurityException e)
        {
            Debug.Log(e);
            return false;
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
