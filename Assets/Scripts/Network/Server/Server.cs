using System.Net;
using System.Text;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    private Socket     listenSocket = null;
    public  IPEndPoint endPoint { get; } = null;

    public Server(byte[] serverIP, int serverPort)
    {
        endPoint     = new IPEndPoint(new IPAddress(serverIP), serverPort);
        listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Blocking = false;
        listenSocket.Bind(endPoint);
        listenSocket.Listen(10);
    }

    public Socket Accept()
    {
        try {
            Socket newRemote = listenSocket?.Accept();
            if (newRemote is not null) newRemote.Blocking = true;
            return newRemote;
        }
        catch (SocketException) {
            return null;
        }
    }

    public void Send(RemoteClient target, string data)
    {
        if (!target.isConnected) return;
        try
        {
            byte[] msg = Encoding.ASCII.GetBytes(data);
            target.serverSocket.Send(msg);
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return;
        }
    }

    public string? Receive(RemoteClient source)
    {
        if (!source.isConnected) return null;
        try
        {
            byte[] bytes = new byte[1024];
            int byteRec = source.serverSocket.Receive(bytes);
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
