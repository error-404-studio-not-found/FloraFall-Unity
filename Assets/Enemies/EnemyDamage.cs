using UnityEngine;
using System.Collections;

public class EnemyDamage : MonoBehaviour, IDamageAble
{
    /* ENEMYDAMAGE
     * This script is an overarching damage controller able to be placed on anything damagable
     * When persistence calls applydamage, it will check if it has this script and if it does then it will do damage
     * Custom death is a bool that will not call the death animation provided in this script and will do something else instead.
     */
    public bool dead = false;
    public bool Dead => dead;
    public float health;
    private SpriteRenderer spriterenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private DruidUI UI;
    private DruidGrowFramework growframework;
    [SerializeField] private int spiritsBack = 3;
    [SerializeField] private BoxCollider2D toBeDestroyed = null;
    public float flashDuration = 0.3f;
    public bool customDeath = false;
    public float flashPeak = 1f;

    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;
    private bool hitImmune = false;


    private void Awake()
    {
        spriterenderer = GetComponent<SpriteRenderer>();
        spriterenderer.material = new Material(spriterenderer.material);

        mpb = new MaterialPropertyBlock();
    }

    void Start()
    {
     
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        //find druidreference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            growframework = player.GetComponent<DruidGrowFramework>();
            UI = player.GetComponent<DruidUI>();
        }
    }

    void Update()
    {
        if (!customDeath)
        {
            if (health < 1 || health == 0)
            {
                animator.SetTrigger("Death");
                if (rb)
                {
                    rb.linearVelocityX = 0f;
                    rb.linearVelocityY = 0f;
                }
                if (dead == false)
                {
                    if (!DruidFrameWork.isTransformed)
                    {
                        UI.spirits += spiritsBack;
                    }
                    if (toBeDestroyed != null)
                    {
                        Destroy(toBeDestroyed);
                    }
                    dead = true;
                    StartCoroutine(growframework.RemoveTether(transform));
                }
            }
        }
    }

    public void TakeDamage(float damage) //call to take damage put damage in parameters
    {
        if (!dead)
        {
            if (!hitImmune)
            {
                hitImmune = true;
                StartCoroutine(HitImmuneCoroutine(0.5f));
                health -= damage;
                Flash();
            }
        }
    }

    // ---- FLASH CALL ----
    public void Flash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(FlashCoroutine());
    }

    //flash coroutine
    private IEnumerator FlashCoroutine()
    {
        float timer = 0f;

        spriterenderer.GetPropertyBlock(mpb);

        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            float t = 1f - (timer / flashDuration);
            float intensity = t * flashPeak;

            mpb.SetFloat("_FlashIntensity", intensity);
            spriterenderer.SetPropertyBlock(mpb);

            yield return null;
        }

        mpb.SetFloat("_FlashIntensity", 0f);
        spriterenderer.SetPropertyBlock(mpb);

        flashRoutine = null; // Clear reference
    }

    private IEnumerator HitImmuneCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        hitImmune = false;
    }
}
