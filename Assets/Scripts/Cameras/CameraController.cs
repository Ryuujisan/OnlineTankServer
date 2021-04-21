using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField]
    private Transform playerCameraTransform;

    [SerializeField]
    private float speed = 20f;

    [SerializeField]
    private float screenBorderThickness;

    [SerializeField]
    private Vector2 screenXLimits = Vector2.zero;

    [SerializeField]
    private Vector2 screenZLimits = Vector2.zero;

    private Vector2 previusInput;

    private Controls controls;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        controls = new Controls();

        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority || !Application.isFocused) return;

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        var pos = playerCameraTransform.position;

        if (previusInput == Vector2.zero)
        {
            var cursorsMovement = Vector3.zero;

            var cursorsPosition = Mouse.current.position.ReadValue();

            if (cursorsPosition.y >= Screen.height - screenBorderThickness)
                cursorsMovement.z += 1;
            else if (cursorsPosition.y <= screenBorderThickness) cursorsMovement.z -= 1;

            if (cursorsPosition.x >= Screen.width - screenBorderThickness)
                cursorsMovement.x += 1;
            else if (cursorsPosition.x <= screenBorderThickness) cursorsMovement.x -= 1;

            pos += cursorsMovement.normalized * speed * Time.deltaTime;
        }
        else
        {
            pos += new Vector3(previusInput.x, 0f, previusInput.y) * speed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);


        playerCameraTransform.position = pos;
    }

    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previusInput = ctx.ReadValue<Vector2>();
    }
}