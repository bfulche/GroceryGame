using UnityEngine;
using System.Collections.Generic;
using GroceryGame.Core;
using GroceryGame.UI;

namespace GroceryGame.Shopping
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class ShoppingCart : MonoBehaviour, IInteractable
    {
        [Header("Cart Settings")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private float maxItems = 20;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 followOffset = new Vector3(1.2f, 0f, 0.8f);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float followRotationSpeed = 5f;

        // Components
        private Rigidbody rb;
        private GameObject player;
        private Transform playerTransform;
        private bool isBeingPushed = false;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        // Inventory
        private List<GroceryItem> items = new List<GroceryItem>();

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Store original position
            originalPosition = transform.position;
            originalRotation = transform.rotation;

            // Configure rigidbody - keep it dynamic but constrained
            rb.mass = 10f;
            rb.linearDamping = 5f;
            rb.angularDamping = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Create item container if not assigned
            if (itemContainer == null)
            {
                GameObject container = new GameObject("ItemContainer");
                container.transform.SetParent(transform);
                container.transform.localPosition = new Vector3(0, 0.8f, 0);
                itemContainer = container.transform;

                // Add a trigger collider to the container to keep items inside
                BoxCollider containerCollider = container.AddComponent<BoxCollider>();
                containerCollider.size = new Vector3(0.8f, 1f, 1.2f);
                containerCollider.isTrigger = true;
            }

            // Make sure we have a collider on the appropriate layer
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider>();
                ((BoxCollider)col).size = new Vector3(1f, 1f, 1.5f);
                ((BoxCollider)col).center = new Vector3(0f, 0.5f, 0f);
            }

            // Make sure we're on the interactable layer
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            if (gameObject.layer == -1)
            {
                Debug.LogWarning("Interactable layer not found! Cart may not be interactable. Setting to Default layer.");
                gameObject.layer = 0; // Default layer
            }
        }

        private void FixedUpdate()
        {
            if (isBeingPushed && playerTransform != null)
            {
                FollowPlayer();
            }
        }

        #region IInteractable Implementation

        public string GetInteractionPrompt()
        {
            if (isBeingPushed)
            {
                return "Click to release cart";
            }
            else
            {
                return "Click to push cart";
            }
        }

        public void OnStartInteract(GameObject interactor)
        {
            // Check if player is holding an item
            InteractionSystem interactionSystem = interactor.GetComponent<InteractionSystem>();
            if (interactionSystem != null && interactionSystem.IsHoldingItem())
            {
                // Item placement is handled by the item itself
                return;
            }

            if (isBeingPushed)
            {
                ReleaseCart();
            }
            else
            {
                StartPushing(interactor);
            }
        }

        public void OnEndInteract(GameObject interactor)
        {
            // Not used for cart - toggle handled in OnStartInteract
        }

        public void OnStartHover()
        {
            // Show outline when hovering
            if (!isBeingPushed)
            {
                Outline cartOutline = GetComponent<Outline>();
                if (cartOutline == null)
                {
                    cartOutline = gameObject.AddComponent<Outline>();
                }
                cartOutline.SetOutlineColor(Color.yellow);
                cartOutline.EnableOutline();
            }
        }

        public void OnEndHover()
        {
            // Hide outline when not hovering (unless being pushed)
            if (!isBeingPushed)
            {
                Outline cartOutline = GetComponent<Outline>();
                if (cartOutline != null)
                {
                    cartOutline.DisableOutline();
                }
            }
        }

        public bool CanInteract()
        {
            return true;
        }

        public bool IsHoldable()
        {
            return true;
        }

        #endregion

        private void StartPushing(GameObject pusher)
        {
            player = pusher;
            playerTransform = pusher.transform;
            isBeingPushed = true;

            // Optional: Add visual feedback
            Outline cartOutline = GetComponent<Outline>();
            if (cartOutline == null)
            {
                cartOutline = gameObject.AddComponent<Outline>();
            }
            cartOutline.SetOutlineColor(Color.green);
            cartOutline.EnableOutline();
        }

        private void ReleaseCart()
        {
            player = null;
            playerTransform = null;
            isBeingPushed = false;

            // Optional: Remove visual feedback
            Outline cartOutline = GetComponent<Outline>();
            if (cartOutline != null)
            {
                cartOutline.DisableOutline();
            }
        }

        private void FollowPlayer()
        {
            if (playerTransform == null) return;

            // Calculate desired position relative to player
            Vector3 desiredPosition = playerTransform.position +
                playerTransform.right * followOffset.x +
                playerTransform.up * followOffset.y +
                playerTransform.forward * followOffset.z;

            // Keep cart at ground level
            desiredPosition.y = transform.position.y;

            // Use MovePosition for physics-safe movement
            rb.MovePosition(Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.fixedDeltaTime));

            // Smoothly rotate to match player's forward direction
            Quaternion desiredRotation = Quaternion.LookRotation(playerTransform.forward, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, desiredRotation, followRotationSpeed * Time.fixedDeltaTime));
        }

        public bool CanAddItem()
        {
            return items.Count < maxItems;
        }

        public void AddItem(GroceryItem item)
        {
            if (!CanAddItem()) return;

            items.Add(item);

            // Parent item to cart's item container
            item.transform.SetParent(itemContainer);

            // Position item in cart (simple stacking for now)
            Vector3 stackPosition = Vector3.zero;
            stackPosition.y = 0.1f * items.Count;
            item.transform.localPosition = stackPosition;

            // Add some randomness to make it look natural
            item.transform.localRotation = Quaternion.Euler(
                Random.Range(-10f, 10f),
                Random.Range(0f, 360f),
                Random.Range(-10f, 10f)
            );
        }

        public void RemoveItem(GroceryItem item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
                Debug.Log($"Removed {item.ItemName} from cart. Total items: {items.Count}");
            }
        }

        public List<GroceryItem> GetItems()
        {
            return new List<GroceryItem>(items);
        }

        public float GetTotalCost()
        {
            float total = 0;
            foreach (var item in items)
            {
                total += item.Price;
            }
            return total;
        }

        public void ClearCart()
        {
            foreach (var item in items)
            {
                Destroy(item.gameObject);
            }
            items.Clear();
        }
    }
}