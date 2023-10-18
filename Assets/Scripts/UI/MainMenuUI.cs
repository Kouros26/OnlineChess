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
    [SerializeField] private TextMeshProUGUI connectedPopup;
    [SerializeField] private GameObject serverPrefab;
    
    private TextMeshProUGUI hostButtonText;
    
    private Server server = null;
    private Client client = null;
    
    void Start()
    {
        client = FindObjectOfType<Client>();
        hostButtonText = hostButton.GetComponentInChildren<TextMeshProUGUI>();
        hostButton .onClick.AddListener(Host);
        joinButton .onClick.AddListener(Join);
        startButton.onClick.AddListener(StartGame);
    }

    private void Update()
    {
        connectedPopup.enabled = server is null && client.isConnected;
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
    }

    private void StartGame()
    {
        if (!client.isConnected) return;
        if (server is not null && server.connectionCount > 2) return;
        SceneManager.LoadScene("MainScene");
    }
}
