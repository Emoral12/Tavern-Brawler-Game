using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;

    public DialogueUI DialogueUI => dialogueUI;

    public IInteractable Interactable { get; set; }

    private float xforce;
    private float zforce;

    private Vector3 playerRot;
    private Vector3 cameraRot;

    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private float lookSpeed = 2;
    [SerializeField] private GameObject cam;

    private Rigidbody rb;

    [SerializeField] private Vector3 boxSize;
    [SerializeField] private float maxDistance;
    [SerializeField] private LayerMask layerMask;

    private bool combatState = false;
    private bool dialogueState = false;
    private string currentTriggerTag = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Check if we're in a combat scene (indices 2, 3, 4, 5)
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == 2 || currentScene == 3 || currentScene == 4 || currentScene == 5)
        {
            EnterCombat(); // Automatically set combat state in combat scenes
        }
        UpdateCursorState(); // Initialize cursor state based on starting conditions
    }

    void Update()
    {
        // Update cursor state dynamically based on game state
        UpdateCursorState();

        // Skip movement and look logic if the dialogue UI is open
        if (dialogueUI.IsOpen) return;

        // Handle interaction with NPCs
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Interactable != null)
            {
                Interactable.Interact(this);
                dialogueState = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentTriggerTag != null)
            {
                Debug.Log("C pressed while in trigger with tag: " + currentTriggerTag);
                LoadSceneBasedOnTag(currentTriggerTag);
            }
            else
            {
                Debug.Log("C pressed but not inside any trigger");
            }
        }

        // Player movement and camera control
        PlayerMovement();
        LookAround();
    }

    void LoadSceneBasedOnTag(string tag)
    {
        switch (tag)
        {
            case "Brawler":
                SceneManager.LoadScene(2);
                EnterCombat(); // Set combat state before loading scene
                break;
            case "Commoner":
                SceneManager.LoadScene(3);
                EnterCombat(); // Set combat state before loading scene
                break;
            case "Mage":
                SceneManager.LoadScene(4);
                EnterCombat(); // Set combat state before loading scene
                break;
            case "Child":
                SceneManager.LoadScene(5);
                EnterCombat(); // Set combat state before loading scene
                break;
            default:
                Debug.LogWarning("Unknown trigger tag: " + tag);
                break;
        }
    }

    void UpdateCursorState()
    {
        if (combatState)
        {
            // If in combat, cursor is visible and unlocked for menu access
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Combat state: Cursor unlocked and visible"); // Debug to confirm
            return;
        }

        if (dialogueState && dialogueUI.IsOpen)
        {
            // If interacting with NPC and dialogue UI is open
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Dialogue state: Cursor unlocked and visible"); // Debug to confirm
            return;
        }

        // Default exploration mode: lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Default state: Cursor locked and hidden"); // Debug to confirm
    }

    public void EndDialogue()
    {
        // Transition out of dialogue state and reset cursor behavior
        dialogueState = false;
        dialogueUI.Close(); // Ensure dialogue UI closes
    }

    public void EnterCombat()
    {
        // Enable combat state
        combatState = true;
        Debug.Log("Entered combat state"); // Debug to confirm
    }

    public void ExitCombat()
    {
        // Disable combat state
        combatState = false;
        Debug.Log("Exited combat state"); // Debug to confirm
    }

    void LookAround()
    {
        // Handles camera rotation according to mouse inputs and player transform
        cameraRot = cam.transform.rotation.eulerAngles;
        cameraRot.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        cameraRot.x = Mathf.Clamp((cameraRot.x <= 180) ? cameraRot.x : -(360 - cameraRot.x), -80f, 80f);
        cam.transform.rotation = Quaternion.Euler(cameraRot);
        playerRot.y = Input.GetAxis("Mouse X") * lookSpeed;
        transform.Rotate(playerRot);
    }

    void PlayerMovement()
    {
        // Basic movement inputs for navigation
        xforce = Input.GetAxis("Horizontal") * moveSpeed;
        zforce = Input.GetAxis("Vertical") * moveSpeed;
        rb.velocity = transform.forward * zforce + transform.right * xforce + transform.up * rb.velocity.y;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered trigger with tag: " + other.tag);

        if (other.CompareTag("Brawler") || other.CompareTag("Commoner") ||
            other.CompareTag("Mage") || other.CompareTag("Child"))
        {
            currentTriggerTag = other.tag;
            Debug.Log("Current trigger tag set to: " + currentTriggerTag);
        }
    }

    void OnDrawGizmos()
    {
        // Visualize ground check box in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
    }

    bool GroundCheck()
    {
        // Checks if the player is grounded using a box cast
        return Physics.BoxCast(transform.position, boxSize, -transform.up, transform.rotation, maxDistance, layerMask);
    }
}