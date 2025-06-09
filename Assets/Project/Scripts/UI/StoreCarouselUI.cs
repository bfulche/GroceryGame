// StoreCarouselUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GroceryGame.Data;
using GroceryGame.Core;

namespace GroceryGame.UI
{
    public class StoreCarouselUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button leftArrowButton;
        [SerializeField] private Button rightArrowButton;
        [SerializeField] private Transform storeDisplayArea;

        [Header("Store Info")]
        [SerializeField] private TextMeshProUGUI storeCounterText;
        [SerializeField] private Button selectStoreButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject storeCardPrefab;

        // Current state
        private List<StoreLocation> availableStores = new List<StoreLocation>();
        private int currentStoreIndex = 0;
        private GameObject currentStoreCard;
                
        private void Start()
        {

            Debug.Log(" StoreCarouselUI.Start() - Component started");            //DEBUG BUTTONS
            Debug.Log($"   GameObject active: {gameObject.activeInHierarchy}");   //DEBUG BUTTONS
            Debug.Log($"   Component enabled: {enabled}");                        //DEBUG BUTTONS

            // Connect button events
            leftArrowButton.onClick.AddListener(ShowPreviousStore);
            rightArrowButton.onClick.AddListener(ShowNextStore);
            selectStoreButton.onClick.AddListener(SelectCurrentStore);

            StartCoroutine(InitializeAfterFrame());
        }

        private void SetupButtonListeners()
        {
            // Remove any existing listeners first
            leftArrowButton.onClick.RemoveAllListeners();
            rightArrowButton.onClick.RemoveAllListeners();
            selectStoreButton.onClick.RemoveAllListeners();

            // Add new listeners
            leftArrowButton.onClick.AddListener(ShowPreviousStore);
            rightArrowButton.onClick.AddListener(ShowNextStore);
            selectStoreButton.onClick.AddListener(SelectCurrentStore);

            Debug.Log(" Button listeners setup complete");
        }

        private void OnEnable()
        {
            Debug.Log(" StoreCarouselUI.OnEnable() - GameObject/Component enabled");

            // ALWAYS try to initialize when enabled
            if (Application.isPlaying)
            {
                StartCoroutine(DelayedInitialize());
            }
        }

        private System.Collections.IEnumerator DelayedInitialize()
        {
            // Wait a frame to ensure everything is ready
            yield return null;

            Debug.Log(" DelayedInitialize running...");

            // Get stores from StoreManager
            StoreManager storeManager = FindObjectOfType<StoreManager>();
            if (storeManager != null)
            {
                List<StoreLocation> stores = storeManager.GetAvailableStores();
                Debug.Log($" Found {stores.Count} stores in StoreManager");

                if (stores.Count > 0)
                {
                    InitializeCarousel(stores);
                    Debug.Log($" Carousel initialized with {stores.Count} stores");

                    // Debug button states
                    Debug.Log($" Left button interactable: {leftArrowButton.interactable}");
                    Debug.Log($" Right button interactable: {rightArrowButton.interactable}");
                }
                else
                {
                    Debug.LogWarning(" No stores found in StoreManager!");
                }
            }
            else
            {
                Debug.LogError(" StoreManager not found!");
            }
        }

