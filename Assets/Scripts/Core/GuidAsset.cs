using UnityEngine;

namespace ProjectWitchcraft.Core
{
    // Base for any ScriptableObject the save system references by a stable, rename-safe id.
    // The GUID is Unity's own asset GUID, populated automatically in the editor via OnValidate,
    // so renaming or moving the asset never breaks saves.
    public abstract class GuidAsset : ScriptableObject
    {
        [SerializeField, HideInInspector] private string _assetGuid;
        public string AssetGuid => _assetGuid;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path)) return;
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            if (_assetGuid == guid) return;
            _assetGuid = guid;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
