using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroSidebar : MonoBehaviour
{
    public static HeroSidebar Instance { get; private set; }

    [Header("Sidebar Panel")]
    public GameObject sidebarPanel;
    public CanvasGroup canvasGroup;

    [Header("Hero Info")]
    public Image heroPortrait;
    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI heroTypeText;
    public TextMeshProUGUI backstoryText;

    [Header("Stats")]
    public TextMeshProUGUI healthText;
    public Slider healthSlider;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI attackSpeedText;

    [Header("Ultimate")]
    public TextMeshProUGUI ultimateNameText;
    public TextMeshProUGUI ultimateDescText;
    public Button finalStandButton;
    public TextMeshProUGUI finalStandButtonText;

    [Header("Sacrifice")]
    public Button sacrificeButton;
    public TextMeshProUGUI sacrificeResolveText;

    [Header("Close")]
    public Button closeButton;

    private Hero selectedHero;

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
        // Subscribe to hero selection
        Hero.OnHeroSelected += ShowSidebar;

        // Setup buttons
        if (finalStandButton != null)
            finalStandButton.onClick.AddListener(OnFinalStandClicked);

        if (sacrificeButton != null)
            sacrificeButton.onClick.AddListener(OnSacrificeClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(HideSidebar);

        // Hide initially
        HideSidebar();
    }

    void OnDestroy()
    {
        Hero.OnHeroSelected -= ShowSidebar;
    }

    void Update()
    {
        // Update health display if hero is selected
        if (selectedHero != null && !selectedHero.isDead)
        {
            UpdateHealthDisplay();
        }
    }

    public void ShowSidebar(Hero hero)
    {
        if (hero == null || hero.heroData == null) return;

        selectedHero = hero;
        HeroData data = hero.heroData;

        // Show panel
        if (sidebarPanel != null)
            sidebarPanel.SetActive(true);

        // Portrait
        if (heroPortrait != null && data.portrait != null)
            heroPortrait.sprite = data.portrait;

        // Basic info
        if (heroNameText != null)
            heroNameText.text = data.heroName;

        if (heroTypeText != null)
            heroTypeText.text = data.heroType.ToString();

        if (backstoryText != null)
            backstoryText.text = data.backstory;

        // Stats
        UpdateHealthDisplay();

        if (damageText != null)
            damageText.text = $"Damage: {data.attackDamage}";

        if (rangeText != null)
            rangeText.text = $"Range: {data.attackRange}";

        if (attackSpeedText != null)
            attackSpeedText.text = $"Attack Speed: {data.attackRate}/s";

        // Ultimate
        if (ultimateNameText != null)
            ultimateNameText.text = data.ultimateName;

        if (ultimateDescText != null)
            ultimateDescText.text = data.ultimateDescription;

        // Final Stand button
        if (finalStandButton != null)
        {
            finalStandButton.interactable = !hero.hasUsedUltimate;
            if (finalStandButtonText != null)
            {
                finalStandButtonText.text = hero.hasUsedUltimate ? "USED" : "FINAL STAND";
            }
        }

        // Sacrifice
        if (sacrificeResolveText != null)
            sacrificeResolveText.text = $"Sacrifice for {data.sacrificeResolveGain} Resolve";
    }

    void UpdateHealthDisplay()
    {
        if (selectedHero == null || selectedHero.heroData == null) return;

        if (healthText != null)
            healthText.text = $"HP: {selectedHero.currentHealth}/{selectedHero.heroData.maxHealth}";

        if (healthSlider != null)
        {
            healthSlider.maxValue = selectedHero.heroData.maxHealth;
            healthSlider.value = selectedHero.currentHealth;
        }
    }

    public void HideSidebar()
    {
        if (sidebarPanel != null)
            sidebarPanel.SetActive(false);

        selectedHero = null;
    }

    void OnFinalStandClicked()
    {
        if (selectedHero == null || selectedHero.isDead) return;

        // Confirmation could be added here
        selectedHero.ActivateFinalStand();

        // Update button state
        if (finalStandButton != null)
            finalStandButton.interactable = false;

        if (finalStandButtonText != null)
            finalStandButtonText.text = "ACTIVATED";

        // Close sidebar after a moment
        Invoke(nameof(HideSidebar), 1.5f);
    }

    void OnSacrificeClicked()
    {
        if (selectedHero == null || selectedHero.isDead) return;

        // Sacrifice the hero
        selectedHero.Sacrifice();

        // Close sidebar
        HideSidebar();
    }
}
