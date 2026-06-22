using System.Collections;
using UnityEngine;

using UnityEngine.Rendering.Universal;

public class CliffCutscene : MonoBehaviour
{
    private bool cutsceneCompleted = false;
    private bool inCutscene = false;
    [SerializeField] private Transform firstMoveTo;
    [SerializeField] private Transform mechMovePos;
    private DruidGrowFramework DGF;
    private Rigidbody2D druidRig;
    private Animator druidAnimator;
    private Transform druidTransform;
    private DruidFrameWork DF;
    [SerializeField] private GameObject boss;
    private Rigidbody2D bossRig;
    private BoxCollider2D bossCollider;
    [SerializeField] private float zoomedOutSize = 1.5f;
    private PixelPerfectCamera ppc;
    private CliffMech cliffMech;

    [SerializeField] private int endPPU = 20;
    [SerializeField] private Transform firstCamLerpPos;
    private bool inEndCutscene = false;
    private SpriteRenderer bossSprite;
    [SerializeField] private float holdDistance = 5f;
    GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            DF = player.GetComponent<DruidFrameWork>();
            druidTransform = player.GetComponent<Transform>();
            druidAnimator = player.GetComponent<Animator>();
            DGF = player.GetComponent<DruidGrowFramework>();
            druidRig = player.GetComponent<Rigidbody2D>();
        }
        if (boss != null)
        {
            cliffMech = boss.GetComponent<CliffMech>();
            bossRig = boss.GetComponent<Rigidbody2D>();
            bossCollider = boss.GetComponent<BoxCollider2D>();
            bossSprite = boss.GetComponent<SpriteRenderer>();
        }
        ppc = Camera.main.GetComponent<PixelPerfectCamera>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (cutsceneCompleted) return;
        if (inCutscene) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            inCutscene = true;
            DruidFrameWork.inCutscene = true;
            CutsceneBars.Instance.CutsceneBarsStart();
            StartCoroutine(CliffCutsceneRoutine());
        }
    }

    public void MechEnd()
    {
        if (inEndCutscene) return;
        DruidFrameWork.inCutscene = true;
        druidAnimator.SetTrigger("Reset");
        DGF.DeGrowAllPlants();
        CutsceneBars.Instance.CutsceneBarsStart();
        StartCoroutine(CliffThrowCutscene());
    }

    private IEnumerator CliffThrowCutscene()
    {
        var druidDir = boss.transform.position.x - druidTransform.position.x;
        if (druidDir < 0)
        {
            bossSprite.flipX = false;
        }
        else bossSprite.flipX = true;
        float directionFacing = bossSprite.flipX? -1 : 1;

        while (Mathf.Abs(druidDir) > 0.1f)
        {
            druidDir = boss.transform.position.x - druidTransform.position.x;
            bossRig.linearVelocityX = cliffMech.movementSpeed * directionFacing;
            yield return null;
        }

        Debug.Log("finished Moving to Druid");
        bool pickedUp = false;
        bossRig.linearVelocity = Vector2.zero;
        while (Mathf.Abs(boss.transform.position.x - mechMovePos.position.x) > 0.1)
        {
            druidRig.bodyType = RigidbodyType2D.Static;
            if (pickedUp == false)
            {
                druidDir = boss.transform.position.x - druidTransform.position.x;
                if (druidDir < 0)
                {
                    bossSprite.flipX = false;
                }
                else bossSprite.flipX = true;
                directionFacing = bossSprite.flipX ? 1 : -1;
            }
            bossRig.linearVelocityX = cliffMech.movementSpeed * directionFacing;
            var offset = holdDistance * directionFacing;
            druidTransform.position = new Vector2(boss.transform.position.x + offset, boss.transform.position.y);
            pickedUp = true;
            yield return null;
        } 
        yield return new WaitForSeconds(1f);
        druidRig.bodyType = RigidbodyType2D.Dynamic;
        DruidFrameWork.inCutscene = false;
    }
    
    private IEnumerator CliffCutsceneRoutine()
    {
        druidAnimator.SetTrigger("Reset");

        DGF.DeGrowAllPlants();
        druidAnimator.SetFloat("XVelo", 1);
        druidAnimator.SetFloat("YVelo", 0);
        while (Vector2.Distance(druidTransform.position, firstMoveTo.position) > 0.1)
        {
            druidRig.linearVelocityX = DF.druidspeed;
            yield return null;
        }
        druidRig.linearVelocityX = 0;
        druidAnimator.SetFloat("XVelo", 0);
        ppc.assetsPPU = ppc.assetsPPU;
        ppc.enabled = false;
        float startOrtho = Camera.main.orthographicSize;
        float t = 0;
        FollowPlayer followPlayer = Camera.main.GetComponent<FollowPlayer>();

        var currentCamPos = Camera.main.transform.position;
        followPlayer.canFollow = false;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            float k = t / 0.75f;
            k = 1f - Mathf.Cos(k * Mathf.PI * 0.5f);
            Camera.main.orthographicSize = Mathf.Lerp(startOrtho, zoomedOutSize, k);
            float newX = Mathf.Lerp(currentCamPos.x, firstCamLerpPos.position.x, k);
            float newY = Mathf.Lerp(currentCamPos.y, firstCamLerpPos.position.y, k);

            Camera.main.transform.position = new Vector3(newX, newY, currentCamPos.z);

            yield return null;
        }
        t = 0;
        bossCollider.enabled = true;
        bossRig.bodyType = RigidbodyType2D.Dynamic;
        bossRig.gravityScale = 4;
        ppc.assetsPPU = endPPU;
        yield return new WaitForSeconds(1f);
        followPlayer.ScreenShake(0.1f, 0.5f);
        yield return new WaitForSeconds(2f);
        druidAnimator.SetTrigger("StaffSlam");
        currentCamPos = Camera.main.transform.position;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            float k = t / 0.75f;
            k = 1f - Mathf.Cos(k * Mathf.PI * 0.5f);
            Camera.main.orthographicSize = Mathf.Lerp(zoomedOutSize, startOrtho, k);
            float newX = Mathf.Lerp(currentCamPos.x, druidTransform.position.x, k);
            float newY = Mathf.Lerp(currentCamPos.y, druidTransform.position.y, k);

            Camera.main.transform.position = new Vector3(newX, newY, currentCamPos.z);

            yield return null;
        }
        ppc.assetsPPU = 32;
        ppc.enabled = true;
        followPlayer.canFollow = true;
        CutsceneBars.Instance.CutsceneBarsEnd();
        cutsceneCompleted = true;
        inCutscene = false;
        yield return new WaitForSeconds(1f);
        DruidFrameWork.inCutscene = false;
        cliffMech.enabled = true;
    }
}