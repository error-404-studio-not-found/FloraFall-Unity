using System.Collections;
using UnityEngine;

public class InnerSpirit : MonoBehaviour, IGrowablePlant
{
    public int storedBolts = 0;
    private Animator animator;
    public bool spiritdb = false;
    public bool candie = false;
    private int spirits = 0;
    public int spiritCost => spirits;
    public bool waterGrown = false;
    public void setWaterGrow(bool value)
    {
        waterGrown = value;
    }
    public bool WaterGrown => waterGrown;
    private bool canGrow = true;
    public bool CanGrow => canGrow;
    public bool IsGrown => spiritdb;
    public bool CanDie => candie;
    CurrencyManager currencyManager;
    DruidGrowFramework DGF;

    void Start()
    {
        animator = GetComponent<Animator>();
        currencyManager = GameObject.FindGameObjectWithTag("NutText").GetComponent<CurrencyManager>();
        DGF = GameObject.FindGameObjectWithTag("Player").GetComponent<DruidGrowFramework>();
    }


    public void Grow()
    {
        if (canGrow)
        {
            StartCoroutine(GrowCycle());
        }
    }

    public void Die()
    {

    }

    private IEnumerator GrowCycle()
    {
        
        canGrow = false;
        currencyManager.gainBolts(storedBolts);
        animator.SetTrigger("Grow");
        Camera.main.GetComponent<FollowPlayer>().ScreenShake(0.02f, 0.4f);
        yield return new WaitForSeconds(0.35f);
        Camera.main.GetComponent<FollowPlayer>().ScreenShake(0.025f, 0.2f);
        canGrow = true;
        spiritdb = true;
        candie = true;
        DGF.DeGrowPlant(transform);
        yield return null;
        Destroy(gameObject);
    }
}
