using UnityEngine;
using System.Collections.Generic;
using GroceryGame.Core;

namespace GroceryGame.Shopping
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class ShoppingCart : MonoBehaviour, IInteractable
    {
        [Header("Cart Settings")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private float pushForce = 5f;
        [SerializeField] private float maxItems = 20;

        [Header("Physics Settings")]
        [SerializeField] private float followDistance = 1.5f;
        [SerializeField] private float followSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;

        // Components
        private Rigidbody rb;
        private GameObject player;
        private bool isBeingPushed = false;

        // Inventory
        private List<GroceryItem> items = new List<GroceryItem>();

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Configure rigidbody for cart physics
            rb.mass = 5f;
            rb.linearDamping = 3f;
            rb.angularDamping = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Create item container if not assigned
            if (itemContainer == null)
            {
                GameObject container = new GameObject("ItemContainer");
                container.transform.SetParent(transform);
                container.transform.localPosition = new Vector3(0, 0.5f, 0);
                itemContainer = container.transform;
            }
        }

        private void FixedUpdate()
        {
            if (isBeingPushed && player != null)
            {
                FollowPlayer();
            }
        }

        #region IInteractable Implementation

        public string GetInteractionPrompt()
        {
            return "Hold to push cart";
        }

        public void OnStartInteract(GameObject interactor)
        {
            StartPushing(interactor);
        }

        public void OnEndInteract(GameObject interactor)
        {
            ReleaseCart();
        }

        public void OnStartHover()
        {
            // Cart doesn't need hover effects
        }

        public void OnEndHover()
        {
            // Cart doesn't need hover effects
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
            isBeingPushed = true;
        }

        private void ReleaseCart()
        {
            player = null;
            isBeingPushed = false;

            // Apply slight brake to stop cart
            rb.linearVelocity *= 0.5f;
        }

        private void FollowPlayer()
        {
            if (player == null) return;

            // Calculate desired position behind player
            Vector3 desiredPosition = player.transform.position - player.transform.forward * followDistance;
            desiredPosition.y = transform.position.y; // Keep cart at same height

            // Move towards desired position
            Vector3 direction = (desiredPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, desiredPosition);

            if (distance > 0.1f)
            {
                // Apply force to move cart
                Vector3 force = direction * followSpeed * Mathf.Min(distance, 1f);
                rb.AddForce(force, ForceMode.VelocityChange);
            }

            // Rotate to face player's direction
            Quaternion desiredRotation = Quaternion.LookRotation(player.transform.forward);
            rb.rotation = Quaternion.Slerp(rb.rotation, desiredRotation, rotationSpeed * Time.fixedDeltaTime);
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
                item.transform.SetParent(null);
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