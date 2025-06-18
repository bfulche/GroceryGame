using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using GroceryGame.Core;
using GroceryGame.Data;
using GroceryGame.Shopping;

namespace GroceryGame.UI
{
    public class ShoppingListUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject shoppingListPanel;
        [SerializeField] private Transform ingredientListContainer;
        [SerializeField] private GameObject ingredientItemPrefab;
        [SerializeField] private Text recipeNameText;
        [SerializeField] private Text budgetText;
        [SerializeField] private Button closeButton;

        [Header("Display Settings")]
        [SerializeField] private Color completedItemColor = new Color(0.5f, 1f, 0.5f);
        [SerializeField] private Color missingItemColor = Color.white;
        [SerializeField] private float updateInterval = 0.5f; // How often to update the list

        // Current recipe data
        private Recipe currentRecipe;
        private List<IngredientListItem> ingredientItems = new List<IngredientListItem>();
        private ShoppingCart playerCart;

        // State
        private bool isVisible = false;
        private float nextUpdateTime;

        private void Start()
        {
            // Get current recipe from GameManager
            if (GameManager.Instance != null)
            {
                currentRecipe = GameManager.Instance.SelectedRecipe;
            }

            // Create UI if panel not assigned
            if (shoppingListPanel == null)
            {
                CreateShoppingListUI();
            }

            // Set up UI
            InitializeUI();

            // Find shopping cart
            playerCart = FindObjectOfType<ShoppingCart>();

            // Hide by default
            shoppingListPanel.SetActive(false);
        }

        private void CreateShoppingListUI()
        {
            // Find or create canvas
            GameObject canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                canvas = new GameObject("GameCanvas");
                Canvas c = canvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<CanvasScaler>();
                canvas.AddComponent<GraphicRaycaster>();
            }

            // Create shopping list panel
            shoppingListPanel = new GameObject("ShoppingListPanel");
            shoppingListPanel.transform.SetParent(canvas.transform, false);

