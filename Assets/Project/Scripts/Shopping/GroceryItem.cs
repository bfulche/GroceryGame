using UnityEngine;
using GroceryGame.Core;
using GroceryGame.UI;
using GroceryGame.Data;

namespace GroceryGame.Shopping
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class GroceryItem : MonoBehaviour, IInteractable
    {
        [Header("Item Data")]
        [SerializeField] private IngredientType ingredientType;
        [SerializeField] private string itemName = "Grocery Item";
        [SerializeField] private float basePrice = 1.99f;
        [TextArea(2, 4)]
        [SerializeField] private string itemDescription = "A fresh grocery item.";

        [Header("Quality Settings")]
        [SerializeField] private ItemQuality quality = ItemQuality.Standard;
        [SerializeField] private Sprite itemIcon;

        [Header("Interaction Settings")]
        [SerializeField] private bool canBePickedUp = true;

        // Components
        private Outline outline;
        private Rigidbody rb;
        private Collider col;

        // State
        private bool isHeld = false;
        private bool isInCart = false;
        private Transform originalParent;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        // Properties
        public IngredientType IngredientType => ingredientType;
        public string ItemName => itemName;
        public string Description => itemDescription;
        public float Price => CalculatePrice();
        public ItemQuality Quality => quality;
        public Sprite Icon => itemIcon;
        public bool IsInCart => isInCart;

        private void Awake()
        {
            // Get components
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();

            // Add outline component if not present
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }

            // Configure rigidbody for items on shelf
            rb.mass = 0.5f;
            rb.linearDamping = 2f;
            rb.angularDamping = 5f;
        }

        #region IInteractable Implementation

        public string GetInteractionPrompt()
        {
            if (isInCart)
            {
                return $"Hold to remove {itemName} from cart";
            }
            else
            {
                return $"Hold to pick up {itemName}";
            }
        }

        public void OnStartInteract(GameObject player)
        {
            if (!canBePickedUp) return;

            if (isInCart)
            {
                // Remove from cart
                RemoveFromCart(player);
            }
            else
            {
                // Pick up item
                PickUpItem(player);
            }
        }

        public void OnEndInteract(GameObject player)
        {
            if (isHeld)
            {
                // Try to place in cart or drop
                TryPlaceOrDrop(player);
            }
        }

        public void OnStartHover()
        {
            if (outline != null && !isHeld)
            {
                outline.EnableOutline();
            }
        }

        public void OnEndHover()
        {
            if (outline != null && !isHeld)
            {
                outline.DisableOutline();
            }
        }

        public bool CanInteract()
        {
            return canBePickedUp;
        }

        public bool IsHoldable()
        {
            return true;
        }

        #endregion

        private void PickUpItem(GameObject player)
        {
            // Store original transform data
            originalParent = transform.parent;
            originalPosition = transform.position;
            originalRotation = transform.rotation;

            // Get interaction system to track held item
            InteractionSystem interactionSystem = player.GetComponent<InteractionSystem>();
            if (interactionSystem != null)
            {
                interactionSystem.SetHeldItem(this);
            }

            // Disable physics while held
            rb.isKinematic = true;
            col.enabled = false;

            // Parent to player's camera for holding
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                transform.SetParent(playerCamera.transform);
                // Center the item in front of the player
                transform.localPosition = new Vector3(0f, -0.1f, 0.8f);
                transform.localRotation = Quaternion.identity;
            }

            isHeld = true;

            // Keep outline active while held
            if (outline != null)
            {
                outline.SetOutlineColor(Color.green);
                outline.EnableOutline();
            }
        }

        private void TryPlaceOrDrop(GameObject player)
        {
            // Check if we're near a shopping cart
            ShoppingCart cart = FindNearbyCart();

            if (cart != null && cart.CanAddItem())
            {
                // Place in cart
                PlaceInCart(cart);
            }
            else
            {
                // Drop item
                DropItem(player);
            }

            // Clear held item from interaction system
            InteractionSystem interactionSystem = player.GetComponent<InteractionSystem>();
            if (interactionSystem != null)
            {
                interactionSystem.ClearHeldItem();
            }
        }

        private void PlaceInCart(ShoppingCart cart)
        {
            // Add to cart's inventory
            cart.AddItem(this);

            // Update state
            isHeld = false;
            isInCart = true;

            // Re-enable physics with constraints for cart
            rb.isKinematic = false;
            col.enabled = true;

            // Disable outline
            if (outline != null)
            {
                outline.DisableOutline();
            }
        }

        private void RemoveFromCart(GameObject player)
        {
            // Find the cart we're in
            ShoppingCart cart = GetComponentInParent<ShoppingCart>();
            if (cart != null)
            {
                cart.RemoveItem(this);
            }

            // Pick up the item
            isInCart = false;
            PickUpItem(player);
        }

        private void DropItem(GameObject player)
        {
            // Return to original parent or world
            transform.SetParent(originalParent);

            // Place in front of player
            transform.position = player.transform.position + player.transform.forward * 1.5f + Vector3.down * 0.5f;
            transform.rotation = originalRotation;

            // Re-enable physics
            rb.isKinematic = false;
            col.enabled = true;

            // Clear held state
            isHeld = false;

            // Reset outline
            if (outline != null)
            {
                outline.SetOutlineColor(Color.yellow);
                outline.DisableOutline();
            }
        }

        private ShoppingCart FindNearbyCart()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider col in colliders)
            {
                ShoppingCart cart = col.GetComponentInParent<ShoppingCart>();
                if (cart != null)
                {
                    return cart;
                }
            }
            return null;
        }

        private float CalculatePrice()
        {
            float price = basePrice;

            switch (quality)
            {
                case ItemQuality.Generic:
                    price *= 0.7f;
                    break;
                case ItemQuality.Premium:
                    price *= 1.8f;
                    break;
            }

            // Apply store price modifier if in a store
            StoreLocation currentStore = GameManager.Instance?.SelectedStore;
            if (currentStore != null)
            {
                price *= currentStore.priceModifier;
            }

            return Mathf.Round(price * 100f) / 100f; // Round to 2 decimal places
        }
    }

    public enum ItemQuality
    {
        Generic,
        Standard,
        Premium
    }
}