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
    [SerializeField] private float jumpForce = 3;

    private bool combatState = false;
    private bool dialogueState = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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

        // Player movement and camera control
        PlayerMovement();
        LookAround();

        // Jump logic
        if (GroundCheck() && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    void UpdateCursorState()
    {
        if (combatState)
        {
            // If in combat, cursor is visible and unlocked for menu access
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (dialogueState && dialogueUI.IsOpen)
        {
            // If interacting with NPC and dialogue UI is open
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Default exploration mode: lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    }

    public void ExitCombat()
    {
        // Disable combat state
        combatState = false;
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
        // Check if the player steps on the tile with the "Combat" tag
        // Debug script lines, delete when scen shift is implemented for dialogue system
        if (other.CompareTag("Combat"))
        {
            combatState = true;
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

