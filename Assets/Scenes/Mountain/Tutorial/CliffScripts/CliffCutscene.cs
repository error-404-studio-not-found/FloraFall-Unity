using System.Collections;
using TMPro;
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
    private DruidUI druidUI;
    private DruidFrameWork DF;
    [SerializeField] private GameObject boss;
    [SerializeField] private GameObject helicopter;
    [SerializeField] private GameObject wallhelicopter;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject dropOffPos;
    [SerializeField] private GameObject wallDropOffPos;
    [SerializeField] private GameObject finalMoveToPos;
    [SerializeField] private SceneField endSceneToTp;
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
    private TextMeshProUGUI saveText;
    private GameObject player;

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
            druidUI = player.GetComponent<DruidUI>();
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
        inEndCutscene = true;
        druidRig.linearVelocity = Vector2.zero;
        druidAnimator.SetTrigger("CutsceneDie");
        DGF.DeGrowAllPlants();
        CutsceneBars.Instance.CutsceneBarsStart();
        StartCoroutine(CliffBlackOut());
    }

    private IEnumerator CliffBlackOut()
    {
        DGF.DeGrowAllPlants();
        float t = 0;
        float startOrtho = Camera.main.orthographicSize;
        ppc.assetsPPU = ppc.assetsPPU;
        ppc.enabled = false;
        druidRig.constraints = RigidbodyConstraints2D.FreezeAll;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            Camera.main.orthographicSize = Mathf.Lerp(startOrtho, 2.5f, t / 0.5f);
            yield return null;
        }
        ppc.assetsPPU = 12;
        yield return new WaitForSeconds(2.5f);
        TransitionManager.Instance.transitions.SetTrigger("Start");
        yield return new WaitForSeconds(4f);
        saveText = GameObject.Find("SaveText").GetComponent<TextMeshProUGUI>();
        saveText.enabled = true;
        saveText.maxVisibleCharacters = 0;
        for (int i = 0; i < saveText.text.Length; i++)
        {
            saveText.maxVisibleCharacters += 1;
            if (saveText.text[i] == '.' || saveText.text[i] == ',' || saveText.text[i] == '!' || saveText.text[i] == '?' || saveText.text[i] == '-')
            {
                yield return new WaitForSeconds(0.35f);
            }
            else
            {
                yield return new WaitForSeconds(0.05f);
            }
        }

        yield return new WaitForSeconds(4f);
        Debug.Log("deletingText");
        for (int i = saveText.text.Length; i > 0; i--)
        {
            saveText.maxVisibleCharacters -= 1;

            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(1f);
        druidUI.hitImmune = false;
        DruidFrameWork.inCutscene = false;
        inEndCutscene = false;
        druidUI.health = 4;
        druidUI.spirits = 5;
        druidAnimator.SetTrigger("WakeUp");
        DruidFrameWork.canmove = true;
        DruidFrameWork.canjump = false;
        druidRig.constraints = RigidbodyConstraints2D.None;
        druidRig.constraints = RigidbodyConstraints2D.FreezeRotation;
        DF.ChangeParticleColours(new Color(20f / 255f, 77f / 255f, 1f / 255f, 1f));
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        StartCoroutine(ChunkLoader.Instance.TeleportPlayer(playerCollider, false, 0, endSceneToTp, "UpperSpawn", false));
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
        var startingHelicopterPosition = helicopter.transform.position;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            float k = t / 0.75f;
            helicopter.transform.position = new Vector3(Mathf.Lerp(startingHelicopterPosition.x, dropOffPos.transform.position.x, t / 1.5f), helicopter.transform.position.y);
            k = 1f - Mathf.Cos(k * Mathf.PI * 0.5f);

            Camera.main.orthographicSize = Mathf.Lerp(startOrtho, zoomedOutSize, k);
            float newX = Mathf.Lerp(currentCamPos.x, boss.transform.position.x, k);
            float newY = Mathf.Lerp(currentCamPos.y, boss.transform.position.y, k);

            Camera.main.transform.position = new Vector3(newX, newY, currentCamPos.z);

            yield return null;
        }
        t = 0;
        ppc.assetsPPU = endPPU;
        bossCollider.enabled = true;
        boss.transform.parent = null;
        StartCoroutine(HelicopterMove(helicopter));
        bossRig.bodyType = RigidbodyType2D.Dynamic;
        bossRig.gravityScale = 4;
        currentCamPos = Camera.main.transform.position;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            float k = t / 0.5f;
            float newX = Mathf.Lerp(currentCamPos.x, boss.transform.position.x, k);
            float newY = Mathf.Lerp(currentCamPos.y, boss.transform.position.y - 1.5f, k);
            Camera.main.transform.position = new Vector3(newX, newY, currentCamPos.z);
            yield return null;
        }
        t = 0;
        followPlayer.ScreenShake(0.1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        var wallHelicopterPos = wallhelicopter.transform.position;
        while (t < 2f)
        {
            t += Time.deltaTime;
            wallhelicopter.transform.position = new Vector3(Mathf.Lerp(wallHelicopterPos.x, wallDropOffPos.transform.position.x, t / 2f), wallhelicopter.transform.position.y);
            yield return null;
        }
        StartCoroutine(HelicopterMove(wallhelicopter));
        Rigidbody2D wallRig = wall.GetComponent<Rigidbody2D>();
        BoxCollider2D wallCollider = wall.GetComponent<BoxCollider2D>();
        wallCollider.enabled = true;
        wall.transform.parent = null;
        wallRig.gravityScale = 4f;
        t = 0;
        yield return new WaitForSeconds(0.5f);
        followPlayer.ScreenShake(0.1f, 0.4f);

        yield return new WaitForSeconds(1.6f);
        wallRig.constraints = RigidbodyConstraints2D.FreezeAll;
        druidAnimator.SetTrigger("StaffSlam");
        currentCamPos = Camera.main.transform.position;
        t = 0;
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

    private IEnumerator HelicopterMove(GameObject helicopter)
    {
        float t = 0;
        var helicopterStartingPos = helicopter.transform.position;
        while (t < 4.5f)
        {
            t += Time.deltaTime;
            helicopter.transform.position = new Vector2(Mathf.Lerp(helicopterStartingPos.x, finalMoveToPos.transform.position.x, t / 4.5f), helicopter.transform.position.y);
            yield return null;
        }
        Destroy(helicopter);
    }
}