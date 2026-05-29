using System.Collections;
using UnityEngine;

public class Switch : MonoBehaviour, IGrowablePlant
{
    public bool waterGrown = false;
    public bool WaterGrown => waterGrown;
    public void setWaterGrow(bool value)
    {
        waterGrown = value;
    }
    private bool canGrow = true;
    public bool CanGrow => canGrow;
    private bool switchDB = false;
    public bool candie = false;
    public int spiritCost => 1;

    public bool CanDie => candie;
    public bool IsGrown => switchDB;

    private Animator animator;
    [SerializeField] private GameObject door;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Grow()
    {
        if (!switchDB)
        {
            if (!candie && canGrow)
            {
                StartCoroutine(GrowCycle());
            }
        }
    }

    public void Die()
    {
        if (switchDB)
        {
            if (candie && canGrow)
            {
                StartCoroutine(DieCycle());
            }
        }
    }
    
    private IEnumerator GrowCycle()
    {
        canGrow = false;
        switchDB = true;
        animator.SetTrigger("On");
        yield return new WaitForSeconds(0.3f);
        var doorAnim = door.GetComponent<Animator>();
        doorAnim.SetTrigger("Open");
        yield return new WaitForSeconds(0.3f);
        canGrow = true;
        candie = true;
    }

    private IEnumerator DieCycle()
    {
        canGrow = false;
        switchDB = false;
        animator.SetTrigger("Off");
        yield return new WaitForSeconds(0.3f);
        var doorAnim = door.GetComponent<Animator>();
        doorAnim.SetTrigger("Close");
        yield return new WaitForSeconds(0.3f);
        candie = true;
        canGrow = true;
    }

}
