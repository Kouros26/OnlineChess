using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private TextMeshProUGUI popup;
    [SerializeField] private GameObject serverPrefab;
    [SerializeField] private ChatManager chatManager;
    
    private TextMeshProUGUI hostButtonText;
    private TextMeshProUGUI serverText;
    private ServerInfo serverInfo = null;

    private Server server = null;
    private Client client = null;
    
    void Start()
    {
        client = FindObjectOfType<Client>();
        client.receiveCallback = p =>
        {
            if (p.type == Packet.Type.Command)
            {
                if (p.DataAsString() != "ready") return;
                if (canStart) StartGame();
                else canStart = true;
                chatManager.ReceiveMessage("Ready to start");
            }
            else if (p.type == Packet.Type.Message)
            {
                string message = p.DataAsString();
                chatManager.ReceiveMessage(message);
            }
        };
        hostButtonText  = hostButton .GetComponentInChildren<TextMeshProUGUI>();
        serverText = serverButton .GetComponentInChildren<TextMeshProUGUI>();
        hostButton .onClick.AddListener(Host);
        joinButton .onClick.AddListener(Join);
    }

    private void Update()
    {
        popup.enabled = server is not null || (server is null && client.isConnected);
        if (server is not null)
        {
            popup.text = server.connectionCount + " player" + (server.connectionCount > 1 ? "s" : "") + " connected!";
            
            if (server.connectionCount == 2)
                SceneManager.LoadScene("MainScene");
        }

    }

    private void Host()
    {
        if (server is not null)
        {
            Destroy(server.gameObject);
            server = null;
            hostButtonText.text = "Host Server";
            joinButton.interactable = true;
        }
        else
        {
            server = Instantiate(serverPrefab).GetComponent<Server>();
            client.serverIP = server.GetAddress();
            client.Connect();
            hostButtonText.text = "Close Server";
            joinButton.interactable = false;
            
        }
    }

    private void Join()
    {
        if (server is not null) Destroy(server.gameObject);
        client.BroadCast();
    }

    public void CreateServerButton(IPAddress address, int port)
    {
        serverInfo = new ServerInfo(address, port);
        serverText.text = address.ToString();
    }

    public void OnServerClick()
    {
        if (serverInfo == null)
            return;

        client.serverIP = serverInfo.address.ToString();
        client.Connect();
        popup.text = "Connected to server";
        SceneManager.LoadScene("MainScene");
    }
}

public class ServerInfo
{
    public ServerInfo(IPAddress address, int port)
    {
        this.address = address;
        this.port = port;
    }

    public IPAddress address;
    public int port;
}


