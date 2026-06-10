using UnityEngine;
using System.Collections;
public class FloatingFlower1 : MonoBehaviour, IGrowablePlant
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
    private Animator flowerPlatformAnimator;
    [SerializeField] private float floatTime = 3f;
    [SerializeField] private float propellTime = 0.75f;
    [SerializeField] private float height = 6f;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private AnimationCurve floatCurve;
    BoxCollider2D flowerCollider;
    Rigidbody2D flowerRig;
    Animator baseAnimtor;
    private ParticleSystem.EmissionModule emission;
    ParticleSystem baseParticle;
    bool up = false;
    float t = 0;
    Vector3 startPos;
    bool down = false;

    private void Start()
    {
        flowerPlatformAnimator = flowerPlatform.GetComponent<Animator>();
        baseAnimtor = GetComponent<Animator>();
        baseParticle = GetComponent<ParticleSystem>();
        emission = baseParticle.emission;
        flowerCollider = flowerPlatform.GetComponent<BoxCollider2D>();
        flowerRig = flowerPlatform.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (up)
        {
            t += Time.fixedDeltaTime;
            float heightOffset = curve.Evaluate(t / propellTime) * height;

            Vector3 targetPos = startPos + Vector3.up * heightOffset;
            flowerRig.MovePosition(targetPos);
            if (t >= propellTime)
            {
                up = false;
                t = 0;
                flowerPlatform.transform.position = new Vector3(startPos.x, startPos.y + height, startPos.z);
                canGrow = true;
                canDie = true;
                flowerDB = true;
                startPos = flowerPlatform.transform.position;
            }
        } else if (down)
        {
            t += Time.fixedDeltaTime;
            float heightOffset = floatCurve.Evaluate(t / floatTime) * height;
            Vector3 targetPos = startPos + Vector3.down * heightOffset;
            flowerRig.MovePosition(targetPos);
            if (t >= floatTime)
            {
                down = false;
                flowerPlatformAnimator.SetTrigger("Stop");
                t = 0;
                flowerPlatform.transform.position = new Vector3(startPos.x, startPos.y - height, startPos.z);
                canGrow = true;
                canDie = false;
                flowerCollider.enabled = false;
            }
        }
    }

    public void Grow()
    {
        if (!flowerDB)
        {
            if (!canDie && canGrow)
            {
                up = true;
                flowerCollider.enabled = true;
                emission.enabled = true;
                baseAnimtor.SetTrigger("Grow");
                flowerDB = true;
                canGrow = false;

                flowerPlatformAnimator.SetTrigger("Blow");
                startPos = flowerPlatform.transform.position;
            }
        }
    }

    public void Die()
    {
        if (flowerDB)
        {
            if (canDie && canGrow)
            {
                emission.enabled = false;
                down = true;
                baseAnimtor.SetTrigger("Die");
                flowerDB = false;
                canGrow = false;
                flowerPlatformAnimator.SetTrigger("Spin");
            }
        }
    }
}
