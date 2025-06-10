using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using GroceryGame.Shopping;
using GroceryGame.Core;

namespace GroceryGame.UI
{
    public class ItemExaminationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private Text itemNameText;
        [SerializeField] private Text itemDescriptionText;
        [SerializeField] private Text itemPriceText;
        [SerializeField] private Text itemQualityText;

        [Header("Visual Effects")]
        [SerializeField] private Material blurMaterial;
        [SerializeField] private float examineScale = 2f;
        [SerializeField] private float rotationSpeed = 50f;

        // Current item being examined
        private GroceryItem currentItem;
        private Transform itemTransform;
        private Vector3 originalScale;
        private Vector3 originalPosition;

        // Blur background
        private GameObject blurOverlay;
        private Image blurImage;

        // State
        private bool isExamining = false;

        private void Awake()
        {
            // Create UI elements if not assigned
            if (infoPanel == null)
            {
                CreateInfoPanel();
            }

            // Create blur overlay
            CreateBlurOverlay();

            // Hide by default
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (isExamining && itemTransform != null)
            {
                HandleRotationInput();
            }
        }

        public void StartExamination(GroceryItem item)
        {
            if (item == null || isExamining) return;

            currentItem = item;
            itemTransform = item.transform;
            isExamining = true;

            // Store original transform
            originalScale = itemTransform.localScale;
            originalPosition = itemTransform.localPosition;

            // Scale up and center the item
            itemTransform.localScale = originalScale * examineScale;
            itemTransform.localPosition = new Vector3(0f, 0f, 0.8f);
            itemTransform.localRotation = Quaternion.identity;

            // Update UI elements
            UpdateInfoPanel();

            // Show UI and blur
            if (infoPanel != null)
                infoPanel.SetActive(true);

            if (blurOverlay != null)
                blurOverlay.SetActive(true);

            // Disable item outline during examination
            Outline outline = item.GetComponent<Outline>();
            if (outline != null)
            {
                outline.DisableOutline();
            }
        }

        public void StopExamination()
        {
            if (!isExamining) return;

            isExamining = false;

            // Restore item transform
            if (itemTransform != null)
            {
                itemTransform.localScale = originalScale;
                itemTransform.localPosition = originalPosition;

                // Re-enable outline
                Outline outline = itemTransform.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.SetOutlineColor(Color.green);
                    outline.EnableOutline();
                }
            }

            // Hide UI and blur
            if (infoPanel != null)
                infoPanel.SetActive(false);

            if (blurOverlay != null)
                blurOverlay.SetActive(false);

            currentItem = null;
            itemTransform = null;
        }

        private void HandleRotationInput()
        {
            // Rotate with WASD
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal != 0)
            {
                itemTransform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime, Space.World);
            }

            if (vertical != 0)
            {
                itemTransform.Rotate(Vector3.right, -vertical * rotationSpeed * Time.deltaTime, Space.World);
            }

            // Also rotate with mouse movement
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                itemTransform.Rotate(Vector3.up, mouseX * rotationSpeed * 2f, Space.World);
                itemTransform.Rotate(Vector3.right, -mouseY * rotationSpeed * 2f, Space.World);
            }
        }

        private void UpdateInfoPanel()
        {
            if (currentItem == null) return;

            if (itemNameText != null)
                itemNameText.text = currentItem.ItemName;

            if (itemDescriptionText != null)
                itemDescriptionText.text = currentItem.Description;

            if (itemPriceText != null)
                itemPriceText.text = $"${currentItem.Price:F2}";

            if (itemQualityText != null)
            {
                itemQualityText.text = currentItem.Quality.ToString();

                // Color code quality
                switch (currentItem.Quality)
                {
                    case ItemQuality.Generic:
                        itemQualityText.color = new Color(0.6f, 0.6f, 0.6f);
                        break;
                    case ItemQuality.Standard:
                        itemQualityText.color = Color.white;
                        break;
                    case ItemQuality.Premium:
                        itemQualityText.color = new Color(1f, 0.843f, 0f); // Gold
                        break;
                }
            }
        }

        private void CreateInfoPanel()
        {
            // Create info panel in top-right corner
            infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(transform, false);

            RectTransform panelRect = infoPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.sizeDelta = new Vector2(300, 200);
            panelRect.anchoredPosition = new Vector2(-20, -20);

            // Add background
            Image bg = infoPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.9f);

            // Add vertical layout
            VerticalLayoutGroup layout = infoPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 15, 15);
            layout.spacing = 10;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Create text elements
            itemNameText = CreateTextElement("Item Name", infoPanel.transform, 24, FontStyle.Bold);
            itemDescriptionText = CreateTextElement("Description", infoPanel.transform, 16, FontStyle.Normal);
            CreateDivider(infoPanel.transform);
            itemQualityText = CreateTextElement("Quality", infoPanel.transform, 18, FontStyle.Normal);
            itemPriceText = CreateTextElement("Price", infoPanel.transform, 20, FontStyle.Bold);

            // Add instruction text at bottom
            GameObject instructionObj = new GameObject("Instructions");
            instructionObj.transform.SetParent(infoPanel.transform, false);
            Text instructionText = instructionObj.AddComponent<Text>();
            instructionText.text = "WASD or Mouse to rotate\nRelease Right Click to exit";
            instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            instructionText.fontSize = 14;
            instructionText.fontStyle = FontStyle.Italic;
            instructionText.color = new Color(0.7f, 0.7f, 0.7f);
            instructionText.alignment = TextAnchor.MiddleCenter;
        }

        private Text CreateTextElement(string name, Transform parent, int fontSize, FontStyle style)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Text text = obj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            return text;
        }

        private void CreateDivider(Transform parent)
        {
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(parent, false);
            Image img = divider.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.3f);
            LayoutElement le = divider.AddComponent<LayoutElement>();
            le.preferredHeight = 2;
        }

        private void CreateBlurOverlay()
        {
            // Create fullscreen blur overlay
            blurOverlay = new GameObject("BlurOverlay");
            blurOverlay.transform.SetParent(transform, false);
            blurOverlay.transform.SetAsFirstSibling(); // Put behind other UI

            RectTransform rect = blurOverlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            blurImage = blurOverlay.AddComponent<Image>();
            blurImage.color = new Color(0, 0, 0, 0.7f); // Dark overlay for now

            // Note: For actual blur effect, you would need a blur shader
            // or post-processing effect. This creates a dark overlay effect.

            blurOverlay.SetActive(false);
        }
    }
}