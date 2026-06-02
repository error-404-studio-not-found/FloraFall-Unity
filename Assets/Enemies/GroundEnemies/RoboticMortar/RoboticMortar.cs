using UnityEngine;
using System.Collections;

public class RoboticMortar : MonoBehaviour, IEnemy
{
    //---- INTERFACE ----
    public bool FlyingEnemy => false;
    public bool GroundEnemy => true;
    public bool Dead => damage.dead;
    private bool isLerping = false;
    public bool IsLerping => isLerping;
    public void SetLerp(bool value)
    {
        isLerping = value;
    }

    private EnemyDamage damage;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float activationDistance = 6f;
    private GameObject player;
    private Transform playerTransform;
    private Animator animator;

    //SHOOTING
    private bool isShooting = false;
    private bool shootCooldown = false;
    [SerializeField] private float timeBetweenShots = 7f;
    [SerializeField] private Transform shotPos;
    [SerializeField] private float shotTime = 1f;
    [SerializeField] private bool facingLeft = true;
    [SerializeField] private AnimationCurve arcCurve;
    void Start()
    {
        damage = GetComponent<EnemyDamage>();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        if (player != null)
        {
            playerTransform = player.GetComponent<Transform>();
        }
    }

    void Update()
    {
        //---- SHOOT ----
        if (!damage.dead)
        {
            if (!isShooting && !shootCooldown)
            {
                if (facingLeft)
                {
                    if (Vector2.Distance(transform.position, playerTransform.position) < activationDistance && playerTransform.position.x < transform.position.x)
                    {
                        StartCoroutine(Shoot(-1));
                    }
                } else
                {
                    if (Vector2.Distance(transform.position, playerTransform.position) < activationDistance && playerTransform.position.x > transform.position.x)
                    {
                        StartCoroutine(Shoot(1));
                    }
                }
              
            }
        }
    }

    private IEnumerator Shoot(int dir)
    {
        isShooting = true;
        shootCooldown = true;
        animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.8f);

        var bulletClone = Instantiate(bullet);
        bulletClone.transform.position = shotPos.position;
        bulletClone.SetActive(true);
        IGrowablePlant bulletPlant = bulletClone.GetComponent<IGrowablePlant>();

        RaycastHit2D druidRayDown = Physics2D.Raycast(playerTransform.position, Vector2.down, 1000f, LayerMask.GetMask("Ground"));
        Vector2 druidPosToHit;
        if (druidRayDown)
        {
            druidPosToHit = new Vector2(playerTransform.position.x, druidRayDown.point.y);
        }
        else druidPosToHit = playerTransform.position;
       
        
        var startPos = transform.position;
        var arcHeight = 5f;

        float time = 0f;
        while (time <= shotTime)
        {
            if (bulletPlant != null)
            {
                if (bulletPlant.IsGrown || !bulletPlant.CanGrow)
                {
                    isShooting = false;
                    yield return new WaitForSeconds(timeBetweenShots);
                    shootCooldown = false;
                    yield break;
                }
            }

            float realTime = time / shotTime;
            Vector2 pos = Vector2.Lerp(startPos, druidPosToHit, realTime);
            float heightOffset = arcCurve.Evaluate(realTime) * arcHeight;
            pos.y += heightOffset;

            bulletClone.transform.position = pos;
            time += Time.deltaTime;
            yield return null;
        }
        isShooting = false;

        yield return new WaitForSeconds(timeBetweenShots);
        shootCooldown = false;
    }
}
