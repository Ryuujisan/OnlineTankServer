using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    public static event Action<Unit> ServerOnUnitSpawn;
    public static event Action<Unit> ServerOnUnitDeSpawn;

    public static event Action<Unit> AuthorityrOnUnitSpawn;
    public static event Action<Unit> AuthorityOnUnitDeSpawn;

    [SerializeField]
    private Health health;

    [SerializeField]
    private UnitMovment unitMovement;

    [SerializeField]
    private Targeter targeter;

    [SerializeField]
    private UnityEvent OnSelected;

    [SerializeField]
    private UnityEvent OnDeSelected;

    [SerializeField]
    private int resourcesCost = 10;

    public UnitMovment UnitMovement => unitMovement;
    public Targeter Targeter => targeter;

    public int ResourcesCost => resourcesCost;

    #region Client

    private void Start()
    {
        OnDeSelected?.Invoke();
    }

    public override void OnStartAuthority()
    {
        AuthorityrOnUnitSpawn?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) return;

        AuthorityOnUnitDeSpawn?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!hasAuthority) return;

        OnSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority) return;

        OnDeSelected?.Invoke();
    }

    #endregion Client

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawn?.Invoke(this);
        health.ServerOnDie += HandleOnServeDie;
    }

    public override void OnStopServer()
    {
        ServerOnUnitDeSpawn?.Invoke(this);
        health.ServerOnDie -= HandleOnServeDie;
    }

    [Server]
    private void HandleOnServeDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion
}