using System.Collections;

using UnityEngine;

public class RustyGolem : MonoBehaviour, IGrowableEnemy, IEnemy
{
    /* RUSTY GOLEM
     * Handles all of rusty golem's logic
     * Handles rusty golem's movement
     * Handles rusty golem's
     */

    // ---- INTERFACE ----

    public bool dead = false;
    public bool FlyingEnemy => false;
    public bool CantGrow => cantGrow;
    private bool cantGrow = false;
    public bool GroundEnemy => true;
    private bool isLerping = false;
    public bool IsLerping => isLerping;
    public void SetLerp(bool value)
    {
        isLerping = value;
    }
    public bool candie = false;
    public bool isgrown = false;
    public bool CanDie => candie;
    public bool IsGrown => isgrown;
    
    public int spiritCost => 3;

    // ---- BASE COMPONENTS ----

    private Animator animator;
    private EnemyDamage enemyDamage;
    private Rigidbody2D rb;
    private DruidFrameWork druid;
    private SpriteRenderer spriterenderer;

    // ---- MOVEMENT ----
    public float movespeed = 2f;

    public float pauseTime = 3f;
    public float movedistance = 5f;

    private bool movingright = true;
    private bool isPaused = false;
    private Vector2 startpos;

    //BouncePad
    [SerializeField] private GameObject collide;

    [SerializeField] private float bounceheight;

    private BoxCollider2D bouncepad;
    public bool Dead => enemyDamage.dead;

    /* START
     * Handles player variable
     * Handles components
     */

    private void Start()
    {
        enemyDamage = GetComponent<EnemyDamage>();
        startpos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriterenderer = GetComponent<SpriteRenderer>();

        //find druidreference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            druid = player.GetComponent<DruidFrameWork>();
        }
    }

    /* UPDATE
     * Handles Death
     */

    private void Update()
    {
        animator.SetFloat("XVelo", rb.linearVelocityX);
    }

    /* FIXED UPDATE
     * Handles Movement
     * Handles Flipping
     */

    private void FixedUpdate()
    {
        if (!enemyDamage.dead && !isPaused)
        {
            if (!isgrown)
            {
                float distanceFromStart = transform.position.x - startpos.x;

                if (movingright)
                {
                    spriterenderer.flipX = true;
                    rb.linearVelocityX = movespeed;
                    if (distanceFromStart >= movedistance)
                    {
                        StartCoroutine(PauseAtEnd(false));
                    }
                }
                else
                {
                    spriterenderer.flipX = false;
                    rb.linearVelocityX = -movespeed;
                    if (distanceFromStart <= -movedistance)
                    {
                        StartCoroutine(PauseAtEnd(true));
                    }
                }
            }
        }
        else if (isPaused)
        {
            rb.linearVelocityX = 0f;
        }
    }

    /* FUNCTIONS
     * Handles grow
     * Handles Dying
     * Handles collision with bouncepad if grown
     */

    //grow the enemy
    public void Grow()
    {
        if (!enemyDamage.dead)
        {
            if (isgrown == false)
            {
                if (candie == false)
                {
                    isgrown = true;
                    candie = true;
                    StartCoroutine(GrowCycle());
                }
            }
        }
    }

    //ungrow the enemy
    public void Die()
    {
        if (!enemyDamage.dead)
        {
            if (isgrown == true)
            {
                if (candie == true)
                {
                    StartCoroutine(DieCycle());
                }
            }
        }
    }

    // ---- DAMAGE ----
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enemyDamage.dead)
        {
            if (collision)
            {
                if (collision.gameObject.CompareTag("Player") && isgrown && bouncepad != null)
                {
                    Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        playerRb.linearVelocity = new Vector2(playerRb.linearVelocityX, 0f);

                        DruidFrameWork.canjump = false;
                        playerRb.AddForce(Vector2.up * bounceheight, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
  
    /* COROUTINES
     * Handles Growing
     * Handles Dying
     * Handles Idle at end of walking
     */

    private IEnumerator PauseAtEnd(bool turnRight) // pauses at the end of the movement
    {
        isPaused = true;
        rb.linearVelocityX = 0f;

        yield return new WaitForSeconds(pauseTime);

        movingright = turnRight;
        isPaused = false;
    }

    private IEnumerator GrowCycle()
    {
        animator.SetTrigger("grow");
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(0.75f);
        collide.AddComponent<BoxCollider2D>();
        bouncepad = collide.GetComponent<BoxCollider2D>();
        bouncepad.enabled = true;
        bouncepad.usedByEffector = true;
        bouncepad.isTrigger = true;
    }

    private IEnumerator DieCycle()
    {
        candie = false;
        Destroy(bouncepad);
        animator.SetTrigger("die");
        yield return new WaitForSeconds(1f);
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isgrown = false;
    }
}