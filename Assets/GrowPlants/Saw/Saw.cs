using UnityEngine;
using System.Collections;

public class Saw : MonoBehaviour, IGrowablePlant
{
    private bool growDB = false;
    public bool IsGrown => growDB;
    private bool canDie = false;
    public bool CanDie => canDie;
    public int spiritCost => 1;
    [SerializeField] private GameObject sawObject;
    public bool waterGrown = false;
    public bool WaterGrown => waterGrown;
    public void setWaterGrow(bool value)
    {
        waterGrown = value;
    }
    private GameObject sawClone;
    private GameObject druid;
    [SerializeField] private float sawSpeed = 2f;
    private DruidGrowFramework druidGrow;
    [SerializeField] private float respawnTime = 2f;
    public Transform plantPos;

    private LineRenderer tether;
    private LineRenderer tetherClone;
    [SerializeField] private float sawDistance = 5;
    private bool isGrowing = false;
    private Animator rootPlantAnimator;
    private bool canGrow = true;
    public bool CanGrow => canGrow;

    private void Start()
    {
        plantPos = GetComponent<Transform>();
        druid = GameObject.FindGameObjectWithTag("Player");
        rootPlantAnimator = GetComponent<Animator>();
        druidGrow = druid.GetComponent<DruidGrowFramework>();
        tether = GameObject.FindGameObjectWithTag("PlantTether").GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (growDB && sawClone != null)
        {
            if (tetherClone != null)
            {
                tetherClone.SetPosition(0, druid.transform.position);
                tetherClone.SetPosition(1, sawClone.transform.position);
            }
            if (!SawHitbox.hitCooldown)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                sawClone.transform.position = Vector2.Lerp(sawClone.transform.position, mousePos, sawSpeed * Time.deltaTime);
                if (Vector2.Distance(druid.transform.position, sawClone.transform.position) > sawDistance) druidGrow.DeGrowPlant(transform);
            }
            
        }
    }

    public void Grow()
    {
        if (!growDB && !isGrowing && canGrow) StartCoroutine(GrowCycle());
    }

    public void Die()
    {
        if (growDB && canDie && canGrow) StartCoroutine(DieCycle());
    }

    private IEnumerator GrowCycle()
    {
        canGrow = false;
        rootPlantAnimator.SetTrigger("Grow");
        isGrowing = true;
        yield return new WaitForSeconds(0.35f);
        canGrow = true;
        canDie = true;
        sawClone = Instantiate(sawObject);
        sawClone.transform.position = gameObject.transform.position;
        sawClone.SetActive(true);
        tetherClone = Instantiate(tether);
        tetherClone.positionCount = 2;

        tetherClone.useWorldSpace = true;

        while (Vector2.Distance(druid.transform.position, sawClone.transform.position) > sawDistance - 2)
        {
            if (tetherClone != null)
            {
                sawClone.transform.position = Vector2.Lerp(sawClone.transform.position, druid.transform.position, sawSpeed * Time.deltaTime);
                tetherClone.SetPosition(0, druid.transform.position);
                tetherClone.SetPosition(1, sawClone.transform.position);
                yield return null;
            }
        }
        growDB = true;
        isGrowing = false;
    }

    public IEnumerator DieCycle()
    {
        canGrow = false;
        rootPlantAnimator.SetTrigger("Die");
        canDie = false;
        isGrowing = false;
        yield return null;
        Destroy(tetherClone.gameObject);

        if (tetherClone)
        {
            Destroy(tetherClone);
        }

        if (sawClone)
        {
            Destroy(sawClone);
        }
        yield return new WaitForSeconds(respawnTime);
        rootPlantAnimator.SetTrigger("Respawn");
        yield return new WaitForSeconds(0.4f);
        canGrow = true;
        growDB = false;
    }
}