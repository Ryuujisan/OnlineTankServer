using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandle : MonoBehaviour
{
    [SerializeField]
    private RectTransform unitSelectionArea = null;

    [SerializeField]
    private LayerMask layerMask = new LayerMask();

    private Vector2 startPosition;

    private RTSPlayer player;
    private Camera mainCamera;

    public List<Unit> SelectedUnit { get; } = new List<Unit>();

    private void Start()
    {
        mainCamera = Camera.main;

        Unit.AuthorityOnUnitDeSpawn += UnitOnAuthorityOnUnitDeSpawn;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDeSpawn += UnitOnAuthorityOnUnitDeSpawn;
    }

    private void Update()
    {
        if (player == null) player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            StartSelectionArea();
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
            ClearSelectionArea();
        else if (Mouse.current.leftButton.isPressed) UpdateSelectionArea();
    }

    private void StartSelectionArea()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (var sUnit in SelectedUnit) sUnit.Deselect();

            SelectedUnit.Clear();
        }


        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        var mousePosition = Mouse.current.position.ReadValue();

        var areaWidth = mousePosition.x - startPosition.x;
        var areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask)) return;

            if (!hit.collider.TryGetComponent<Unit>(out var unit)) return;

            if (!unit.hasAuthority) return;

            SelectedUnit.Add(unit);

            foreach (var sUnit in SelectedUnit) sUnit.Select();

            return;
        }

        var min = unitSelectionArea.anchoredPosition - unitSelectionArea.sizeDelta / 2;
        var max = unitSelectionArea.anchoredPosition + unitSelectionArea.sizeDelta / 2;

        foreach (var unit in player.MyUnits)
        {
            if (SelectedUnit.Contains(unit)) continue;
            var screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x &&
                screenPosition.x < max.x &&
                screenPosition.y > min.y &&
                screenPosition.y < max.y)
            {
                SelectedUnit.Add(unit);
                unit.Select();
            }
        }
    }

    private void UnitOnAuthorityOnUnitDeSpawn(Unit unit)
    {
        SelectedUnit.Remove(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}