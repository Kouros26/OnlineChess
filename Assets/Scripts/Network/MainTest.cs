#define IS_SERVER

using System;
using System.Threading;
using UnityEngine;


public class MainTest : MonoBehaviour
{
    [SerializeField] private string serverIP   = "10.2.103.130";
    [SerializeField] private int    serverPort = 11000;
    private Server       server;
    private LocalClient  local1;
    private RemoteClient remote1;
    private RemoteClient remote2;
    
    #if IS_SERVER
    void Start()
    {
        byte[] ip = Array.ConvertAll(serverIP.Split('.'), e => (byte)int.Parse(e));
        server  = new(ip, serverPort);
        local1  = new(ip, serverPort);
        remote1 = new(server);
        remote2 = new(server);
        local1 .Connect();
        remote1.Connect();
        
        Debug.Log("Waiting for external connexion.");
    }

    void Update()
    {
        if (remote2.isConnected) return;

        if (remote2.Connect())
        {
            Debug.Log("Connected.");
            local1.Send("Hello from client 1!");
            Thread.Sleep(new TimeSpan(0, 0, 0, 0, 500));
            Debug.Log(server.Receive(remote1));
            Debug.Log(server.Receive(remote2));
        }
    }
    #else
    void Start()
    {
        byte[] ip = Array.ConvertAll(serverIP.Split('.'), e => (byte)int.Parse(e));
        local1 = new(ip, serverPort);
    }
    
    void Update()
    {
        if (local1.isConnected) return;

        if (local1.Connect())
        {
            Debug.Log("Connected.");
            local1.Send("Hello from client 2!");
        }
    }
    #endif
}
