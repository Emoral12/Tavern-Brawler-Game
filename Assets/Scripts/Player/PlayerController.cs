using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;

    public DialogueUI DialogueUI => dialogueUI;

    public IInteractable Interactable { get; set; }

    float xforce;
    float zforce;

    Vector3 playerRot;
    Vector3 cameraRot;

    [SerializeField] float moveSpeed = 2;
    [SerializeField] float lookSpeed = 2;
    [SerializeField] GameObject cam;

    Rigidbody rb;

    [SerializeField] Vector3 boxSize;
    [SerializeField] float maxDistance;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float jumpForce = 3;

    // private bool bartkeepingState = true; optimize later for in-game no cursor until dialogue initiated
    private bool combatState = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (dialogueUI.isOpen) return;


        if (Input.GetKeyDown(KeyCode.E))
        {
            Interactable?.Interact(this);
        }

        PlayerMovement();
        LookAround();

        if (GroundCheck() && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(transform.up * jumpForce);
        }

        

        if (combatState == true) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LookAround()
    {
        cameraRot = cam.transform.rotation.eulerAngles;
        cameraRot.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        cameraRot.x = Mathf.Clamp((cameraRot.x <= 180) ? cameraRot.x : -(360 - cameraRot.x), -80f, 80f);
        cam.transform.rotation = Quaternion.Euler(cameraRot);
        playerRot.y = Input.GetAxis("Mouse X") * lookSpeed;
        transform.Rotate(playerRot);
    }

    void PlayerMovement()
    {
        xforce = Input.GetAxis("Horizontal") * moveSpeed;
        zforce = Input.GetAxis("Vertical") * moveSpeed;
        rb.velocity = transform.forward * zforce + transform.right * xforce + transform.up * rb.velocity.y;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
    }

    bool GroundCheck()
    {
        return Physics.BoxCast(transform.position, boxSize, -transform.up, transform.rotation, maxDistance, layerMask);
    }
}
