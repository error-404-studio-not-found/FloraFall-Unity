using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class DruidFrameWork : MonoBehaviour
{
    /* DRUIDFRAMEWORK
     * This script handles movement, transformations, and jumping for the main character
     * Includes functions for attack
     * Includes coroutines for freezeframes and transformations
     * Connects to other druid scripts
     */

    /* VARIABLES
     * Handles all changing parts in druidframework
     * Handles UI
     * Handles Components
     * Handles Jump
     * etc
     */

    // ---- MOVEMENT ----
    private Rigidbody2D druidrb;

    private Animator animator;
    private SpriteRenderer druidspriterender;
    private BoxCollider2D boxcollider;
    private float speedx;

    public float druidspeed;
    public static bool canjump = true;
    public static bool canmove = true;
    public Transform druidtransform;
    [SerializeField] private ParticleSystem walkingParticle;
    [SerializeField] private GameObject druid;
    [SerializeField] private ParticleSystem fallingParticle;
    [SerializeField] private float gravityScale = 0.5f;
    public static bool inCutscene = false;

    // ---- CUSTOM JUMP PHYSICS ----
    private float coyoteTimeCounter;

    private float jumpBufferCounter;

    private bool isJumping;
    public bool isGrounded;
    private bool gravityjump = false;
    private float jumpheight = 7.5f;
    private bool hasJumped = false;
    private bool wasGroundedLastFrame = false;
    private float impactSpeed = 0f;
    private bool isStunned = false;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float variableJumpMultiplier = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float druidJumpHeight;
    [SerializeField] private float bearJumpHeight;
    [SerializeField] private float stunHeight = 9;
    [SerializeField] private float maximumYVelocity = -10f;

    private FollowPlayer followPlayer;

    // ---- VOID CHECK ----
    public Vector2 lastGroundPosition = Vector2.zero;

    // ---- CUSTOM CURSOR ----
    public Texture2D cursorTexture;

    private Vector2 cursorHotspot;

    // ---- UI ----
    private DruidUI UI;

    // ---- TRANSFORMATIONS ----
    private bool isAttacking = false;

    private bool damagecd = false;
    private bool istransforming = false;
    private bool transformcd = false;

    public static bool Transitioning = false;
    public static bool bearattackcd = false;
    public static bool isTransformed = false;

    [SerializeField] private Vector2 biteSize = new Vector2(3.2f, 2f);

    /* START
     * Handles all components
     * Handles custom cursor
     */

    private void Start()
    {
        // ---- BASIC COMPONENTS ----
        UI = GetComponent<DruidUI>();
        boxcollider = GetComponent<BoxCollider2D>();
        druidrb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        druidspriterender = GetComponent<SpriteRenderer>();
        followPlayer = Camera.main.GetComponent<FollowPlayer>();

        // ---- CURSOR ----
        cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    /* FIXEDUPDATE
     * Handles all left to right movement logic
     * Handles coyote time
     * Handles UI transitions with the druid and bear in the circle wipe
     * Handles flip x of druid sprite
     * Handles animations for movement such as jump and walking
     */

    private void FixedUpdate()
    {
        if (!UI.dead && !inCutscene) //checks if not dead
        {
            if (walkingParticle != null)
            {
                var VOL = walkingParticle.velocityOverLifetime;
                VOL.enabled = true;
                VOL.xMultiplier = druidspriterender.flipX ? 1f : -1f;
            }

            if (canmove && !isStunned)
            {
                // ---- WALKING ----
                if (!isGrounded)
                {
                    animator.SetFloat("YVelo", druidrb.linearVelocityY);
                }
                else animator.SetFloat("YVelo", 0);

                if (!isAttacking) //checks if not attacking
                {
                    speedx = Input.GetAxisRaw("Horizontal");
                    druidrb.linearVelocityX = speedx * druidspeed; //sets velo to your movement direction times speed

                    if (isGrounded)
                    {
                        animator.SetFloat("XVelo", Mathf.Abs(speedx));
                    }
                    else
                    {
                        animator.SetFloat("XVelo", 0f);
                    }
                }
                else
                {
                    animator.SetFloat("XVelo", 0f);
                }

                // ---- FLIP X LOGIC AND UI LOGIC ----
                if (speedx > 0f) //forwards
                {
                    druidspriterender.flipX = false;
                }
                else if (speedx < 0f) //backwards
                {
                    druidspriterender.flipX = true;
                }

                // ---- WALKING PARTICLES ----
                if (speedx > 0f || speedx < 0f)
                {
                    if (isGrounded)
                    {
                        if (!isAttacking)
                        {
                            walkingParticle.Emit(1);
                        }
                    }
                }

                // ---- JUMP ANIMATIONS ----
                if (!isGrounded)
                {
                    if (druidrb.linearVelocityY > 0.5f)
                    {
                        animator.SetTrigger("Jump");
                    }
                }

                // ---- GROUND CHECK ----
                float rayLength = 0.2f;
                LayerMask platformLayer = LayerMask.GetMask("Platform");

                RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, platformLayer);
                bool touchingPlatform = hit.collider != null;
                bool validPlatformGround = touchingPlatform && druidrb.linearVelocityY <= 0f;

                bool touchingGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, LayerMask.GetMask("Ground"));
                isGrounded = touchingGround || validPlatformGround;
                animator.SetBool("IsGrounded", isGrounded);

                // ---- GROUND PARTICLE COLOUR CHANGER ----
                RaycastHit2D groundTag = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, LayerMask.GetMask("Ground"));
                ParticleSystem.MainModule walkMain;
                ParticleSystem.MainModule fallMain;
                fallMain = fallingParticle.main;
                walkMain = walkingParticle.main;
                if (groundTag)
                {
                    if (groundTag.collider.gameObject.CompareTag("Grass"))
                    {
                        fallMain.startColor = new Color(20f / 255f, 77f / 255f, 1f / 255f, 1f);
                        walkMain.startColor = new Color(20f / 255f, 77f / 255f, 1f / 255f, 1f);
                    }
                    else if (groundTag.collider.gameObject.CompareTag("Snow"))
                    {
                        fallMain.startColor = Color.white;
                        walkMain.startColor = Color.white;
                    }
                }
            }
        }
    }

    /* UPDATE
     * Update handles most jump logic and key inputs
     * Handles jump buffer
     * Handles Q to transform
     * Handles jump input
     */

    private void Update()
    {
        if (!UI.dead && !inCutscene)
        {
            if (canmove && !isStunned)
            {
                // ---- JUMP ----

                // ---- BUFFER ----
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    jumpBufferCounter = jumpBufferTime;
                }
                else
                {
                    jumpBufferCounter -= Time.deltaTime;
                }

                if (!isGrounded && druidrb.linearVelocityY > 0.1f)
                {
                    jumpBufferCounter = 0f;
                }

                if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isAttacking && !istransforming && !hasJumped)
                {
                    Debug.Log("JUMP!");
                    druidrb.linearVelocityY = jumpheight;
                    isJumping = true;
                    hasJumped = true;
                    jumpBufferCounter = 0f;
                }

                // ---- VARIABLE JUMP HEIGHT ----

                if (Input.GetKeyUp(KeyCode.Space) && isJumping)
                {
                    if (druidrb.linearVelocityY > 0f)
                    {
                        druidrb.linearVelocityY *= variableJumpMultiplier;
                    }
                    isJumping = false;
                }

                // ---- RESET ON LAND ----
                if (isGrounded)
                {
                    if (impactSpeed >= stunHeight && !wasGroundedLastFrame)
                    {
                        Stun();
                    }
                    else
                    {
                        coyoteTimeCounter = coyoteTime;
                        canjump = true;
                        hasJumped = false;
                    }
                    RaycastHit2D groundPosCheck = Physics2D.Raycast(druid.transform.position, Vector2.down, 1f, LayerMask.GetMask("Ground"));
                    if (groundPosCheck) lastGroundPosition = druidtransform.position;
                }
                else
                {
                    Debug.Log("Reset JumpOnGround");
                    coyoteTimeCounter -= Time.deltaTime;
                    canjump = false;
                }

                wasGroundedLastFrame = isGrounded;

                // ---- FASTER JUMP FALL ----
                if (!istransforming)
                {
                    if (canjump == false)
                    {
                        if (gravityjump)
                        {
                            druidrb.gravityScale += gravityScale; //add gravityScale to gravity when falling so it feels less floaty when jumping
                            gravityjump = false;
                        }
                    }
                    else
                    {
                        if (!gravityjump)
                        {
                            gravityjump = true;
                            druidrb.gravityScale = 1f; //set back to normal gravity
                        }
                    }
                }

                // ---- STUN FALL ----
                if (!isGrounded)
                {
                    impactSpeed = Mathf.Abs(druidrb.linearVelocityY);
                }

                // ---- CAP DOWNWARDS VELO ----
                if (druidrb.linearVelocityY <= maximumYVelocity)
                {
                    druidrb.linearVelocityY = maximumYVelocity;
                }

                // ---- TRANSFORMATIONS INPUT ----
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (!isTransformed)
                    {
                        if (UI.spirits == 5)
                        {
                            if (!istransforming)
                            {
                                if (!Transitioning)
                                {
                                    StartCoroutine(TransformIntoAnimal("Bear"));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!istransforming)
                        {
                            if (!Transitioning)
                            {
                                StartCoroutine(TransformIntoDruid());
                            }
                        }
                    }
                }
            }
        }
    }

    /* FUNCTIONS
     * Beartattack will trigger the bear to attack by calling a coroutine
     * Changecollidersize will change the druids collider to a new size and a new offset useful for transformations
     * Stun will stun the player
     */

    public void Stun()
    {
        isStunned = true;
        fallingParticle.Emit(10);
        canjump = false;
        canmove = false;
        druidrb.linearVelocityX = 0f;
        animator.SetTrigger("Land");
        druidrb.linearVelocityX = 0f;
        followPlayer.ScreenShake(0.02f, 0.5f);
        Invoke("Recover", 0.4f);
    }

    public void BearAttack() //call this to attack while bear
    {
        StartCoroutine(attack());
    }

    // ---- COLLIDER SIZE ----
    private void ChangeColliderSize(Vector2 newsize, Vector2 newoffset)
    {
        boxcollider.offset = newoffset;
        boxcollider.size = newsize;
    }

    //Call to recover from fall
    public void Recover()
    {
        isStunned = false;
        canjump = true;
        canmove = true;
        animator.SetTrigger("Recover");
        Debug.Log("Druid Recovered!");
    }

    /* COROUTINES
     * Calling attack will trigger the bearattack animation and raycast a hit
     * calling freezeframe will freeze the game for a set duration as stated in parameters
     * calling transform into animal will trigger you to transform into an animal as stated in parameters
     * calling transform into druid will trigger you to transform back into a druid
     */

    private IEnumerator attack() //call to attack as bear
    {
        bearattackcd = true;
        isAttacking = true;
        canjump = false;

        animator.SetTrigger("BearBite");
        druidrb.linearVelocityX = 0f;

        yield return new WaitForSeconds(0.7f);

        float direction = druidspriterender.flipX ? -1f : 1f; //checks which way facing
        Vector2 directionVector = new Vector2(direction, 0f);
        Vector2 offset = directionVector * (biteSize.x / 2f + 0.1f);

        RaycastHit2D hit = Physics2D.BoxCast((Vector2)druidtransform.position + offset, biteSize, 0f, directionVector, 0f, LayerMask.GetMask("GrowEnemy", "RoboticEnemy"));
        if (hit.collider != null)
        {
            if (!damagecd)
            {
                if (hit.collider != null)
                {
                    IDamageAble enemy = hit.collider.GetComponent<IDamageAble>();
                    if (enemy != null && hit.collider != druid)
                    {
                        if (!enemy.Dead)
                        {
                            Debug.Log(hit.collider.gameObject.name + " has been hit!");
                            damagecd = true;
                            Persistence.instance.ApplyDamage(hit.collider.gameObject, 2f);
                            yield return StartCoroutine(FreezeFrame(0.25f));
                            float burstSpeed = 4f;

                            //Applys a backwards force when hitting something
                            druidrb.AddForce(new Vector2(burstSpeed * -direction, 0f), ForceMode2D.Impulse);
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.1f);
        damagecd = false;
        isAttacking = false;
        canjump = true;
        yield return new WaitForSeconds(3f); // Cooldown
        bearattackcd = false;
    }

    public IEnumerator FreezeFrame(float duration) //freezeframe
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
    }

    private IEnumerator TransformIntoAnimal(string animal) //call to transform into animal as stated in parameters
    {
        if (animal == "Bear")
        {
            if (transformcd == false)
            {
                UI.spirits = 0;
                druidrb.linearVelocityX = 0f;
                druidrb.linearVelocityY = 0f;
                druidrb.gravityScale = 0f;

                isAttacking = true;
                animator.SetTrigger("TransformBear");
                istransforming = true;
                canjump = false;

                yield return new WaitForSeconds(0.4f);//after anim plays

                animator.SetBool("Bear", true);
                istransforming = false;
                isAttacking = false;
                druidrb.gravityScale = 1f;
                canjump = true;
                groundCheck.localPosition -= new Vector3(0, 0.17f, 0);
                jumpheight = bearJumpHeight;
                ChangeColliderSize(new Vector2(0.9f, 0.43f), new Vector2(-0.05f, -0.42f));
                isTransformed = true;
                animator.SetFloat("XVelo", speedx);
            }
        }
    }

    private IEnumerator TransformIntoDruid() //call to transform back into a druid
    {
        transformcd = true;
        canjump = false;

        UI.spirits = 5;
        druidrb.linearVelocityX = 0f;
        druidrb.linearVelocityY = 0f;
        druidrb.gravityScale = 0f;

        istransforming = true;
        animator.SetBool("IsTransforming", true);
        isAttacking = true;
        jumpheight = druidJumpHeight;
        animator.SetBool("Bear", false);

        yield return new WaitForSeconds(0.3f);//after anim plays
        animator.SetBool("IsTransforming", false);
        istransforming = false;
        isAttacking = false;
        druidrb.gravityScale = 1f;
        canjump = true;
        groundCheck.localPosition += new Vector3(0, 0.17f, 0);
        ChangeColliderSize(new Vector2(0.7f, 0.53f), new Vector2(0f, -0.2f));
        isTransformed = false;
        animator.SetFloat("XVelo", speedx);

        yield return new WaitForSeconds(1f);
        transformcd = false;
    }
}