using Pathfinding;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class HummingRock : MonoBehaviour, IEnemy, IGrowableEnemy
{
    /* HUMMING ROCK
     * Handles the HummingRock AI
     * Includes PathFinding
     * Grow Behaviour
     * Dash Behaviour
     */

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float playerDetectionDistance = 2f;
    [SerializeField] private float dashDetectionDistance = 2.5f;
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float hoverDistance = 4f;
    [SerializeField] private GameObject dirtExplosion;
    [SerializeField] private float deActivationDistance = 12f;
    private SpriteRenderer dirtExplosionSprite;
    ParticleSystem dirtExplosionParticle;
    public bool CantGrow => cantGrow;
    private bool cantGrow = false;
    private bool isDashing = false;
    private bool dashCD = false;
    private bool dirtCrashing = false;

    private bool growDb;
    public bool IsGrown => growDb;
    private bool candie = false;
    public bool FlyingEnemy => true;
    private bool isLerping = false;
    public bool IsLerping => isLerping;
    public void SetLerp(bool value)
    {
        isLerping = value;
    }
    public bool GroundEnemy => false;
    public int spiritCost => 4;
    public bool CanDie => candie;
    private Rigidbody2D enemyRig;
    private BoxCollider2D spikeGrowHitbox;
    private SpriteRenderer enemySprite;
    private EnemyDamage damage;

    private Animator animator;
    bool telegraphing = false;
    private bool playerInSight = false;
    private Transform enemyTransform;
    private Transform playerTransform;
    private int direction;
    private ParticleSystem explodeParticle;

    //PATHFINDING
    private Seeker seeker;

    private Path path;
    [SerializeField] private float pathRepeatRate = 0.15f;
    public float nextWaypointDistance = 0.15f;
    private int currentWaypoint = 0;
    public bool Dead => damage.dead;

    private void Start()
    {
        damage = GetComponent<EnemyDamage>();
        explodeParticle = GetComponent<ParticleSystem>();
        seeker = GetComponent<Seeker>();
        animator = GetComponent<Animator>();
        enemyTransform = GetComponent<Transform>();
        enemySprite = GetComponent<SpriteRenderer>();
        enemyRig = GetComponent<Rigidbody2D>();
        dirtExplosionSprite = dirtExplosion.GetComponent<SpriteRenderer>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.gameObject.GetComponent<Transform>();
        }

        InvokeRepeating("UpdatePath", 0f, pathRepeatRate);
    }

    private void UpdatePath()
    {
        if (seeker.IsDone())
        {
            var hoverDirection = enemySprite.flipX ? -1f : 1f;
            Vector2 hoverTarget = new Vector2(hoverDirection * hoverDistance + playerTransform.position.x, playerTransform.position.y);
            seeker.StartPath(transform.position, hoverTarget, OnPathComplete);
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void FixedUpdate()
    {
        if (!growDb && !isLerping && !damage.dead)
        {
            if (path == null) return;
            if (currentWaypoint >= path.vectorPath.Count) return;

            if (playerInSight && !isDashing)
            {
                Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - (Vector2)transform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + direction, moveSpeed * Time.fixedDeltaTime);

                float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);
                if (distance < nextWaypointDistance) currentWaypoint++;
            }
        }
    }

    private void Update()
    {

        //---- DEATH ----
        if (damage.health < 1 || damage.health == 0)
        {

            enemyRig.linearVelocityX = 0f;
            enemyRig.linearVelocityY = 0f;
            if (damage.dead == false)
            {
                StartCoroutine(Death());
            }
        }

        if (!growDb && !damage.dead && !isLerping)
        {
            if (!isDashing)
            {
                if (playerTransform.position.x < enemyTransform.position.x)
                {
                    enemySprite.flipX = false;
                    dirtExplosionSprite.flipX = false;
                }
                else
                {
                    enemySprite.flipX = true;
                    dirtExplosionSprite.flipX = true;
                }
            }

            direction = enemySprite.flipX ? 1 : -1;
            float distance = Vector2.Distance(enemyTransform.position, playerTransform.position);
            if (distance < playerDetectionDistance && playerInSight == false && !telegraphing)
            {
                StartCoroutine(PlayerSpotted());
            }

            if (playerInSight)
            {
                // ---- DASH AVOIDANCE ----
                if (isDashing)
                {
                    RaycastHit2D dashRay = Physics2D.BoxCast((Vector2)enemyTransform.position + new Vector2(0.2f * direction, 0), new Vector2(0.2f, 0.5f), 0, new Vector2(direction, 0), 0.1f, LayerMask.GetMask("Ground", "Breakables"));
                    if (dashRay && !dirtCrashing)
                    {
                        StartCoroutine(DirtCrash());
                    }
                }

                RaycastHit2D dashHit = Physics2D.Raycast(enemyTransform.position, new Vector2(direction, 0), dashDetectionDistance, LayerMask.GetMask("Player", "Ground"));
                if (dashHit && isDashing == false && dashCD == false  && dashHit.collider.CompareTag("Player"))
                {
                    dashCD = true;
                    isDashing = true;
                    StartCoroutine(Dash());
                }

                if (Vector2.Distance(gameObject.transform.position, playerTransform.position) > deActivationDistance)
                {
                    playerInSight = false;
                }
            }
        }
    }
    private IEnumerator Death()
    {
        enemySprite.enabled = false;
        explodeParticle.Emit(10);
        damage.dead = true;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    public void Grow()
    {
        if (!damage.dead)
        {
            if (IsGrown == false)
            {
                if (candie == false)
                {
                    growDb = true;
                    candie = true;
                    StartCoroutine(GrowCycle());
                }
            }
        }
    }

    public void Die()
    {
        if (!damage.dead)
        {
            if (growDb == true)
            {
                if (candie == true)
                {
                    growDb = false;
                    candie = false;
                    StartCoroutine(DieCycle());
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawRay(transform.position, new Vector3(direction * dashDetectionDistance, 0, 0), Color.red);
        Gizmos.DrawWireSphere(transform.position, playerDetectionDistance);
    }

    private IEnumerator GrowCycle()
    {
        enemyRig.gravityScale = 1f;
        spikeGrowHitbox.isTrigger = true;
        animator.SetTrigger("Grow");
        yield return null;
    }

    private IEnumerator DieCycle()
    {
        enemyRig.gravityScale = 0f;
        spikeGrowHitbox.isTrigger = false;
        animator.SetTrigger("UnGrow");
        yield return null;
    }


    private IEnumerator StopDash()
    {
        isDashing = false;
        animator.SetTrigger("StopDash");
        enemyRig.linearVelocityX = 0;
        yield return new WaitForSeconds(Random.Range(2, 5));
        dashCD = false;
        cantGrow = false;
    }

    private IEnumerator DirtCrash()
    {
        var sprite = GetComponent<SpriteRenderer>();
        sprite.sortingOrder = -3;
        dirtCrashing = true;
        enemyRig.linearVelocityX = 0;
        animator.SetTrigger("DirtCrash");
        var dirAnimator = dirtExplosion.GetComponent<Animator>();
        dirAnimator.SetTrigger("Explode");
        dirtExplosionParticle = dirtExplosion.GetComponent<ParticleSystem>();
        dirtExplosionParticle.Emit(10);
        if (Vector2.Distance(playerTransform.position, transform.position) <= 11f)
        {
            Camera.main.GetComponent<FollowPlayer>().ScreenShake(0.025f, 0.5f);
        }
        yield return new WaitForSeconds(1);
        sprite.sortingOrder = 1;
        isDashing = false;
        dirtCrashing = false;
        cantGrow = false;
        yield return new WaitForSeconds(Random.Range(2, 5));
        dashCD = false;
    }

    private IEnumerator PlayerSpotted()
    {
        telegraphing = true;
        enemyRig.linearVelocityX = 0;
        animator.SetTrigger("SpotPlayer");
        yield return new WaitForSeconds(0.5f);
        playerInSight = true;
    }

    private IEnumerator Dash()
    {
        cantGrow = true;
        var direction = enemySprite.flipX ? 1 : -1;
        enemyRig.linearVelocityX = 0;
        animator.SetTrigger("Dash");
        yield return new WaitForSeconds(0.5f);
        enemyRig.linearVelocityX += dashForce * direction;
        yield return new WaitForSeconds(1.2f);
        if (isDashing && !dirtCrashing)
        {
            StartCoroutine(StopDash());
        }
    }
}
