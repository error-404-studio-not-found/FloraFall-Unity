using System.Collections;
using UnityEngine;

public class IcicleFalling : MonoBehaviour
{
    [SerializeField] private float activationDistance = 3f;
    private Rigidbody2D iceRig;
    private bool falling = false;
    private Animator animator;
    private bool hitCD = false;
    [SerializeField] private float telegraphTime = 1f;

    void Start()
    {
        iceRig = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!falling)
        {
            RaycastHit2D iceDetect = Physics2D.Raycast(gameObject.transform.position, Vector2.down, activationDistance, LayerMask.GetMask("Player"));
            if (iceDetect)
            {
                Debug.Log("Falling");
                StartCoroutine(fallingRoutine());
                falling = true;
            }
        }
    }

    private IEnumerator fallingRoutine()
    {
        animator.SetTrigger("Fall");
        yield return new WaitForSeconds(telegraphTime);
        animator.SetTrigger("Falling");
        iceRig.bodyType = RigidbodyType2D.Dynamic;
        iceRig.gravityScale = 2f;
        iceRig.constraints = RigidbodyConstraints2D.FreezePositionX;
        iceRig.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!hitCD)
            {
                Persistence.instance.ApplyDamageToDruid(collision.gameObject, 1f);
                hitCD = true;
                Invoke("coolDown", 1f);
            } 
        } 
        else if (collision.gameObject.CompareTag("Ground"))
        {

        }
    }

    void coolDown()
    {
        hitCD = false;
    }
}