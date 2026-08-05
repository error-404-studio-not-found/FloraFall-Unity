using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class VineCrawler : MonoBehaviour, IEnemy, IGrowableEnemy
{
    private Rigidbody2D crawlerRig;
    [SerializeField] private float crawlSpeed = 1f;
    public bool CantGrow => cantGrow;
    private bool cantGrow = false;
    public bool dead = false;
    public bool GroundEnemy => true;
    public bool FlyingEnemy => false;
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

    public int spiritCost => 1;
    public bool Dead => enemyDamage.dead;
    private EnemyDamage enemyDamage;
    public float pauseTime = 3f;
    public float movedistance = 5f;

    private bool normalDirection = true;
    private bool isPaused = false;
    private Vector2 startpos;
    public static bool canMove = true;
    private SpriteRenderer spriteRenderer;
    private Animator vineCrawlerAnimator;
    [SerializeField] private bool right = true;
    private bool shotdb = false;
    [SerializeField] private Transform bulletspawn;
    [SerializeField] private GameObject Bullet;
    [SerializeField] private float bulletSpeed = 2f;
    private Animator animator;

    private void Start()
    {
        crawlerRig = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyDamage = GetComponent<EnemyDamage>();
        startpos = transform.position;
        vineCrawlerAnimator = GetComponent<Animator>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!Dead && !isPaused && !isgrown)
        {
            float distanceFromStart = transform.position.y - startpos.y;

            if (normalDirection)
            {
                spriteRenderer.flipX = true;
                crawlerRig.linearVelocityY = crawlSpeed;
                if (distanceFromStart >= movedistance)
                {
                    StartCoroutine(PauseAtEnd(false));
                }
            }
            else
            {
                spriteRenderer.flipX = false;
                crawlerRig.linearVelocityY = -crawlSpeed;
                if (distanceFromStart <= -movedistance)
                {
                    StartCoroutine(PauseAtEnd(true));
                }
            }
        }
        else if (isPaused)
        {
            crawlerRig.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator PauseAtEnd(bool turnRight) // pauses at the end of the movement
    {
        isPaused = true;
        crawlerRig.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(pauseTime);

        normalDirection = turnRight;
        isPaused = false;
    }

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

    public void Die()
    {
        if (isgrown == true)
        {
            if (candie == true && !cantGrow)
            {
                StartCoroutine(DieCycle());
            }
        }
    }

    private IEnumerator GrowCycle()
    {
        animator.SetTrigger("Grow");
        crawlerRig.linearVelocity = Vector2.zero;
        shotdb = true;
        isgrown = true;
        cantGrow = true;
        yield return new WaitForSeconds(0.75f);
        cantGrow = false;
        candie = true;
        shotdb = false;
    }

    private IEnumerator DieCycle()
    {
        candie = false;
        crawlerRig.linearVelocity = Vector2.zero;
        cantGrow = true;
        shotdb = true;
        animator.SetTrigger("Die");
        yield return new WaitForSeconds(0.75f);
        cantGrow = false;
        isgrown = false;
    }

    private void Update()
    {
        vineCrawlerAnimator.SetFloat("Velocity", Mathf.Abs(crawlerRig.linearVelocityY));
        if (isgrown && !shotdb && !cantGrow)
        {
            StartCoroutine(Shoot());
        }
    }

    private IEnumerator Shoot()
    {
        shotdb = true;
        yield return new WaitForSeconds(3f);
        Debug.Log("Shooting");
        if (isgrown && candie && !cantGrow)
        {
            animator.SetTrigger("Shoot");

            yield return new WaitForSeconds(0.3f);

            //Get Bullets components/add components
            GameObject BulletClone = Instantiate(Bullet, bulletspawn.transform.position, bulletspawn.transform.rotation);
            BulletClone.SetActive(true);

            //Rig
            Rigidbody2D bulletrig = BulletClone.AddComponent<Rigidbody2D>();
            bulletrig.gravityScale = 0;

            //BoxCollider
            BoxCollider2D bulletcollider = BulletClone.AddComponent<BoxCollider2D>();
            bulletcollider.isTrigger = true;

            //Renderer
            SpriteRenderer bulletrender = BulletClone.GetComponent<SpriteRenderer>();
            bulletrender.enabled = true;

            //force

            float direction = right ? 1f : -1f;
            bulletrig.AddForce(Vector2.right * bulletSpeed * direction, ForceMode2D.Impulse);

            //death

            yield return new WaitForSeconds(0.85f);
            if (BulletClone)
            {
                Animator bulletAnimator = BulletClone.GetComponent<Animator>();
                if (bulletAnimator != null)
                {
                    bulletAnimator.SetTrigger("Death");
                }
                yield return new WaitForSeconds(0.3f);
                Destroy(BulletClone);
            }
        }

        shotdb = false;
    }
}