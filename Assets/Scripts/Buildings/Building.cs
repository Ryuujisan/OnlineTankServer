using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Building : NetworkBehaviour
{
    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;

    public static event Action<Building> AuthorityrOnBuildingSpawn;
    public static event Action<Building> AuthorityOnBuildingDeSpawn;

    [SerializeField]
    private GameObject buildingPreview;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private int id = -1;

    [SerializeField]
    private int price = 100;

    public Sprite Icon => icon;

    public int Price => price;

    public int ID => id;

    public GameObject BuildingPreview => buildingPreview;

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);
    }

    #endregion Server

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityrOnBuildingSpawn?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) return;

        AuthorityOnBuildingDeSpawn?.Invoke(this);
    }

    #endregion
}