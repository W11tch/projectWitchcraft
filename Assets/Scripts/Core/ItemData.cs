using UnityEngine;

namespace ProjectWitchcraft.Core
{
    public class ItemData : ScriptableObject
    {
        [Header("Item Information")]
        public string Name;
        public Sprite Icon;
        [Tooltip("The maximum number of this item that can be held in a single inventory slot.")]
        public int maxStackSize = 9999;
        [TextArea]
        public string description;

        // -1 means no durability. Override in durable item types (EquipmentItemData, WeaponItemData, ToolItemData).
        public virtual float GetMaxDurability() => -1f;

        // Populated automatically in the editor via OnValidate.
        // Used by the save system so renaming an asset never breaks saves.
        [SerializeField, HideInInspector] private string _assetGuid;
        public string AssetGuid => _assetGuid;

#if UNITY_EDITOR
        private void OnValidate()
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
