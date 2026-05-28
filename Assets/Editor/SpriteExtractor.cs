// Located at: Assets/Editor/SpriteExtractor.cs
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// This script adds a new menu item to Unity that allows you to select a UI Image
/// and save its sprite's texture as a new PNG file in your Assets folder.
/// It correctly handles sprites that are part of a texture atlas.
/// </summary>
public class SpriteExtractor
{
    [MenuItem("Assets/Extract Sprite Texture")]
    public static void ExtractSpriteTexture()
    {
        // Get the currently selected GameObject in the Hierarchy
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject in the Hierarchy.", "OK");
            return;
        }

        // Check if the selected object has an Image component
        Image image = selectedObject.GetComponent<Image>();
        if (image == null || image.sprite == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected object must have a UI Image component with a Sprite assigned.", "OK");
            return;
        }

        Sprite sprite = image.sprite;
        Texture2D sourceTexture = sprite.texture;

        // Create a new, temporary readable texture
        RenderTexture tmp = RenderTexture.GetTemporary(
            sourceTexture.width,
            sourceTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(sourceTexture, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        // Create a new Texture2D to copy the pixels into
        Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);


        // Create a new texture that is the size of the sprite's rect
        Texture2D newTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

        // Get the pixels from the readable texture within the sprite's rect
        Color[] pixels = readableTexture.GetPixels(
            (int)sprite.rect.x,
            (int)sprite.rect.y,
            (int)sprite.rect.width,
            (int)sprite.rect.height);

        newTexture.SetPixels(pixels);
        newTexture.Apply();

        // Encode the new texture to a PNG and save it
        byte[] bytes = newTexture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, sprite.name + ".png");
        File.WriteAllBytes(path, bytes);

        Debug.Log("Saved sprite to: " + path);

        // Clean up the readable texture
        Object.DestroyImmediate(readableTexture);

        // Refresh the AssetDatabase to show the new file
        AssetDatabase.Refresh();
    }
}