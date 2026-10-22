using System.Collections;
using UnityEngine;

public class WoodBridge : MonoBehaviour, IGrowablePlant
{
    [SerializeField] private int spirits = 1;
    public bool waterGrown = false;
    public bool WaterGrown => waterGrown;
    public void setWaterGrow(bool value)
    {
        waterGrown = value;
    }
    private bool canGrow = true;
    public bool CanGrow => canGrow;
    private bool bridgeDB = false;
    public bool candie = false;
    public int spiritCost => spirits;

    public bool CanDie => candie;
    public bool IsGrown => bridgeDB;

    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Grow()
    {
        if (!bridgeDB)
        {
            if (!candie && canGrow)
            {
                StartCoroutine(GrowCycle());
            }
        }
    }

    public void Die()
    {
        if (bridgeDB)
        {
            if (candie && canGrow)
            {
                StartCoroutine(DieCycle());
            }
        }
    }

    private IEnumerator GrowCycle()
    {
        yield return null;
    }

    private IEnumerator DieCycle()
    {
        yield return null;
    }
}
