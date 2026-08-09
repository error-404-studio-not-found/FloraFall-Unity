using System.Collections;

using UnityEngine;

public class ClimbingWals : MonoBehaviour
{
    private bool grabbedOn = false;
    private GameObject player;
    private Rigidbody2D playerRig;
    private float grabCooldown = 0.5f;
    private bool canGrab = true;
    private DruidFrameWork DF;
    private Animator druidAnims;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerRig = player.GetComponent<Rigidbody2D>();
        DF = player.GetComponent<DruidFrameWork>();
        druidAnims = player.GetComponent<Animator>();
    }

    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!grabbedOn && canGrab)
            {
                canGrab = false;
                player.transform.position = new Vector2(transform.position.x, player.transform.position.y);
                playerRig.constraints = RigidbodyConstraints2D.FreezePositionX;
                playerRig.linearVelocity = Vector2.zero;
                playerRig.gravityScale = 0;
                DruidFrameWork.canJumpOffClimb = false;
                DruidFrameWork.canjump = true;
                DF.isJumping = false;
                druidAnims.ResetTrigger("Jump");
                druidAnims.ResetTrigger("StopClimbing");
                druidAnims.ResetTrigger("ClimbWall");

                druidAnims.SetFloat("XVelo", 0f);
                druidAnims.SetFloat("YVelo", 0f);
                druidAnims.SetFloat("ClimbMoving", 0f);

                druidAnims.SetBool("IsGrounded", false);
                druidAnims.SetTrigger("ClimbWall");

                grabbedOn = true;
                DruidFrameWork.climbing = true;
                StartCoroutine(CanJumpCooldown());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (grabbedOn)
            {
                druidAnims.SetTrigger("StopClimbing");
                playerRig.constraints = RigidbodyConstraints2D.None;
                playerRig.constraints = RigidbodyConstraints2D.FreezeRotation;
                playerRig.gravityScale = 1.5f;
                grabbedOn = false;
                DruidFrameWork.canJumpOffClimb = false;
                DruidFrameWork.climbing = false;
                StartCoroutine(GrabCooldown());
            }
        }
    }

    private IEnumerator GrabCooldown()
    {
        yield return new WaitForSeconds(grabCooldown);
        canGrab = true;
    }

    private IEnumerator CanJumpCooldown()
    {
        yield return new WaitForSeconds(0.15f);
        DruidFrameWork.canJumpOffClimb = true;
    }
}