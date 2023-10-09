using System;
using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager instance;
    private CharacterSelect[] characterPositions;
    /** Only one instance allowed at one time - destroy otherwise and log it **/
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Duplicate instance of this class");
            Destroy(gameObject);
        }
    }

    /* Hides all character select locations */
    private void HideAll()
    {
        if (characterPositions == null) return;
        for (var i = 0; i < characterPositions.Length; i++)
        {
            characterPositions[i].Hide();
        }
    }
    
    /* Hides all ready game objects */
    private void HideAllReady()
    {
        if (characterPositions == null) return;
        for (var i = 0; i < characterPositions.Length; i++)
        {
            characterPositions[i].HideReady();
        }
    }

    private void Start()
    {
        characterPositions = GetComponentsInChildren<CharacterSelect>();
        GameNetworkManager.instance.OnPlayerDataNetworkListChanged += ShowPlayerPosition;
        GameNetworkManager.instance.OnReadyChanged += ShowReady;
        ActivateCharacterSlot();
    }

    /** Clears ready on all characters before finding the network list index assigned to the character select location. 
     * Then checks if the player is ready and shows the ready game object
     */
    private void ShowReady(object sender, ulong clientId)
    {
        HideAllReady();
        for (var i = 0; i < GameNetworkManager.instance.playerDataNetworkList.Count; i++)
        {
            for (var k = 0; k < characterPositions.Length; k++)
            {
                if (GameNetworkManager.instance.playerDataNetworkList[i].index == characterPositions[k].playerIndex)
                {
                    if (GameNetworkManager.instance.playerDataNetworkList[i].playerReady)
                    {
                        characterPositions[k].ShowReady();
                    }
                }
            }
        }
    }
    
    /** Clears all characters and then loops through network list showing each character select location (index synced)
     * E.g. if player 1 is in position 0, then the character select location with index 0 will show the player's character
     */
    private void ActivateCharacterSlot()
    {
        HideAll();
        for (var i = 0; i < GameNetworkManager.instance.playerDataNetworkList.Count; i++)
        {
            for (var k = 0; k < characterPositions.Length; k++)
            {
                if (GameNetworkManager.instance.playerDataNetworkList[i].index == characterPositions[k].playerIndex)
                {
                    characterPositions[k].Show(GameNetworkManager.instance.playerDataNetworkList[i]);
                    break;
                }
            }
        }
    }
  

    /* Multiplayer: shows the player's character in the character select location */
    private void ShowPlayerPosition(object sender, ulong clientId)
    {
       ActivateCharacterSlot();
    }
}
