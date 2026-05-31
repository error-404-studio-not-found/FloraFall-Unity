using System.Collections;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class SceneTP : MonoBehaviour
{
    [SerializeField] private TMP_InputField sceneName;
    [SerializeField] private TMP_InputField spawnName;
    private string sceneTPName;
    private string spawnTPName;
    GameObject player;

    void Start()
    {
        sceneName.onEndEdit.AddListener(OnSceneSubmitted);
        spawnName.onEndEdit.AddListener(OnSpawnSubmitted);
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (sceneTPName != null && spawnTPName != null)
        {
            StartCoroutine(TP(spawnTPName));
            sceneTPName = null;
            spawnTPName = null;
            
        }
    }

    private IEnumerator TP(string spawn)
    {
        ChunkLoader.Instance.EnterChunk(sceneTPName);
        yield return null;
        yield return null;
        yield return null;
        var spawnPoint = GameObject.Find(spawn).transform;
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
        }
        else
        {
            Debug.LogWarning("No spawnPoint found in scene!");
        }
    }
    private void OnSceneSubmitted(string text)
    {
        sceneTPName = text;
    }

    private void OnSpawnSubmitted(string text)
    {
        spawnTPName = text;
    }

}
