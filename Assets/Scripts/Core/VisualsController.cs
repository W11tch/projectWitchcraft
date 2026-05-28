using UnityEngine;

namespace ProjectWitchcraft.Core
{
    public class VisualsController : MonoBehaviour
    {
        // We no longer need to assign the transparent material here,
        // as the script will create it correctly at runtime.

        private Renderer _renderer;
        private Material _originalMaterial;
        private Material _transparentMaterialInstance;

        // Cache the Shader Property IDs for performance.
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();

            if (_renderer != null)
            {
                // Store the object's original, opaque material.
                _originalMaterial = _renderer.sharedMaterial;

                // Create a unique, transparent instance of the original material.
                _transparentMaterialInstance = new Material(_originalMaterial);
                _transparentMaterialInstance.SetFloat("_Surface", 1); // Set to Transparent
                _transparentMaterialInstance.SetFloat("_ZWrite", 0);
                _transparentMaterialInstance.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _transparentMaterialInstance.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _transparentMaterialInstance.SetOverrideTag("RenderType", "Transparent");
                _transparentMaterialInstance.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }

        public void SetIsTransparent(bool isTransparent, float alpha = 0.5f)
        {
            if (_renderer == null) return;

            if (isTransparent)
            {
                // **THE DEFINITIVE FIX**:
                // 1. Get the original color from the original material.
                Color color = _originalMaterial.color;
                // 2. Set the alpha on that color.
                color.a = alpha;
                // 3. Apply the color with the new alpha to our transparent material instance.
                _transparentMaterialInstance.SetColor(ColorProperty, color);

                // 4. Switch the renderer to use the transparent material.
                _renderer.material = _transparentMaterialInstance;
            }
            else
            {
                // Switch back to the original, opaque material.
                _renderer.material = _originalMaterial;
            }
        }

        public void SetColor(Color newColor)
        {
            if (_renderer == null) return;

            _originalMaterial.color = newColor;

            // Also update the transparent instance's color so it matches when we switch.
            _transparentMaterialInstance.color = newColor;
        }
    }
}