using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Linq;
using UnityEditor.SceneManagement;

public class HierarchySnapshotter
{
    private const int MaxChildrenToDetail = 10;
    private static HashSet<string> scriptableObjectReferences = new HashSet<string>();

    [MenuItem("Tools/Snapshot Hierarchy")]
    public static void TakeSnapshot()
    {
        string documentationPath = Path.Combine(Application.dataPath, "Documentation");
        if (!Directory.Exists(documentationPath))
        {
            Directory.CreateDirectory(documentationPath);
        }

        string path = Path.Combine(documentationPath, "HierarchySnapshot.md");

        using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
        {
            writer.WriteLine($"# Hierarchy Snapshot for Scene: {EditorSceneManager.GetActiveScene().name}");
            writer.WriteLine($"**Generated on:** {System.DateTime.Now}");
            writer.WriteLine("---");
            writer.WriteLine();

            scriptableObjectReferences.Clear();

            GameObject[] rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects)
            {
                ProcessGameObject(rootObject, writer, 1);
            }

            if (scriptableObjectReferences.Count > 0)
            {
                writer.WriteLine("# Referenced ScriptableObject Assets");
                writer.WriteLine("---");
                writer.WriteLine("The following ScriptableObjects were found to be referenced by scripts in this scene:");
                foreach (string soName in scriptableObjectReferences)
                {
                    writer.WriteLine($"- `{soName}`");
                }
                writer.WriteLine();
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Hierarchy snapshot saved and asset database refreshed.");
    }

    private static void ProcessGameObject(GameObject obj, StreamWriter writer, int level)
    {
        string headingPrefix = new string('#', level + 1);
        writer.WriteLine($"{headingPrefix} {obj.name}");
        writer.WriteLine($"- **Tag:** `{obj.tag}`");
        writer.WriteLine($"- **Layer:** `{LayerMask.LayerToName(obj.layer)}`");

        Component[] components = obj.GetComponents<Component>();
        writer.WriteLine($"**Components ({components.Length}):**");
        foreach (Component comp in components)
        {
            if (comp != null)
            {
                string componentType = comp.GetType().Name;
                if (comp is MonoBehaviour)
                {
                    writer.WriteLine($"- **Script:** `{componentType}`");
                    ProcessComponentReferences(comp, writer);
                }
                else
                {
                    writer.WriteLine($"- **Component:** `{componentType}`");
                }
            }
        }

        int childCount = obj.transform.childCount;
        if (childCount > 0)
        {
            writer.WriteLine($"**Children: ({childCount} total)**");

            int childrenToProcess = Mathf.Min(MaxChildrenToDetail, childCount);
            for (int i = 0; i < childrenToProcess; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                ProcessGameObject(child, writer, level + 1);
            }

            if (childCount > MaxChildrenToDetail)
            {
                writer.WriteLine($"...and {childCount - MaxChildrenToDetail} more children.");
            }
        }
        writer.WriteLine();
    }

    private static void ProcessComponentReferences(Component comp, StreamWriter writer)
    {
        FieldInfo[] fields = comp.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        string indent = "    ";

        foreach (FieldInfo field in fields)
        {
            if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
            {
                object value = field.GetValue(comp);

                if (value is UnityEngine.Object unityObjectValue)
                {
                    if (unityObjectValue == null) continue;
                }
                else if (value == null)
                {
                    continue;
                }

                if (value is GameObject referencedObj)
                {
                    writer.WriteLine($"{indent}- **Reference:** `{field.Name}` -> `GameObject: {referencedObj.name}`");
                }
                else if (value is ScriptableObject referencedSO)
                {
                    writer.WriteLine($"{indent}- **Reference:** `{field.Name}` -> `ScriptableObject: {referencedSO.name}`");
                    scriptableObjectReferences.Add(referencedSO.name);
                }
                else if (value is IList list)
                {
                    foreach (object listItem in list)
                    {
                        if (listItem != null)
                        {
                            FieldInfo[] listItemFields = listItem.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                            foreach (FieldInfo listItemField in listItemFields)
                            {
                                object listItemValue = listItemField.GetValue(listItem);

                                if (listItemValue is ScriptableObject so)
                                {
                                    writer.WriteLine($"{indent}- **List Reference:** `{field.Name}` -> `ScriptableObject: {so.name}`");
                                    scriptableObjectReferences.Add(so.name);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}