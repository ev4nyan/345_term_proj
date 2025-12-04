using UnityEngine;
using System;

/// <summary>
/// Base class for all heroes. Use the specialized classes:
/// - HeroKnight: Melee AoE attacks (like KingController)
/// - HeroArcher: Ranged projectile attacks
/// - HeroMage: Cone attacks (like FlamethrowerTower)
/// </summary>
public class Hero : MonoBehaviour
{
    [Header("Hero Data")]
    public HeroData heroData;

    [Header("Runtime State")]
    public int currentHealth;
    public bool isDead = false;
    public bool hasUsedUltimate = false;

    [Header("Combat")]
    public LayerMask enemyLayer;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected bool facingRight = true;

    // Events
    public static event Action<Hero> OnHeroSelected;
    public static event Action<Hero> OnHeroDied;
    public static event Action<Hero> OnHeroSacrificed;

    protected Animator animator;
    protected Transform target;
    protected float nextAttackTime = 0f;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    protected virtual void Start()
    {
        if (heroData != null)
        {
            currentHealth = heroData.maxHealth;
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // Flip towards target if we have one
        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
            if (direction.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (direction.x < 0 && facingRight)
            {
                Flip();
            }
        }
    }

    protected void DealDamageToTarget(GameObject targetObj, float damage)
    {
        EnemyHealth eh = targetObj.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.TakeDamage((int)damage);
            return;
        }

        Enemy enemy = targetObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Flash red
        if (spriteRenderer != null)
        {
            StopCoroutine(nameof(FlashDamage));
            StartCoroutine(FlashDamage());
        }

        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashDamage()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;

        if (animator != null)
            animator.SetTrigger("Die");

        // Notify the roster - PERMADEATH
        OnHeroDied?.Invoke(this);

        // Disable combat
        CancelInvoke();
        enabled = false;

        // Destroy after death animation
        Destroy(gameObject, 1.5f);
    }

    // FINAL STAND - Ultimate ability that kills the hero
    public void ActivateFinalStand()
    {
        if (isDead || hasUsedUltimate) return;

        hasUsedUltimate = true;

        // Execute ultimate based on type
        switch (heroData.ultimateType)
        {
            case UltimateType.UnbreakableShield:
                ExecuteUnbreakableShield();
                break;
            case UltimateType.ArrowStorm:
                ExecuteArrowStorm();
                break;
            case UltimateType.Meteor:
                ExecuteMeteor();
                break;
        }

        // Hero dies after using ultimate
        StartCoroutine(DieAfterUltimate());
    }

    System.Collections.IEnumerator DieAfterUltimate()
    {
        yield return new WaitForSeconds(1f);
        Die();
    }

    void ExecuteUnbreakableShield()
    {
        // Knight: Massive AoE damage and brief invulnerability to nearby allies
        Debug.Log($"{heroData.heroName} activates UNBREAKABLE SHIELD!");

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, heroData.ultimateRadius, enemyLayer);
        foreach (var enemy in enemies)
        {
            DealDamageToTarget(enemy.gameObject, heroData.ultimateDamage);
        }

        // Visual effect placeholder
        // TODO: Add particle effects
    }

    void ExecuteArrowStorm()
    {
        // Archer: Rain of arrows across a large area
        Debug.Log($"{heroData.heroName} activates ARROW STORM!");

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, heroData.ultimateRadius * 1.5f, enemyLayer);
        foreach (var enemy in enemies)
        {
            DealDamageToTarget(enemy.gameObject, heroData.ultimateDamage * 0.8f);
        }
    }

    void ExecuteMeteor()
    {
        // Mage: Devastating meteor strike
        Debug.Log($"{heroData.heroName} activates METEOR!");

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, heroData.ultimateRadius * 2f, enemyLayer);
        foreach (var enemy in enemies)
        {
            DealDamageToTarget(enemy.gameObject, heroData.ultimateDamage * 1.5f);
        }
    }

    // Sacrifice for Resolve
    public void Sacrifice()
    {
        if (isDead) return;

        Debug.Log($"{heroData.heroName} sacrifices themselves for {heroData.sacrificeResolveGain} Resolve!");

        OnHeroSacrificed?.Invoke(this);

        // Grant resolve
        ResolveManager.Instance?.AddResolve(heroData.sacrificeResolveGain);

        // Die immediately
        Die();
    }

    // Click detection
    void OnMouseDown()
    {
        if (isDead) return;
        OnHeroSelected?.Invoke(this);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (heroData == null) return;

        // Attack range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, heroData.attackRange);

        // Ultimate range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
    }
}
