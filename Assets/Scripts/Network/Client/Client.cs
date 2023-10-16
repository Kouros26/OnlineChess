using System;
using System.Net;
using System.Text;
using System.Net.Sockets;

public class Client
{
    public Socket clientSocket { get; private set; }
    public Socket serverSocket { get; set; }

    public Client()
    {
    }

    public Client(byte[] serverIP, int serverPort)
    {
        // Create the client socket and connect it to the server on the right port.
        IPAddress ipAddress = new IPAddress(serverIP);
        clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect(new IPEndPoint(ipAddress, serverPort));
    }

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
}