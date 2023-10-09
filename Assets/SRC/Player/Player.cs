using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    [SerializeField] private string hideScene;

    private void Start()
    {
        gameObject.SetActive(false);    
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Get the current scene name
        string sceneName = SceneManager.GetActiveScene().name;

        // Check if it's the scene where you want to hide the player
        Debug.Log(sceneName);
        if (sceneName == hideScene)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
