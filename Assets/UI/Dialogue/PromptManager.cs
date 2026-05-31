using UnityEngine;

public class PromptManager : MonoBehaviour
{

    private Transform druidTransform;
    [SerializeField] private Vector2 promptCheckSize = new Vector2(2, 2);
    [SerializeField] private GameObject promptShower;
    private bool showingPrompt = false;
    private GameObject promptShowClone;

    void Start()
    {
        druidTransform = GetComponent<Transform>();
    }

    void Update()
    {
        druidTransform.position = transform.position;

        Collider2D promptCheck = Physics2D.OverlapBox(druidTransform.position, promptCheckSize, 0f, LayerMask.GetMask("Dialogue"));

        if (promptCheck != null)
        {
            if (showingPrompt == false)
            {
                Transform dialogueTransform = promptCheck.gameObject.GetComponent<Transform>();
                if (dialogueTransform != null)
                {
                    showingPrompt = true;
                    promptShowClone = Instantiate(promptShower, dialogueTransform);
                    promptShowClone.SetActive(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                IDialogue dialogue = promptCheck.gameObject.GetComponent<IDialogue>();
                if (dialogue != null)
                {
                    if (dialogue.isInteracting == false && DruidFrameWork.canmove)
                    {
                        dialogue.Interact();
                        Debug.Log("interacting with " + promptCheck.gameObject.name);
                    }
                }
                else
                {
                    DoorScript doorScript = promptCheck.gameObject.GetComponent<DoorScript>();
                    if (doorScript != null)
                    {
                        Collider2D playerCollider = druidTransform.gameObject.GetComponent<Collider2D>();
                       doorScript.TeleportPlayer(playerCollider);
                    }

                }
            }
        }
        else
        {
            showingPrompt = false;
            Destroy(promptShowClone);  
        }
    }
}
