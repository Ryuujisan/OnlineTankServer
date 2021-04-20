using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    [SerializeField]
    private Targetable targetable;

    public Targetable Targetable => targetable;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }
    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }
    
    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if (!targetGameObject.TryGetComponent<Targetable>(out var newTarget))
        {
            return;
        }

        targetable = newTarget;
    }
    [Server]
    public void ClearTarget()
    {
        targetable = null;
    }
    #endregion
}
