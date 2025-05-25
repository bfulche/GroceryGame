// IngredientItemUI.cs
using UnityEngine;
using TMPro;
using GroceryGame.Data;

namespace GroceryGame.UI
{
    public class IngredientItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ingredientText;

        public void SetIngredient(IngredientType type, int quantity)
        {
            if (ingredientText == null)
            {
                // Create text component if it doesn't exist
                ingredientText = GetComponent<TextMeshProUGUI>();
                if (ingredientText == null)
                {
                    ingredientText = gameObject.AddComponent<TextMeshProUGUI>();
                }
            }

            // Set the text
            string quantityText = quantity > 1 ? $" x{quantity}" : "";
            ingredientText.text = $"• {type}{quantityText}";
            ingredientText.fontSize = 14;
            ingredientText.color = Color.white;
        }

        public void SetCollected(bool collected)
        {
            if (ingredientText != null)
            {
                ingredientText.color = collected ? Color.green : Color.white;
                if (collected)
                {
                    ingredientText.text = "*" + ingredientText.text.Substring(2); // Replace bullet with checkmark
                }
            }
        }
    }
}