using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlowRootPlant : MonoBehaviour, IGrowablePlant
{
    private Animator animator;
    public bool glowdb = false;
    public bool candie = false;
    private int spirits = 1;
    public int spiritCost => spirits;
    public bool waterGrown = false;
    public void setWaterGrow(bool value)
    {
        waterGrown = value;
    }
    public bool WaterGrown => waterGrown;
    private bool canGrow = true;
    public bool CanGrow => canGrow;
    public bool IsGrown => glowdb;
    public bool CanDie => candie;
    [SerializeField] private Light2D light;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Grow()
    {
        if (glowdb == false && canGrow)
        {
            StartCoroutine(GrowCycle());
        }
    }

    public void Die()
    {
        if (glowdb == true)
        {
            if (candie == true && canGrow)
            {
                StartCoroutine(diecycle());
            }
        }
    }

    private IEnumerator GrowCycle()
    {
        animator.SetTrigger("Grow");
        glowdb = true;
        canGrow = false;
        yield return new WaitForSeconds(0.3f);
        light.enabled = true;
        canGrow = true;
        candie = true;
    }

    private IEnumerator diecycle()
    {
        candie = false;
        animator.SetTrigger("Die");
        canGrow = false;
        light.enabled = false;
        yield return new WaitForSeconds(1.5f);
        canGrow = true;
        glowdb = false;
    }
}