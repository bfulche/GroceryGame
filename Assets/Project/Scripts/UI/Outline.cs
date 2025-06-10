using UnityEngine;
using System.Collections.Generic;

namespace GroceryGame.UI
{
    [RequireComponent(typeof(Renderer))]
    public class Outline : MonoBehaviour
    {
        [Header("Outline Settings")]
        [SerializeField] private Color outlineColor = Color.yellow;
        [SerializeField] private float outlineWidth = 0.03f;
        [SerializeField] private bool enableOnStart = false;

        private Renderer objectRenderer;
        private Material[] originalMaterials;
        private Material[] outlineMaterials;

        private bool isOutlineEnabled = false;

        private void Awake()
        {
            objectRenderer = GetComponent<Renderer>();

            // Store original materials
            originalMaterials = objectRenderer.sharedMaterials;

            // Create highlighted versions of the materials
            List<Material> highlightMats = new List<Material>();
            foreach (Material mat in originalMaterials)
            {
                Material highlightMat = new Material(mat);
                highlightMat.EnableKeyword("_EMISSION");
                highlightMat.SetColor("_EmissionColor", outlineColor * 0.5f);
                highlightMats.Add(highlightMat);
            }
            outlineMaterials = highlightMats.ToArray();

            if (enableOnStart)
            {
                EnableOutline();
            }
        }

        public void EnableOutline()
        {
            if (!isOutlineEnabled && objectRenderer != null)
            {
                objectRenderer.materials = outlineMaterials;
                isOutlineEnabled = true;
            }
        }

        public void DisableOutline()
        {
            if (isOutlineEnabled && objectRenderer != null)
            {
                objectRenderer.materials = originalMaterials;
                isOutlineEnabled = false;
            }
        }

        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            // Recreate highlight materials with new color
            if (outlineMaterials != null)
            {
                for (int i = 0; i < outlineMaterials.Length; i++)
                {
                    outlineMaterials[i].SetColor("_EmissionColor", outlineColor * 0.5f);
                }
            }
        }

        public void SetOutlineWidth(float width)
        {
            outlineWidth = width;
            // Width doesn't apply to emission-based highlighting
        }

        private void OnDestroy()
        {
            // Clean up created materials
            if (outlineMaterials != null)
            {
                foreach (Material mat in outlineMaterials)
                {
                    if (mat != null)
                        Destroy(mat);
                }
            }
        }
    }
}