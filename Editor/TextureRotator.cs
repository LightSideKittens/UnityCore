using UnityEngine;
using UnityEditor;

public static class TextureRotator
{
    private const string Section = LSPaths.AssetMenuItem.Root + "/" + nameof(TextureRotator);

    [MenuItem(Section + "/Rotate Left")]
    private static void RotateLeft()
    {
        RotateTexture(1);
    }

    [MenuItem(Section + "/Rotate 180")]
    private static void Rotate180()
    {
        RotateTexture(2);
    }
    
    [MenuItem(Section + "/Rotate Right")]
    private static void RotateRight()
    {
        RotateTexture(3);
    }

    [MenuItem(Section + "/Rotate Texture Left", true)]
    [MenuItem(Section + "/Rotate Texture Right", true)]
    private static bool ValidateRotateTexture(MenuCommand command)
    {
        return Selection.activeObject is Texture2D;
    }

    private static void RotateTexture(int rotations)
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        bool wasReadable = importer.isReadable;
        if (!wasReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        try
        {
            Color[] originalPixels = texture.GetPixels();
            int width = texture.width;
            int height = texture.height;
            Color[] newPixels;

            switch (rotations % 4)
            {
                case 1: // 90 Degrees Clockwise
                    newPixels = RotatePixels90(originalPixels, width, height);
                    (width, height) = (height, width);
                    break;
                case 2: // 180 Degrees
                    newPixels = RotatePixels180(originalPixels, width, height);
                    break;
                case 3: // 270 Degrees Clockwise (or 90 Degrees Counter-Clockwise)
                    newPixels = RotatePixels270(originalPixels, width, height);
                    (width, height) = (height, width);
                    break;
                default:
                    newPixels = originalPixels; // No rotation
                    break;
            }

            Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            newTexture.SetPixels(newPixels);
            newTexture.Apply();

            byte[] bytes = newTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            Debug.Log($"Texture rotated {rotations * 90} degrees and saved.");
        }
        finally
        {
            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }
    }

    private static Color[] RotatePixels90(Color[] pixels, int width, int height)
    {
        Color[] newPixels = new Color[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                newPixels[x * height + (height - y - 1)] = pixels[y * width + x];
            }
        }

        return newPixels;
    }

    private static Color[] RotatePixels180(Color[] pixels, int width, int height)
    {
        Color[] newPixels = new Color[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            newPixels[i] = pixels[pixels.Length - 1 - i];
        }

        return newPixels;
    }

    private static Color[] RotatePixels270(Color[] pixels, int width, int height)
    {
        Color[] newPixels = new Color[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                newPixels[(width - x - 1) * height + y] = pixels[y * width + x];
            }
        }

        return newPixels;
    }
}