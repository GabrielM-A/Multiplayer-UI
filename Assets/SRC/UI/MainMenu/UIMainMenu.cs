using System;
using System.Collections;
using System.Collections.Generic;
using UI.SFX;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
[ExecuteInEditMode]
public class MainMenu : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private StyleSheet _styleSheet;
    [SerializeField] private GameObject _lobbyMenu;
    [SerializeField] private GameObject _joinCodeMenu;
    [SerializeField] private GameObject _uiProfileDialogMenu;
    private bool canPlay = true;
    public static event Action OnStartGame;
    public static event Action OnShowLobby;
    public static event Action OnStartHost;
    private List<Button> buttons = new List<Button>();
    public TextField playerNameText;
    private Label playerName;

    void Start()
    {
        StartCoroutine(Generate());
        GameNetworkManager.instance.OnProfileNameChanged += NameChanged;

    }
    
    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        if (Application.isPlaying) return;
        StartCoroutine(Generate());
    }

    private void OnEnable()
    {
        if (Application.isPlaying) return;
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        yield return null;
        var root = _uiDocument.rootVisualElement;
        root.Clear();
        root.styleSheets.Add(_styleSheet);
        var container = GenerateMainContainer();
        GenerateStatsSection(root);
        // Menu Section
        var menu = Create("menuPaper");
        container.Add(menu);
        GenerateSinglePlayerSection(menu);
        GenerateMultiplayerSection(menu);
        // Apply to container
        root.Add(container);
    }
    
    private void NameChanged(object ob, string playerName)
    {
        this.playerName.text = playerName;
    }

    private VisualElement GenerateMainContainer()
    {
        var container = Create("container");
        var label = new Label();
        label.AddToClassList("mainMenuTitle");
        label.text = "LOGUM GATE";
        container.Add(label);
        return container;
    }

    private void GenerateStatsSection(VisualElement root)
    {
        var stats = Create("statsContainer");
        var statsTitle = new Label();
        statsTitle.AddToClassList("statsTitle");
        statsTitle.text = "STATS";
        stats.Add(statsTitle);
        var statsInnerContainer = Create("statsInnerContainer");
        stats.Add(statsInnerContainer);
        playerName = new Label();
        playerName.AddToClassList("highestScoreLabel");
        playerName.text = GetPlayerName();
        statsInnerContainer.Add(playerName);
        var highestScoreLabel = new Label();
        highestScoreLabel.AddToClassList("highestScoreLabel");
        highestScoreLabel.text = "Highest Score";
        statsInnerContainer.Add(highestScoreLabel);
        var highestScoreValue = new Label();
        highestScoreValue.AddToClassList("highestScore");
        highestScoreValue.text = "0";
        statsInnerContainer.Add(highestScoreValue);
        
        var profileButton = AddButtonTo(statsInnerContainer, "Profile", "profileButton");
        profileButton.clicked += ShowPlayerProfileDialog;
        root.Add(stats);
    }
    
    void ShowPlayerProfileDialog()
    {
        _uiProfileDialogMenu.SetActive(true);
    }

    string GetPlayerName()
    {
        if (GameNetworkManager.instance == null)
        {
            return "Player Name";
        }

        Debug.Log("GAME SHOULD SHOW NEW NAME");
        return GameNetworkManager.instance.GetPlayerName();
    }

    /** Creates the single player section of the main menu */
    private void GenerateSinglePlayerSection(VisualElement menu)
    {
        var singlePlayerTitleContainer = Create("singlePlayerTitleContainer");
        var singlePlayerTitleIcon = Create("singlePlayerTitleIcon");
        singlePlayerTitleContainer.Add(singlePlayerTitleIcon);
        var singlePlayerLabel = new Label();
        singlePlayerLabel.AddToClassList("singlePlayerTitle");
        singlePlayerLabel.text = "SINGLEPLAYER";
        singlePlayerTitleContainer.Add(singlePlayerLabel);
        menu.Add(singlePlayerTitleContainer);
        var singlePlayerLine = Create("separator");
        menu.Add(singlePlayerLine);
        var newGameBtn = AddButtonTo(menu, "New Game", "menuButton", "startButton");
        buttons.Add(newGameBtn);
        newGameBtn.clicked += OnStartGame;
        newGameBtn.RegisterCallback<MouseEnterEvent>(PlaySound);
    }

    /** Creates the multiplayer section of the main menu */
    private void GenerateMultiplayerSection(VisualElement menu)
    {
        var multiplayerTitleContainer = Create("multiplayerTitleContainer");
        var multiplayerTitleIcon = Create("multiplayerTitleIcon");
        multiplayerTitleContainer.Add(multiplayerTitleIcon);
        var multiplayerLabel = new Label();
        multiplayerLabel.AddToClassList("multiplayerTitle");
        multiplayerLabel.text = "MULTIPLAYER";
        multiplayerTitleContainer.Add(multiplayerLabel);
        menu.Add(multiplayerTitleContainer);
        var line = Create("separator");
        menu.Add(line);
        var hostBtn = AddButtonTo(menu, "Host", "menuButton", "startButton");
        var lobbyBtn = AddButtonTo(menu, "Lobby", "menuButton", "lobbyButton");
        var joinCodeBtn = AddButtonTo(menu, "Join Private", "menuButton", "privateLobbyButton");
        buttons.Add(lobbyBtn);
        buttons.Add(hostBtn);
        buttons.Add(joinCodeBtn);
        hostBtn.clicked += OnStartHost;
        lobbyBtn.clicked += StartClient;
        joinCodeBtn.clicked += ShowEnterJoinCodeUI;
        hostBtn.RegisterCallback<MouseEnterEvent>(PlaySound);
        lobbyBtn.RegisterCallback<MouseEnterEvent>(PlaySound);
        joinCodeBtn.RegisterCallback<MouseEnterEvent>(PlaySound);
    }

    void ShowEnterJoinCodeUI()
    {
        UISoundEffectsManager.instance.PlayBoomSFX();
        _joinCodeMenu.SetActive(true);
    }
    
    void StartClient()
    {
        UISoundEffectsManager.instance.PlayBoomSFX();
        LobbyAPI.instance.QuickJoin();
        // NetworkManager.Singleton.StartClient();
        //  OnShowLobby?.Invoke();
        //  _lobbyMenu.SetActive(true);
        //  this.gameObject.SetActive(false);
    }

    void PlaySound(MouseEnterEvent evt)
    {
        if (canPlay)
        {
            UISoundEffectsManager.instance.PlayHoverSFX();
            canPlay = false; // Disable sound
            StartCoroutine(ResetCooldown()); // Start the cooldown
        }
    }

    IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(0.3f); // Wait for 1 seconds
        canPlay = true; // Enable sound
    }
    void OnDisable()
    {
        foreach (var button in buttons)
        {
            if (button != null)
            {
                button.UnregisterCallback<MouseEnterEvent>(PlaySound);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var button in buttons)
        {
            if (button != null)
            {
                button.UnregisterCallback<MouseEnterEvent>(PlaySound);
                Debug.Log(button);
            }
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
