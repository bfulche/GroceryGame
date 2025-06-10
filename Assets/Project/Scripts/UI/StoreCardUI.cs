// StoreCardUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GroceryGame.Data;

namespace GroceryGame.UI
{
    public class StoreCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image storeImage;
        [SerializeField] private TextMeshProUGUI storeNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceModifierText;
        [SerializeField] private Button selectButton;

        private StoreLocation store;

        public void Initialize(StoreLocation storeData, bool showSelectButton = true)
        {
            store = storeData;

            // Populate basic info
            if (storeNameText != null)
                storeNameText.text = store.storeName;

            if (descriptionText != null)
                descriptionText.text = store.description;

            // Set store image if available
            if (storeImage != null && store.storeImage != null)
            {
                storeImage.sprite = store.storeImage;
                storeImage.gameObject.SetActive(true);
            }
            else if (storeImage != null)
            {
                storeImage.gameObject.SetActive(false);
            }

            // Show price modifier info
            if (priceModifierText != null)
            {
                if (store.priceModifier < 1.0f)
                {
                    priceModifierText.text = "Budget Friendly";
                    priceModifierText.color = Color.green;
                }
                else if (store.priceModifier > 1.0f)
                {
                    priceModifierText.text = "Premium Prices";
                    priceModifierText.color = Color.yellow;
                }
                else
                {
                    priceModifierText.text = "Standard Prices";
                    priceModifierText.color = Color.white;
                }
            }

            // Handle select button
            if (selectButton != null)
            {
                selectButton.gameObject.SetActive(showSelectButton);
                if (showSelectButton)
                {
                    selectButton.onClick.RemoveAllListeners();
                    selectButton.onClick.AddListener(OnSelectButtonClicked);
                }
            }
        }

        private void OnSelectButtonClicked()
        {
            Debug.Log($"Store card select button clicked for: {store.storeName}");
        }
    }
}