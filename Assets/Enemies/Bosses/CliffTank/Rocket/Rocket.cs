using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

public class Rocket : MonoBehaviour
{
    bool exploding = false;
    Animator animator;
    [SerializeField] private GameObject explosionHitbox;
    private Transform druidTransform;
    private FollowPlayer followPlayer;
    [SerializeField] private float shakeDistane = 5f;
    void Start()
    {
        animator = GetComponent<Animator>();
        followPlayer = Camera.main.GetComponent<FollowPlayer>();
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            druidTransform = player.GetComponent<Transform>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!exploding)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                StartCoroutine(NormalExplode());
            }
        }
    }

    private IEnumerator NormalExplode()
    {
        transform.rotation = Quaternion.identity;
        var normalHitbox = gameObject.GetComponent<BoxCollider2D>();
        normalHitbox.enabled = false;
        RaycastHit2D movePoint = Physics2D.Raycast(transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (movePoint)
        {
            transform.position = movePoint.point;
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        }
        exploding = true;
        animator.SetTrigger("Explode");
        explosionHitbox.SetActive(true);
        if (Vector2.Distance(explosionHitbox.transform.position, druidTransform.position) <= shakeDistane)
        {
            followPlayer.ScreenShake(0.035f, 0.35f);
        }
        yield return new WaitForSeconds(0.35f);
        var explosionHitboxCollider = explosionHitbox.GetComponent<BoxCollider2D>();
        explosionHitboxCollider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        explosionHitbox.SetActive(false);
        gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
