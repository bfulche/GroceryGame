using System.Collections.Generic;
using UnityEngine;
using GroceryGame.Data;

namespace GroceryGame.Core
{
    public class RecipeManager : MonoBehaviour
    {
        // List of all available recipes (configured in Unity Inspector)
        [SerializeField] private List<Recipe> availableRecipes = new List<Recipe>();

        // Get all available recipes
        public List<Recipe> GetAvailableRecipes()
        {
            return availableRecipes;
        }

        // Select a recipe
        public void SelectRecipe(Recipe recipe)
        {
            if (recipe != null)
            {
                Debug.Log($"Selected recipe: {recipe.recipeName}");

                // Tell the GameManager about our selection
                GameManager.Instance.SelectRecipe(recipe);
            }
        }

        // Find a recipe by name
        public Recipe FindRecipeByName(string recipeName)
        {
            return availableRecipes.Find(r => r.recipeName == recipeName);
        }

        // Add a recipe to the available list
        public void AddRecipe(Recipe recipe)
        {
            if (recipe != null && !availableRecipes.Contains(recipe))
            {
                availableRecipes.Add(recipe);
            }
        }
    }
}