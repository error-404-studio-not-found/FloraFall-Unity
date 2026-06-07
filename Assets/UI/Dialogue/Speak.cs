using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class Speak : MonoBehaviour, IDialogue
{
    private bool interacting = false;
    public bool isInteracting => interacting;
    [SerializeField] private float typingSpeed = 0.1f;

    public string[] text;
    private Image dialogueBox;
    private TextMeshProUGUI textBox;
    private Animator dialogueAnimator;
    private TextMeshProUGUI npcName;
    [SerializeField] private float rangeOfSight = 12f;
    [SerializeField] private int reInteractIndex = 0;
    private int startPoint = 0;
    private SpriteRenderer NPCSprite;
    private Transform NPCTransform;
    [SerializeField] private string npcRevealName;
    [SerializeField] private int npcRevealLine;
    [SerializeField] private bool[] choices;
    [SerializeField] private bool shop = false;
    [SerializeField] private string leaveText;
    [SerializeField] private string stayText;
    private bool choiceMade = false;
    private int selectedChoice = 0;
    private Button choice1Button;
    private Button choice2Button;

    private Transform druidTransform;
    private Rigidbody2D druidRig;
    private Animator druidAnimator;

    private bool textOn = false;
    private bool skippedText = false;
    private bool canSkip = false;

    private Coroutine dialogueRoutine;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        NPCTransform = GetComponent<Transform>();
        NPCSprite = GetComponent<SpriteRenderer>();

      
        if (player != null)
        {
            druidAnimator = player.GetComponent<Animator>();
            druidRig = player.GetComponent<Rigidbody2D>();
            druidTransform = player.transform;
        }

        if (dialogueBox == null)
            dialogueBox = GameObject.FindGameObjectWithTag("DialogueBox").GetComponent<Image>();

        if (textBox == null)
            textBox = GameObject.FindGameObjectWithTag("DialogueText").GetComponent<TextMeshProUGUI>();

        if (dialogueAnimator == null)
            dialogueAnimator = GameObject.FindGameObjectWithTag("DialogueBox").GetComponent<Animator>();

        if (npcName == null)
            npcName = GameObject.FindGameObjectWithTag("DialogueName").GetComponent<TextMeshProUGUI>();

        if (choice1Button == null)
            choice1Button = GameObject.FindGameObjectWithTag("Choice1").GetComponent<Button>();

        if (choice2Button == null)
            choice2Button = GameObject.FindGameObjectWithTag("Choice2").GetComponent<Button>();

        choice1Button.onClick.AddListener(Choice1Pressed);
        choice2Button.onClick.AddListener(Choice2Pressed);

    }

    private void Update()
    {

        if (textOn == true && skippedText == false && canSkip)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                skippedText = true;
                canSkip = false;
            }
        }

        float flipDistance = Vector2.Distance(NPCTransform.position, druidTransform.position);

        if (flipDistance < rangeOfSight)
        {
            if (druidTransform.position.x < NPCTransform.position.x)
            {
                NPCSprite.flipX = true;

            } else if (druidTransform.position.x > NPCTransform.position.x)
            {
                NPCSprite.flipX = false;
            }
        } else if (flipDistance < rangeOfSight)
        {
            NPCSprite.flipX = false;
        }
    }

    public void Interact()
    {
        if (interacting == false)
        {
            interacting = true;
            dialogueRoutine = StartCoroutine(InteractingCoroutine());
        }
    }
    public void Choice1Pressed()
    {
        selectedChoice = 1;
        choiceMade = true;
    }

    public void Choice2Pressed()
    {
        selectedChoice = 2;
        choiceMade = true;
    }

    private void OnEnable()
    {
        if (dialogueBox == null)
            dialogueBox = GameObject.FindGameObjectWithTag("DialogueBox").GetComponent<Image>();

        if (textBox == null)
            textBox = GameObject.FindGameObjectWithTag("DialogueText").GetComponent<TextMeshProUGUI>();

        if (dialogueAnimator == null)
            dialogueAnimator = GameObject.FindGameObjectWithTag("DialogueBox").GetComponent<Animator>();

        if (npcName == null)
            npcName = GameObject.FindGameObjectWithTag("DialogueName").GetComponent<TextMeshProUGUI>();

        if (choice1Button == null)
            choice1Button = GameObject.FindGameObjectWithTag("Choice1").GetComponent<Button>();

        if (choice2Button == null)
            choice2Button = GameObject.FindGameObjectWithTag("Choice2").GetComponent<Button>();

        choice1Button.onClick.AddListener(Choice1Pressed);
        choice2Button.onClick.AddListener(Choice2Pressed);
    }

    private IEnumerator InteractingCoroutine()
    {
        DruidFrameWork.canjump = false;
        DruidFrameWork.canmove = false;
        dialogueAnimator.SetTrigger("Show");
        yield return new WaitForSeconds(0.5f);
        for (int i = startPoint; i < text.Length; i++)
        {
            druidRig.linearVelocity = new Vector2(0, 0);
            npcName.text = gameObject.name;

            druidAnimator.SetFloat("XVelo", 0f);
            textOn = true;
            dialogueBox.enabled = true;
            textBox.text = text[i];
            textBox.maxVisibleCharacters = 0;
          
            druidAnimator.SetFloat("XVelo", 0f);
            canSkip = true;
            for (int j = 0; j < textBox.text.Length; j++)
            {
                textBox.maxVisibleCharacters += 1;
                if (text[i][j] == '.' || text[i][j] == ',' || text[i][j] == '!' || text[i][j] == '?' || text[i][j] == '-')
                {
                    yield return new WaitForSeconds(typingSpeed + 0.5f);
                }
                else
                {
                    yield return new WaitForSeconds(typingSpeed);
                }

                if (skippedText == true)
                {
                    skippedText = false;
                    textBox.maxVisibleCharacters = text[i].Length;
                    break;
                }
            }
            if (i == npcRevealLine)
            {
                gameObject.name = npcRevealName;
                npcName.text = gameObject.name;
            }

            startPoint = reInteractIndex;

            if (choices[i] == true)
            {
                choice1Button.enabled = true;
                choice2Button.enabled = true;
                choice1Button.gameObject.GetComponent<Image>().enabled = true;
                choice2Button.gameObject.GetComponent<Image>().enabled = true;
                GameObject.FindGameObjectWithTag("Choice1Text").GetComponent<TextMeshProUGUI>().enabled = true;
                GameObject.FindGameObjectWithTag("Choice2Text").GetComponent<TextMeshProUGUI>().enabled = true;
                yield return new WaitUntil(() => choiceMade);
                choiceMade = false;
                choice1Button.enabled = false;
                choice2Button.enabled = false;
                choice1Button.gameObject.GetComponent<Image>().enabled = false;
                choice2Button.gameObject.GetComponent<Image>().enabled = false;
                GameObject.FindGameObjectWithTag("Choice1Text").GetComponent<TextMeshProUGUI>().enabled = false;
                GameObject.FindGameObjectWithTag("Choice2Text").GetComponent<TextMeshProUGUI>().enabled = false;

                if (selectedChoice == 1)
                {
                    if (shop)
                    {
                        break;
                    }
                    else
                    {
                        textBox.maxVisibleCharacters = 0;
                        textBox.text = stayText;
                        for (int j = 0; j < stayText.Length; j++)
                        {
                            textBox.maxVisibleCharacters += 1;
                            if (stayText[j] == '.' || stayText[j] == ',' || stayText[j] == '!' || stayText[j] == '?' || stayText[j] == '-')
                            {
                                yield return new WaitForSeconds(typingSpeed + 0.5f);
                            }
                            else
                            {
                                yield return new WaitForSeconds(typingSpeed);
                            }

                            if (skippedText == true)
                            {
                                skippedText = false;
                                textBox.maxVisibleCharacters = stayText.Length;
                                break;
                            }
                        }
                        break;
                    }
                }
                else if (selectedChoice == 2) 
                {
                    textBox.maxVisibleCharacters = 0;
                    textBox.text = leaveText;
                    for (int j = 0; j < leaveText.Length; j++)
                    {
                        textBox.maxVisibleCharacters += 1;
                        if (leaveText[j] == '.' || leaveText[j] == ',' || leaveText[j] == '!' || leaveText[j] == '?' || leaveText[j] == '-')
                        {
                            yield return new WaitForSeconds(typingSpeed + 0.5f);
                        }
                        else
                        {
                            yield return new WaitForSeconds(typingSpeed);
                        }

                        if (skippedText == true)
                        {
                            skippedText = false;
                            textBox.maxVisibleCharacters = leaveText.Length;
                            break;
                        }
                       
                    }
                    break;
                }
            
            }
            selectedChoice = 0;
            yield return new WaitForSeconds(1.5f);

            textBox.maxVisibleCharacters = 0;
        }
        selectedChoice = 0;
        DruidFrameWork.canjump = true;
        DruidFrameWork.canmove = true;
        textOn = false;
        yield return new WaitForSeconds(0.1f);
        skippedText = false;
      
        textBox.text = "";
        npcName.text = "";
        dialogueAnimator.SetTrigger("Leave");
        yield return new WaitForSeconds(1f);
        canSkip = false;
        dialogueBox.enabled = false;
        interacting = false;
    }

    private void OnDisable()
    {
        if (interacting)
        {
            if (dialogueRoutine != null)
            {
                StopCoroutine(dialogueRoutine);
            }

            ResetDialogue();
        } 
    }

    private void ResetDialogue()
    {
        Debug.Log("Dialogue Reset!");
        interacting = false;

        if (textBox != null)
        {
            textBox.text = "";
        }

        if (dialogueBox != null)
        {
            dialogueBox.enabled = false;
        }
    }
}