using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Net.Sockets;
using System.Security;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] public string serverIP   = "10.2.103.130";
    [SerializeField] public int serverPort = 11000;
    [SerializeField] public int    serverListeningPort = 11001;
    
    private Socket    clientSocket = null;
    private Socket discoverySocket = null;

    private IPAddress ipAddress    = null;
    public  bool      isConnected => clientSocket is not null && clientSocket.Connected;
    public Action<Packet> receiveCallback = null;

    void Awake()
    {
        receiveCallback = Debug.Log;
        DontDestroyOnLoad(gameObject);

        discoverySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        discoverySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
    }

    private void Update()
    {
        if (isConnected)
        {
            if (clientSocket.Poll(0, SelectMode.SelectRead))
            {
                if (clientSocket.Available <= 0)
                {
                    Close();
                    return;
                }

                Receive();
            }
        }
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

    public void BroadCast()
    {
        try
        {
            Packet packet = new Packet("broadcast");
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Broadcast, serverListeningPort);
            discoverySocket.SendTo(packet.Serialize(), endpoint);

            if (discoverySocket.Poll(1000000, SelectMode.SelectRead))
            {
                byte[] serverData = new byte[discoverySocket.Available];
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                discoverySocket.ReceiveFrom(serverData, ref remoteEndPoint);
                Packet receivePacket = new Packet();
                packet.Deserialize(serverData);
                Debug.Log(receivePacket.GetMessage());
                var splitted = receivePacket.GetMessage().Split(' ');
                mainMenuUI.CreateServerButton(IPAddress.Parse(splitted[0]), int.Parse(splitted[1]));
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return;
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

    public void SendDelayed(Packet data, float delay)
    {
        StartCoroutine(SendDelayedCoroutine(data, delay));
    }

    private IEnumerator SendDelayedCoroutine(Packet data, float delay)
    {
        yield return new WaitForSeconds(delay);
        Send(data);
    }

    public Packet Receive()
    {
        if (!isConnected) return null;
        try
        {
            byte[] data = new byte[clientSocket.Available];
            clientSocket.Receive(data);
            Packet packet = new Packet();
            packet.Deserialize(data);

            if (receiveCallback != null)
            {
                receiveCallback(packet);
            }

            return packet;
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public void Close()
    {
        if (clientSocket is not null && clientSocket.Connected) 
            clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket?.Close();
        clientSocket = null;
    }
}
