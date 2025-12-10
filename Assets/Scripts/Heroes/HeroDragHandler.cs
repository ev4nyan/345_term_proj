using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Add this to your existing hero portrait buttons.
/// Just assign the HeroData asset and it handles the rest.
/// </summary>
public class HeroDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Hero Settings")]
    public HeroData heroData; // The hero data asset

    [Header("Visual Settings")]
    public float dragAlpha = 0.6f;
    public Color normalColor = Color.white;
    public Color deployedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color fallenColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private GameObject draggedHero;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;
    private Image portraitImage;
    private Color originalColor;

    [Header("Grid / Tilemap Snapping")]
    [SerializeField] private Grid grid;

    void Awake()
    {
        mainCamera = Camera.main;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && GetComponent<RectTransform>() != null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Get the Image component if it exists (for visual feedback)
        portraitImage = GetComponent<Image>();
        if (portraitImage != null)
        {
            originalColor = portraitImage.color;
        }
    }

    void Start()
    {
        // Subscribe to roster changes to update portrait
        HeroRoster.OnRosterChanged += UpdatePortraitVisual;
        UpdatePortraitVisual();
    }

    void OnDestroy()
    {
        HeroRoster.OnRosterChanged -= UpdatePortraitVisual;
    }

    /// <summary>
    /// Updates the portrait color based on hero state.
    /// Only modifies color, not the sprite (keeps your existing portrait).
    /// </summary>
    public void UpdatePortraitVisual()
    {
        if (portraitImage == null || heroData == null) return;

        bool isFallen = heroData.hasPermadeath && HeroRoster.Instance != null && HeroRoster.Instance.IsHeroFallen(heroData);
        bool isDeployed = heroData.singlePlacement && HeroRoster.Instance != null && HeroRoster.Instance.IsHeroDeployed(heroData);

        if (isFallen)
        {
            // Gray out - hero is dead forever (only for permadeath heroes)
            portraitImage.color = fallenColor;
        }
        else if (isDeployed)
        {
            // Slightly dimmed - already on field (only for single placement heroes)
            portraitImage.color = deployedColor;
        }
        else
        {
            // Available
            portraitImage.color = normalColor;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (heroData == null || heroData.heroPrefab == null)
        {
            Debug.LogWarning("No hero data or prefab assigned!");
            return;
        }

        // Check if hero can be summoned
        if (!CanSummonHero())
        {
            Debug.Log($"Cannot summon {heroData.heroName}!");
            return;
        }

        // Make UI semi-transparent
        if (canvasGroup != null)
        {
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
        }

        // Create preview hero
        Vector3 worldPos = GetWorldPosition(eventData);
        draggedHero = Instantiate(heroData.heroPrefab, worldPos, Quaternion.identity);

        // Remove tag during drag so enemies don't target it
        draggedHero.tag = "Untagged";

        // Disable hero script during drag
        Hero heroScript = draggedHero.GetComponent<Hero>();
        if (heroScript != null)
        {
            heroScript.enabled = false;
        }

        // Also disable Tower script if present
        Tower towerScript = draggedHero.GetComponent<Tower>();
        if (towerScript != null)
        {
            towerScript.enabled = false;
        }

        // Make semi-transparent
        SetHeroAlpha(draggedHero, dragAlpha);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedHero != null)
        {
            Vector3 worldPos = GetWorldPosition(eventData);
            Vector3 snappedPos = SnapToGrid(worldPos);

            draggedHero.transform.position = snappedPos;
            UpdatePlacementVisual(snappedPos);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore UI
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        if (draggedHero == null) return;

        Vector3 worldPos = GetWorldPosition(eventData);
        Vector3 snappedPos = SnapToGrid(worldPos);

        // Check placement validity
        bool canPlace = CanPlaceAt(snappedPos);

        if (canPlace && ResolveManager.Instance.SpendResolve(heroData.resolveCost))
        {
            // Place the hero
            Hero heroScript = draggedHero.GetComponent<Hero>();
            if (heroScript != null)
            {
                heroScript.enabled = true;
                heroScript.heroData = heroData;

                // Register with roster
                if (HeroRoster.Instance != null)
                {
                    HeroRoster.Instance.DeployHero(heroData, heroScript);
                }
            }

            // Enable Tower script if present
            Tower towerScript = draggedHero.GetComponent<Tower>();
            if (towerScript != null)
            {
                towerScript.enabled = true;
            }

            // Restore opacity
            RestoreHeroOpacity(draggedHero);

            // Tag as Hero for enemy targeting
            draggedHero.tag = "Hero";

            // Register with placement manager if exists
            if (TowerPlacementManager.Instance != null)
            {
                TowerPlacementManager.Instance.RegisterTowerPlacement(snappedPos);
            }

            Debug.Log($"{heroData.heroName} deployed for {heroData.resolveCost} Resolve!");

            // Update portrait to show deployed state
            UpdatePortraitVisual();
        }
        else
        {
            // Invalid placement or not enough resolve
            Destroy(draggedHero);
            if (!canPlace)
                Debug.Log("Cannot place hero here!");
            else
                Debug.Log("Not enough Resolve!");
        }

        draggedHero = null;
    }

    bool CanSummonHero()
    {
        // Check if fallen (only matters if hasPermadeath)
        if (heroData.hasPermadeath && HeroRoster.Instance != null && HeroRoster.Instance.IsHeroFallen(heroData))
        {
            Debug.Log($"{heroData.heroName} has fallen and cannot return.");
            return false;
        }

        // Check if already deployed (only matters if singlePlacement)
        if (heroData.singlePlacement && HeroRoster.Instance != null && HeroRoster.Instance.IsHeroDeployed(heroData))
        {
            Debug.Log($"{heroData.heroName} is already on the battlefield.");
            return false;
        }

        // Check resolve
        if (ResolveManager.Instance == null || !ResolveManager.Instance.CanAfford(heroData.resolveCost))
        {
            Debug.Log($"Not enough Resolve to summon {heroData.heroName}. Need {heroData.resolveCost}.");
            return false;
        }

        return true;
    }

    bool CanPlaceAt(Vector3 position)
    {
        // Use TowerPlacementManager if available
        if (TowerPlacementManager.Instance != null)
        {
            return TowerPlacementManager.Instance.CanPlaceTower(position);
        }

        // Fallback: check for overlapping colliders
        Collider2D hit = Physics2D.OverlapCircle(position, 0.5f);
        return hit == null;
    }

    Vector3 SnapToGrid(Vector3 worldPos)
    {
        if (grid == null)
            return worldPos; // fallback if grid not assigned

        Vector3Int cell = grid.WorldToCell(worldPos);
        Vector3 snapped = grid.GetCellCenterWorld(cell);
        snapped.z = 0f;
        return snapped;
    }

    Vector3 GetWorldPosition(PointerEventData eventData)
    {
        Vector3 mousePos = eventData.position;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        return worldPos;
    }

    void UpdatePlacementVisual(Vector3 position)
    {
        bool canPlace = CanPlaceAt(position);
        Color visualColor = canPlace ? new Color(0.5f, 1f, 0.5f, dragAlpha) : new Color(1f, 0.5f, 0.5f, dragAlpha);

        SetHeroColor(draggedHero, visualColor);
    }

    void SetHeroAlpha(GameObject hero, float alpha)
    {
        SpriteRenderer sprite = hero.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color c = sprite.color;
            c.a = alpha;
            sprite.color = c;
        }

        foreach (var childSprite in hero.GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = childSprite.color;
            c.a = alpha;
            childSprite.color = c;
        }
    }

    void SetHeroColor(GameObject hero, Color color)
    {
        SpriteRenderer sprite = hero.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = color;
        }

        foreach (var childSprite in hero.GetComponentsInChildren<SpriteRenderer>())
        {
            childSprite.color = color;
        }
    }

    void RestoreHeroOpacity(GameObject hero)
    {
        SpriteRenderer sprite = hero.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.white;
        }

        foreach (var childSprite in hero.GetComponentsInChildren<SpriteRenderer>())
        {
            childSprite.color = Color.white;
        }
    }
}
