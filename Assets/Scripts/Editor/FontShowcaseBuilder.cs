using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class FontShowcaseBuilder
{
    private const string OutputPath = "Assets/Prefabs/UI/FontShowcase.prefab";
    private const string HeartPrefabPath = "Assets/Prefabs/UI/Health.prefab";
    private const string CounterPrefabPath = "Assets/Prefabs/UI/Counter.prefab";
    private const string FontsFolder = "Assets/Visuals/Fonts";
    private const string HeartNumber = "10";
    private const string CounterNumber = "99";
    private const float LabelWidth = 360f;
    private const float CellWidth = 176f;
    private const float RowHeight = 72f;

    private static readonly string[] Weights = { "ExtraLight", "Light", "Regular", "Medium", "SemiBold", "Bold" };

    private static readonly (string Label, string Folder, string File)[] Families =
    {
        ("Oswald", "Oswald", "Oswald"),
    };

    [MenuItem("Tools/Build Font Showcase")]
    private static void Build()
    {
        var heartPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeartPrefabPath);
        var counterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CounterPrefabPath);
        var headerFont = GetOrCreateFontAsset($"{FontsFolder}/Oswald/Oswald-Regular.ttf") ?? TMP_Settings.defaultFontAsset;

        var root = new GameObject("FontShowcase", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(FontShowcase));
        root.GetComponent<Image>().color = new Color32(30, 34, 42, 255);
        var column = root.GetComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(24, 24, 24, 24);
        column.spacing = 16;
        column.childControlWidth = false;
        column.childControlHeight = false;
        column.childForceExpandWidth = false;
        column.childForceExpandHeight = false;
        Fit(root);

        var header = AddRow(root.transform, "Weights");
        AddSpacer(header, LabelWidth);
        foreach (var weight in Weights) AddText(header, weight, headerFont, CellWidth, 28, TextAlignmentOptions.Center);

        foreach (var (label, folder, file) in Families)
        {
            var fonts = new TMP_FontAsset[Weights.Length];
            TMP_FontAsset labelFont = null;
            for (var i = 0; i < Weights.Length; i++)
            {
                fonts[i] = GetOrCreateFontAsset($"{FontsFolder}/{folder}/{file}-{Weights[i]}.ttf");
                if (labelFont == null || Weights[i] == "Regular") labelFont = fonts[i] ?? labelFont;
            }
            if (labelFont == null) continue;

            var row = AddRow(root.transform, label);
            AddText(row, label, labelFont, LabelWidth, 36, TextAlignmentOptions.MidlineLeft);
            for (var i = 0; i < Weights.Length; i++)
            {
                if (fonts[i] != null) AddCell(row, Weights[i], fonts[i], heartPrefab, counterPrefab);
                else AddSpacer(row, CellWidth);
            }
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, OutputPath);
        Object.DestroyImmediate(root);
        EditorGUIUtility.PingObject(prefab);
        Selection.activeObject = prefab;
    }

    private static TMP_FontAsset GetOrCreateFontAsset(string fontPath)
    {
        if (!File.Exists(fontPath)) return null;
        var assetPath = Path.ChangeExtension(fontPath, null) + " SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (existing != null) return existing;

        var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null) return null;

        var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
        fontAsset.name = Path.GetFileNameWithoutExtension(assetPath);
        AssetDatabase.CreateAsset(fontAsset, assetPath);
        fontAsset.atlasTextures[0].name = fontAsset.name + " Atlas";
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        fontAsset.material.name = fontAsset.name + " Material";
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        fontAsset.TryAddCharacters("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz -");
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        return fontAsset;
    }

    private static Transform AddRow(Transform parent, string name)
    {
        var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        row.transform.SetParent(parent, false);
        Layout(row.GetComponent<HorizontalLayoutGroup>(), 16, TextAnchor.MiddleLeft);
        Fit(row);
        return row.transform;
    }

    private static void AddCell(Transform row, string weight, TMP_FontAsset fontAsset, GameObject heartPrefab, GameObject counterPrefab)
    {
        var cell = new GameObject(weight, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        cell.transform.SetParent(row, false);
        ((RectTransform)cell.transform).sizeDelta = new Vector2(CellWidth, RowHeight);
        Layout(cell.GetComponent<HorizontalLayoutGroup>(), 16, TextAnchor.MiddleCenter);
        AddNumbered(cell.transform, heartPrefab, fontAsset, HeartNumber);
        AddNumbered(cell.transform, counterPrefab, fontAsset, CounterNumber);
    }

    private static void AddSpacer(Transform row, float width)
    {
        var spacer = new GameObject("Empty", typeof(RectTransform));
        spacer.transform.SetParent(row, false);
        ((RectTransform)spacer.transform).sizeDelta = new Vector2(width, RowHeight);
    }

    private static void AddText(Transform row, string text, TMP_FontAsset fontAsset, float width, float size, TextAlignmentOptions alignment)
    {
        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(row, false);
        ((RectTransform)label.transform).sizeDelta = new Vector2(width, RowHeight);
        var tmp = label.GetComponent<TextMeshProUGUI>();
        tmp.font = fontAsset;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.text = text;
    }

    private static void AddNumbered(Transform parent, GameObject prefab, TMP_FontAsset fontAsset, string number)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        foreach (var text in instance.GetComponentsInChildren<TMP_Text>(true))
        {
            text.font = fontAsset;
            text.text = number;
        }
    }

    private static void Layout(HorizontalOrVerticalLayoutGroup layout, float spacing, TextAnchor alignment)
    {
        layout.spacing = spacing;
        layout.childAlignment = alignment;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private static void Fit(GameObject target)
    {
        var fitter = target.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
}
