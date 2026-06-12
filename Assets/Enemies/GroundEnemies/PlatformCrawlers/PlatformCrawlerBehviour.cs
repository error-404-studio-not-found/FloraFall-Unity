using Unity.Mathematics;
using UnityEngine;

public class PlatformCrawlerBehviour : MonoBehaviour
{
    Rigidbody2D crawlerRig;
    Quaternion currentRotation;
    [SerializeField] private float rayDists = 1f;

    void Start()
    {
       crawlerRig = GetComponent<Rigidbody2D>(); 
       currentRotation = transform.rotation;
    }

   
    void Update()
    {
        if (currentRotation == Quaternion.identity)
        {
            RaycastHit2D downRay = Physics2D.Raycast(transform.position, Vector2.down, rayDists, LayerMask.GetMask("Ground"));
            if (!downRay)
            {
                currentRotation = quaternion.Euler(0, 0, 90);
                transform.rotation = currentRotation;
            }
        }
    }
}
