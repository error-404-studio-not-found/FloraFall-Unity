using System.Collections;
using UnityEngine;

public class InnerSpirit : MonoBehaviour, IGrowablePlant
{
    public static int storedBolts = 0;
    private Animator animator;
    public bool spiritdb = false;
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
        if (spiritdb == false && canGrow)
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
        spiritdb = true;
        currencyManager.gainBolts(storedBolts);
        animator.SetTrigger("Grow");
        Camera.main.GetComponent<FollowPlayer>().ScreenShake(0.02f, 0.4f);
        yield return new WaitForSeconds(0.4f);
        Camera.main.GetComponent<FollowPlayer>().ScreenShake(0.025f, 0.2f);
        candie = true;
        canGrow = true;
        spiritdb = true;
        DGF.DeGrowPlant(transform);
        gameObject.SetActive(false);
        yield return null;
        spiritdb = false;
        candie = false;
    }
}
