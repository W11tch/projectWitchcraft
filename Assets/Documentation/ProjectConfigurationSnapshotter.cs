using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditorInternal;
using UnityEngine.InputSystem;

public class ProjectConfigurationSnapshotter
{
    [MenuItem("Tools/Snapshot Project Configuration")]
    public static void TakeSnapshot()
    {
        string documentationPath = Path.Combine(Application.dataPath, "Documentation");
        if (!Directory.Exists(documentationPath))
        {
            Directory.CreateDirectory(documentationPath);
        }

        string path = Path.Combine(documentationPath, "ProjectConfigurationSnapshot.md");

        using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
        {
            writer.WriteLine("# Project Configuration Snapshot");
            writer.WriteLine($"**Generated on:** {System.DateTime.Now}");
            writer.WriteLine("---");
            writer.WriteLine();

            SnapshotTagsAndLayers(writer);
            writer.WriteLine("---");
            writer.WriteLine();

            SnapshotPhysicsSettings(writer);
            writer.WriteLine("---");
            writer.WriteLine();

            SnapshotInputSystem(writer);
        }

        AssetDatabase.Refresh();
        Debug.Log("Project configuration snapshot saved and asset database refreshed.");
    }

    private static void SnapshotTagsAndLayers(StreamWriter writer)
    {
        writer.WriteLine("## Tags and Layers");
        writer.WriteLine("### Tags");
        string[] tags = InternalEditorUtility.tags;
        foreach (string tag in tags)
        {
            writer.WriteLine($"- `{tag}`");
        }
        writer.WriteLine();
        writer.WriteLine("### Layers");
        string[] layers = InternalEditorUtility.layers;
        for (int i = 0; i < layers.Length; i++)
        {
            if (!string.IsNullOrEmpty(layers[i]))
            {
                writer.WriteLine($"- **Layer {i}:** `{layers[i]}`");
            }
        }
        writer.WriteLine();
    }

    private static void SnapshotPhysicsSettings(StreamWriter writer)
    {
        writer.WriteLine("## Physics Settings");
        writer.WriteLine($"- **Gravity:** `{Physics.gravity}`");
        writer.WriteLine($"- **Default Max Depenetration Velocity:** `{Physics.defaultMaxDepenetrationVelocity}`");
        writer.WriteLine($"- **Sleep Threshold:** `{Physics.sleepThreshold}`");
        writer.WriteLine();
        writer.WriteLine("### Collision Matrix");
        writer.WriteLine("| Layer | Collides With |");
        writer.WriteLine("|---|---|");
        string[] layers = InternalEditorUtility.layers.Where(l => !string.IsNullOrEmpty(l)).ToArray();
        foreach (string layer in layers)
        {
            string collidesWith = string.Empty;
            int layerIndex = LayerMask.NameToLayer(layer);
            foreach (string otherLayer in layers)
            {
                int otherLayerIndex = LayerMask.NameToLayer(otherLayer);
                if (!Physics.GetIgnoreLayerCollision(layerIndex, otherLayerIndex))
                {
                    collidesWith += $"`{otherLayer}`, ";
                }
            }
            if (collidesWith.Length > 0)
            {
                collidesWith = collidesWith.Substring(0, collidesWith.Length - 2);
            }
            writer.WriteLine($"| `{layer}` | {collidesWith} |");
        }
        writer.WriteLine();
    }

    private static void SnapshotInputSystem(StreamWriter writer)
    {
        writer.WriteLine("## Input System Configuration");
        InputSettings settings = InputSystem.settings;
        if (settings != null)
        {
            writer.WriteLine("### Global Settings");
            writer.WriteLine($"- **Update Mode:** `{settings.updateMode}`");
            writer.WriteLine();
        }
        writer.WriteLine("### Found Input Actions");
        string[] actionGuids = AssetDatabase.FindAssets("t:InputActionAsset");
        if (actionGuids.Length == 0)
        {
            writer.WriteLine("No InputActionAsset found in the project.");
            return;
        }
        foreach (string guid in actionGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (actions != null)
            {
                writer.WriteLine($"#### {actions.name} (`{path}`)");
                foreach (InputActionMap map in actions.actionMaps)
                {
                    writer.WriteLine($"- **Action Map:** `{map.name}`");
                    foreach (InputAction action in map.actions)
                    {
                        writer.WriteLine($"  - **Action:** `{action.name}` (`{action.type}` with `{action.expectedControlType}`)");
                        foreach (InputBinding binding in action.bindings)
                        {
                            writer.WriteLine($"    - **Binding:** `{binding.effectivePath}`");
                        }
                    }
                }
            }
        }
    }
}