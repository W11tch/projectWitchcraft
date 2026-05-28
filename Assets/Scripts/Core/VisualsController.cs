using UnityEngine;

namespace ProjectWitchcraft.Core
{
    // Manages per-object transparency without modifying shared materials.
    // Uses a single material instance for the transparent state and the
    // shared material for the opaque state. MaterialPropertyBlock handles
    // per-frame alpha changes so no extra allocations occur during placement.
    public class VisualsController : MonoBehaviour
    {
        private Renderer _renderer;
        private Material _sharedMaterial;
        private Material _transparentInstance;
        private MaterialPropertyBlock _propertyBlock;

        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;

            _propertyBlock = new MaterialPropertyBlock();
            _sharedMaterial = _renderer.sharedMaterial;

            _transparentInstance = new Material(_sharedMaterial);
            _transparentInstance.SetFloat("_Surface", 1);
            _transparentInstance.SetFloat("_ZWrite", 0);
            _transparentInstance.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _transparentInstance.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _transparentInstance.SetOverrideTag("RenderType", "Transparent");
            _transparentInstance.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private void OnDestroy()
        {
            if (_transparentInstance != null)
                Destroy(_transparentInstance);
        }

        public void SetIsTransparent(bool isTransparent, float alpha = 0.5f)
        {
            if (_renderer == null) return;

            if (isTransparent)
            {
                _renderer.sharedMaterial = _transparentInstance;
                _renderer.GetPropertyBlock(_propertyBlock);
                Color color = _sharedMaterial.GetColor(BaseColorProperty);
                color.a = alpha;
                _propertyBlock.SetColor(BaseColorProperty, color);
                _renderer.SetPropertyBlock(_propertyBlock);
            }
            else
            {
                _renderer.sharedMaterial = _sharedMaterial;
                _renderer.SetPropertyBlock(null);
            }
        }

        public void SetColor(Color newColor)
        {
            if (_renderer == null) return;

            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorProperty, newColor);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
