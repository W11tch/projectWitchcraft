using UnityEditor;
using UnityEngine;
using ProjectWitchcraft.Core;

public static class AssetGuidPopulator
{
    [MenuItem("ProjectWitchcraft/Populate Asset GUIDs")]
    public static void PopulateAllGuids()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        int updated = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item == null) continue;

            // Write the GUID directly via SerializedObject so we don't need to call OnValidate.
            var so = new SerializedObject(item);
            var prop = so.FindProperty("_assetGuid");
            if (prop == null) continue;

            if (prop.stringValue != guid)
            {
                prop.stringValue = guid;
                so.ApplyModifiedPropertiesWithoutUndo();
                updated++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[AssetGuidPopulator] Done. {updated} asset(s) updated out of {guids.Length} found.");
    }
}
