using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class FloatingFlower : MonoBehaviour, IGrowablePlant
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
    private bool flowerDB = false;
    public bool canDie = false;
    public int spiritCost => spirits;

    public bool CanDie => canDie;
    public bool IsGrown => flowerDB;
    [SerializeField] private GameObject flowerPlatform;
    [SerializeField] private float floatTime = 3f;
    [SerializeField] private float propellTime = 0.75f;
    [SerializeField] private float height = 6f;
    [SerializeField] private AnimationCurve curve;
    void Start()
    {
        
    }

    public void Grow()
    {
        if (!flowerDB)
        {
            if (!canDie && canGrow)
            {
                StartCoroutine(GrowCycle());
            }
        }
    }

    public void Die()
    {
        if (flowerDB)
        {
            if (canDie && canGrow)
            {
                StartCoroutine(DieCycle());
            }
        }
    }
    
    private IEnumerator GrowCycle()
    {
        flowerDB = true;
        canGrow = false;
        float t = 0;
        var startPos = flowerPlatform.transform.position;
        while (t <= propellTime)
        {
            yield return null;
            float heightOffset = curve.Evaluate(t / propellTime) * height;
            flowerPlatform.transform.position = startPos + new Vector3(0, heightOffset, 0);
            t += Time.deltaTime;

        }
        flowerPlatform.transform.position = new Vector3(startPos.x, startPos.y + height, startPos.z);
        canGrow = true;
        canDie = true;
    }

    private IEnumerator DieCycle()
    {
        flowerDB = false;
        canGrow = false;
        float t = 0;
        var startPos = flowerPlatform.transform.position;
        while (t <= floatTime)
        {
        
            yield return null;
            flowerPlatform.transform.position = Vector2.Lerp(startPos, new Vector2(startPos.x, startPos.y - height), t / floatTime);
            t += Time.deltaTime;
        }
        yield return new WaitForSeconds(1f);
        canGrow = true;
        canDie = false;
    }
}
