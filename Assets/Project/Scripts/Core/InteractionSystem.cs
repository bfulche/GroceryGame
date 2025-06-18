using UnityEngine;
using UnityEngine.UI;
using GroceryGame.UI;
using GroceryGame.Shopping;

namespace GroceryGame.Core
{
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Interaction Settings")]
        private Camera playerCamera;
        private float interactionRange;
        private LayerMask interactionLayers;

        [Header("UI References")]
        [SerializeField] private GameObject interactionPromptPrefab;
        private Text interactionPromptText;
        private GameObject interactionPromptUI;

        // Current interaction target
        private IInteractable currentInteractable;
        private GameObject currentTarget;
        private bool isHoldingInteraction = false;

        // Held item for examination
        private GroceryItem heldItem;
        private bool isExamining = false;

        // References
        private FirstPersonController playerController;

        public void Initialize(Camera camera, float range, LayerMask layers)
        {
            playerCamera = camera;
            interactionRange = range;
            interactionLayers = layers;
            playerController = GetComponent<FirstPersonController>();

            // Create interaction prompt UI if not provided
            CreateInteractionPrompt();
        }

        private void Update()
        {
            // Don't process interactions if not in shopping state
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameState.Shopping)
            {
                return;
            }

            if (!isExamining)
            {
                CheckForInteractable();
            }
        }

        private void CheckForInteractable()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange, interactionLayers))
            {
                GameObject hitObject = hit.collider.gameObject;
                IInteractable interactable = hitObject.GetComponent<IInteractable>();

                if (interactable != null && interactable.CanInteract())
                {
                    // New target
                    if (currentTarget != hitObject)
                    {
                        // End hover on previous target
                        if (currentInteractable != null)
                        {
                            currentInteractable.OnEndHover();
                        }

                        // Start hover on new target
                        currentTarget = hitObject;
                        currentInteractable = interactable;
                        currentInteractable.OnStartHover();

                        // Show interaction prompt
                        ShowInteractionPrompt(interactable.GetInteractionPrompt());
                    }
                }
                else
                {
                    ClearCurrentInteractable();
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }

        private void ClearCurrentInteractable()
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnEndHover();
                currentInteractable = null;
                currentTarget = null;
                HideInteractionPrompt();
            }
        }

        public void ToggleInteract()
        {
            // If holding an item
            if (heldItem != null)
            {
                // Check if we're looking at the cart
                ShoppingCart cart = currentTarget?.GetComponent<ShoppingCart>();
                if (cart != null && cart.CanAddItem())
                {
                    // Place item in cart through the item's logic
                    heldItem.OnEndInteract(gameObject);
                }
                else
                {
                    // Drop the item
                    heldItem.OnEndInteract(gameObject);
                }
            }
            // If looking at cart and not holding item
            else if (currentTarget != null)
            {
                ShoppingCart cart = currentTarget.GetComponent<ShoppingCart>();
                if (cart != null)
                {
                    cart.OnStartInteract(gameObject);
                }
                else if (currentInteractable != null && currentInteractable.CanInteract())
                {
                    // Regular interaction (pick up item)
                    currentInteractable.OnStartInteract(gameObject);
                }
            }
        }

        public void ToggleExamineHeldItem()
        {
            if (isExamining)
            {
                StopExamineHeldItem();
            }
            else
            {
                StartExamineHeldItem();
            }
        }

        private void StartExamineHeldItem()
        {
            if (heldItem != null && !isExamining)
            {
                isExamining = true;

                // Find or create ItemExaminationUI
                ItemExaminationUI examUI = FindObjectOfType<ItemExaminationUI>();
                if (examUI == null)
                {
                    // Create examination UI
                    GameObject canvas = GameObject.Find("GameCanvas");
                    if (canvas == null)
                    {
                        canvas = new GameObject("GameCanvas");
                        Canvas c = canvas.AddComponent<Canvas>();
                        c.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvas.AddComponent<CanvasScaler>();
                        canvas.AddComponent<GraphicRaycaster>();
                    }

                    GameObject examUIObj = new GameObject("ItemExaminationUI");
                    examUIObj.transform.SetParent(canvas.transform, false);
                    examUI = examUIObj.AddComponent<ItemExaminationUI>();
                }

                examUI.StartExamination(heldItem);

                // Pause player movement
                if (playerController != null)
                {
                    playerController.SetCanMove(false);
                }
            }
        }

        private void StopExamineHeldItem()
        {
            if (isExamining)
            {
                isExamining = false;

                ItemExaminationUI examUI = FindObjectOfType<ItemExaminationUI>();
                if (examUI != null)
                {
                    examUI.StopExamination();
                }

                // Resume player movement
                if (playerController != null)
                {
                    playerController.SetCanMove(true);
                }
            }
        }

        public void SetHeldItem(GroceryItem item)
        {
            heldItem = item;
        }

        public void ClearHeldItem()
        {
            heldItem = null;
        }

        public bool IsHoldingItem()
        {
            return heldItem != null;
        }

        private void CreateInteractionPrompt()
        {
            // Create a simple interaction prompt UI
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                // Create canvas if it doesn't exist
                canvas = new GameObject("GameCanvas");
                Canvas c = canvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<CanvasScaler>();
                canvas.AddComponent<GraphicRaycaster>();
            }

            // Create prompt UI
            interactionPromptUI = new GameObject("InteractionPrompt");
            interactionPromptUI.transform.SetParent(canvas.transform, false);

            // Position at center of screen
            RectTransform rect = interactionPromptUI.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.4f);
            rect.anchorMax = new Vector2(0.5f, 0.4f);
            rect.sizeDelta = new Vector2(300, 50);
            rect.anchoredPosition = Vector2.zero;

            // Add background
            Image bg = interactionPromptUI.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(interactionPromptUI.transform, false);
            interactionPromptText = textObj.AddComponent<Text>();
            interactionPromptText.text = "Click to Interact";
            interactionPromptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            interactionPromptText.fontSize = 20;
            interactionPromptText.color = Color.white;
            interactionPromptText.alignment = TextAnchor.MiddleCenter;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // Hide by default
            interactionPromptUI.SetActive(false);
        }

        private void ShowInteractionPrompt(string prompt)
        {
            if (interactionPromptUI != null && interactionPromptText != null)
            {
                interactionPromptText.text = prompt;
                interactionPromptUI.SetActive(true);
            }
        }

        private void HideInteractionPrompt()
        {
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (interactionPromptUI != null)
            {
                Destroy(interactionPromptUI);
            }
        }
    }
}