        private void TryInitialize()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(DelayedInitialize());
            }
        }

        // Manual test method
        [ContextMenu("Force Initialize Now")]
        public void ForceInitializeNow()
        {
            Debug.Log(" FORCE INITIALIZE CALLED!");
            StartCoroutine(DelayedInitialize());
        }

        private System.Collections.IEnumerator InitializeAfterFrame()
        {
            // Wait one frame to ensure StoreManager.Start() has completed
            yield return null;

            // Now get stores from StoreManager
            StoreManager storeManager = FindObjectOfType<StoreManager>();
            if (storeManager != null)
            {
                List<StoreLocation> stores = storeManager.GetAvailableStores();
                InitializeCarousel(stores);

                Debug.Log($"Initialized with {stores.Count} stores");
                foreach (var store in stores)
                {
                    Debug.Log($"Store: {store.storeName}");
                }
            }
            else
            {
                Debug.LogError("StoreManager not found! Cannot initialize store carousel.");
            }
        }

        public void InitializeCarousel(List<StoreLocation> stores)
        {
            availableStores = stores;
            currentStoreIndex = 0;

            if (availableStores.Count > 0)
            {
                ShowStoreAtIndex(currentStoreIndex);
            }

            UpdateNavigationButtons();
            UpdateStoreCounter();
        }

        public void ShowNextStore()
        {
            //DEBUG
            Debug.Log(" RIGHT ARROW CLICKED!"); // Add this first line
            Debug.Log($"   Current index: {currentStoreIndex}");
            Debug.Log($"   Available stores: {availableStores.Count}");

            if (availableStores.Count == 0) return;

            currentStoreIndex = (currentStoreIndex + 1) % availableStores.Count;
            ShowStoreAtIndex(currentStoreIndex);
            UpdateNavigationButtons();
            UpdateStoreCounter();

            AudioManager.Instance?.PlaySound("UINavigate");
        }

        public void ShowPreviousStore()
        {
            //DEBUG
            Debug.Log(" LEFT ARROW CLICKED!"); // Add this first line
            Debug.Log($"   Current index: {currentStoreIndex}");
            Debug.Log($"   Available stores: {availableStores.Count}");

            if (availableStores.Count == 0) return;

            currentStoreIndex = (currentStoreIndex - 1 + availableStores.Count) % availableStores.Count;
            ShowStoreAtIndex(currentStoreIndex);
            UpdateNavigationButtons();
            UpdateStoreCounter();

            AudioManager.Instance?.PlaySound("UINavigate");
        }

        private void ShowStoreAtIndex(int index)
        {
            Debug.Log($" ShowStoreAtIndex({index}) called");

            if (currentStoreCard != null)
            {
                Debug.Log("   Destroying old store card");
                Destroy(currentStoreCard);
            }

            if (index >= 0 && index < availableStores.Count)
            {
                StoreLocation store = availableStores[index];
                Debug.Log($"   Creating card for store: '{store.storeName}'"); //DEBUG BUTTONS
                currentStoreCard = Instantiate(storeCardPrefab, storeDisplayArea);

                StoreCardUI cardUI = currentStoreCard.GetComponent<StoreCardUI>();
                if (cardUI != null)
                {
                    cardUI.Initialize(store, false);
                    Debug.Log("    StoreCardUI initialized");
                }
                else
                {
                    Debug.LogError("   StoreCardUI component not found on prefab!");
                }

                // Manual positioning - no layout group interference!
                RectTransform cardRect = currentStoreCard.GetComponent<RectTransform>();

                // Center anchoring
                cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                cardRect.anchoredPosition = Vector2.zero;

                // FIXED SIZE for all store cards
                cardRect.sizeDelta = new Vector2(480f, 240f);  // Fits nicely in 530x280 area

                // Update counter if assigned
                if (storeCounterText != null)
                {
                    storeCounterText.text = $"{currentStoreIndex + 1} / {availableStores.Count}";
                    Debug.Log($"   Counter updated: {storeCounterText.text}"); //DEBUG BUTTONS
                }

                Debug.Log(" ShowStoreAtIndex completed successfully"); //DEBUG BUTTONS
            }
            else //DEBUG BUTTONS
            {
                Debug.LogError($" Invalid store index: {index} (available: {availableStores.Count})"); //DEBUG BUTTONS
            }
        }

        private void UpdateNavigationButtons()
        {
            bool canNavigate = availableStores.Count > 1; //DEBUG
            
            leftArrowButton.interactable = availableStores.Count > 1;
            rightArrowButton.interactable = availableStores.Count > 1;

            Debug.Log($" UpdateNavigationButtons: {availableStores.Count} stores, buttons {(canNavigate ? "ENABLED" : "DISABLED")}"); //DEBUG
        }

        private void UpdateStoreCounter()
        {
            if (storeCounterText != null && availableStores.Count > 0)
            {
                storeCounterText.text = $"Store {currentStoreIndex + 1} of {availableStores.Count}";
            }
        }

        private void SelectCurrentStore()
        {
            if (currentStoreIndex >= 0 && currentStoreIndex < availableStores.Count)
            {
                StoreLocation selectedStore = availableStores[currentStoreIndex];

                StoreManager storeManager = FindObjectOfType<StoreManager>();
                if (storeManager != null)
                {
                    storeManager.SelectStore(selectedStore);
                }

                AudioManager.Instance?.PlaySound("UISelect");
            }
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    ShowPreviousStore();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    ShowNextStore();
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    SelectCurrentStore();
                }
            }
        }
    }
}