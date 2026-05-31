using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class ChunkLoader : MonoBehaviour
{
    /* CHUNKLOADER
     * This script handles moving between scenes via unloading and reloading
     * This script is a singleton
     * This script is persistent
     */
    public static ChunkLoader Instance { get; private set; }

    private string currentChunk;
    Camera cam;
    private FollowPlayer camFollow;
    private Animator fade;
    PixelPerfectCamera ppc;

    /* AWAKE
     * Handles persistence
     */

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /* Start
     * Sets the current chunk to the currently loaded scene on start up
     */

    private void Start()
    {
        cam = Camera.main;
        camFollow = cam.GetComponent<FollowPlayer>();
        ppc = cam.GetComponent<PixelPerfectCamera>();
        if (TransitionManager.Instance != null)
        {
            fade = TransitionManager.Instance.transitions;
        }
        currentChunk = SceneManager.GetActiveScene().name;
    }

    //Enter chunk starts a courotine which takes a scene name and loads it

    public void EnterChunk(string sceneName, System.Action onChunkLoaded = null)
    {
        StartCoroutine(LoadAndUnload(sceneName, onChunkLoaded));
    }

    /* LOAD AND UNLOAD
     * Checks if it's loading the current scene if it forces reload
     * If its not it loads the scene and while its loading it waits frames until it's done loading
     * Checks if the sceneName is empty and then loads the starting scene via current chunk
     * Sets the current chunk to the now loaded scene if all is good
     */

    private IEnumerator LoadAndUnload(string sceneName, System.Action onChunkLoaded = null)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;
        Debug.Log("Unloaded" + sceneName);

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        while (!newScene.isLoaded)
            yield return null;

        SceneManager.SetActiveScene(newScene);

        yield return null;

        if (AstarPath.active != null)
        {
            AstarPath.active.Scan();
        }

        onChunkLoaded?.Invoke();

        if (!string.IsNullOrEmpty(currentChunk))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentChunk);
            while (!unloadOp.isDone)
                yield return null;
        }

        Debug.Log("Chunkloader Loaded scene " + sceneName + ", unloaded " + currentChunk + ".");

        currentChunk = sceneName;
    }

    /* TELEPORT LOGIC
     * TeleportPlayer requires 5 parameters for the spawn point, player, cellingdoor, upwardsjumpforce, and targetChunk
     * This coroutine loads a chunk and unloads the previous one teleporting the player to the set spawn point in the doorscripts inspector
     */

    public IEnumerator TeleportPlayer(Collider2D player, bool cellingDoor, float upwardsJumpForce, SceneField targetChunk, string targetSpawnID)
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
            yield break;
        }

        EnterChunk(targetChunk.SceneName);

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
            fade.SetTrigger("End");
            player.transform.position = spawnPoint.position;
            camFollow.SnapToTarget();
            Debug.Log("TimeScale = " + Time.timeScale);
            Debug.Log("Spawned Druid");
            playerRig.linearVelocity = new Vector2(0, 0);
            StartCoroutine(SpawnRoutine());
        }
        else
        {
            Debug.LogWarning(spawnPoint + " Not Found!");
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(0.75f);
        Debug.Log("FinishedSpawnm");
        DruidFrameWork.Transitioning = false;
        DruidFrameWork.canmove = true;

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