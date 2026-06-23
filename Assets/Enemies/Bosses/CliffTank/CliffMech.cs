using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CliffMech : MonoBehaviour
{
    private CliffCutscene cliffCutscene;
    private Rigidbody2D bossRig;
    private SpriteRenderer bossSprite;
    private Transform druidTransform;
    public float movementSpeed = 2f;
    [SerializeField] private GameObject rocket;
    private bool rocketCooldown = false;
    [SerializeField] private float behindChecker = 4f;
    private bool isShooting = false;
    private bool isDashing = false;
    [SerializeField] private float timeBetweenRockets = 0.2f;
    [SerializeField] private float rocketAmount;
    private List<GameObject> rocketList = new List<GameObject>();
    private List<Transform> rocketHitPositions = new List<Transform>();
    [SerializeField] private float rocketCooldownTime = 6f;
    [SerializeField] private Transform normalPos;
    [SerializeField] private float offset = 3f;
    [SerializeField] private bool mechDone = false;
    [SerializeField] private bool dashCd = false;
    [SerializeField] private float dashCooldownTime = 5f;
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashMovementSpeed = 4f;
    DruidUI druidUI;
    [SerializeField] private float timeConstraint = 180f;
    [SerializeField] private float dashInFrontDetection = 2f;
    private bool canUseAbility = true;
    private Animator mechAnimator;
    private bool startedDash = false;
    EnemyDamage damage;
    public bool Dead => damage.dead;
    

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        cliffCutscene = GameObject.Find("Cutscene").GetComponent<CliffCutscene>();
        mechAnimator = GetComponent<Animator>();
        bossRig = GetComponent<Rigidbody2D>();
        druidUI = player.GetComponent<DruidUI>();
        druidTransform = player.GetComponent<Transform>();
        bossSprite = GetComponent<SpriteRenderer>();
        damage = GetComponent<EnemyDamage>();
    }
    private void FixedUpdate()
    {
        if (bossRig.bodyType == RigidbodyType2D.Dynamic && !mechDone)
        {
            Vector2 movePos = new Vector2(druidTransform.position.x, gameObject.transform.position.y) + new Vector2((bossSprite.flipX ? -1 : 1) * offset, 0);
            var movementDir = bossSprite.flipX ? -1 : 1;
            if (!isDashing && !isShooting)
            {
                bool backwards = false;
                if (!bossSprite.flipX)
                {
                    backwards = movePos.x > transform.position.x;
                } else
                {
                    backwards = movePos.x < transform.position.x;
                }
                RaycastHit2D backCheck = Physics2D.Raycast(transform.position + new Vector3(0, -1f, 0), new Vector2(movementDir, 0), behindChecker, LayerMask.GetMask("Ground", "InvisibleBounds"));
                if (backCheck && backwards)
                {
                    bossRig.linearVelocity = Vector2.zero;
                    mechAnimator.SetFloat("XVelo", 0);
                } else
                {
                    Debug.Log("Moving");
                    float distance = Vector2.Distance(bossRig.position, movePos);
                    if (distance > 0.1f)
                    {
                        Vector2 direction = (movePos - bossRig.position).normalized;
                        bossRig.linearVelocity = direction * movementSpeed;
                    }
                    else bossRig.linearVelocity = Vector2.zero;
                    mechAnimator.SetFloat("XVelo", Mathf.Abs(bossRig.linearVelocity.x));
                }
                mechAnimator.SetBool("Backwards", backwards);
              
            }
        } 
    }

    float t = 0;
    private void Update()
    {
        if (bossRig.bodyType == RigidbodyType2D.Dynamic && !mechDone)
        {
            t += Time.deltaTime;
            var druidPos = gameObject.transform.position.x - druidTransform.position.x;
            if (damage.health <= 90 || druidUI.health == 1 || t >= timeConstraint)
            {
                bossRig.linearVelocity = Vector2.zero;
                mechDone = true;
                cliffCutscene.MechEnd();
            }
            if (!isDashing && !isShooting)
            {
                if (druidPos < 0)
                {
                    bossSprite.flipX = true;
                }
                else
                {
                    bossSprite.flipX = false;
                }
            }  

            if (!rocketCooldown && !isDashing && canUseAbility)
            {
                StartCoroutine(RocketShoot());
            }

            if (!dashCd && !isShooting && canUseAbility)
            {
                if (Vector2.Distance(druidTransform.position, transform.position) <= dashDistance)
                {
                    Debug.Log("Dashing!");
                    StartCoroutine(DashRoutine());
                }
            }

            if (isDashing && startedDash)
            {
                var dashDir = bossSprite.flipX ? 1 : -1;
                RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, new Vector2(dashDir, 0), dashInFrontDetection, LayerMask.GetMask("Ground", "InvisibleBounds"));
                if (wallCheck)
                {
                    StartCoroutine(StopDash());
                }
            }
        }
    }

    private IEnumerator DashRoutine()
    {
        canUseAbility = false;
        dashCd = true;
        isDashing = true;
        mechAnimator.SetTrigger("Dash");
        bossRig.linearVelocity = Vector2.zero;
        yield return new WaitUntil(() => mechAnimator.GetCurrentAnimatorStateInfo(0).IsName("CliffTankDashing"));
        var dashDir = bossSprite.flipX ? 1 : -1;
        startedDash = true;
        bossRig.AddForceX(dashMovementSpeed * dashDir, ForceMode2D.Impulse);
        yield return new WaitForSeconds(1.5f);
        if (startedDash == true)
        {
            StartCoroutine(StopDash());
        } 
    }

    private IEnumerator StopDash()
    {
        bossRig.linearVelocity = Vector2.zero;
        startedDash = false;
        mechAnimator.SetTrigger("StopDash");
        yield return new WaitUntil(() => mechAnimator.GetCurrentAnimatorStateInfo(0).IsName("CliffTankIdle"));
        bossRig.linearVelocity = Vector2.zero;
        mechAnimator.SetFloat("XVelo", 0f);
        yield return new WaitForSeconds(1f);
        isDashing = false;
        yield return new WaitForSeconds(3f);
        canUseAbility = true;
        yield return new WaitForSeconds(dashCooldownTime);
        dashCd = false;
    }

    private IEnumerator RocketShoot()
    {
        canUseAbility = false;
        isShooting = true;
        rocketCooldown = true;
        mechAnimator.SetTrigger("Shoot");
        yield return new WaitForSeconds(1.6f);
        for (int i = 0; i < rocketAmount; i++)
        {
            if (mechDone) break;
            Debug.Log("Shooting");
            var newRocket = Instantiate(rocket);
            newRocket.SetActive(true);
            newRocket.transform.position = normalPos.position;
            newRocket.transform.rotation = Quaternion.Euler(0, 0, -90f);
            rocketList.Add(newRocket);
            rocketHitPositions.Add(druidTransform);
            StartCoroutine(RocketMove(newRocket, druidTransform));
            if (i == rocketAmount - 1) break;
            else yield return new WaitForSeconds(timeBetweenRockets);
        }
        mechAnimator.SetTrigger("StopShoot");
        yield return new WaitForSeconds(1f);
        isShooting = false;
        yield return new WaitForSeconds(5f);
        canUseAbility = true;
        yield return new WaitForSeconds(rocketCooldownTime);
        rocketCooldown = false;
    }

    private IEnumerator RocketMove(GameObject rocket, Transform rocketHitPos)
    {
        var rocketLine = rocket.GetComponent<LineRenderer>();
        rocketLine.positionCount = 2;
        float launchTime = 1f;

        Vector2 startPos = rocket.transform.position;

        RaycastHit2D hit = Physics2D.Raycast(rocketHitPos.position, Vector2.down, 50f, LayerMask.GetMask("Ground"));

        float t = 0;
        float launchHeight = 20;
        Vector2 launchPos = new Vector2(rocket.transform.position.x, rocket.transform.position.y + launchHeight);
        while (t < launchTime)
        {
            t += Time.deltaTime;
            rocket.transform.position = Vector2.Lerp(startPos, launchPos, t / launchTime);
            yield return null;
        }

        Vector2 targetPos = rocketHitPos.position;

        rocket.transform.rotation = Quaternion.Euler(0, 0, 90f);
        float speed = 5f;
        while (Mathf.Abs(druidTransform.position.x - rocket.transform.position.x) > 0.5f)
        {
            RaycastHit2D beamCast = Physics2D.Raycast(rocket.transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));
            rocketLine.SetPosition(0, rocket.transform.position);
            rocketLine.SetPosition(1, beamCast.point);
            Vector2 rotationPos = new Vector2(druidTransform.position.x, rocket.transform.position.y);
            rocket.transform.position = Vector2.MoveTowards(rocket.transform.position, rotationPos, speed * Time.deltaTime);
          
            yield return null;
            targetPos = beamCast.point;
        }

        int flashCount = 5;
        for (int i = 0; i < flashCount; i++)
        {
            rocketLine.startColor = Color.white;
            rocketLine.endColor = Color.white;
            yield return new WaitForSeconds(0.1f);
            rocketLine.startColor = Color.red;
            rocketLine.endColor = Color.red;
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(rocketLine);
        t = 0;
        float diveTime = 0.7f;
        startPos = rocket.transform.position;

        while (t < diveTime)
        {
            rocket.transform.position = Vector2.Lerp(startPos, targetPos, t / diveTime);
            t += Time.deltaTime;
            yield return null;

        }
        rocket.transform.rotation = Quaternion.identity;
        rocketHitPositions.Remove(rocketHitPos);
        rocketList.Remove(rocket);
    }
}