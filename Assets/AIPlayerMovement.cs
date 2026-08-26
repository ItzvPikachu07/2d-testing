using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AIPlayerMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5.5f;
    public float attackRange = 1.8f;
    public int damage = 25;
    public float attackCooldown = 0.6f;
    public float damageDelay = 0.2f;
    public float animationLength = 0.517f;
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

    private string currentState = "";

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

        // HARD LOCK: Stop movement completely while attacking
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();

        // Handle defensive decision timing
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
            ChangeAnimationState("ai");
            return;
        }
        else
        {
            sr.color = Color.white;
        }

        // Only move IF completely out of attack range AND not currently attacking
        if (distance > attackRange)
        {
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            // Inverted sprite flip logic
            sr.flipX = direction > 0;

            ChangeAnimationState("AI_Walk");
        }
        else
        {
            // Full stop when inside attack range
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (Time.time >= attackTimer)
            {
                StartCoroutine(PerformAttack(playerScript));
                attackTimer = Time.time + attackCooldown + animationLength;
            }
            else
            {
                ChangeAnimationState("ai");
            }
        }
    }

    private IEnumerator PerformAttack(PlayerMovement playerScript)
    {
        isAttacking = true;

        // Zero out velocity instantly so momentum doesn't slide him into you
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.speed = 1.0f;
        }

        ChangeAnimationState("AI_Attack", true);

        // Wait for impact frame
        yield return new WaitForSeconds(damageDelay);

        if (player != null && playerScript != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            playerScript.TakeDamage(damage);
        }

        // Wait out remaining animation duration
        float remainingTime = Mathf.Max(0.05f, animationLength - damageDelay);
        yield return new WaitForSeconds(remainingTime);

        ChangeAnimationState("ai", true);

        isAttacking = false;
    }

    private void ChangeAnimationState(string newState, bool forceRestart = false)
    {
        if (anim == null) return;
        if (currentState == newState && !forceRestart) return;

        anim.CrossFadeInFixedTime(newState, 0.05f);
        currentState = newState;
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