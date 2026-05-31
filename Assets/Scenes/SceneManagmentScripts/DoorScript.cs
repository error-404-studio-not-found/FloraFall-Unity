using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class DoorScript : MonoBehaviour
{
    public SceneField targetChunk;
    public string targetSpawnID;
  
    [SerializeField] private bool interactDoor = false;
    [SerializeField] private bool cellingDoor = false;
    [SerializeField] private float upwardsJumpForce = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (interactDoor) return;
        if (!other.CompareTag("Player")) return;

        if (DruidFrameWork.Transitioning) return;
        TeleportPlayer(other);
    }

    public void TeleportPlayer(Collider2D player)
    {
        Debug.Log("Teleporting");
        StartCoroutine(ChunkLoader.Instance.TeleportPlayer(player, cellingDoor, upwardsJumpForce, targetChunk, targetSpawnID));
    }
}