using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGive : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask = new LayerMask();

    [SerializeField]
    private UnitSelectionHandle unitSelectionHandle;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }


    private void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;

        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask)) return;

        if (hit.collider.TryGetComponent<Targetable>(out var target))
        {
            if (target.hasAuthority)
            {
                TryMove(hit.point);
                return;
            }

            TryTarget(target);
            return;
        }

        TryMove(hit.point);
    }

    private void TryTarget(Targetable target)
    {
        foreach (var unit in unitSelectionHandle.SelectedUnit) unit.Targeter.CmdSetTarget(target.gameObject);
    }

    private void TryMove(Vector3 point)
    {
        foreach (var unit in unitSelectionHandle.SelectedUnit) unit.UnitMovement.CmdMove(point);
    }

    private void ClientHandleGameOver(string winner)
    {
        enabled = false;
    }
}