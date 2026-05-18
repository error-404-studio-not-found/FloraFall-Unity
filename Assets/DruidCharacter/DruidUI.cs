using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DruidUI : MonoBehaviour, IDamageAble
{
    public Image[] spiritimages;
    public Animator[] healthAnimators;
    public Image[] healthImage;

    public Sprite fullSpirit;
    public Sprite emptySpirit;
    public int maxSpirits; //change to set max spirits max is 8
    public int spirits; //current spirits
    public Image circleWipe;
    private Transform spawnPoint; //current spawnpoint;
    public string currentRespawnPointName;
    public float health = 5;
    public float MaxHealth = 5;
    private float previousHealth;
    public bool dead = false;
    public GameObject druid;
    [SerializeField] private Animator deathScreen;
    private bool waitCycle = false;
    public string spawnSceneName;
    private Animator druidanims;
    private Rigidbody2D druidRig;
    public bool hitImmune = false;

    private SpriteRenderer spriterenderer;
    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;
    private DruidFrameWork frameWork;

    FollowPlayer followPlayer;

    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float flashPeak = 1f;


    public bool Dead => dead;

    private void Start()
    {
        spriterenderer = gameObject.GetComponent<SpriteRenderer>();
        spriterenderer.material = new Material(spriterenderer.material);
        frameWork = GetComponent<DruidFrameWork>();
        druidanims = GetComponent<Animator>();
        mpb = new MaterialPropertyBlock();
        health = MaxHealth;
        druidRig = GetComponent<Rigidbody2D>();
        previousHealth = health;
        followPlayer = Camera.main.GetComponent<FollowPlayer>();
    }

    private void Update()
    {
        for (int i = 0; i < spiritimages.Length; i++) //set spirit UI can change in Inspector
        {
            if (i < spirits)
            {
                spiritimages[i].sprite = fullSpirit;
            }
            else
            {
                spiritimages[i].sprite = emptySpirit;
            }

            if (i < maxSpirits)
            {
                spiritimages[i].enabled = true;
            }
            else
            {
                spiritimages[i].enabled = false;
            }
        }

        if (health != previousHealth)
        {
            for (int i = 0; i < healthAnimators.Length; i++)
            {
                if (i >= health && i < previousHealth)
                {
                    healthAnimators[i].SetTrigger("Die");
                }
                else if (i < health && i >= previousHealth)
                {
                    healthAnimators[i].SetTrigger("Appear");
                }

                if (i < MaxHealth)
                {
                    healthImage[i].enabled = true;
                }
                else
                {
                    healthImage[i].enabled = false;
                }
            }

            previousHealth = health;
        }

        if (health <= 0)
        {
            if (dead == false)
            {
                if (!waitCycle)
                {
                    dead = true;
                    StartCoroutine(DeathScreenCycle());
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
                health -= damage;
                StartCoroutine(HitImmuneCoroutine(0.5f));
                Flash();
                StartCoroutine(frameWork.FreezeFrame(0.3f));
                followPlayer.ScreenShake(0.02f, 0.5f);
            }
        }
    }

    public void Flash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(FlashCoroutine());
    }

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

    private IEnumerator DeathScreenCycle()
    {
        druidRig.linearVelocityX = 0f;
        deathScreen.SetTrigger("Start");
        health = 0;
        waitCycle = true;
        druidanims.SetTrigger("Death");
        druidRig.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(0.5f);
        druidRig.constraints = RigidbodyConstraints2D.None;
        druidRig.constraints = RigidbodyConstraints2D.FreezeRotation;
        StartCoroutine(RespawnCycle());
    }

    private IEnumerator HitImmuneCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        hitImmune = false;
    }

    private IEnumerator RespawnCycle()
    {
        yield return new WaitForSeconds(0.1f);
        druid.transform.position = Vector2.zero;
        druidRig.linearVelocityX = 0f;
        Scene currentScene = SceneManager.GetActiveScene();
        druidanims.SetTrigger("Respawn");

        ChunkLoader.Instance.EnterChunk(spawnSceneName);

        yield return null;
        yield return null;
        yield return null;

        spawnPoint = GameObject.Find(currentRespawnPointName).transform;
       
        druidRig.gravityScale = 1f;
        health = MaxHealth;
        dead = false;
        hitImmune = false;
        spirits = maxSpirits;
        druidanims.SetFloat("XVelo", 0);
      
        if (spawnPoint != null)
        {
            druid.transform.position = spawnPoint.position;
        }
        else
        {
            Debug.LogWarning("No spawnPoint found in scene!");
        }

        yield return new WaitForSeconds(0.2f);
        DruidFrameWork.canmove = true;
        waitCycle = false;
        deathScreen.SetTrigger("End");
    }
}