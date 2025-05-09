using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag = 5f; // Not used with CharacterController but retained for feel

    public float jumpForce = 10f;
    public float jumpCooldown = 0.5f;
    public float airMultiplier = 0.6f;
    private bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    private bool grounded;

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private float yVelocity;

    private CharacterController controller;

    [Header("Slope Check")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprint,
        air
    }

    [HideInInspector] public TextMeshProUGUI text_speed;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        readyToJump = true;
    }

    private void Update()
    {
        // Ground check using CharacterController
        grounded = controller.isGrounded;

        MyInput();
        StateHandler();

        // Gravity
        if (grounded && yVelocity < 0)
        {
            yVelocity = -2f; // Small downward force to stay grounded
        }
        else
        {
            yVelocity += Physics.gravity.y * Time.deltaTime;
        }

        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection = moveDirection.normalized;

        Vector3 move = moveDirection * moveSpeed;

        // Add vertical velocity (gravity/jump)
        move.y = yVelocity;

        // Apply movement
        controller.Move(move * Time.deltaTime);
    }

    private void StateHandler()
    {
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprint;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
            moveSpeed = walkSpeed * airMultiplier; // slower in air
        }
    }

    private void Jump()
    {
        yVelocity = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
