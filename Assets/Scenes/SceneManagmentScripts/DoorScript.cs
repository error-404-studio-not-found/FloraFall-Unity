using System.Collections;

using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class DoorScript : MonoBehaviour
{
    public SceneField targetChunk;

    private Animator fade;
    public string targetSpawnID;
    private FollowPlayer camFollow;
    [SerializeField] private bool interactDoor = false;
    [SerializeField] private bool cellingDoor = false;
    [SerializeField] private float upwardsJumpForce = 3f;
    Camera cam;
    PixelPerfectCamera ppc;

    private void Start()
    {
        cam = Camera.main;
        camFollow = cam.GetComponent<FollowPlayer>();
        ppc = cam.GetComponent<PixelPerfectCamera>();
        if (TransitionManager.Instance != null)
        {
            fade = TransitionManager.Instance.transitions;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (interactDoor) return;
        if (!other.CompareTag("Player")) return;

        if (DruidFrameWork.Transitioning) return;
        StartCoroutine(TeleportPlayer(other));
    }

    public IEnumerator TeleportPlayer(Collider2D player)
    {
        DruidFrameWork.Transitioning = true;
        fade.SetTrigger("Start");
        DruidUI UI = player.GetComponent<DruidUI>();
        DruidFrameWork.canmove = false;
        Rigidbody2D playerRig = player.GetComponent<Rigidbody2D>();
        if (cellingDoor)
        {
            playerRig.AddForceY(upwardsJumpForce, ForceMode2D.Impulse);
        }

        yield return new WaitUntil(() =>
        fade.GetCurrentAnimatorStateInfo(0).IsName("CircleWipeExposed"));

        DruidFrameWork druid = player.GetComponent<DruidFrameWork>();

        if (druid != null)
        {
            if (!DruidFrameWork.isTransformed)
            {
                UI.spirits = UI.maxSpirits;
            }
        }
        else
        {
            Debug.LogWarning("no framework lol");
        }

        ChunkLoader.Instance.EnterChunk(targetChunk.SceneName);

        Scene targetScene = SceneManager.GetSceneByName(targetChunk.SceneName);
        while (!targetScene.isLoaded)
            yield return null;

        Transform spawnPoint = null;
        ppc.assetsPPU = 32;
        cam.orthographicSize = 4.5f;
        ppc.enabled = true;
        foreach (GameObject root in targetScene.GetRootGameObjects())
        {
            spawnPoint = FindSpawnRecursively(root.transform, targetSpawnID);
            if (spawnPoint != null) break;
        }

        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
            
            playerRig.linearVelocity = new Vector2(0, 0);
            fade.SetTrigger("End");
            DruidFrameWork.Transitioning = false;
            DruidFrameWork.canmove = true;
            camFollow.SnapToTarget();
        }
        else
        {
            Debug.LogWarning(spawnPoint + " Not Found!");
        }
    }

    private Transform FindSpawnRecursively(Transform parent, string name)
    {
        if (parent.name == name) return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindSpawnRecursively(child, name);
            if (result != null) return result;
        }
        return null;
    }
}