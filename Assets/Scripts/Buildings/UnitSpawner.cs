using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Health health;

    [SerializeField]
    private Unit unitPrefab;

    [SerializeField]
    private Transform spawnUnit;

    [SerializeField]
    private TMP_Text remainingUnitsText;

    [SerializeField]
    private Image unitProgressImage;

    [SerializeField]
    private int maxUnitQueue;

    [SerializeField]
    private float spawnMoveRange = 7;

    [SerializeField]
    private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdate))]
    private int queudeUnits;

    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += HandleServerOnDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= HandleServerOnDie;
    }

    private void ClientHandleQueuedUnitsUpdate(int oldValue, int newValue)
    {
        remainingUnitsText.text = newValue.ToString();
    }

    [Server]
    private void HandleServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (queudeUnits == maxUnitQueue) return;

        var player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.Resources < unitPrefab.ResourcesCost) return;

        queudeUnits++;
        player.Resources -= unitPrefab.ResourcesCost;
    }

    [Server]
    private void ProduceUnit()
    {
        if (queudeUnits == 0) return;

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) return;

        var unitInstant = Instantiate(unitPrefab,
            spawnUnit.position,
            spawnUnit.rotation);

        NetworkServer.Spawn(unitInstant.gameObject, connectionToClient);

        var spawnOffSet = UnityEngine.Random.insideUnitSphere * spawnMoveRange;
        spawnOffSet.y = spawnUnit.position.y;

        var unitMovement = unitInstant.GetComponent<UnitMovment>();
        unitMovement.ServerMove(spawnUnit.position + spawnOffSet);
        queudeUnits--;
        unitTimer = 0f;
    }

    #endregion Server

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!hasAuthority) return;

        CmdSpawnUnit();
    }

    private void UpdateTimerDisplay()
    {
        var newProgress = unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount)
            unitProgressImage.fillAmount = newProgress;
        else
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f);
    }

    #endregion Client

    private void Update()
    {
        if (isServer) ProduceUnit();

        if (isClient) UpdateTimerDisplay();
    }
}