using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourcesUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text resourcesText;

    private RTSPlayer player;


    private void Update()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            if (player != null)
            {
                ClientHandleResourcesUpdated(player.Resources);
                player.ClientOnResourcesChanged += ClientHandleResourcesUpdated;
            }
        }
    }

    private void OnDestroy()
    {
        player.ClientOnResourcesChanged -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int value)
    {
        resourcesText.text = $"Resources: {value}";
    }
}