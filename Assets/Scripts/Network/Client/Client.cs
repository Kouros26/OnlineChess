using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Security;

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
    private IPEndPoint endPoint = null;

    public LocalClient(byte[] serverIP, int serverPort)
    {
        endPoint     = new IPEndPoint(new IPAddress(serverIP), serverPort);
        clientSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Connect()
    {
        try
        {
            clientSocket.Connect(endPoint);
        }
        catch (Exception e) when (e is SocketException or SecurityException)
        {
            Console.WriteLine(e);
            return;
        }
    }

    public void Send(string data)
    {
        if (clientSocket is null) return;
        try
        {
            byte[] msg = Encoding.ASCII.GetBytes(data);
            clientSocket.Send(msg);
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
            return;
        }
    }

    public string? Receive()
    {
        if (clientSocket is null) return null;
        try
        {
            byte[] msg = new byte[1024];
            int byteCount = clientSocket.Receive(msg);
            return Encoding.ASCII.GetString(msg, 0, byteCount);
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
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
