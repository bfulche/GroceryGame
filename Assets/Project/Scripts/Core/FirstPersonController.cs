using GroceryGame.UI;
using UnityEngine;

namespace GroceryGame.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Camera Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float lookXLimit = 90f;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactionLayers = -1;

        private CharacterController characterController;
        private Vector3 moveDirection = Vector3.zero;
        private float rotationX = 0;

        // References
        private InteractionSystem interactionSystem;
        private bool canMove = true;

        // Cursor state management
        private bool cursorLocked = true;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            // If no camera assigned, try to find one in children
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    Debug.LogError("No camera found for FirstPersonController!");
                }
            }

            // Set up interaction system
            interactionSystem = gameObject.AddComponent<InteractionSystem>();
            interactionSystem.Initialize(playerCamera, interactionRange, interactionLayers);
        }

        private void Start()
        {
            // Lock cursor at start
            SetCursorState(true);
        }

        private void Update()
        {
            // Check if game is paused or in UI mode
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameState.Shopping)
            {
                return;
            }

            HandleMovement();
            HandleMouseLook();
            HandleInteractionInput();
            HandleUIInput();
        }

        private void HandleMovement()
        {
            if (!canMove) return;

            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Calculate movement direction
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            float curSpeedX = moveSpeed * vertical;
            float curSpeedY = moveSpeed * horizontal;

            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            // Apply gravity
            if (!characterController.isGrounded)
            {
                moveDirection.y = movementDirectionY + (gravity * Time.deltaTime);
            }
            else
            {
                moveDirection.y = 0;
            }

            // Move the character
            characterController.Move(moveDirection * Time.deltaTime);
        }

        private void HandleMouseLook()
        {
            if (!canMove || !cursorLocked) return;

            // Get mouse movement
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate the player body left/right
            transform.rotation *= Quaternion.Euler(0, mouseX, 0);

            // Rotate the camera up/down
            rotationX += mouseY;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(-rotationX, 0, 0);
        }

        private void HandleInteractionInput()
        {
            // Left click to toggle interact
            if (Input.GetMouseButtonDown(0))
            {
                interactionSystem.ToggleInteract();
            }

            // Right click to toggle item examination (if holding item)
            if (Input.GetMouseButtonDown(1))
            {
                interactionSystem.ToggleExamineHeldItem();
            }
        }

        private void HandleUIInput()
        {
            // Toggle shopping list
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
            {
                ShoppingListUI shoppingList = FindObjectOfType<ShoppingListUI>();
                if (shoppingList != null)
                {
                    shoppingList.ToggleVisibility();
                }
            }

            // Pause menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Toggle pause - this should integrate with your UIManager
                UIManager uiManager = FindObjectOfType<UIManager>();
                if (uiManager != null)
                {
                    // You'll need to implement ShowPauseMenu() in your UIManager
                    Debug.Log("Pause menu requested");
                }
            }
        }

        public void SetCursorState(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void SetCanMove(bool value)
        {
            canMove = value;
        }

        // Called when examining items or using UI
        public void EnableUIMode()
        {
            SetCursorState(false);
            SetCanMove(false);
        }

        // Called when returning to gameplay
        public void DisableUIMode()
        {
            SetCursorState(true);
            SetCanMove(true);
        }
    }
}