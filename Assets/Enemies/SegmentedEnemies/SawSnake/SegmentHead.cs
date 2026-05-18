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
    [SerializeField] private float dashCooldown = 5f;
    [SerializeField] private int maxPathLength = 500;
    [SerializeField] private float digDistance = 5f;
    [SerializeField] private float digTime = 2f;
    [SerializeField] private float moveSpeed = 10f;
    private bool canDash = true;
    private GameObject player;
    private Transform playerTransform;
    private bool digging = false;
    private bool dashing = false;
    private Vector2 currentMoveDir;

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
                    segments[i].transform.position = Vector2.Lerp(segments[i].transform.position,path[pathIndex], 15f * Time.deltaTime);
                    if (i != 0)
                    {
                        segments[i].transform.rotation = Quaternion.Lerp(segments[i].transform.rotation, segments[0].transform.rotation, 15f * Time.deltaTime);
                    }
                }
            }
        }

        //---- BEHAVIOUR ----
        if (Vector2.Distance(transform.position, playerTransform.position) <= detectionRange)
        {
            //---- DASH ----
            if (canDash && !digging && !dashing)
            {
                Vector2 playerPosToDashTo = playerTransform.position;
                StartCoroutine(DashRoutine(playerPosToDashTo));
            }
        }


        //---- DIG LOGIC ----
        RaycastHit2D digRay = Physics2D.Raycast(transform.position, (Vector2)transform.position + currentMoveDir, 1, LayerMask.GetMask("Ground"));
        if (digRay && !dashing && !digging)
        {
            Debug.Log("Digging");
            StartCoroutine(Dig());
        }
        //---- PATROL LOGIC ----

    }

    //---- DASH LOGIC ----
    private IEnumerator DashRoutine(Vector2 posToDash)
    {
        Vector2 dashDirection = (posToDash - (Vector2)transform.position).normalized;
        canDash = false;
        dashing = true;
        var startPos = transform.position;
        float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        currentMoveDir = dashDirection;

        while (Vector2.Distance(transform.position, posToDash) >= 0.5f)
        {
            yield return null;
            transform.position = Vector2.MoveTowards(transform.position, posToDash, moveSpeed * Time.deltaTime);
        }

        RaycastHit2D groundRay = Physics2D.Raycast(startPos, dashDirection, 100f, LayerMask.GetMask("Ground"));
        if (groundRay)
        {
            while (Vector2.Distance(transform.position, groundRay.point) >= 0.5)
            {
                yield return null;
                transform.position = Vector2.MoveTowards(transform.position, groundRay.point, moveSpeed * Time.deltaTime);
            }
            dashing = false;
        } else
        {
            dashing = false;
        }
        yield return new WaitForSeconds(dashCooldown);
        canDash = true; 
    }

    private void ChangeSortingOrder(int order)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            SpriteRenderer segmentSprite = segments[i].GetComponent<SpriteRenderer>();
            segmentSprite.sortingOrder = order;
        }
    }

    private IEnumerator Dig()
    {
        digging = true;
        Debug.Log("Digging");
        ChangeSortingOrder(-1);
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + currentMoveDir * digDistance;

        while (Vector2.Distance(transform.position, targetPos) >= 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(digTime);
        transform.position = new Vector2(playerTransform.position.x, transform.position.y);
        digging = false;
        if (canDash)
        {
            StartCoroutine(DashRoutine(playerTransform.position));
        } else
        {

        }
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