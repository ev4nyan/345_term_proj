using UnityEngine;
using UnityEngine.EventSystems;

public class TowerDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Tower Settings")]
    public GameObject towerPrefab; // The actual tower prefab to instantiate
    
    [Header("Visual Settings")]
    public float dragAlpha = 0.6f; // Transparency while dragging
    
    private GameObject draggedTower;
    private SpriteRenderer draggedTowerSprite;
    private Camera mainCamera;
    private Vector3 offset;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        mainCamera = Camera.main;
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Add CanvasGroup if not present (for UI elements)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && GetComponent<RectTransform>() != null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (towerPrefab == null)
        {
            Debug.LogWarning("No tower prefab assigned to TowerDragHandler!");
            return;
        }
        
        // Make the UI element semi-transparent while dragging
        if (canvasGroup != null)
        {
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
        }
        
        // Create a ghost/preview of the tower at mouse position
        Vector3 worldPos = GetWorldPosition(eventData);
        draggedTower = Instantiate(towerPrefab, worldPos, Quaternion.identity);
        
        // Disable the tower script during drag (so it doesn't start attacking)
        Tower towerScript = draggedTower.GetComponent<Tower>();
        if (towerScript != null)
        {
            towerScript.enabled = false;
        }
        
        // Make it semi-transparent
        draggedTowerSprite = draggedTower.GetComponent<SpriteRenderer>();
        if (draggedTowerSprite != null)
        {
            Color color = draggedTowerSprite.color;
            color.a = dragAlpha;
            draggedTowerSprite.color = color;
        }
        else
        {
            // Check children for sprite renderers
            SpriteRenderer[] childSprites = draggedTower.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in childSprites)
            {
                Color color = sprite.color;
                color.a = dragAlpha;
                sprite.color = color;
            }
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedTower != null)
        {
            // Update position to follow mouse
            Vector3 worldPos = GetWorldPosition(eventData);
            draggedTower.transform.position = worldPos;
            
            // Optional: Add visual feedback for valid/invalid placement
            UpdatePlacementVisual(worldPos);
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore UI element
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        if (draggedTower == null)
            return;
        
        Vector3 worldPos = GetWorldPosition(eventData);
        
        // Check if placement is valid
        if (TowerPlacementManager.Instance != null && 
            TowerPlacementManager.Instance.CanPlaceTower(worldPos))
        {
            // Place the tower permanently
            Tower towerScript = draggedTower.GetComponent<Tower>();
            if (towerScript != null)
            {
                towerScript.enabled = true;
            }
            
            // Restore full opacity
            RestoreTowerOpacity(draggedTower);
            
            // Register with placement manager
            TowerPlacementManager.Instance.RegisterTowerPlacement(worldPos);
        }
        else
        {
            // Invalid placement - destroy the tower
            Destroy(draggedTower);
            Debug.Log("Cannot place tower here!");
        }
        
        draggedTower = null;
    }
    
    private Vector3 GetWorldPosition(PointerEventData eventData)
    {
        Vector3 mousePos = eventData.position;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0; // Keep it at z=0 for 2D
        return worldPos;
    }
    
    private void UpdatePlacementVisual(Vector3 position)
    {
        // Change color based on whether placement is valid
        bool canPlace = TowerPlacementManager.Instance != null && 
                       TowerPlacementManager.Instance.CanPlaceTower(position);
        
        Color visualColor = canPlace ? new Color(0.5f, 1f, 0.5f, dragAlpha) : new Color(1f, 0.5f, 0.5f, dragAlpha);
        
        if (draggedTowerSprite != null)
        {
            draggedTowerSprite.color = visualColor;
        }
        else
        {
            SpriteRenderer[] childSprites = draggedTower.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in childSprites)
            {
                sprite.color = visualColor;
            }
        }
    }
    
    private void RestoreTowerOpacity(GameObject tower)
    {
        SpriteRenderer sprite = tower.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.white; // Reset to full white color
        }
        
        SpriteRenderer[] childSprites = tower.GetComponentsInChildren<SpriteRenderer>();
        foreach (var childSprite in childSprites)
        {
            childSprite.color = Color.white; // Reset to full white color
        }
    }
}