            // Configure panel
            RectTransform panelRect = shoppingListPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 0.5f);
            panelRect.sizeDelta = new Vector2(300, -100);
            panelRect.anchoredPosition = new Vector2(50, 0);

            // Add background
            Image bg = shoppingListPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            // Create header
            GameObject header = new GameObject("Header");
            header.transform.SetParent(shoppingListPanel.transform, false);
            recipeNameText = header.AddComponent<Text>();
            recipeNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            recipeNameText.fontSize = 24;
            recipeNameText.color = Color.white;
            recipeNameText.alignment = TextAnchor.MiddleCenter;
            recipeNameText.text = "Shopping List";

            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.sizeDelta = new Vector2(0, 40);
            headerRect.anchoredPosition = new Vector2(0, -10);

            // Create ingredient list container
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(shoppingListPanel.transform, false);
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();

            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.sizeDelta = new Vector2(-20, -100);
            scrollRect.anchoredPosition = new Vector2(0, -30);

            // Create content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(scrollView.transform, false);
            ingredientListContainer = content.transform;

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
            scroll.vertical = true;
            scroll.horizontal = false;

            // Create budget text
            GameObject budgetObj = new GameObject("BudgetText");
            budgetObj.transform.SetParent(shoppingListPanel.transform, false);
            budgetText = budgetObj.AddComponent<Text>();
            budgetText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            budgetText.fontSize = 18;
            budgetText.color = Color.green;
            budgetText.alignment = TextAnchor.MiddleCenter;

            RectTransform budgetRect = budgetObj.GetComponent<RectTransform>();
            budgetRect.anchorMin = new Vector2(0, 0);
            budgetRect.anchorMax = new Vector2(1, 0);
            budgetRect.pivot = new Vector2(0.5f, 0);
            budgetRect.sizeDelta = new Vector2(0, 30);
            budgetRect.anchoredPosition = new Vector2(0, 10);

            // Create close button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(shoppingListPanel.transform, false);
            closeButton = closeObj.AddComponent<Button>();
            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = Color.red;

            Text closeText = new GameObject("Text").AddComponent<Text>();
            closeText.transform.SetParent(closeObj.transform, false);
            closeText.text = "X";
            closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            closeText.fontSize = 20;
            closeText.color = Color.white;
            closeText.alignment = TextAnchor.MiddleCenter;

            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.sizeDelta = new Vector2(30, 30);
            closeRect.anchoredPosition = new Vector2(-5, -5);
        }

        private void Update()
        {
            if (isVisible && Time.time >= nextUpdateTime)
            {
                UpdateIngredientStatus();
                nextUpdateTime = Time.time + updateInterval;
            }
        }

        private void InitializeUI()
        {
            if (currentRecipe == null)
            {
                Debug.LogError("No recipe selected!");
                return;
            }

            // Set recipe name
            if (recipeNameText != null)
            {
                recipeNameText.text = currentRecipe.recipeName;
            }

            // Set budget
            if (budgetText != null)
            {
                budgetText.text = $"Budget: ${currentRecipe.recommendedBudget:F2}";
            }

            // Create ingredient list items
            CreateIngredientListItems();

            // Set up close button
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
        }

        private void CreateIngredientListItems()
        {
            // Clear existing items
            foreach (var item in ingredientItems)
            {
                if (item.gameObject != null)
                    Destroy(item.gameObject);
            }
            ingredientItems.Clear();

            // Create prefab if not assigned
            if (ingredientItemPrefab == null)
            {
                ingredientItemPrefab = CreateIngredientItemPrefab();
            }

            // Create item for each required ingredient
            foreach (var requirement in currentRecipe.requiredIngredients)
            {
                GameObject itemObj = Instantiate(ingredientItemPrefab, ingredientListContainer);
                IngredientListItem listItem = itemObj.GetComponent<IngredientListItem>();

                if (listItem == null)
                {
                    listItem = itemObj.AddComponent<IngredientListItem>();
                }

                // Set up references to UI elements
                listItem.checkmark = itemObj.transform.Find("Checkbox").GetComponent<Image>();
                listItem.label = itemObj.transform.Find("Text").GetComponent<Text>();

                listItem.Setup(requirement);
                ingredientItems.Add(listItem);
            }
        }

        private GameObject CreateIngredientItemPrefab()
        {
            // Create a simple ingredient item prefab
            GameObject prefab = new GameObject("IngredientItem");
            RectTransform rect = prefab.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 30);

            // Add horizontal layout
            HorizontalLayoutGroup layout = prefab.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.childAlignment = TextAnchor.MiddleLeft;

            // Add background
            Image bg = prefab.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f);

            // Create checkbox
            GameObject checkboxObj = new GameObject("Checkbox");
            checkboxObj.transform.SetParent(prefab.transform);
            Image checkImg = checkboxObj.AddComponent<Image>();
            checkImg.color = Color.white;
            RectTransform checkRect = checkboxObj.GetComponent<RectTransform>();
            checkRect.sizeDelta = new Vector2(20, 20);

            // Create text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(prefab.transform);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
            textLayout.flexibleWidth = 1;

            // Add IngredientListItem component after creating UI elements
            IngredientListItem item = prefab.AddComponent<IngredientListItem>();

            return prefab;
        }

        private void UpdateIngredientStatus()
        {
            if (playerCart == null)
            {
                playerCart = FindObjectOfType<ShoppingCart>();
                if (playerCart == null) return;
            }

            // Get all items in cart
            List<GroceryItem> cartItems = playerCart.GetItems();

            // Update each ingredient requirement
            foreach (var listItem in ingredientItems)
            {
                int countInCart = cartItems.Count(item => item.IngredientType == listItem.requirement.type);
                bool isComplete = countInCart >= listItem.requirement.quantity;

                listItem.UpdateStatus(isComplete, countInCart);
            }

            // Update total cost
            UpdateTotalCost(cartItems);
        }

        private void UpdateTotalCost(List<GroceryItem> cartItems)
        {
            if (budgetText != null)
            {
                float totalCost = cartItems.Sum(item => item.Price);
                float budget = currentRecipe.recommendedBudget;

                budgetText.text = $"Budget: ${budget:F2} | Cart: ${totalCost:F2}";

                // Color code based on budget
                if (totalCost > budget)
                {
                    budgetText.color = Color.red;
                }
                else if (totalCost > budget * 0.8f)
                {
                    budgetText.color = Color.yellow;
                }
                else
                {
                    budgetText.color = Color.green;
                }
            }
        }

        public void ToggleVisibility()
        {
            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show()
        {
            shoppingListPanel.SetActive(true);
            isVisible = true;

            // Update immediately
            UpdateIngredientStatus();

            // Enable cursor for UI interaction
            FirstPersonController player = FindObjectOfType<FirstPersonController>();
            if (player != null)
            {
                player.SetCursorState(false);
            }
        }

        public void Hide()
        {
            shoppingListPanel.SetActive(false);
            isVisible = false;

            // Re-enable player controls
            FirstPersonController player = FindObjectOfType<FirstPersonController>();
            if (player != null)
            {
                player.SetCursorState(true);
            }
        }
    }

    // Helper class for ingredient list items
    public class IngredientListItem : MonoBehaviour
    {
        public IngredientRequirement requirement;
        public Image checkmark;
        public Text label;

        private Color completedColor = new Color(0.5f, 1f, 0.5f);
        private Color incompleteColor = Color.white;

        public void Setup(IngredientRequirement req)
        {
            requirement = req;
            if (label != null)
            {
                label.text = $"{req.type} x{req.quantity}";
            }
            UpdateStatus(false, 0);
        }

        public void UpdateStatus(bool isComplete, int currentCount)
        {
            if (checkmark != null)
            {
                checkmark.color = isComplete ? completedColor : incompleteColor;

                // You can replace this with a proper checkmark sprite
                checkmark.enabled = isComplete;
            }

            if (label != null)
            {
                label.text = $"{requirement.type} ({currentCount}/{requirement.quantity})";
                label.color = isComplete ? completedColor : incompleteColor;

                // Strike through if complete (using rich text)
                if (isComplete)
                {
                    label.fontStyle = FontStyle.Italic;
                }
                else
                {
                    label.fontStyle = FontStyle.Normal;
                }
            }
        }
    }
}