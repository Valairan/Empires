using UnityEngine;
using UnityEditor;

public class ChannelPacker : EditorWindow
{
    Texture2D redTexture;
    Texture2D greenTexture;
    Texture2D blueTexture;
    Texture2D alphaTexture;

    string outputName = "PackedTexture";

    [MenuItem("Tools/Texture Packer")]
    public static void ShowWindow()
    {
        GetWindow<ChannelPacker>("Texture Packer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Assign Textures for Channels", EditorStyles.boldLabel);

        redTexture = (Texture2D)EditorGUILayout.ObjectField("Red Channel", redTexture, typeof(Texture2D), false);
        greenTexture = (Texture2D)EditorGUILayout.ObjectField("Green Channel", greenTexture, typeof(Texture2D), false);
        blueTexture = (Texture2D)EditorGUILayout.ObjectField("Blue Channel", blueTexture, typeof(Texture2D), false);
        alphaTexture = (Texture2D)EditorGUILayout.ObjectField("Alpha Channel", alphaTexture, typeof(Texture2D), false);

        outputName = EditorGUILayout.TextField("Output Name", outputName);

        if (GUILayout.Button("Pack Textures"))
        {
            PackTextures();
        }
    }

    private void PackTextures()
    {
        // Determine output size from first non-null texture
        Texture2D[] textures = { redTexture, greenTexture, blueTexture, alphaTexture };
        Vector2Int size = Vector2Int.zero;

        foreach (var tex in textures)
        {
            if (tex != null)
            {
                size = new Vector2Int(tex.width, tex.height);
                break;
            }
        }

        if (size == Vector2Int.zero)
        {
            Debug.LogError("No textures assigned!");
            return;
        }

        // Create new texture
        Texture2D packed = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                float r = SampleTexture(redTexture, x, y, size);
                float g = SampleTexture(greenTexture, x, y, size);
                float b = SampleTexture(blueTexture, x, y, size);
                float a = SampleTexture(alphaTexture, x, y, size);

                packed.SetPixel(x, y, new Color(r, g, b, a));
            }
        }

        packed.Apply();

        // Save the texture as an asset
        string path = "Assets/" + outputName + ".png";
        System.IO.File.WriteAllBytes(path, packed.EncodeToPNG());
        AssetDatabase.Refresh();

        Debug.Log("Packed texture saved at: " + path);
    }

    private float SampleTexture(Texture2D tex, int x, int y, Vector2Int size)
    {
        if (tex == null) return 0f;
        // Get pixel color using bilinear sampling
        float u = (x + 0.5f) / size.x;
        float v = (y + 0.5f) / size.y;
        return tex.GetPixelBilinear(u, v).grayscale; // pack as grayscale
    }
}
