using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/** Singleton class for controlling character selection and multiplayer */
public class GameNetworkManager : NetworkBehaviour
{
    public static GameNetworkManager instance;
    [SerializeField] private string characterSelectSceneName;
    [SerializeField] private string worldSceneName;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    private string playerName; 
    public NetworkList<PlayerData> playerDataNetworkList;
    private Dictionary<ulong, bool> playerReadyDictionary;
    public event EventHandler<ulong> OnPlayerDataNetworkListChanged;
    public event EventHandler<ulong> OnReadyChanged;
    public event EventHandler<string> OnProfileNameChanged;
    [SerializeField] private Color[] colorList = new Color[4];

    /** Only one instance allowed at one time - destroy otherwise and log it
     * Sets up playerDataNetworkList to use for multiplayer management*
     */
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            playerDataNetworkList = new NetworkList<PlayerData>();
            playerReadyDictionary = new Dictionary<ulong, bool>();
            playerDataNetworkList.OnListChanged += (NetworkListEvent<PlayerData> playerData) =>
            {
                OnPlayerDataNetworkListChanged?.Invoke(this, playerData.Value.clientId);
                OnReadyChanged?.Invoke(this, playerData.Value.clientId);
            };
            playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER,
                "PlayerName" + UnityEngine.Random.Range(100, 1000));
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Duplicate instance of this class");
            Destroy(gameObject);
        }
    }
    
    public string GetPlayerName()
    {
        Debug.Log(playerName);
        return playerName;
    }
    
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
        OnProfileNameChanged?.Invoke(this, playerName);
    }
    
    /** Events for the main menu - single player and multiplayer */
    private void OnEnable()
    {
        MainMenu.OnStartGame += StartGame;
        MainMenu.OnShowLobby += ShowLobby;
        MainMenu.OnStartHost += ShowLobbyCreateMenu;
    }

    /** Single Player - Starts the game */
    private void StartGame()
    {
        Debug.Log("STARTED GAME!"); 
    }

    /* Sends user to lobby create menu*/
    private void ShowLobbyCreateMenu()
    {
        SceneManager.LoadScene("LobbyCreate");
    }

    /** Multiplayer: starts the host (post lobby create menu)*/
    public void StartHost()
    {
        Debug.Log("START HOST!");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);
    }

    /** Multiplayer: adds the player's clientId to the playerDataNetworkList */
    private void OnClientConnected(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            index = playerDataNetworkList.Count,
            characterId = colorList[playerDataNetworkList.Count],
            playerReady = false,
            playerName = GetPlayerName()
        });
        Debug.Log("PLAYER CONNECTED: " + playerDataNetworkList[0].playerName);
        if (playerDataNetworkList.Count <= 1) return; 
        Debug.Log("PLAYER CONNECTED @:" + playerDataNetworkList[1].playerName);
    }
    
    
    /** Multiplayer: removes the player's clientId from the playerDataNetworkList */
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("CLIENT DISCONNECTED" + clientId);
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                playerDataNetworkList.RemoveAt(i);
                break;
            }
        }
        
        playerReadyDisconnectedServerRpc(clientId);
        // Shift character position index
        DecreaseAllPlayerPositions();
    }

    /** Used to shift all players down one index when a player disconnects */
    public void DecreaseAllPlayerPositions()
    {
        // Reduce index by 1
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerDataTemp = playerDataNetworkList[i];
            if (playerDataTemp.index != 0 && playerDataTemp.index != 1)
            {
                playerDataTemp.index -= 1;
                playerDataNetworkList[i] = playerDataTemp;
            }
        }
    }

    /** Multiplayer: starts the client */
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientUserConnected;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("SERVER RPC PLAYER NAME: " + playerName);
        PlayerData playerData = GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId);
        playerData.playerName = playerName;
        playerDataNetworkList[playerData.index] = playerData;
        Debug.Log("BAKED INTO NETWORK LIST: " + playerDataNetworkList[playerData.index].playerName);
    }

    private void OnClientUserConnected(ulong obj)
    {
        Debug.Log("CLIENT CONNECTED: " + GetPlayerName());
        SetPlayerNameServerRpc(GetPlayerName());
    }


    /** Multiplayer: shows the lobby list */
    private void ShowLobby()
    {
        Debug.Log("SHOWS LOBBY");
    }
    
    /** Player Ready ---------------------------------------------------------------- */ 
    #region Player Ready
    /** UI Link -> Multiplayer: sets the player ready */
    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }
    
    /** Players Ready - will set the player is ready in the network list and repeat in clients */
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyOnNetworkList(serverRpcParams);
        BroadcastPlayerReadyChangeClientRpc(serverRpcParams.Receive.SenderClientId);
        
        if (IsAllClientsReady())
        {
            Destroy(CharacterSelectionManager.instance.gameObject);
            MoveToWorldSceneClientRpc();
            NetworkManager.SceneManager.LoadScene(worldSceneName, LoadSceneMode.Single);
        }
    }

    /* Ensure the character selection manager is destroyed on all clients */
    [ClientRpc]
    private void MoveToWorldSceneClientRpc()
    {
        Destroy(CharacterSelectionManager.instance.gameObject);
    }

    private bool IsAllClientsReady()
    {
        bool allClientsReady = true;
        for (var i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (!playerDataNetworkList[i].playerReady)
            {
                allClientsReady = false;
                break;
            }
        }

        return allClientsReady;
    }

    [ClientRpc]
    private void BroadcastPlayerReadyChangeClientRpc(ulong clientId)
    {
        OnReadyChanged?.Invoke(this, clientId);
    }

    private void SetPlayerReadyOnNetworkList(ServerRpcParams serverRpcParams)
    {
        for (var i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (serverRpcParams.Receive.SenderClientId == playerDataNetworkList[i].clientId)
            {
                PlayerData playerDataTemp = playerDataNetworkList[i];
                playerDataTemp.playerReady = true;
                playerDataNetworkList[i] = playerDataTemp;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void playerReadyDisconnectedServerRpc(ulong clientId)
    {
        for (var i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (clientId == playerDataNetworkList[i].clientId)
            {
                PlayerData playerDataTemp = playerDataNetworkList[i];
                playerDataTemp.playerReady = false;
                playerDataNetworkList[i] = playerDataTemp;
            }
          
        }
        playerReadyDisconnectedClientRpc(clientId);
    }
    
    [ClientRpc]
    private void playerReadyDisconnectedClientRpc(ulong clientId)
    {
        OnReadyChanged?.Invoke(this, clientId);
    }
    
    #endregion
    
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }

        return default;
    }
    /** Kick Player ---------------------------------------------------------------- */
    #region Kick Player 
    [ServerRpc]
    public void KickPlayerServerRpc(ulong clientId)
    {
        SendKickedPlayerBackToMainMenuClientRpc(clientId);
        NetworkManager.Singleton.DisconnectClient(clientId);
    }
    
    [ClientRpc]
    private void SendKickedPlayerBackToMainMenuClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
    #endregion

    /** Unsubscribes from events */
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        MainMenu.OnStartGame -= StartGame;
        MainMenu.OnShowLobby -= ShowLobby;
        MainMenu.OnStartHost -= StartHost;
        // Have to Dispose network list variables
        if (playerDataNetworkList != null && playerDataNetworkList.Count > 0)
        {
            playerDataNetworkList.Dispose();
        }

    }
}
