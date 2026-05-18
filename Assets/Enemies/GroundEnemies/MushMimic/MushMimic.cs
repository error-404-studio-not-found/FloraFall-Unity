using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class MushMimic : MonoBehaviour, IGrowableEnemy, IEnemy
{
    private bool isGrown = false;
    private bool canDie = false;
    [SerializeField] private int cost;
    public bool CantGrow => cantGrow;
    private bool cantGrow = true;
    private EnemyDamage damage;
    public bool IsGrown => isGrown;
    public int spiritCost => cost;
    public bool Dead => damage.dead;
    public bool FlyingEnemy => false;
    private bool isLerping = false;
    public bool IsLerping => isLerping;

    public void SetLerp(bool value)
    {
        isLerping = value;
    }

    public bool GroundEnemy => true;

    public bool CanDie => canDie;

    public float flashDuration = 0.3f;

    public float flashPeak = 1f;
    [SerializeField] private float jumpDistanceCheck = 2f;
    [SerializeField] private float attackCooldown = 5f;
    private bool canHop = true;
    private bool canJump = true;
    private bool isJumping = false;
    private GameObject druid;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D enemyRig;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float timeBetweenHops = 0.35f;
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float jumpXForce = 2f;

    [SerializeField] private float pauseTime = 3f;
    [SerializeField] private float moveDistance = 5f;
    [SerializeField] private Transform checkPointPos;
    private BoxCollider2D collide;
    [SerializeField] private GameObject platform;
    [SerializeField] private BoxCollider2D landingHitbox;
    [SerializeField] private GameObject hitbox;
    private EnemyDamage enemyDamage;
    private Animator animator;

    private bool movingright = true;
    private bool isPaused = false;
    private Vector2 startpos;

    private void Start()
    {
        startpos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        druid = GameObject.FindGameObjectWithTag("Player");
        damage = GetComponent<EnemyDamage>();
        enemyRig = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        canHop = false;
        StartCoroutine(JumpAttack());
        enemyDamage = GetComponent<EnemyDamage>();
    }

    /* FIXED UPDATE
     * Handles movement
     * Handles pausing and starting at end via coroutine call
     */

    private void FixedUpdate()
    {
        if (!isGrown && !isJumping)
        {
            animator.SetFloat("XVelo", enemyRig.linearVelocityX);
            if (!damage.dead && !isPaused)
            {
                float distanceFromStart = transform.position.x - startpos.x;

                if (movingright)
                {
                    spriteRenderer.flipX = true;
                    if (canHop && !isPaused)
                    {
                        StartCoroutine(HopRoutine(1));
                    }

                    if (distanceFromStart >= moveDistance)
                    {
                        StartCoroutine(PauseAtEnd(false));
                    }
                }
                else
                {
                    spriteRenderer.flipX = false;
                    if (canHop && !isPaused)
                    {
                        StartCoroutine(HopRoutine(-1));
                    }

                    if (distanceFromStart <= -moveDistance)
                    {
                        StartCoroutine(PauseAtEnd(true));
                    }
                }
            }
            else if (isPaused)
            {
                enemyRig.linearVelocityX = 0f;
            }
        }
    }

    private void Update()
    {
        float distanceFromStart = transform.position.x - startpos.x;
        //Attack
        if (canJump && !(distanceFromStart <= -moveDistance || distanceFromStart >= moveDistance) && !IsGrown)
        { 
            if (Vector2.Distance(gameObject.transform.position, druid.transform.position) <= jumpDistanceCheck)
            {
                Debug.Log("Jumping");
                canHop = false;
                StartCoroutine(JumpAttack());
            }
        }
    }

    /* COROUTINES
     * PauseAtEnd handles the mush pausing at the end of their roll
     */

    private IEnumerator HopRoutine(int direction)
    {
        animator.SetBool("CanIdle", false);
        canHop = false;
        enemyRig.linearVelocityX = 0f;
        animator.SetTrigger("Move");
        yield return new WaitForSeconds(0.15f);
        enemyRig.linearVelocityX = direction * moveSpeed;
        yield return new WaitForSeconds(timeBetweenHops);
        animator.SetBool("CanIdle", true);
        enemyRig.linearVelocityX = 0f;
        yield return new WaitForSeconds(1.5f);
        canHop = true;
    }

    private IEnumerator JumpAttack()
    {
        cantGrow = true;
        var playerDir = druid.transform.position.x > gameObject.transform.position.x ? 1 : -1;
        if (playerDir > 0)
        {
            spriteRenderer.flipX = true;
            movingright = true;
        }
        else
        {
            spriteRenderer.flipX = false;
            movingright = false;
        }
        isJumping = true;
        canHop = false;
        canJump = false;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.65f);
        enemyRig.AddForce(new Vector2(jumpXForce * playerDir, jumpForce), ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.4f);
        while (enemyRig.linearVelocityY > 0)
        {
            yield return null;
            Debug.Log("Floating");
        }
        animator.SetTrigger("StopSpinning");
        enemyRig.gravityScale = 3f;
        bool touchingGround = false;
        while (!touchingGround)
        {
            touchingGround = Physics2D.OverlapCircle(checkPointPos.position, 0.2f, LayerMask.GetMask("Ground"));
            yield return null;
        }
        enemyRig.gravityScale = 1;
        animator.SetTrigger("Land");
        landingHitbox.enabled = true;
        yield return new WaitForSeconds(0.3f);
        landingHitbox.enabled = false;
        yield return new WaitForSeconds(0.7f); 
        canHop = true;
        cantGrow = false;
        isJumping = false;
        yield return new WaitForSeconds(attackCooldown);
        canJump = true;
    }

    private IEnumerator PauseAtEnd(bool turnRight) // pauses at the end of the movement
    {
        isPaused = true;
        enemyRig.linearVelocityX = 0f;

        yield return new WaitForSeconds(pauseTime);

        movingright = turnRight;
        isPaused = false;
    }

    public void Grow()
    {
        if (!damage.dead && !cantGrow)
        {
            if (IsGrown == false)
            {
                if (canDie == false)
                {
                    isGrown = true;
                    canDie = true;
                    StartCoroutine(GrowCycle());
                }
            }
        }
    }

    public void Die()
    {
        if (!damage.dead && !cantGrow)
        {
            if (isGrown == true)
            {
                if (canDie == true)
                {
                    isGrown = false;
                    canDie = false;
                    StartCoroutine(DieCycle());
                }
            }
        }
    }

    private IEnumerator GrowCycle()
    {
        animator.SetTrigger("Grow");
        isGrown = true;
        hitbox.SetActive(false);
        cantGrow = true;
        enemyRig.constraints = RigidbodyConstraints2D.FreezeAll;
        enemyDamage.enabled = false;
        yield return new WaitForSeconds(0.75f);
        cantGrow = false;
        canDie = true;
        platform.AddComponent<BoxCollider2D>();
        collide = platform.GetComponent<BoxCollider2D>();
        collide.enabled = true;
        collide.usedByEffector = true;
    }

    private IEnumerator DieCycle()
    {
        canDie = false;
        animator.SetTrigger("Die");
        Destroy(collide);
        cantGrow = true;
        yield return new WaitForSeconds(0.4f);
        hitbox.SetActive(true);
        enemyDamage.enabled = true;
        enemyRig.constraints = RigidbodyConstraints2D.None;
        enemyRig.constraints = RigidbodyConstraints2D.FreezeRotation;
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("dbdone");
        cantGrow = false;
        isGrown = false;
    }
}