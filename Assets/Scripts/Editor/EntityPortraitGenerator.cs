using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

public static class EntityPortraitGenerator
{
    private const int Size = 512;
    private const float FocusHeight = 1f;
    private const int CropPadding = 4;
    private const float Padding = 1.05f;
    private const string OutputFolder = "Assets/Visuals/Portraits";

    [MenuItem("Assets/Generate Entity Portrait", true)]
    private static bool Validate() => Selection.GetFiltered<EntityAssets>(SelectionMode.Assets).Length > 0;

    [MenuItem("Assets/Generate Entity Portrait")]
    private static void Generate()
    {
        foreach (var assets in Selection.GetFiltered<EntityAssets>(SelectionMode.Assets)) GenerateFor(assets);
        AssetDatabase.SaveAssets();
    }

    private static void GenerateFor(EntityAssets assets)
    {
        if (assets.Model3D == null)
        {
            Debug.LogWarning($"[Portrait] {assets.Id} has no Model3D");
            return;
        }

        var stage = new GameObject("PortraitStage") { hideFlags = HideFlags.HideAndDontSave };
        stage.transform.position = new Vector3(0f, 10000f, 0f);

        var model = Object.Instantiate(assets.Model3D, stage.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        PoseFirstFrame(model);

        if (!TryGetBounds(model, out var bounds))
        {
            Debug.LogWarning($"[Portrait] {assets.Id} model has no renderers");
            Object.DestroyImmediate(stage);
            return;
        }

        var camera = BuildCamera(stage.transform, bounds);
        var capture = Capture(camera);
        Object.DestroyImmediate(stage);

        var texture = CropToContent(capture);
        if (texture != capture) Object.DestroyImmediate(capture);

        Directory.CreateDirectory(OutputFolder);
        var path = $"{OutputFolder}/{assets.Id}_Portrait.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        assets.Model2D = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        EditorUtility.SetDirty(assets);
        Debug.Log($"[Portrait] {assets.Id} -> {path}");
    }

    private static Texture2D CropToContent(Texture2D source)
    {
        var pixels = source.GetPixels32();
        int minX = source.width, minY = source.height, maxX = -1, maxY = -1;

        for (var y = 0; y < source.height; y++)
        for (var x = 0; x < source.width; x++)
        {
            if (pixels[y * source.width + x].a == 0) continue;
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        if (maxX < 0) return source;

        minX = Mathf.Max(0, minX - CropPadding);
        minY = Mathf.Max(0, minY - CropPadding);
        maxX = Mathf.Min(source.width - 1, maxX + CropPadding);
        maxY = Mathf.Min(source.height - 1, maxY + CropPadding);

        var width = maxX - minX + 1;
        var height = maxY - minY + 1;
        if (width == source.width && height == source.height) return source;

        var cropped = new Texture2D(width, height, TextureFormat.RGBA32, false);
        cropped.SetPixels(source.GetPixels(minX, minY, width, height), 0);
        cropped.Apply();
        return cropped;
    }

    private static void PoseFirstFrame(GameObject model)
    {
        foreach (var skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            skinned.updateWhenOffscreen = true;

        var animator = model.GetComponentInChildren<Animator>();
        if (animator == null) return;

        var clip = DefaultClip(animator.runtimeAnimatorController);
        if (clip == null) return;

        clip.SampleAnimation(animator.gameObject, 0f);
    }

    private static AnimationClip DefaultClip(RuntimeAnimatorController controller)
    {
        if (controller == null) return null;

        if (controller is AnimatorController editable)
            foreach (var layer in editable.layers)
                if (layer.stateMachine != null && layer.stateMachine.defaultState != null
                    && layer.stateMachine.defaultState.motion is AnimationClip stateClip)
                    return stateClip;

        var clips = controller.animationClips;
        return clips != null && clips.Length > 0 ? clips[0] : null;
    }

    private static bool TryGetBounds(GameObject model, out Bounds bounds)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        bounds = default;
        if (renderers.Length == 0) return false;
        bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return true;
    }

    private static Camera BuildCamera(Transform stage, Bounds bounds)
    {
        var cameraObject = new GameObject("PortraitCamera");
        cameraObject.transform.SetParent(stage);

        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;
        camera.orthographic = true;
        var halfHeight = bounds.extents.y * FocusHeight;
        var focus = new Vector3(bounds.center.x, bounds.max.y - halfHeight, bounds.center.z);

        camera.orthographicSize = Mathf.Max(halfHeight, bounds.extents.x) * Padding;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = bounds.size.magnitude * 4f + 10f;
        camera.transform.position = focus + Vector3.forward * (bounds.extents.magnitude * 2f + 1f);
        camera.transform.LookAt(focus);
        camera.enabled = false;

        var lightObject = new GameObject("PortraitLight");
        lightObject.transform.SetParent(cameraObject.transform);
        lightObject.transform.localPosition = Vector3.zero;
        lightObject.transform.localRotation = Quaternion.Euler(25f, -20f, 0f);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;

        return camera;
    }

    private static Texture2D Capture(Camera camera)
    {
        var target = RenderTexture.GetTemporary(Size, Size, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = target;

        if (GraphicsSettings.currentRenderPipeline != null)
            camera.SubmitRenderRequest(new RenderPipeline.StandardRequest { destination = target });
        else
            camera.Render();

        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
        texture.Apply();
        RenderTexture.active = previous;

        camera.targetTexture = null;
        RenderTexture.ReleaseTemporary(target);
        return texture;
    }
}
