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
            // Connect button events
            leftArrowButton.onClick.AddListener(ShowPreviousStore);
            rightArrowButton.onClick.AddListener(ShowNextStore);
            selectStoreButton.onClick.AddListener(SelectCurrentStore);
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
            if (availableStores.Count == 0) return;

            currentStoreIndex = (currentStoreIndex + 1) % availableStores.Count;
            ShowStoreAtIndex(currentStoreIndex);
            UpdateNavigationButtons();
            UpdateStoreCounter();

            AudioManager.Instance?.PlaySound("UINavigate");
        }

        public void ShowPreviousStore()
        {
            if (availableStores.Count == 0) return;

            currentStoreIndex = (currentStoreIndex - 1 + availableStores.Count) % availableStores.Count;
            ShowStoreAtIndex(currentStoreIndex);
            UpdateNavigationButtons();
            UpdateStoreCounter();

            AudioManager.Instance?.PlaySound("UINavigate");
        }

        private void ShowStoreAtIndex(int index)
        {
            if (currentStoreCard != null)
            {
                Destroy(currentStoreCard);
            }

            if (index >= 0 && index < availableStores.Count)
            {
                StoreLocation store = availableStores[index];
                currentStoreCard = Instantiate(storeCardPrefab, storeDisplayArea);

                StoreCardUI cardUI = currentStoreCard.GetComponent<StoreCardUI>();
                if (cardUI != null)
                {
                    cardUI.Initialize(store, false);
                }

                // Make it fill the display area
                RectTransform cardRect = currentStoreCard.GetComponent<RectTransform>();
                cardRect.anchorMin = Vector2.zero;
                cardRect.anchorMax = Vector2.one;
                cardRect.offsetMin = Vector2.zero;
                cardRect.offsetMax = Vector2.zero;
            }
        }

        private void UpdateNavigationButtons()
        {
            leftArrowButton.interactable = availableStores.Count > 1;
            rightArrowButton.interactable = availableStores.Count > 1;
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