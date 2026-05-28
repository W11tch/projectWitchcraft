using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System;
using System.Linq;

public class ContextPackager
{
    private const string ContextFolderName = "Context";
    private const string DocumentationFolderName = "Documentation";

    [MenuItem("Tools/Create Context Package")]
    public static void CreateContextPackage()
    {
        // 1. Call the public methods from the modular snapshot scripts to create the files
        ProjectConfigurationSnapshotter.TakeSnapshot();
        HierarchySnapshotter.TakeSnapshot();

        // 2. Begin the packaging process
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string contextFolderPath = Path.Combine(projectPath, ContextFolderName);

        // Clean up any old context folder to ensure a fresh package
        if (Directory.Exists(contextFolderPath))
        {
            Directory.Delete(contextFolderPath, true);
        }
        Directory.CreateDirectory(contextFolderPath);
        Debug.Log($"Created new context folder at: {contextFolderPath}");

        // Define paths for snapshots
        string documentationPath = Path.Combine(Application.dataPath, DocumentationFolderName);
        string hierarchySnapshotPath = Path.Combine(documentationPath, "HierarchySnapshot.md");
        string configSnapshotPath = Path.Combine(documentationPath, "ProjectConfigurationSnapshot.md");

        // 3. Copy snapshot files into the Context folder
        if (File.Exists(hierarchySnapshotPath))
        {
            File.Copy(hierarchySnapshotPath, Path.Combine(contextFolderPath, "HierarchySnapshot.md"), true);
            Debug.Log($"Copied Hierarchy Snapshot to {ContextFolderName}.");
        }
        else
        {
            Debug.LogError("HierarchySnapshot.md not found! There was an issue generating the snapshot.");
        }

        if (File.Exists(configSnapshotPath))
        {
            File.Copy(configSnapshotPath, Path.Combine(contextFolderPath, "ProjectConfigurationSnapshot.md"), true);
            Debug.Log($"Copied Project Configuration Snapshot to {ContextFolderName}.");
        }
        else
        {
            Debug.LogError("ProjectConfigurationSnapshot.md not found! There was an issue generating the snapshot.");
        }

        // 4. Copy the Assets folder into the Context folder
        string assetsPath = Application.dataPath;
        string assetsCopyPath = Path.Combine(contextFolderPath, "Assets");

        CopyDirectory(assetsPath, assetsCopyPath);
        Debug.Log($"Copied Assets folder to {ContextFolderName}.");

        // 5. Compress the copied Assets folder into a .zip
        string zipFileName = $"assets_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        string zipFilePath = Path.Combine(contextFolderPath, zipFileName);

        // This line is corrected to specify the System.IO.Compression version
        ZipFile.CreateFromDirectory(assetsCopyPath, zipFilePath, System.IO.Compression.CompressionLevel.Fastest, false);
        Directory.Delete(assetsCopyPath, true); // Clean up the uncompressed copy
        Debug.Log($"Compressed Assets folder into {zipFilePath}.");

        // Refresh the Asset Database so the new files appear in the Editor
        AssetDatabase.Refresh();
        Debug.Log("Session context creation complete.");
    }

    /// <summary>
    /// Recursively copies a directory and its contents.
    /// </summary>
    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            return;

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }
}