using System.IO;
using UnityEditor;
using UnityEngine;

public static class BracketSpriteGenerator
{
    private const int Size = 256;
    private const int Samples = 4;
    private const string OutputPath = "Assets/Visuals/UI/bracket-256.png";

    private static readonly float Thickness = 0.05f;
    private static readonly float CornerLength = 0.25f;

    [MenuItem("Tools/Generate Bracket Sprite")]
    private static void Generate()
    {
        if (Thickness <= 0f || Thickness >= CornerLength || CornerLength > 0.5f)
        {
            Debug.LogError($"[Bracket] need 0 < Thickness ({Thickness}) < CornerLength ({CornerLength}) <= 0.5");
            return;
        }

        var pixels = new Color32[Size * Size];
        var step = 1f / (Size * Samples);

        for (var py = 0; py < Size; py++)
        for (var px = 0; px < Size; px++)
        {
            var hits = 0;
            for (var sy = 0; sy < Samples; sy++)
            for (var sx = 0; sx < Samples; sx++)
                if (Inside((px * Samples + sx + 0.5f) * step, (py * Samples + sy + 0.5f) * step)) hits++;

            var alpha = (byte)(255 * hits / (Samples * Samples));
            pixels[(Size - 1 - py) * Size + px] = new Color32(255, 255, 255, alpha);
        }

        var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();

        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
        File.WriteAllBytes(OutputPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(OutputPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        Debug.Log($"[Bracket] {Size}x{Size} thickness={Thickness:0.###} cornerLength={CornerLength:0.###} -> {OutputPath}");
    }

    private static bool Inside(float x, float y)
    {
        var dx = Mathf.Min(x, 1f - x);
        var dy = Mathf.Min(y, 1f - y);
        return (dx < Thickness && dy < CornerLength) || (dy < Thickness && dx < CornerLength);
    }
}
