using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SegmentHead : MonoBehaviour
{
    public List<Segment> segments = new List<Segment>();
    public List<EnemyDamage> segmentHealth = new List<EnemyDamage>();
    private List<Vector2> path = new List<Vector2>();
    private float segmentSpacing = 1.5f;
    private float moveDistForRecord = 0.2f;
    private Vector2 lastRecordedPos;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float turnSpeed = 200f;
    [SerializeField] private float dashCooldown = 5f;
    [SerializeField] private int maxPathLength = 500;
    [SerializeField] private float digDistance = 5f;
    [SerializeField] private float digTime = 4f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float heightOffset = 4f;
    private GameObject player;
    private Transform playerTransform;
    private bool jumping = false;
    private bool canDig = true;
    private bool active = false;

    private void Start()
    {
        StartUpSegment();
    }

    public void StartUpSegment()
    {
        lastRecordedPos = transform.position;
        path.Add(transform.position);
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.GetComponent<Transform>();
        }
    }

    private void Update()
    {
        if (Vector2.Distance(playerTransform.position, transform.position) <= detectionRange)
        {
            if (active != true)
            {
                StartCoroutine(Jump());
                active = true;
            }
        }
        ;

        if (active)
        {
            if (Vector2.Distance(transform.position, lastRecordedPos) >= moveDistForRecord)
            {
                path.Insert(0, transform.position);
                lastRecordedPos = transform.position;
            }

            //---- PERFORMANCE ----
            if (path.Count >= maxPathLength)
            {
                path.RemoveAt(path.Count - 1);
            }

            //---- SEGMENT FOLLOW ----
            for (int i = 0; i < segments.Count; i++)
            {
                int pathIndex = Mathf.RoundToInt((i * segmentSpacing) / moveDistForRecord);
                segments[i].segmentPosition = i;
                if (pathIndex < path.Count)
                {
                    if (segments[i].currentOwner == this)
                    {
                        segments[i].transform.position = Vector2.Lerp(segments[i].transform.position, path[pathIndex], 15f * Time.deltaTime);
                        if (i != 0)
                        {
                            segments[i].transform.rotation = Quaternion.Lerp(segments[i].transform.rotation, segments[0].transform.rotation, 15f * Time.deltaTime);
                        }
                    }
                }
            }

            //---- BEHAVIOUR ----
            RaycastHit2D digRay = Physics2D.Raycast(transform.position, transform.right, 1f, LayerMask.GetMask("Ground"));
            if (digRay && !canDig && !jumping)
            {
                Debug.Log("Digging");
                StartCoroutine(Dig());
            }

            //---- PATROL LOGIC ----
        }
    }

    //---- DASH LOGIC ----

    private IEnumerator Dig()
    {
        canDig = false;
        Debug.Log("Digging");
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (Vector2)transform.right * digDistance;

        float t = 0;
        while (Vector2.Distance(transform.position, targetPos) >= 0.5f)
        {
            t += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0, 0, 0);
        yield return new WaitForSeconds(digTime);
        canDig = true;
        StartCoroutine(Jump());
    }

    private IEnumerator Jump()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        Vector2 posToJumpTo = playerTransform.position + new Vector3(0, heightOffset, 0);
        Vector2 startPos = transform.position;
        jumping = true;
        Vector2 endPos = new Vector2((2 * posToJumpTo.x) - startPos.x, startPos.y);

        float totalDist = endPos.x - startPos.x;
        while (Vector2.Distance(transform.position, endPos) >= 0.05f)
        {
            float xPos = Mathf.MoveTowards(transform.position.x, endPos.x, moveSpeed * Time.deltaTime);

            float t = Mathf.Clamp01((xPos - startPos.x) / totalDist);
            float yPos = startPos.y + 4 * (posToJumpTo.y - startPos.y) * t * (1 - t);

            Vector2 finalPos = new Vector2(xPos, yPos);
            Vector2 faceDirection = (finalPos - (Vector2)transform.position).normalized;
            Quaternion angle = Quaternion.Euler(0, 0, Mathf.Atan2(faceDirection.y, faceDirection.x) * Mathf.Rad2Deg);

            transform.rotation = Quaternion.Lerp(transform.rotation, angle, turnSpeed * Time.deltaTime);
            transform.position = finalPos;
            yield return null;
        }
        Debug.Log("FinishedJump");
        jumping = false;
        transform.position = endPos;
        StartCoroutine(Dig());
    }

    //---- SPLITTING LOGIC ----

    /* Void Split
     * Split is a helper function that allows the segmented enemy to split
     * Split will destroy the segment at split pos and separate the entire segemented enemy into multiple parts
     * If there is no segments after the splitpos, it will simply destroy the segment
     */

    public void Split(int splitPos)
    {
        if (splitPos < 0 || splitPos >= segments.Count)
        {
            Debug.LogError("Invalid split position");
            return;
        }

        Debug.Log("Split at " + splitPos);
        Segment segmentToBeDestroyed = segments[splitPos];
        int newHeadIndex = splitPos + 1;

        if (newHeadIndex >= segments.Count)
        {
            Destroy(segmentToBeDestroyed.gameObject);
            segments.RemoveAt(splitPos);
            return;
        }

        var headOfNewPack = segments[newHeadIndex].gameObject;

        var newHead = headOfNewPack.AddComponent<SegmentHead>();
        newHead.StartUpSegment();

        for (int i = newHeadIndex; i < segments.Count; i++)
        {
            segments[i].currentOwner = newHead;
            newHead.segments.Add(segments[i]);
            newHead.segmentHealth.Add(segments[i].gameObject.GetComponent<EnemyDamage>());
        }

        for (int i = segments.Count - 1; i >= newHeadIndex; i--)
        {
            segmentHealth.RemoveAt(i);
            segments.RemoveAt(i);
        }
        Destroy(segments[splitPos].gameObject);
        segments.Remove(segments[splitPos]);
    }
}