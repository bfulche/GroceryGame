using System.Collections.Generic;
using UnityEngine;

namespace GroceryGame.Data
{
    [CreateAssetMenu(fileName = "New Store", menuName = "Grocery Game/Store Location")]
    public class StoreLocation : ScriptableObject
    {
        [Header("Basic Info")]
        public string storeName;
        public string description;
        public Sprite storeImage;

        [Header("Scene Reference")]
        public string sceneName; // The name of the Unity scene for this store

        [Header("Store Properties")]
        [Tooltip("Which ingredients are available in this store")]
        public List<IngredientType> availableIngredients = new List<IngredientType>();

        [Tooltip("Multiplier applied to all item prices in this store (1.0 = normal price)")]
        [Range(0.5f, 2.0f)]
        public float priceModifier = 1.0f;

        [Header("Unlock Settings")]
        public bool isStartingStore = false; // Is this store available from the beginning?
    }
}