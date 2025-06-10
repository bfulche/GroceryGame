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

        [Header("Ingredient Item")]
        [SerializeField] private GameObject ingredientItemPrefab;

        [Header("Layout Settings")]
        [SerializeField] private float ingredientSpacing = 8f; // Adjustable in inspector
        [SerializeField] private float ingredientFontSize = 34f; //font size for ingredients list

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
        }

        private void PopulateIngredients()
        {
            // Clear existing ingredients
            foreach (Transform child in ingredientsContainer)
            {
                Destroy(child.gameObject);
            }

            // Set the spacing on the Vertical Layout Group
            VerticalLayoutGroup layoutGroup = ingredientsContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.spacing = ingredientSpacing;
            }

            // Add ingredient items
            foreach (var ingredient in recipe.requiredIngredients)
            {
                GameObject ingredientObj;
                TextMeshProUGUI ingredientText;

                // Use the prefab if assigned, otherwise create manually
                if (ingredientItemPrefab != null)
                {
                    // PROPER WAY: Use the assigned prefab
                    ingredientObj = Instantiate(ingredientItemPrefab, ingredientsContainer);
                    ingredientText = ingredientObj.GetComponent<TextMeshProUGUI>();

                    if (ingredientText == null)
                    {
                        Debug.LogError("Ingredient Item Prefab doesn't have TextMeshProUGUI component!");
                        continue;
                    }

                    //Apply font size to prefab instance
                    ingredientText.fontSize = ingredientFontSize;
                }
                else
                {
                    // FALLBACK: Create manually (with black text!)
                    ingredientObj = new GameObject("Ingredient");
                    ingredientObj.transform.SetParent(ingredientsContainer);

                    ingredientText = ingredientObj.AddComponent<TextMeshProUGUI>();
                    ingredientText.fontSize = 34;
                    ingredientText.color = Color.black;  // BLACK text instead of white

                    // Reset transform
                    RectTransform rect = ingredientObj.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;
                    rect.anchoredPosition = Vector2.zero;
                }

                // Set the ingredient text
                ingredientText.text = $"• {ingredient.type} x{ingredient.quantity}";
            }
        }

        private void OnSelectButtonClicked()
        {
            // This method is here for when the card has its own select button
            Debug.Log($"Recipe card select button clicked for: {recipe.recipeName}");
        }
    }
}