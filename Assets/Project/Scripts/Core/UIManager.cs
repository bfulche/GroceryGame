using UnityEngine;
using System.Collections.Generic;
using GroceryGame.Data;
using GroceryGame.Core; // This is important for accessing GameManager and GameStateg

namespace GroceryGame.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject recipeSelectionPanel;
        [SerializeField] private GameObject storeSelectionPanel;
        [SerializeField] private GameObject shoppingListPanel;
        [SerializeField] private GameObject recipeSuccessPanel;
        [SerializeField] private GameObject recipeFailedPanel;

        // Hide all UI panels
        private void HideAllPanels()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (recipeSelectionPanel) recipeSelectionPanel.SetActive(false);
            if (storeSelectionPanel) storeSelectionPanel.SetActive(false);
            if (shoppingListPanel) shoppingListPanel.SetActive(false);
            if (recipeSuccessPanel) recipeSuccessPanel.SetActive(false);
            if (recipeFailedPanel) recipeFailedPanel.SetActive(false);
        }

        // Show the main menu
        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel) mainMenuPanel.SetActive(true);
            Debug.Log("Main menu displayed");
        }

        // Show recipe selection screen
        public void ShowRecipeSelection(List<Recipe> recipes)
        {
            HideAllPanels();
            if (recipeSelectionPanel)
            {
                recipeSelectionPanel.SetActive(true);
                // Find and initialize the recipe carousel
                RecipeCarouselUI carousel = recipeSelectionPanel.GetComponentInChildren<RecipeCarouselUI>();
                if (carousel != null)
                {
                    Debug.Log("RecipeCarouselUI found, initializing carousel");
                    carousel.InitializeCarousel(recipes);
                }
                else
                {
                    Debug.LogError("RecipeCarouselUI component not found in Recipe Selection Panel or its children!");
                    Debug.LogError("Make sure RecipeCarouselUI script is attached to the Recipe Selection Panel or a child object");
                }
            }
            else
            {
                Debug.LogError("Recipe Selection Panel is NULL! Make sure it's assigned in UIManager inspector");
            }
        }

        // Hide recipe selection
        public void HideRecipeSelection()
        {
            if (recipeSelectionPanel) recipeSelectionPanel.SetActive(false);
        }

        // Show store selection screen
        public void ShowStoreSelection(List<StoreLocation> stores)
        {
            HideAllPanels();
            if (storeSelectionPanel)
            {
                storeSelectionPanel.SetActive(true);
                // Find and initialize the store carousel
                StoreCarouselUI carousel = storeSelectionPanel.GetComponentInChildren<StoreCarouselUI>();
                if (carousel != null)
                {
                    carousel.InitializeCarousel(stores);
                }
            }
        }

        // Hide store selection
        public void HideStoreSelection()
        {
            if (storeSelectionPanel) storeSelectionPanel.SetActive(false);
        }

        // Show recipe success screen
        public void ShowRecipeSuccess(Recipe recipe)
        {
            HideAllPanels();
            if (recipeSuccessPanel) recipeSuccessPanel.SetActive(true);
            Debug.Log($"Recipe success displayed for: {recipe.recipeName}");
        }

        // Show recipe failed screen
        public void ShowRecipeFailed(Recipe recipe)
        {
            HideAllPanels();
            if (recipeFailedPanel) recipeFailedPanel.SetActive(true);
            Debug.Log($"Recipe failed displayed for: {recipe.recipeName}");
        }

        // Button handlers
        public void OnStartButtonClicked()
        {
            Debug.Log("Start button clicked");

            // Check if GameManager exists
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.Instance is NULL! Make sure GameManager is in the scene.");
                return;
            }

            // Transition to Recipe Selection state
            GameManager.Instance.TransitionToState(GameState.RecipeSelection);
        }

        // Handle back button from recipe selection
        public void OnRecipeSelectionBackClicked()
        {
            Debug.Log("Recipe selection back button clicked");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionToState(GameState.MainMenu);
            }
        }

        // Handle back button from store selection
        public void OnStoreSelectionBackClicked()
        {
            Debug.Log("Store selection back button clicked");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionToState(GameState.RecipeSelection);
            }
        }

    }
}