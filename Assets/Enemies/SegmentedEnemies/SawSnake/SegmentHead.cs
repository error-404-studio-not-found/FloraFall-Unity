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
    [SerializeField] private float digTime = 2f;
    [SerializeField] private float moveSpeed = 10f;
    private GameObject player;
    private Rigidbody2D headRig;
    private Transform playerTransform;
    private bool digging = false;
    private bool dashing = false;
  

    private void Start()
    {
        StartUpSegment();
    }
    public void StartUpSegment()
    {
        lastRecordedPos = transform.position;
        path.Add(transform.position);
        player = GameObject.FindGameObjectWithTag("Player");
        headRig = GetComponent<Rigidbody2D>();

        if (player != null)
        {
            playerTransform = player.GetComponent<Transform>();
        }
    }

    private void FixedUpdate()
    {
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - headRig.position).normalized;
        float rotateAmount = Vector3.Cross(transform.right, directionToPlayer).z;
        headRig.angularVelocity = -rotateAmount * turnSpeed;
      
        if (!digging)
        {
            headRig.linearVelocity = transform.right * moveSpeed;
        }

        //---- DIG LOGIC ----
        RaycastHit2D digRay = Physics2D.Raycast(headRig.position, transform.right, 0.1f, LayerMask.GetMask("Ground")); 
        if (!digging && digRay && !dashing)
        {
            digging = true;
        } 
        if (digging)
        {
            if (playerTransform.position.y > transform.position.y)
            {
                if (headRig.linearVelocityY != 0)
                {
                    headRig.linearVelocityY -= 0.01f;
                }
            } else
            {
                if (headRig.linearVelocityY != 0)
                {
                    headRig.linearVelocityY -= 0.01f;
                }
            }
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
     


       
        
        //---- PATROL LOGIC ----

    }

    //---- DASH LOGIC ----
   

    private void ChangeSortingOrder(int order)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            SpriteRenderer segmentSprite = segments[i].GetComponent<SpriteRenderer>();
            segmentSprite.sortingOrder = order;
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
        var newRig = headOfNewPack.AddComponent<Rigidbody2D>();
        newRig.gravityScale = 0;
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