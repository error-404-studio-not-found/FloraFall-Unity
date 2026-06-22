using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

public class FollowPlayer : MonoBehaviour
{
    public static Transform target;
    public GameObject Maincharacter;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    private float camHalfWidth;
    private float camHalfHeight;
    private Vector3 velocity = Vector3.zero;
    public float smoothTime = 0.25f;
    private bool snapThisFrame = false;
    private Camera cam;
    private SpriteRenderer druidSprite;
    private Rigidbody2D druidRig;
    [SerializeField] private float lookAheadOffset = 2;
    private Vector2 currentOffset;
    private Vector2 offsetVelocity;
    [SerializeField] private float offsetSmoothTime = 0.2f;
    private float shakeDuration;
    private float shakeTimer;
    private float shakeMagnitude;
    private float shakeFrequency = 25f;
    public bool canFollow = true;

    private Vector3 shakeOffset;
    private float shakeSeed;

    private void Start()
    {
        target = Maincharacter.transform;

        cam = GetComponent<Camera>();

        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
        druidSprite = Maincharacter.GetComponent<SpriteRenderer>();
        druidRig = Maincharacter.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (canFollow)
        {
            var druidDir = druidSprite.flipX ? -1f : 1f;

            //LOOK AHEAD
            Vector2 offset = Vector2.zero;
            if (druidRig.linearVelocityX > 0.1 || druidRig.linearVelocityX < -0.1)
            {
                offset.x = lookAheadOffset * druidDir;
            }
            else if (druidRig.linearVelocityX == 0)
            {
                offset.x = 0;
            }

            float verticalChangeThreshold = 1f;
            if (druidRig.linearVelocityY > verticalChangeThreshold)
            {
                offset.y = lookAheadOffset;
            }
            else if (druidRig.linearVelocityY < -verticalChangeThreshold)
            {
                offset.y = -lookAheadOffset;
            }
            else offset.y = 0;

            currentOffset = Vector2.SmoothDamp(currentOffset, offset, ref offsetVelocity, offsetSmoothTime);
            target = Maincharacter.transform;
            Vector3 newpos = new Vector3(target.position.x + currentOffset.x, target.position.y + currentOffset.y, -10);

            float clampedX = Mathf.Clamp(newpos.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
            float clampedY = Mathf.Clamp(newpos.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);
            Vector3 clampedTarget = new Vector3(clampedX, clampedY, -10);

            if (snapThisFrame)
            {
                transform.position = clampedTarget;

                if (Vector3.Distance(transform.position, clampedTarget) < 0.001f)
                {
                    snapThisFrame = false;
                }
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, clampedTarget, ref velocity, smoothTime);
            }
        }

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            float progress = 1f - (shakeTimer / shakeDuration);

            float damper = 1f - Mathf.SmoothStep(0f, 1f, progress);

            float x = (Mathf.PerlinNoise(shakeSeed, Time.time * shakeFrequency) * 2f - 1f) * shakeMagnitude * damper;
            float y = (Mathf.PerlinNoise(shakeSeed + 1f, Time.time * shakeFrequency) * 2f - 1f) * shakeMagnitude * damper;

            shakeOffset = new Vector3(x, y, 0);
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
        transform.position += shakeOffset;
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
    }

    public void ScreenShake(float amount, float duration)
    {
        shakeMagnitude = amount;
        shakeDuration = duration;
        shakeTimer = duration;

        shakeSeed = Random.Range(0f, 1000f);
    }

    public void SnapToTarget() //call to snap to Target
    {
        Vector3 newPos = new Vector3(target.position.x, target.position.y, -10);
        float clampedX = Mathf.Clamp(newPos.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
        float clampedY = Mathf.Clamp(newPos.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);
        transform.position = new Vector3(clampedX, clampedY, -10);

        snapThisFrame = true;
        velocity = Vector3.zero;
    }
}