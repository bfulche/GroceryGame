// RecipeCarouselUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GroceryGame.Data;
using GroceryGame.Core;

namespace GroceryGame.UI
{
    public class RecipeCarouselUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button leftArrowButton;
        [SerializeField] private Button rightArrowButton;
        [SerializeField] private Transform recipeDisplayArea;

        [Header("Recipe Info")]
        [SerializeField] private TextMeshProUGUI recipeCounterText;
        [SerializeField] private Button selectRecipeButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject recipeCardPrefab;

        // Current state
        private List<Recipe> availableRecipes = new List<Recipe>();
        private int currentRecipeIndex = 0;
        private GameObject currentRecipeCard;

        private void Start()
        {
            // Connect button events
            leftArrowButton.onClick.AddListener(ShowPreviousRecipe);
            rightArrowButton.onClick.AddListener(ShowNextRecipe);
            selectRecipeButton.onClick.AddListener(SelectCurrentRecipe);
        }

        public void InitializeCarousel(List<Recipe> recipes)
        {
            availableRecipes = recipes;
            currentRecipeIndex = 0;

            // Show first recipe
            if (availableRecipes.Count > 0)
            {
                ShowRecipeAtIndex(currentRecipeIndex);
            }

            UpdateNavigationButtons();
            UpdateRecipeCounter();
        }

        public void ShowNextRecipe()
        {
            if (availableRecipes.Count == 0) return;

            currentRecipeIndex = (currentRecipeIndex + 1) % availableRecipes.Count;
            ShowRecipeAtIndex(currentRecipeIndex);
            UpdateNavigationButtons();
            UpdateRecipeCounter();

            // Play sound effect
            AudioManager.Instance?.PlaySound("UINavigate");
        }

        public void ShowPreviousRecipe()
        {
            if (availableRecipes.Count == 0) return;

            currentRecipeIndex = (currentRecipeIndex - 1 + availableRecipes.Count) % availableRecipes.Count;
            ShowRecipeAtIndex(currentRecipeIndex);
            UpdateNavigationButtons();
            UpdateRecipeCounter();

            // Play sound effect
            AudioManager.Instance?.PlaySound("UINavigate");
        }

        private void ShowRecipeAtIndex(int index)
        {
            // Destroy current recipe card if it exists
            if (currentRecipeCard != null)
            {
                Destroy(currentRecipeCard);
            }

            // Create new recipe card
            if (index >= 0 && index < availableRecipes.Count)
            {
                Recipe recipe = availableRecipes[index];
                currentRecipeCard = Instantiate(recipeCardPrefab, recipeDisplayArea);

                // Initialize the recipe card
                RecipeCardUI cardUI = currentRecipeCard.GetComponent<RecipeCardUI>();
                if (cardUI != null)
                {
                    cardUI.Initialize(recipe, false); // false = don't show select button on card
                }

                // Make sure it fills the display area
                RectTransform cardRect = currentRecipeCard.GetComponent<RectTransform>();
                cardRect.anchorMin = Vector2.zero;
                cardRect.anchorMax = Vector2.one;
                cardRect.offsetMin = Vector2.zero;
                cardRect.offsetMax = Vector2.zero;
            }
        }

        private void UpdateNavigationButtons()
        {
            // Enable/disable buttons based on current position and settings
            // For carousel style, you might want buttons always enabled (wrapping)
            // Or disable them at the ends for linear navigation

            // Option 1: Always enabled (wrapping carousel)
            leftArrowButton.interactable = availableRecipes.Count > 1;
            rightArrowButton.interactable = availableRecipes.Count > 1;

            // Option 2: Disable at ends (linear navigation)
            // leftArrowButton.interactable = currentRecipeIndex > 0;
            // rightArrowButton.interactable = currentRecipeIndex < availableRecipes.Count - 1;
        }

        private void UpdateRecipeCounter()
        {
            if (recipeCounterText != null && availableRecipes.Count > 0)
            {
                recipeCounterText.text = $"Recipe {currentRecipeIndex + 1} of {availableRecipes.Count}";
            }
        }

        private void SelectCurrentRecipe()
        {
            if (currentRecipeIndex >= 0 && currentRecipeIndex < availableRecipes.Count)
            {
                Recipe selectedRecipe = availableRecipes[currentRecipeIndex];

                // Tell the RecipeManager to select this recipe
                RecipeManager recipeManager = FindObjectOfType<RecipeManager>();
                if (recipeManager != null)
                {
                    recipeManager.SelectRecipe(selectedRecipe);
                }

                // Play selection sound
                AudioManager.Instance?.PlaySound("UISelect");
            }
        }

        // Handle keyboard input for navigation
        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    ShowPreviousRecipe();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    ShowNextRecipe();
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    SelectCurrentRecipe();
                }
            }
        }
    }
}