using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public int health = 100;
    public int attackDamage = 25;
    public float attackRange = 1.8f;
    public float attackCooldown = 0.6f;

    public bool isBlocking = false;

    [Header("UI Setup")]
    public Slider healthSlider;
    public Image sliderFillImage;
    public Gradient healthGradient;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float moveInput;
    private float nextAttackTime;
    private int maxHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        maxHealth = health;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
            CreateDefaultGradient();
            UpdateHealthBarColor();
        }
    }

    void Update()
    {
        moveInput = 0;

        if (Keyboard.current.aKey.isPressed)
            moveInput = -1;

        if (Keyboard.current.dKey.isPressed)
            moveInput = 1;

        isBlocking = Mouse.current.rightButton.isPressed;

        if (isBlocking)
        {
            sr.color = Color.blue;
        }
        else if (Time.time < nextAttackTime - (attackCooldown - 0.15f))
        {
            sr.color = Color.red;
        }
        else
        {
            sr.color = Color.white;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime && !isBlocking)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void FixedUpdate()
    {
        if (isBlocking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }
    }

    void Attack()
    {
        sr.color = Color.red;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            AIPlayerMovement enemy = hit.GetComponent<AIPlayerMovement>();

            if (enemy != null)
            {
                // CRITICAL: Pass 'this' so the AI knows who attacked it
                enemy.TakeDamage(attackDamage, this);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isBlocking)
        {
            damage = (int)(damage * 0.15f);
        }

        health -= damage;

        if (healthSlider != null)
        {
            healthSlider.value = health;
            UpdateHealthBarColor();
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    void UpdateHealthBarColor()
    {
        if (sliderFillImage != null && healthSlider != null)
        {
            float healthNormalized = (float)health / maxHealth;
            sliderFillImage.color = healthGradient.Evaluate(healthNormalized);
        }
    }

    void CreateDefaultGradient()
    {
        healthGradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[3];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[1];

        colorKey[0] = new GradientColorKey(Color.red, 0.0f);
        colorKey[1] = new GradientColorKey(Color.yellow, 0.5f);
        colorKey[2] = new GradientColorKey(Color.green, 1.0f);

        alphaKey[0] = new GradientAlphaKey(1.0f, 0.0f);
        healthGradient.SetKeys(colorKey, alphaKey);
    }
}