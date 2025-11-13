using UnityEngine;
using System.Collections.Generic;

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance { get; private set; }
    
    [Header("Placement Rules")]
    public float minDistanceBetweenTowers = 2f;
    public LayerMask blockingLayers; // Layers that block tower placement (e.g., path, other towers)
    public float placementCheckRadius = 0.5f;
    
    [Header("Placement Area (Optional)")]
    public bool useRestrictedArea = false;
    public Bounds placementBounds; // Area where towers can be placed
    
    private List<Vector3> towerPositions = new List<Vector3>();
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Check if a tower can be placed at the given position
    /// </summary>
    public bool CanPlaceTower(Vector3 position)
    {
        // Check if position is within bounds (if restricted area is enabled)
        if (useRestrictedArea && !placementBounds.Contains(position))
        {
            return false;
        }
        
        // Check if too close to other towers
        foreach (Vector3 towerPos in towerPositions)
        {
            if (Vector3.Distance(position, towerPos) < minDistanceBetweenTowers)
            {
                return false;
            }
        }
        
        // Check if there's a blocking object at this position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, placementCheckRadius, blockingLayers);
        if (colliders.Length > 0)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Register a tower placement (called after successful placement)
    /// </summary>
    public void RegisterTowerPlacement(Vector3 position)
    {
        towerPositions.Add(position);
    }
    
    /// <summary>
    /// Remove a tower from the registry (useful for selling/removing towers)
    /// </summary>
    public void UnregisterTowerPlacement(Vector3 position)
    {
        towerPositions.Remove(position);
    }
    
    /// <summary>
    /// Get the nearest tower position to a given point
    /// </summary>
    public Vector3 GetNearestTowerPosition(Vector3 position)
    {
        if (towerPositions.Count == 0)
            return Vector3.zero;
        
        Vector3 nearest = towerPositions[0];
        float minDistance = Vector3.Distance(position, nearest);
        
        foreach (Vector3 towerPos in towerPositions)
        {
            float distance = Vector3.Distance(position, towerPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = towerPos;
            }
        }
        
        return nearest;
    }
    
    void OnDrawGizmos()
    {
        // Draw placement bounds in editor
        if (useRestrictedArea)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(placementBounds.center, placementBounds.size);
        }
        
        // Draw tower positions
        Gizmos.color = Color.cyan;
        foreach (Vector3 pos in towerPositions)
        {
            Gizmos.DrawWireSphere(pos, minDistanceBetweenTowers / 2f);
        }
    }
}
