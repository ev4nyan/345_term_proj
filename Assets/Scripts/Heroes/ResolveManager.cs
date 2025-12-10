using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResolveManager : MonoBehaviour
{
    public static ResolveManager Instance { get; private set; }

    [Header("Resolve")]
    public int currentResolve = 100; // Starting resolve
    public int resolvePerKill = 5;

    [Header("UI")]
    public TextMeshProUGUI resolveText;
    public Image resolveIcon;

    // Events
    public static event Action<int> OnResolveChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        UpdateUI();

        // Subscribe to enemy deaths for resolve gain
        EnemyHealth.OnEnemyDied += OnEnemyKilled;
    }

    void OnDestroy()
    {
        EnemyHealth.OnEnemyDied -= OnEnemyKilled;
    }

    void OnEnemyKilled()
    {
        AddResolve(resolvePerKill);
    }

    public void AddResolve(int amount)
    {
        currentResolve += amount;
        UpdateUI();
        OnResolveChanged?.Invoke(currentResolve);
    }

    public bool SpendResolve(int amount)
    {
        if (currentResolve >= amount)
        {
            currentResolve -= amount;
            UpdateUI();
            OnResolveChanged?.Invoke(currentResolve);
            CheckForGameOver();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reduce resolve directly (e.g., when enemies reach the end).
    /// </summary>
    public void LoseResolve(int amount)
    {
        currentResolve -= amount;
        if (currentResolve < 0) currentResolve = 0;
        UpdateUI();
        OnResolveChanged?.Invoke(currentResolve);
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (currentResolve <= 0)
        {
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.TriggerLose();
            }
        }
    }

    public bool CanAfford(int amount)
    {
        return currentResolve >= amount;
    }

    void UpdateUI()
    {
        if (resolveText != null)
        {
            resolveText.text = currentResolve.ToString();
        }
    }
}
