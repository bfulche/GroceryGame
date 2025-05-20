using System.Collections.Generic;
using UnityEngine;
using GroceryGame.Data;

namespace GroceryGame.Core
{
    public class StoreManager : MonoBehaviour
    {
        // List of all store locations (configured in Unity Inspector)
        [SerializeField] private List<StoreLocation> allStores = new List<StoreLocation>();

        // List of currently available/unlocked stores
        private List<StoreLocation> availableStores = new List<StoreLocation>();

        private void Start()
        {
            // Initialize available stores
            RefreshAvailableStores();
        }

        // Get stores that are currently available to the player
        public List<StoreLocation> GetAvailableStores()
        {
            return availableStores;
        }

        // Refresh the list of available stores
        public void RefreshAvailableStores()
        {
            availableStores.Clear();

            // For the POC, all stores that are marked as starting stores are available
            foreach (var store in allStores)
            {
                if (store.isStartingStore)
                {
                    availableStores.Add(store);
                }
            }

            // Make sure we have at least one store available
            if (availableStores.Count == 0 && allStores.Count > 0)
            {
                Debug.LogWarning("No starting stores found. Making the first store available by default.");
                availableStores.Add(allStores[0]);
            }
        }

        // Select a store to shop at
        public void SelectStore(StoreLocation store)
        {
            if (store != null)
            {
                Debug.Log($"Selected store: {store.storeName}");

                // Tell the GameManager about our selection
                GameManager.Instance.SelectStore(store);
            }
        }

        // Find a store by name
        public StoreLocation FindStoreByName(string storeName)
        {
            return allStores.Find(s => s.storeName == storeName);
        }
    }
}