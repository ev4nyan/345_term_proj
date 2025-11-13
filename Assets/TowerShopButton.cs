using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Optional script for tower shop buttons. Attach this to UI buttons to show tower info.
/// The TowerDragHandler should be attached to the same button for drag functionality.
/// </summary>
public class TowerShopButton : MonoBehaviour
{
    [Header("Tower Info")]
    public string towerName = "Basic Tower";
    public int cost = 100;
    public Sprite towerIcon;
    
    [Header("UI References (Optional)")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Image iconImage;
    
    void Start()
    {
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (nameText != null)
        {
            nameText.text = towerName;
        }
        
        if (costText != null)
        {
            costText.text = "$" + cost.ToString();
        }
        
        if (iconImage != null && towerIcon != null)
        {
            iconImage.sprite = towerIcon;
        }
    }
    
    /// <summary>
    /// Check if player can afford this tower (requires a resource manager)
    /// </summary>
    public bool CanAfford()
    {
        // TODO: Implement this when you have a resource/gold manager
        // Example: return GameManager.Instance.Gold >= cost;
        return true; // For now, always return true
    }
    
    /// <summary>
    /// Deduct the cost from player resources (requires a resource manager)
    /// </summary>
    public void Purchase()
    {
        // TODO: Implement this when you have a resource/gold manager
        // Example: GameManager.Instance.Gold -= cost;
        Debug.Log($"Purchased {towerName} for ${cost}");
    }
}
