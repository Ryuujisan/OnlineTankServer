using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeamColorSetter : NetworkBehaviour
{
    [SerializeField]
    private Renderer[] colorRenderers;

    [SyncVar(hook = nameof(HandleTeamColorUpdate))]
    private Color teamColor = new Color();

    #region Server

    public override void OnStartServer()
    {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        teamColor = player.TeamColor;
    }

    #endregion Server

    #region Client

    private void HandleTeamColorUpdate(Color oldColor, Color newColor)
    {
        foreach (var c in colorRenderers)
        {
            c.material.SetColor("_BaseColor", newColor);
        }
    }
    
    #endregion Client
}
