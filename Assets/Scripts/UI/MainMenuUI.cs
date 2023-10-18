using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private TextMeshProUGUI popup;
    [SerializeField] private GameObject serverPrefab;
    
    private TextMeshProUGUI hostButtonText;
    private TextMeshProUGUI startButtonText;
    
    private Server server = null;
    private Client client = null;
    private bool   canStart = false;
    
    void Start()
    {
        client = FindObjectOfType<Client>();
        client.receiveCallback = s =>
        {
            if (s != "ready") return; 
            if (canStart) StartGame();
            else canStart = true;
        };
        hostButtonText  = hostButton .GetComponentInChildren<TextMeshProUGUI>();
        startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
        hostButton .onClick.AddListener(Host);
        joinButton .onClick.AddListener(Join);
        startButton.onClick.AddListener(StartGame);
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
            addressInput.interactable = true;
            joinButton.interactable = true;
        }
        else
        {
            server = Instantiate(serverPrefab).GetComponent<Server>();
            client.serverIP = server.GetAddress();
            client.Connect();
            hostButtonText.text = "Close Server";
            addressInput.text = server.GetAddress();
            addressInput.interactable = false;
            joinButton.interactable = false;
            
        }
    }

    private void Join()
    {
        string address = addressInput.text;
        if (address == "" || address.Split('.').Length != 4) return;
        if (server is not null) Destroy(server.gameObject);
        client.serverIP = address;
        client.Connect();
        popup.text = "Connected to server!";
    }

    private void StartGame()
    {
        if (!client.isConnected) return;
        if (server is not null && server.connectionCount < 2) return;
        client.Send("ready");
        if (canStart) {
            SceneManager.LoadScene("MainScene");
        }
        else {
            canStart = true;
            startButtonText.text = "Waiting for opponent";
            startButton.interactable = false;
        }
    }
}
