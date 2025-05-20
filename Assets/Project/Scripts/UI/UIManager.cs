using UnityEngine;
using System.Collections.Generic;
using GroceryGame.Data;

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
            if (recipeSelectionPanel) recipeSelectionPanel.SetActive(true);
            Debug.Log($"Recipe selection displayed with {recipes.Count} recipes");

            // We'll implement the recipe card creation later
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
            if (storeSelectionPanel) storeSelectionPanel.SetActive(true);
            Debug.Log($"Store selection displayed with {stores.Count} stores");

            // We'll implement the store card creation later
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
            // We'll add this functionality once GameManager is working
            Debug.Log("Start button clicked");
        }
    }
}