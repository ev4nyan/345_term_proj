using UnityEngine;
using TMPro;   // or using UnityEngine.UI; if you use normal Text

public class EssenceManager : MonoBehaviour
{
    public static EssenceManager Instance { get; private set; }

    [Header("Starting Amount")]
    public int startingEssence = 10;

    [Header("UI")]
    public TMP_Text essenceText;

    private int currentEssence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // If you want essence to persist across scenes:
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        currentEssence = startingEssence;
        UpdateUI();
    }

    // --- Public API ---

    public int GetEssence()
    {
        return currentEssence;
    }

    public bool CanAfford(int amount)
    {
        return currentEssence >= amount;
    }

    public bool Spend(int amount)
    {
        if (!CanAfford(amount))
            return false;

        currentEssence -= amount;
        UpdateUI();
        return true;
    }

    public void Add(int amount)
    {
        currentEssence += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (essenceText != null)
            essenceText.text = currentEssence.ToString();
    }
}