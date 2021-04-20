using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField]
    private LayerMask buildingPlaceMask;

    [SerializeField]
    private float rangeBuilding;
    
    [SerializeField]
    private List<Building> buildingsPrefabs = null;
    
    [SerializeField]
    private List<Unit> myUnits = new List<Unit>();

    [SerializeField]
    private List<Building> myBuilding = new List<Building>();

    [SyncVar(hook = nameof(ClientHandleResourcesUpdate))]
    private int resources = 500;

    private Color teamColor;
    
    public event Action<int> ClientOnResourcesChanged; 
    
    public List<Unit> MyUnits => myUnits;

    public List<Building> MyBuilding => myBuilding;

    public Color TeamColor
    {
        [Server]
        set => teamColor = value;
        get => teamColor;
    }

    public int Resources
    {
        [Server]
        set => resources = value;
        get => resources;
    }

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        if (Physics.CheckBox(point + buildingCollider.center, 
            buildingCollider.size / 2,
            Quaternion.identity,
            buildingPlaceMask))
        {
            return false;
        }

        foreach (var building in myBuilding)
        {
            if ((point - building.transform.position).sqrMagnitude 
                <= rangeBuilding * rangeBuilding)
            {
                return true;
            }
        }

        return false;
    }
    
    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawn += UnitOnServerOnUnitSpawn;
        Unit.ServerOnUnitDeSpawn += UnitOnServerOnUnitDeSpawn;
        
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawn;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawn;
    }
    
    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawn -= UnitOnServerOnUnitSpawn;
        Unit.ServerOnUnitDeSpawn -= UnitOnServerOnUnitDeSpawn;
        
        Building.ServerOnBuildingSpawned  -= ServerHandleBuildingSpawn;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawn;
    }

    [Server]
    private void ServerHandleBuildingSpawn(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }
        myBuilding.Add(building);
    }

    [Server]
    private void ServerHandleBuildingDespawn(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }
        myBuilding.Remove(building);
    }
    
    [Server]
    private void UnitOnServerOnUnitSpawn(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }
        myUnits.Add(unit);
    }
    
    [Server]
    private void UnitOnServerOnUnitDeSpawn(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }
        myUnits.Remove(unit);
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        var buildingToPlace = buildingsPrefabs.Find(f => f.ID == buildingId);
        if (buildingToPlace == null)
        {
            return;
        }

        if (resources < buildingToPlace.Price)
        {
            return;
        }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();
        
        if (!CanPlaceBuilding(buildingCollider, point))
        {
            return;
        }
        
        resources -= buildingToPlace.Price;
        
        var buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        
        NetworkServer.Spawn(buildingInstance, connectionToClient);
    }
    
    #endregion Server

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active)
        {
            return;
        }
        
        Unit.AuthorityrOnUnitSpawn += UnitOnAuthorityOnUnitSpawn;
        Unit.AuthorityOnUnitDeSpawn += UnitOnAuthorityOnUnitDeSpawn;

        Building.AuthorityrOnBuildingSpawn += AuthorityOnBuildingSpawn;
        Building.AuthorityOnBuildingDeSpawn += AuthorityOnBuildingDespawn;
    }


    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority)
        {
            return;
        }
        
        Unit.AuthorityrOnUnitSpawn -= UnitOnAuthorityOnUnitSpawn;
        Unit.AuthorityOnUnitDeSpawn -= UnitOnAuthorityOnUnitDeSpawn;
        
        Building.AuthorityrOnBuildingSpawn -= AuthorityOnBuildingSpawn;
        Building.AuthorityOnBuildingDeSpawn -= AuthorityOnBuildingDespawn;
    }
    
    private void UnitOnAuthorityOnUnitSpawn(Unit unit)
    {
        myUnits.Add(unit);
    }
    
    private void UnitOnAuthorityOnUnitDeSpawn(Unit unit)
    {
        myUnits.Remove(unit);
    }
    
    private void AuthorityOnBuildingSpawn(Building building)
    {
        myBuilding.Add(building);
    }
    
    private void AuthorityOnBuildingDespawn(Building building)
    {
        myBuilding.Remove(building);
    }

    private void ClientHandleResourcesUpdate(int oldValue, int newValue)
    {
        ClientOnResourcesChanged?.Invoke(newValue);
    }

    #endregion
}
