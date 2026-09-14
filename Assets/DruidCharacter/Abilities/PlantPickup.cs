using DG.Tweening;
using System.Collections;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlantPickup : MonoBehaviour
{
    private DruidUI UI;
    public GameObject heldPlant;
    public bool active = true;
    private DruidGrowFramework DGF;
    private bool canPlace = false;
    private bool canPick = true;
    private bool placing = false;
    private bool canPutDownPlant = false;
    [SerializeField] private float offset = 1f;
    private Camera cam;
    [SerializeField] private float distance = 3f;

    private void Start()
    {
        UI = GetComponent<DruidUI>();
        DGF = GetComponent<DruidGrowFramework>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (!UI.dead && active)
        {
            if (heldPlant == null)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("GrowPlants"));
                if (hit.collider != null && canPick)
                {
                    IGrowablePlant growInterface = hit.collider.gameObject.GetComponent<IGrowablePlant>();
                    if (growInterface.CanGrow && !growInterface.IsGrown && !growInterface.CanDie)
                    {
                        if (Input.GetKeyDown(KeyCode.Q))
                        {
                            Transform target = hit.collider.transform;
                            float distance = Vector2.Distance(transform.position, target.position);
                            if (distance < DGF.maxTetherDistance - 2)
                            {
                                var clonedPlant = Instantiate(hit.collider.gameObject, transform.parent);
                                clonedPlant.SetActive(false);
                                hit.collider.gameObject.SetActive(false);
                                heldPlant = clonedPlant;
                                heldPlant.transform.position = transform.position;
                                canPlace = true;
                                canPick = false;
                            }
                        }
                    }
                }
            }
            else
            {
                if (canPlace && !placing)
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        StartCoroutine(startPlacing());
                    }
                }
            }
        }

        //---- PLACING LOGIC ----
        if (placing)
        {
            var heldPlantSR = heldPlant.GetComponent<SpriteRenderer>();
            Vector3 mousPos = Input.mousePosition;

            RaycastHit2D snapDownCast = Physics2D.Raycast(new Vector2(cam.ScreenToWorldPoint(mousPos).x, (cam.ScreenToWorldPoint(mousPos).y + 100f)),
              Vector2.down, 1000f, LayerMask.GetMask("Ground"));

            if (snapDownCast)
            {
                if (Vector2.Distance(snapDownCast.point, transform.position) < distance)
                {
                    canPutDownPlant = true;
                    heldPlantSR.color = new Color32(255, 255, 255, 150);
                    heldPlant.transform.position = snapDownCast.point + new Vector2(0, offset);
                }
                else
                {
                    canPutDownPlant = false;
                    heldPlantSR.color = new Color32(255, 0, 0, 150);
                }
            }

            if (canPutDownPlant)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    heldPlantSR.color = new Color32(255, 255, 255 , 255);
                    StartCoroutine(PlaceDownPlant(snapDownCast.point + new Vector2(0, offset)));
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(QuitPlace());
            }
        }
    }

    private IEnumerator QuitPlace()
    {
        heldPlant.SetActive(false);
        canPutDownPlant = false;
        placing = false;
        yield return new WaitForSeconds(3f);
        canPick = true;
        canPlace = true;
    }
    private IEnumerator startPlacing()
    {
        canPlace = false;
        heldPlant.transform.position = transform.position;
        Behaviour[] components = heldPlant.GetComponents<Behaviour>();
        foreach (Behaviour comp in components)
            comp.enabled = false;

        heldPlant.SetActive(true);
        var heldPlantSR = heldPlant.GetComponent<SpriteRenderer>();
        heldPlantSR.enabled = true;
        heldPlantSR.color = new Color32(255, 0, 0, 100);
        yield return null;
        placing = true;
    }

    private IEnumerator PlaceDownPlant(Vector2 position)
    {
        placing = false;
        canPlace = false;
 
        canPutDownPlant = false;
        Behaviour[] components = heldPlant.GetComponents<Behaviour>();
        foreach (Behaviour comp in components)
            comp.enabled = true;

        heldPlant = null;
        yield return new WaitForSeconds(2f);
        canPick = true;
    }
}