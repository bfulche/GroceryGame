using System;
using System.Collections.Generic;
using UnityEngine;

namespace GroceryGame.Data
{
    [CreateAssetMenu(fileName = "New Recipe", menuName = "Grocery Game/Recipe")]
    public class Recipe : ScriptableObject
    {
        [Header("Basic Info")]
        public string recipeName;
        public Sprite recipeImage;

        [TextArea(3, 8)]
        public string description;

        [Header("Ingredients")]
        [Tooltip("List of ingredients required for this recipe")]
        public List<IngredientRequirement> requiredIngredients = new List<IngredientRequirement>();

        [Header("Budget")]
        [Tooltip("Recommended budget for completing this recipe")]
        public float recommendedBudget = 15f;

        [Header("Future Features - Not Used in POC")]
        [Range(1, 5)]
        [Tooltip("Difficulty rating from 1-5 stars")]
        public int difficulty = 1;
    }

    [Serializable]
    public class IngredientRequirement
    {
        public IngredientType type;
        [Min(1)]
        public int quantity = 1;
        [Tooltip("If true, any quality of this ingredient will work")]
        public bool allowsQualitySubstitution = true;
    }
}