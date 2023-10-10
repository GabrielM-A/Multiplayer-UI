using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class UICharacterPanel : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private StyleSheet _styleSheet;
    private VisualElement playerListContainer;
    private string joinCode;
    private string lobbyName;
    
    void OnEnable()
    {
        Debug.Log("ENABLED");
        StartCoroutine(Generate());
        GameNetworkManager.instance.OnPlayerDataNetworkListChanged += OnPlayerDataNetworkListChange;
        StartCoroutine(GeneratePlayerList());   
    }
    
    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        if (Application.isPlaying) return;
        StartCoroutine(Generate());
    }

    /** Removes the event listener when the object is disabled & also destroyed*/
    private void OnDisable()
    {
        GameNetworkManager.instance.OnPlayerDataNetworkListChanged -= OnPlayerDataNetworkListChange;
    }

    private IEnumerator Generate()
    {
        yield return null;
        var root = _uiDocument.rootVisualElement;
        root.Clear();
        root.styleSheets.Add(_styleSheet);
        var container = Create("container");
        var label = new Label();
        label.AddToClassList("characterPanelTitle");
        label.text = "Characters";
        container.Add(label);
        var characterPortraitContainer = Create("characterPortraitContainer");
        container.Add(characterPortraitContainer);
        var characterPortrait = Create("characterPortrait");
        characterPortraitContainer.Add(characterPortrait);
        var cycleOptionsContainer = Create("cycleOptionsContainer");
        container.Add(cycleOptionsContainer);
        var switchLeft = Create("switchLeft");
        cycleOptionsContainer.Add(switchLeft);
        var nameOfCharacter = Create("nameOfCharacter");
        cycleOptionsContainer.Add(nameOfCharacter);
        var switchRight = Create("switchRight");
        cycleOptionsContainer.Add(switchRight);
        root.Add(container);
        var readyBtn = AddButtonTo(container, "Ready", "readyButton");
        readyBtn.clicked += () => GameNetworkManager.instance.SetPlayerReady();
        // Kick character button 
        playerListContainer = Create("playerList");
        // Add private join code
        var privateJoinCodeContainer = Create("privateJoinCodeContainer");
        var privateJoinCodeInnerContainer = Create("privateJoinCodeInnerContainer");
        var privateJoinCodeLabel = new Label();
        var lobbyName = new Label();
        lobbyName.AddToClassList("lobbyName");
        lobbyName.text = GetLobbyName();
        privateJoinCodeInnerContainer.Add(lobbyName);
        privateJoinCodeLabel.AddToClassList("privateJoinCodeLabel");
        privateJoinCodeLabel.text = "Join Code: ";
        privateJoinCodeInnerContainer.Add(privateJoinCodeLabel);
        var privateJoinCodeValue = new Label();
        privateJoinCodeValue.AddToClassList("privateJoinCodeValue");
        privateJoinCodeValue.text = GetJoinCode();
        privateJoinCodeInnerContainer.Add(privateJoinCodeValue);
        privateJoinCodeContainer.Add(privateJoinCodeInnerContainer);
        root.Add(privateJoinCodeContainer);
        root.Add(playerListContainer);
   
    }

    private string GetLobbyName()
    {
        string lobbyName;
        if (LobbyAPI.instance)
        {
          lobbyName = LobbyAPI.instance.GetLobby().Name.Length > 0 ? LobbyAPI.instance.GetLobby().Name : "Lobby";
        }
        else
        {
            return "Lobby Name";
        }
        return lobbyName;
    }

    private string GetJoinCode()
    {
        if (LobbyAPI.instance)
        {
            joinCode = LobbyAPI.instance.GetLobby().LobbyCode;
        }

        return joinCode;
    }

    public void OnPlayerDataNetworkListChange(object sender, ulong clientId)
    {
        StartCoroutine(GeneratePlayerList());   
    }
    

    private IEnumerator GeneratePlayerList()
    {
        yield return null;
        playerListContainer.Clear();
        var playerListTitle = new Label();
        playerListTitle.AddToClassList("playerListTitle");
        playerListTitle.text = "Players";
        playerListContainer.Add(playerListTitle);
        Debug.Log("count: " + GameNetworkManager.instance.playerDataNetworkList.Count);
        for (var i = 0; i < GameNetworkManager.instance.playerDataNetworkList.Count; i++)
        {
            var clientId = GameNetworkManager.instance.playerDataNetworkList[i].clientId;
            var playerRowContainer = Create("playerRowContainer");
            playerListContainer.Add(playerRowContainer);
            if (GameNetworkManager.instance.IsServer)
            {
                var kickBtn = AddButtonTo(playerRowContainer, "Kick", "kickButton");
                kickBtn.clicked += () => GameNetworkManager.instance.KickPlayerServerRpc(clientId);
            }
            var playerLabel = new Label();
            playerLabel.AddToClassList("playerLabel");
            playerLabel.text = GameNetworkManager.instance.playerDataNetworkList[i].clientId.ToString();
            playerRowContainer.Add(playerLabel);
        }
    }

    private Button AddButtonTo(VisualElement container, String text, params string[] classNames)
    {
        var button = Create<Button>(classNames);
        var buttonText = new Label();
        buttonText.AddToClassList("menuButtonText");
        buttonText.text = text;
        button.Add(buttonText);
        container.Add(button);
        return button;
    }
    VisualElement Create(params string[] classNames)
    {
        return Create<VisualElement>(classNames);
    }

    T Create<T>(params string[] classNames) where T : VisualElement, new()
    {
        var ele = new T();
        foreach (var className in classNames)
        {
            ele.AddToClassList(className);
        }
        return ele;
    }
}
