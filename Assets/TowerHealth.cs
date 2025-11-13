using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 200f;
    private float currentHealth;
    
    [Header("Visual Feedback")]
    public bool flashOnDamage = true;
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        Debug.Log($"Tower took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Visual feedback
        if (flashOnDamage && !isFlashing)
        {
            StartCoroutine(FlashDamage());
        }
        
        // Check if tower is destroyed
        if (currentHealth <= 0)
        {
            DestroyTower();
        }
    }
    
    void DestroyTower()
    {
        // Unregister from placement manager
        if (TowerPlacementManager.Instance != null)
        {
            TowerPlacementManager.Instance.UnregisterTowerPlacement(transform.position);
        }
        
        // Add destruction effects here (particles, sound, etc.)
        Debug.Log("Tower destroyed!");
        
        Destroy(gameObject);
    }
    
    System.Collections.IEnumerator FlashDamage()
    {
        isFlashing = true;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
        else
        {
            // Flash all child sprites
            SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();
            Color[] originalColors = new Color[childSprites.Length];
            
            for (int i = 0; i < childSprites.Length; i++)
            {
                originalColors[i] = childSprites[i].color;
                childSprites[i].color = damageColor;
            }
            
            yield return new WaitForSeconds(flashDuration);
            
            for (int i = 0; i < childSprites.Length; i++)
            {
                childSprites[i].color = originalColors[i];
            }
        }
        
        isFlashing = false;
    }
    
    /// <summary>
    /// Get current health percentage (0-1)
    /// </summary>
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    /// <summary>
    /// Heal the tower
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw health bar in editor
        if (Application.isPlaying)
        {
            float healthPercentage = currentHealth / maxHealth;
            Vector3 barPosition = transform.position + Vector3.up * 1f;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(barPosition - Vector3.right * 0.5f, barPosition + Vector3.right * 0.5f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(barPosition - Vector3.right * 0.5f, 
                           barPosition - Vector3.right * 0.5f + Vector3.right * healthPercentage);
        }
    }
}
