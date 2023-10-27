using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public struct ChatMessage
{
    public string text;
    public bool fromOpponent;

    public ChatMessage(string text, bool fromOpponent)
    {
        this.text = text;
        this.fromOpponent = fromOpponent;
    }
}

public class ChatManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private GameObject logDisplayContent;
    [SerializeField] private GameObject chatMessagePrefab;
    
    private Client client;
    private List<Transform> messageTransforms = new();
    private static List<ChatMessage> messages = new();
    
    void Start()
    {
        client = FindObjectOfType<Client>();

        foreach (ChatMessage message in messages) {
            UpdateLogDisplay(message);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (EventSystem.current.currentSelectedGameObject == chatInput.gameObject && chatInput.text != "")
            {
                SendMessage(chatInput.text);
                chatInput.text = "";
            }
            else
            {
                chatInput.Select();
            }
        }
    }

    private void UpdateLogDisplay(ChatMessage message)
    {
        GameObject newDisplay = Instantiate(chatMessagePrefab, logDisplayContent.transform);
        TextMeshProUGUI displayText = newDisplay.GetComponent<TextMeshProUGUI>();
        displayText.text = (message.fromOpponent ? "Opponent: " : "You: ") + message.text;
        displayText.color = message.fromOpponent ? Color.blue : Color.green;

        foreach (Transform t in messageTransforms)
            t.position += Vector3.down * 40;
        messageTransforms.Add(newDisplay.transform);
    }

    public new void SendMessage(string text)
    {
        ChatMessage message = new(text, false);
        messages.Add(message);
        UpdateLogDisplay(message);
        client.Send(new Packet(Packet.Type.Message, text));
    }

    public void ReceiveMessage(string text)
    {
        ChatMessage message = new(text, true);
        messages.Add(message);
        UpdateLogDisplay(message);
    }
}
