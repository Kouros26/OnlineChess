using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;

public class Server
{
    private Socket listenSocket;

    public Server(byte[] serverIP, int serverPort)
    {
        // Create the client socket and connect it to the server on the right port.
        IPAddress ipAddress = new IPAddress(serverIP);
        listenSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Bind(new IPEndPoint(ipAddress, serverPort));
    }

    public void Listen()
    {
        listenSocket.Listen(10);
    }

    public Socket Accept()
    {
        return listenSocket.Accept();
    }

    public void Send(Client target, string data)
    {
        byte[] msg = Encoding.ASCII.GetBytes(data);
        target.serverSocket.Send(msg);
    }

    public string Receive(Client source)
    {
        byte[] bytes = new byte[1024];
        int byteRec = source.serverSocket.Receive(bytes);

        return Encoding.ASCII.GetString(bytes, 0, byteRec);
    }

    public void Close()
    {
        listenSocket.Shutdown(SocketShutdown.Both);
        listenSocket.Close();
    }
}