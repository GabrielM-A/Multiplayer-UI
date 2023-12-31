using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField] public int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    private MeshRenderer renderer;
    private void OnEnable()
    {
        // Get the Mesh Renderer component
        // Can be swapped out for character or player model
        renderer = GetComponentInChildren<MeshRenderer>();
    }

    /** Used to show the ready icon when the player is ready */
    public void ShowReady()
    {
        readyGameObject.SetActive(true);
    }
    
    /** Used to hide the ready icon when the player is not ready */
    public void HideReady()
    {
        readyGameObject.SetActive(false);
    }

    public void Show(PlayerData playerData)
    {
        // Change the color to red
        renderer.material.color = playerData.characterId;
        gameObject.SetActive(true);
    }

  
    public void Hide()
    {
        if (!gameObject) return; 
        gameObject.SetActive(false);
    }
}
