using System.Collections;
using UnityEngine;

public class IcicleDestroy : MonoBehaviour
{
    ParticleSystem deathParticle;
    [SerializeField] private GameObject icicle;
    void Start()
    {
        deathParticle = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
       {
            deathParticle.transform.parent = null;
            deathParticle.Emit(10);
            Destroy(deathParticle.gameObject, 1f);
            Destroy(icicle);
        } 
    }
}
