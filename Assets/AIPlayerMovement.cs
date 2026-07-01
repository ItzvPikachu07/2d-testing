using UnityEngine;
using UnityEngine.UI;

public class AIPlayerMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5.5f;
    public float attackRange = 1.8f;
    public int damage = 25;
    public float attackCooldown = 0.5f;
    public int health = 100;

    [Header("UI Setup")]
    public Slider healthSlider;
    public Image sliderFillImage;
    public Gradient healthGradient;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float attackTimer;
    private bool isBlocking;

    private float blockDecisionTimer;
    private float blockDecisionCooldown = 0.6f;
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

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();

        if (Time.time >= blockDecisionTimer)
        {
            if (playerScript != null && distance < attackRange + 0.8f)
            {
                isBlocking = Random.value < 0.4f;
            }
            else
            {
                isBlocking = false;
            }

            blockDecisionTimer = Time.time + blockDecisionCooldown;
        }

        if (isBlocking)
        {
            sr.color = Color.blue;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            sr.color = Color.white;
        }

        if (distance > attackRange && !isBlocking)
        {
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
        else if (distance <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (!isBlocking && Time.time >= attackTimer)
            {
                sr.color = Color.red;

                if (playerScript != null)
                {
                    playerScript.TakeDamage(damage);
                }

                attackTimer = Time.time + attackCooldown + Random.Range(0.1f, 0.4f);
            }
        }
    }

    public void TakeDamage(int incomingDamage, PlayerMovement playerScript)
    {
        if (isBlocking)
        {
            if (playerScript != null)
            {
                playerScript.TakeDamage(15); // Counter-damage
            }
            incomingDamage = (int)(incomingDamage * 0.15f);
        }

        health -= incomingDamage;

        if (healthSlider != null)
        {
            healthSlider.value = health;
            UpdateHealthBarColor(); // Updates AI bar size & color
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