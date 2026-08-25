using System.IO;
using UnityEditor;
using UnityEngine;

public static class HeartSpriteGenerator
{
    private const int Size = 256;
    private const int Samples = 4;
    private const float Canvas = 1024f;
    private const string OutputPath = "Assets/Visuals/UI/heart-256.png";

    private static readonly float DiameterFraction = 35f / 64f;

    private static readonly float Radius;
    private static readonly float LobeY;
    private static readonly float LeftLobeX;
    private static readonly float RightLobeX;
    private static readonly float CleftY;
    private static readonly float TangentY;
    private static readonly float SideSlope;

    static HeartSpriteGenerator()
    {
        Radius = Canvas * DiameterFraction * 0.5f;
        LobeY = Radius;
        LeftLobeX = Radius;
        RightLobeX = Canvas - Radius;

        var halfGap = (RightLobeX - LeftLobeX) * 0.5f;
        CleftY = LobeY - Mathf.Sqrt(Radius * Radius - halfGap * halfGap);

        var tipX = Canvas * 0.5f;
        var a = tipX - LeftLobeX;
        var b = Canvas - LobeY;
        var distanceSq = a * a + b * b;
        var tangentLength = Mathf.Sqrt(distanceSq - Radius * Radius);
        var dirX = (-a * tangentLength - b * Radius) / distanceSq;
        var dirY = (a * Radius - b * tangentLength) / distanceSq;

        TangentY = Canvas + tangentLength * dirY;
        SideSlope = tangentLength * dirX / (TangentY - Canvas);
    }

    [MenuItem("Tools/Generate Heart Sprite")]
    private static void Generate()
    {
        if (DiameterFraction < 0.5f || DiameterFraction > 1f)
        {
            Debug.LogError($"[Heart] DiameterFraction must be between 0.5 and 1 (got {DiameterFraction})");
            return;
        }

        var pixels = new Color32[Size * Size];
        var scale = Canvas / Size;
        var step = scale / Samples;

        for (var py = 0; py < Size; py++)
        for (var px = 0; px < Size; px++)
        {
            var hits = 0;
            for (var sy = 0; sy < Samples; sy++)
            for (var sx = 0; sx < Samples; sx++)
                if (Inside(px * scale + (sx + 0.5f) * step, py * scale + (sy + 0.5f) * step)) hits++;

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

        Debug.Log($"[Heart] {Size}x{Size} diameter={DiameterFraction:0.###} cleftY={CleftY:0.#} -> {OutputPath}");
    }

    private static bool Inside(float x, float y)
    {
        if (y <= CleftY) return InCircle(x, y, LeftLobeX) || InCircle(x, y, RightLobeX);

        if (y <= TangentY)
        {
            var dy = y - LobeY;
            var dx = Mathf.Sqrt(Mathf.Max(0f, Radius * Radius - dy * dy));
            return x >= LeftLobeX - dx && x <= RightLobeX + dx;
        }

        var edge = Canvas * 0.5f + SideSlope * (y - Canvas);
        return x >= edge && x <= Canvas - edge;
    }

    private static bool InCircle(float x, float y, float centerX)
    {
        var dx = x - centerX;
        var dy = y - LobeY;
        return dx * dx + dy * dy <= Radius * Radius;
    }
}
