using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Sprinkler : MonoBehaviour, IGrowablePlant
{
    [SerializeField] private int growthCost = 5;
    private bool isGrown = false;
    private bool canDie = false;
    public bool CanDie => canDie;
    public bool IsGrown => isGrown;
    public bool waterGrown = false;
    [SerializeField] private ParticleSystem waterLittleDrop;
    [SerializeField] private ParticleSystem showerParticle;
    [SerializeField] private float boxCheckWidth = 3f;
    List<Transform> plantsGrown = new List<Transform>();
    private bool canGrow = true;
    public bool CanGrow => canGrow;

    public void setWaterGrow(bool value)
    {
        waterGrown = value;
    }

    public bool WaterGrown => waterGrown;
    public int spiritCost => growthCost;
    private Animator animator;
    [SerializeField] private Transform waterDropPos;
    private DruidGrowFramework DGF;

    private void Start()
    {
        animator = GetComponent<Animator>();
        DGF = GameObject.FindGameObjectWithTag("Player").GetComponent<DruidGrowFramework>();
    }

    private void Update()
    {
        if (isGrown)
        {
            RaycastHit2D growChecker = Physics2D.Raycast(transform.position, Vector2.down, 1000, LayerMask.GetMask("Ground"));

            if (!growChecker) return;

            float height = Mathf.Abs(transform.position.y - growChecker.point.y);
            Vector2 boxSize = new Vector2(boxCheckWidth, 0.5f);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, boxSize, 0f, Vector2.down, height, LayerMask.GetMask("GrowPlants"));

            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == gameObject) continue;

                IGrowablePlant growInterface = hit.collider.GetComponent<IGrowablePlant>();
                if (growInterface != null)
                {
                    if (growInterface == GetComponent<IGrowablePlant>()) continue;

                    Debug.Log("GrowInterface Valid!");
                    if (!growInterface.IsGrown)
                    {
                        plantsGrown.Add(hit.collider.transform);
                        Debug.Log("WaterGrowning" + hit.collider.name);
                        growInterface.setWaterGrow(true);
                        growInterface.Grow();
                    }
                }

            }
        }
    }

    public void Grow()
    {
        if (!canDie && !isGrown && canGrow) StartCoroutine(GrowCycle());
    }

    public void Die()
    {
        if (canDie && isGrown && canGrow) StartCoroutine(DieCycle());
    }

    private IEnumerator GrowCycle()
    {
        animator.SetTrigger("Grow");
        canDie = true;
        var emission1 = waterLittleDrop.emission;
        var emission2 = showerParticle.emission;
        emission1.enabled = false;
        canGrow = false;
        yield return new WaitForSeconds(1f);
        canGrow = true;
        emission2.enabled = true;
        isGrown = true;
    }

    private IEnumerator DieCycle()
    {
        foreach (var plant in plantsGrown)
        {
            
            plant.GetComponent<IGrowablePlant>().setWaterGrow(false);
            var plantGrow = plant.GetComponent<IGrowablePlant>();
            while (!plantGrow.CanGrow)
            {
                yield return null;
            }
            DGF.DeGrowPlant(plant);
        }

        animator.SetTrigger("Die");
        isGrown = false;
        canGrow = false;
        var emission1 = waterLittleDrop.emission;
        var emission2 = showerParticle.emission;
        emission2.enabled = false;
        yield return new WaitForSeconds(1f);
        canGrow = true;
        emission1.enabled = true;
        canDie = false;
    }
}