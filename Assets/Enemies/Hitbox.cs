using System.Collections;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    /* HITBOX
     * This script will make any gameobject attached to it do damage
     * Druid damage parameter determines if it damages the player or not
     */
    [SerializeField] private bool druidDamaging = false;
    [SerializeField] private bool knockBack = false;
    [SerializeField] private GameObject parentObject;
    [SerializeField] private float damage = 1f;
    private GameObject druid;
    private Rigidbody2D druidRig;
    private DruidUI druidUI;
    [SerializeField] private float knockBackForce = 2f;
    [SerializeField] private float timeStunned = 0.2f;

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            druidUI = player.GetComponent<DruidUI>();
            druid = player;
            druidRig = player.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Entered Trigger");
        if (collision.gameObject.CompareTag("Player") && druidDamaging && !druidUI.dead)
        {
            Persistence.instance.ApplyDamageToDruid(collision.gameObject, damage);
            if (knockBack)
            {
                StartCoroutine(KnockBack());
            }
        }
        else if ((collision.gameObject.layer == LayerMask.NameToLayer("GrowEnemy")
            || collision.gameObject.layer == LayerMask.NameToLayer("RoboticEnemy")
            || collision.gameObject.layer == LayerMask.NameToLayer("Breakables")) && !druidDamaging)
        {
            Debug.Log("Hitbox Passed Layer Check!");
            if (collision.gameObject != parentObject)
            {
                Debug.Log("Hit Enemy!");
                Persistence.instance.ApplyDamage(collision.gameObject, damage);
            }
        }
    }

    private IEnumerator KnockBack()
    {
        druidRig.linearVelocity = Vector2.zero;
        DruidFrameWork.canmove = false;
        var hitXDir = transform.position.x > druid.transform.position.x ? -1 : 1;
        druidRig.AddForce(new Vector2(hitXDir * knockBackForce, knockBackForce), ForceMode2D.Impulse);
        yield return new WaitForSeconds(timeStunned);
        Debug.Log("Stopped KnockBack");
        DruidFrameWork.canmove = true;
    }
}