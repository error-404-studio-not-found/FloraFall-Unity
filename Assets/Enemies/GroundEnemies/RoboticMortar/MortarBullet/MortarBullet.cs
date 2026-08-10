using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MortarBullet : MonoBehaviour, IGrowablePlant
{
    private bool growDB = false;
    private bool canDie = false;
    private bool isGrown = false;
    private bool canGrow = true;
    public bool IsGrown => growDB;
    public bool CanDie => canDie;
    public bool CanGrow => canGrow;

    public int spiritCost => 1;
    public bool waterGrown = false;
    public bool WaterGrown => waterGrown;
    private SpriteRenderer sprite;

    public void setWaterGrow(bool value)
    {
        waterGrown = value;
    }

    [SerializeField] private GameObject airExplosionHitbox;
    [SerializeField] private float explodeTime = 2.5f;
    [SerializeField] private GameObject explosionHitbox;
    [SerializeField] private float mouseMoveSpeed = 2f;
    private Rigidbody2D rigClone;
    private Animator animator;
    private DruidGrowFramework DGF;
    private Transform druidTransform;
    private FollowPlayer followPlayer;
    [SerializeField] private float shakeDistane = 5f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rigClone = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        followPlayer = Camera.main.GetComponent<FollowPlayer>();
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            druidTransform = player.GetComponent<Transform>();
            DGF = player.GetComponent<DruidGrowFramework>();
        }
    }

    private void FixedUpdate()
    {
        if (isGrown)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 direction = (mousePos - rigClone.position);

            rigClone.linearVelocity = direction * mouseMoveSpeed;
        }
    }

    public void Grow()
    {
        if (!growDB && !canDie && canGrow)
        {
            growDB = true;
            canDie = true;
            isGrown = true;
            animator.SetTrigger("Grow");
            rigClone.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void Die()
    {
        if (growDB && canDie && canGrow)
        {
            growDB = false;
            canDie = false;
            isGrown = false;
            animator.SetTrigger("Die");
            rigClone.gravityScale = 2;
            rigClone.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    //---- BOTTOM EXPLOSION ----
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canGrow)
        {
            if (!IsGrown)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Breakables"))
                {
                    canGrow = false;
                    RaycastHit2D groundCheck = Physics2D.Raycast(transform.position, Vector2.down, 1f, LayerMask.GetMask("Ground", "Breakables"));
                    if (groundCheck && groundCheck.collider.gameObject.CompareTag("Grass"))
                    {
                        Debug.Log("GroundBall");
                        StartCoroutine(Explode(false));
                    }
                    else
                    {
                        Debug.Log("AirBall");
                        StartCoroutine(Explode(true));
                    }
                }
                else if (collision.gameObject.CompareTag("Player"))
                {
                    canGrow = false;
                    StartCoroutine(Explode(true));
                }
            }
            else
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Breakables"))
                {
                    canGrow = false;
                    StartCoroutine(Explode(true));
                    isGrown = false;
                    DGF.DeGrowPlant(transform);
                }
            }
        }
    }

    private IEnumerator Explode(bool air)
    {
        Debug.Log("Exploding");
        if (air == true)
        {
            animator.SetTrigger("AirExplode");
            Destroy(rigClone);
            sprite.sortingOrder = 2;
            airExplosionHitbox.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            var explosionHitboxCollider = airExplosionHitbox.GetComponent<BoxCollider2D>();
            explosionHitboxCollider.enabled = false;
            yield return new WaitForSeconds(0.5f);
            airExplosionHitbox.SetActive(false);
            gameObject.SetActive(false);
            sprite.enabled = false;
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
        else
        {
            animator.SetTrigger("Explode");
            Destroy(rigClone);
            sprite.sortingOrder = -1;
            var yPos = transform.position.y;
            transform.position = new Vector3(transform.position.x, yPos -= 0.25f, transform.position.z);
            explosionHitbox.SetActive(true);
            if (Vector2.Distance(explosionHitbox.transform.position, druidTransform.position) <= shakeDistane)
            {
                followPlayer.ScreenShake(0.025f, 0.5f);
            }
            yield return new WaitForSeconds(0.5f);
            var explosionHitboxCollider = explosionHitbox.GetComponent<BoxCollider2D>();
            explosionHitboxCollider.enabled = false;
            yield return new WaitForSeconds(0.5f);
            explosionHitbox.SetActive(false);
            gameObject.SetActive(false);
            sprite.enabled = false;
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }
}