using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;

[ExecuteInEditMode]
public class UILobbyCreateMenu : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private StyleSheet _styleSheet;
    private TextField lobbyNameInput; 
    private bool privateLobby = false;
    private Button lobbyPrivate;
    private Button lobbyPublic;
    private int maxPlayers = 4;
    private Label maxPlayerControlValueLabel;

    void OnEnable()
    {
        StartCoroutine(Generate());
    }
    
    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        if (Application.isPlaying) return;
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        yield return null;
        var root = _uiDocument.rootVisualElement;
        root.Clear();
        root.styleSheets.Add(_styleSheet);
        // Lobby settings
        var lobbySettingsContainer = Create("lobbySettingsContainer");
        root.Add(lobbySettingsContainer);
        var lobbySettingsTitle = new Label();
        lobbySettingsTitle.AddToClassList("lobbySettingsTitle");
        lobbySettingsTitle.text = "Lobby Settings";
        var lobbyNameLabel = new Label();
        lobbyNameLabel.AddToClassList("lobbyNameLabel");
        lobbyNameLabel.text = "Lobby Name:";
        var lobbyMaxPlayers = new Label();
        lobbyMaxPlayers.AddToClassList("lobbyMaxPlayers");
        lobbyMaxPlayers.text = "Max Players:";
        lobbySettingsContainer.Add(lobbySettingsTitle);
        lobbySettingsContainer.Add(lobbyNameLabel);
        // Create a new TextField (equivalent to an input field)
        lobbyNameInput = new TextField();
        lobbyNameInput.AddToClassList("lobbyNameInput");
        lobbySettingsContainer.Add(lobbyNameInput);
        // Add private or public lobby 
        var lobbyToggleContainer = Create("lobbyToggleContainer");
        lobbyPrivate = AddButtonTo(lobbyToggleContainer, "Private", "lobbyPrivateToggle", "lobbyToggle");
        lobbyPublic = AddButtonTo(lobbyToggleContainer, "Public", "lobbyPublicToggle", "lobbyToggle", "selected");
        lobbyPrivate.clicked += PrivateLobby;
        lobbyPublic.clicked += PublicLobby;
        
        lobbySettingsContainer.Add(lobbyToggleContainer);
        lobbySettingsContainer.Add(lobbyMaxPlayers);
        // Max Player Controls
        MaxPlayerSection(lobbySettingsContainer);
        // Create Lobby Button
        var createLobbyBtn = AddButtonTo(lobbySettingsContainer, "Create Lobby", "createLobbyBtn");
        createLobbyBtn.clicked += CreateLobby;
    }
    
    void PrivateLobby()
    {
        privateLobby = true;
        lobbyPublic.RemoveFromClassList("selected");
        lobbyPrivate.AddToClassList("selected");
    }

    void PublicLobby()
    {
        privateLobby = false;
        lobbyPrivate.RemoveFromClassList("selected");
        lobbyPublic.AddToClassList("selected");
    }

    private void CreateLobby()
    {
        string lobbyName = lobbyNameInput.text.Length > 0 ? lobbyNameInput.text : "Lobby";
        LobbyAPI.instance.CreateLobby(lobbyName, privateLobby);
    }

    private void MaxPlayerSection(VisualElement lobbySettingsContainer)
    {
        var maxPlayerControlsContainer = Create("maxPlayerControlsContainer");
        lobbySettingsContainer.Add(maxPlayerControlsContainer);
        var maxPlayerControlArrowContainer = Create("maxPlayerControlArrowContainer");
        maxPlayerControlsContainer.Add(maxPlayerControlArrowContainer);
        var maxPlayerControlUpBtn =
            AddButtonTo(maxPlayerControlArrowContainer, "", "maxPlayerControlUp", "maxPlayerControl");
        var maxPlayerControlDownBtn =
            AddButtonTo(maxPlayerControlArrowContainer, "", "maxPlayerControlDown", "maxPlayerControl");
        maxPlayerControlArrowContainer.Add(maxPlayerControlUpBtn);
        maxPlayerControlArrowContainer.Add(maxPlayerControlDownBtn);
        maxPlayerControlValueLabel = new Label();
        maxPlayerControlValueLabel.AddToClassList("maxPlayerControlValueLabel");
        maxPlayerControlValueLabel.text = maxPlayers.ToString();
        maxPlayerControlsContainer.Add(maxPlayerControlValueLabel);
        maxPlayerControlUpBtn.clicked += MaxPlayersIncrease;
        maxPlayerControlDownBtn.clicked += MaxPlayersDecrease;
        
    }
    
    private void MaxPlayersDecrease()
    {
        if (maxPlayers == 1) return;
        maxPlayers--;
        maxPlayerControlValueLabel.text = maxPlayers.ToString();
        LobbyAPI.instance.SetMaxPlayers(maxPlayers);
    }
    
    private void MaxPlayersIncrease()
    {
        if (maxPlayers >= 4) return;
        maxPlayers++;
        maxPlayerControlValueLabel.text = maxPlayers.ToString();
        LobbyAPI.instance.SetMaxPlayers(maxPlayers);
        Debug.Log(maxPlayers);
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
