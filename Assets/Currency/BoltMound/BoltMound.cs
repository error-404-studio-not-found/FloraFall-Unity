using System.Collections;
using UnityEngine;

public class BoltMound : MonoBehaviour, IPurify
{
    public float PurifyAmount => 0.05f;
    bool isPurified = false;
    public bool IsPurified => isPurified;
    [SerializeField] private Transform[] boltsToSpawnPositions;
    [SerializeField] private GameObject boltToSpawn;
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(PurifyRoutine());
    }

    public void Purify()
    {
        if (!isPurified)
        {
            isPurified = true;
            StartCoroutine(PurifyRoutine());
        }
    }

    private IEnumerator PurifyRoutine()
    {
        yield return new WaitForSeconds(2);
        animator.SetTrigger("Purify");
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < boltsToSpawnPositions.Length; i++) 
        {
            var boltClone = Instantiate(boltToSpawn);
            boltClone.SetActive(true);
            boltClone.transform.position = boltsToSpawnPositions[i].position;
        }
    }
}
