using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    public static event Action<UnitBase> ServerOnBaseSpawn;
    public static event Action<UnitBase> ServerOnBaseDeSpawn;

    public static event Action<int> ServerOnPlayerDie;
    
    [SerializeField]
    private Health health;

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
        ServerOnBaseSpawn?.Invoke(this);
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        ServerOnBaseDeSpawn?.Invoke(this);
    }

    [Server]
    private void ServerHandleDie()
    {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);
        
        NetworkServer.Destroy(gameObject);
    }
    
    #endregion

    #region Client



    #endregion
}
