using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AIPlayerMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5.5f;
    public float attackRange = 1.8f;
    public int damage = 25;
    public float attackCooldown = 0.8f;
    public float damageDelay = 0.15f;
    public float animationLength = 0.5f; // Set to your exact attack clip length
    public int health = 100;

    [Header("UI Setup")]
    public Slider healthSlider;
    public Image sliderFillImage;
    public Gradient healthGradient;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private float attackTimer;
    private bool isBlocking;
    private bool isAttacking;

    private float blockDecisionTimer;
    private float blockDecisionCooldown = 0.6f;
    private int maxHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

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

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

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
                StartCoroutine(PerformAttack(playerScript));
                attackTimer = Time.time + attackCooldown + animationLength;
            }
        }
    }

    private IEnumerator PerformAttack(PlayerMovement playerScript)
    {
        isAttacking = true;

        // Force an immediate transition into AI_Attack over 0.01 seconds
        if (anim != null)
        {
            anim.CrossFadeInFixedTime("AI_Attack", 0.01f);
        }

        // Wait for visual impact frame
        yield return new WaitForSeconds(damageDelay);

        if (player != null && playerScript != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            playerScript.TakeDamage(damage);
        }

        // Wait out remaining animation duration
        float remainingTime = Mathf.Max(0.05f, animationLength - damageDelay);
        yield return new WaitForSeconds(remainingTime);

        // Force return to idle state
        if (anim != null)
        {
            anim.CrossFadeInFixedTime("ai", 0.01f);
        }

        isAttacking = false;
    }

    public void TakeDamage(int incomingDamage, PlayerMovement playerScript)
    {
        if (isBlocking)
        {
            if (playerScript != null)
            {
                playerScript.TakeDamage(15);
            }
            incomingDamage = (int)(incomingDamage * 0.15f);
        }

        health -= incomingDamage;

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