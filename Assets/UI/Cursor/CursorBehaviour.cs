using UnityEngine;

public class CursorBehaviour : MonoBehaviour
{
    private Animator animator;
    private RectTransform rectTransform;

    private void Start()
    {
        animator = GetComponent<Animator>();
        Cursor.visible = false;
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector3 cursorPos = Input.mousePosition;
        rectTransform.position = new Vector3(cursorPos.x, cursorPos.y, 10);
        Cursor.visible = false;

        if (Input.GetMouseButton(0))
        {
            animator.SetTrigger("Click");
        }
        else
        {
            animator.SetTrigger("Release");
        }
    }
}