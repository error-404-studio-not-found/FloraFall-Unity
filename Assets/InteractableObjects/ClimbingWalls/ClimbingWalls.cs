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

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerRig = player.GetComponent<Rigidbody2D>();
        DF = player.GetComponent<DruidFrameWork>();
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