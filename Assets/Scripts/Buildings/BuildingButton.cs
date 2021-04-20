using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Building building;

    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TMP_Text priceText;

    [SerializeField]
    private LayerMask floorMask;

    private BoxCollider boxCollider;
    
    private Camera mainCamera;
    private RTSPlayer player;

    private GameObject buildingPrevievInstance;
    private Renderer buildingRenderer;

    private void Start()
    {
        mainCamera = Camera.main;

        iconImage.sprite = building.Icon;
        priceText.text = building.Price.ToString();

        boxCollider = building.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        UpdateBuildingInstance();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (player.Resources < building.Price)
        {
            return;
        }
        
        buildingPrevievInstance = Instantiate(building.BuildingPreview);
        buildingRenderer = buildingPrevievInstance.GetComponentInChildren<Renderer>();
        
        buildingPrevievInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPrevievInstance == null)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, floorMask))
        {
            player.CmdTryPlaceBuilding(building.ID, hit.point);
        }
        
        Destroy(buildingPrevievInstance);
    }

    private void UpdateBuildingInstance()
    {
        if (buildingPrevievInstance == null)
        {
            return;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, floorMask))
        {
            return;
        }

        buildingPrevievInstance.transform.position = hit.point;
        if (!buildingPrevievInstance.activeSelf)
        {
            buildingPrevievInstance.SetActive(true);
        }

        Color color = player.CanPlaceBuilding(boxCollider, hit.point) ? Color.green : Color.red;
        
        buildingRenderer.material.SetColor("_BaseColor", color);
    }
}
