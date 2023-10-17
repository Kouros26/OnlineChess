using UnityEngine;

public class MainTest : MonoBehaviour
{
    Server       server;
    LocalClient  local1;
    RemoteClient remote1;
    RemoteClient remote2;
    
    void Start()
    {
        byte[] serverIP   = { 10, 2, 103, 130 };
        int    serverPort = 11000;
                    
        server  = new(serverIP, serverPort);
        local1  = new(serverIP, serverPort);
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
            local1.Send("BOB!!!!");
            Debug.Log(server.Receive(remote1));
            Debug.Log(server.Receive(remote2));
        }
    }
}
