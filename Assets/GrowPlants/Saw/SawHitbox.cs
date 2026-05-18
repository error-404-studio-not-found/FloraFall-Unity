using System.Collections;
using UnityEngine;

public class SawHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float bounceForce = 2f;
    [SerializeField] private int hitsBeforeBreaking = 2;
    private Rigidbody2D rb;
    public static bool hitCooldown = false;
    private int currentHits = 0;
    [SerializeField] private Saw saw;
    private DruidGrowFramework druidGrowFramework;

    private void Start()
    {
        druidGrowFramework = GameObject.FindGameObjectWithTag("Player").GetComponent<DruidGrowFramework>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {;
        if (hitCooldown) return;
        int enemyMask = LayerMask.GetMask("GrowEnemy", "RoboticEnemy");

        if (((1 << collision.gameObject.layer) & enemyMask) != 0)
        {
            IEnemy enemy = collision.gameObject.GetComponent<IEnemy>();
            if (enemy != null && !enemy.Dead)
            {
                StartCoroutine(Bounce(collision.gameObject.transform));
                Persistence.instance.ApplyDamage(collision.gameObject, damage);
            } 
        } 
    }

    private IEnumerator Bounce(Transform hitTarget)
    {
        Debug.Log("Bouncing");
        hitCooldown = true;
        Vector2 bounceDirection = (transform.position - hitTarget.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
        
        currentHits++;
        if (currentHits >= hitsBeforeBreaking)
        {
            hitCooldown = false;
            druidGrowFramework.DeGrowPlant(saw.plantPos);
        }
        yield return new WaitForSeconds(0.3f);
        hitCooldown = false;
    }
}
