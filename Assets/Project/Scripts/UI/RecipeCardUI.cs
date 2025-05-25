// RecipeCardUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GroceryGame.Data;

namespace GroceryGame.UI
{
    public class RecipeCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image recipeImage;
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Transform ingredientsContainer;
        [SerializeField] private Button selectButton;

        [Header("Ingredient Item")]
        [SerializeField] private GameObject ingredientItemPrefab;

        private Recipe recipe;

        public void Initialize(Recipe recipeData, bool showSelectButton = true)
        {
            recipe = recipeData;

            // Populate basic info
            if (recipeNameText != null)
                recipeNameText.text = recipe.recipeName;

            if (descriptionText != null)
                descriptionText.text = recipe.description;

            // Set recipe image if available
            if (recipeImage != null && recipe.recipeImage != null)
            {
                recipeImage.sprite = recipe.recipeImage;
                recipeImage.gameObject.SetActive(true);
            }
            else if (recipeImage != null)
            {
                recipeImage.gameObject.SetActive(false);
            }

            // Handle ingredients
            if (ingredientsContainer != null)
            {
                PopulateIngredients();
            }

            // Handle select button
            if (selectButton != null)
            {
                selectButton.gameObject.SetActive(showSelectButton);
                if (showSelectButton)
                {
                    selectButton.onClick.RemoveAllListeners();
                    selectButton.onClick.AddListener(OnSelectButtonClicked);
                }
            }
        }

        private void PopulateIngredients()
        {
            // Clear existing ingredients
            foreach (Transform child in ingredientsContainer)
            {
                Destroy(child.gameObject);
            }

            // Add ingredient items
            foreach (var ingredient in recipe.requiredIngredients)
            {
                // For now, create simple text items
                // We'll enhance this when we create IngredientItemUI
                GameObject ingredientObj = new GameObject("Ingredient");
                ingredientObj.transform.SetParent(ingredientsContainer);

                TextMeshProUGUI ingredientText = ingredientObj.AddComponent<TextMeshProUGUI>();
                ingredientText.text = $"• {ingredient.type} x{ingredient.quantity}";
                ingredientText.fontSize = 14;
                ingredientText.color = Color.white;

                // Reset transform
                RectTransform rect = ingredientObj.GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                rect.anchoredPosition = Vector2.zero;
            }
        }

        private void OnSelectButtonClicked()
        {
            // This method is here for when the card has its own select button
            Debug.Log($"Recipe card select button clicked for: {recipe.recipeName}");
        }
    }
}