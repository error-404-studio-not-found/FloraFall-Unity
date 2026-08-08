using UnityEngine;

public class ClimbingWals : MonoBehaviour
{
    private bool grabbedOn = false;

    private void Start()
    {
    }

    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!grabbedOn)
            {
                grabbedOn = true;
                DruidFrameWork.climbing = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (grabbedOn)
            {
                grabbedOn = false;
                DruidFrameWork.climbing = false;
            }
        }
    }
}