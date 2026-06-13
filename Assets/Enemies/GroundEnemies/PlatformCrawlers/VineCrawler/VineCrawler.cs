using UnityEngine;
using System.Collections;

public class VineCrawler : MonoBehaviour, IEnemy
{
    Rigidbody2D crawlerRig;
    [SerializeField] private float crawlSpeed = 1f;
    public bool dead = false;
    public bool GroundEnemy => true;
    public bool FlyingEnemy => false;
    private bool isLerping = false;
    public bool IsLerping => isLerping;
    public void SetLerp(bool value)
    {
        isLerping = value;
    }
    public bool Dead => enemyDamage.dead;
    private EnemyDamage enemyDamage;
    public float pauseTime = 3f;
    public float movedistance = 5f;

    private bool normalDirection = true;
    private bool isPaused = false;
    private Vector2 startpos;
    public static bool canMove = true;
    SpriteRenderer spriteRenderer;
    Animator vineCrawlerAnimator;


    void Start()
    {
       crawlerRig = GetComponent<Rigidbody2D>(); 
       spriteRenderer = GetComponent<SpriteRenderer>();
       enemyDamage = GetComponent<EnemyDamage>();
       startpos = transform.position;
       vineCrawlerAnimator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!Dead && !isPaused)
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
                crawlerRig.linearVelocityY= -crawlSpeed;
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

    void Update()
    {
        vineCrawlerAnimator.SetFloat("Velocity", Mathf.Abs(crawlerRig.linearVelocityY));
    }
}
