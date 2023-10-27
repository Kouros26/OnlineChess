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
    [SerializeField] private Button startButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private TextMeshProUGUI popup;
    [SerializeField] private GameObject serverPrefab;
    
    private TextMeshProUGUI hostButtonText;
    private TextMeshProUGUI startButtonText;
    private TextMeshProUGUI serverText;
    private ServerInfo serverInfo = null;

    private Server server = null;
    private Client client = null;
    private bool   canStart = false;
    
    void Start()
    {
        client = FindObjectOfType<Client>();
        client.receiveCallback = p =>
        {
            string s = p.GetMessage();
            if (s != "ready") return; 
            if (canStart) StartGame();
            else canStart = true;
        };
        hostButtonText  = hostButton .GetComponentInChildren<TextMeshProUGUI>();
        startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
        serverText = serverButton .GetComponentInChildren<TextMeshProUGUI>();
        hostButton .onClick.AddListener(Host);
        joinButton .onClick.AddListener(Join);
        startButton.onClick.AddListener(StartGame);
        startButton.interactable = false;
    }

    private void Update()
    {
        popup.enabled = server is not null || (server is null && client.isConnected);
        if (server is not null)
            popup.text = server.connectionCount + " player" + (server.connectionCount > 1 ? "s" : "") + " connected!";
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

    private void StartGame()
    {
        if (!client.isConnected) return;
        if (server is not null && server.connectionCount < 2) return;
        client.Send(new Packet("ready"));
        if (canStart) {
            SceneManager.LoadScene("MainScene");
        }
        else {
            canStart = true;
            startButtonText.text = "Waiting for opponent";
            startButton.interactable = false;
        }
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
        startButton.interactable = true;
        popup.text = "Connected to server";
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


