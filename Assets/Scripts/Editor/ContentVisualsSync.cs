using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ContentVisualsSync
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string CharacterPrefabsFolder = "Assets/Prefabs/Characters";

    [MenuItem("Tools/Sync Content Visuals")]
    private static void Sync()
    {
        var created = new List<string>();
        var entities = SyncAssets<EntityConfig, EntityAssets>(typeof(UnitConfigs), "Assets/ScriptableObjects/Entity Assets", created, AssignModel);
        var abilities = SyncAssets<AbilityConfig, AbilityVisuals>(typeof(AbilityConfigs), "Assets/ScriptableObjects/Ability Visuals", created);
        var statuses = SyncAssets<StatusConfig, StatusAssets>(typeof(StatusConfigs), "Assets/ScriptableObjects/Status Assets", created);
        var terrain = SyncAssets<TerrainConfig, TerrainAssets>(typeof(TerrainConfigs), "Assets/ScriptableObjects/Terrain Assets", created);
        AssetDatabase.SaveAssets();

        var scenes = RegisterInScenes(new Dictionary<string, IReadOnlyList<UnityEngine.Object>>
        {
            { "_entityAssets", entities },
            { "_abilityVisuals", abilities },
            { "_statusAssets", statuses },
            { "_terrainAssets", terrain }
        });

        EntityPortraitGenerator.GenerateMissing();

        Debug.Log($"[ContentSync] created {created.Count}: {string.Join(", ", created)}; registered in {string.Join(", ", scenes)}");
    }

    private static List<UnityEngine.Object> SyncAssets<TConfig, TVisuals>(Type container, string folder, List<string> created,
                                                                         Action<TVisuals> configure = null)
        where TConfig : INameId
        where TVisuals : ScriptableObject, INameId
    {
        var byId = new Dictionary<string, TVisuals>();
        foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(TVisuals).Name}"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<TVisuals>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset == null || string.IsNullOrEmpty(asset.Id)) continue;
            if (!byId.TryAdd(asset.Id, asset))
                Debug.LogError($"[ContentSync] two {typeof(TVisuals).Name} assets use id '{asset.Id}'");
        }

        foreach (var field in container.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not TConfig config || byId.ContainsKey(config.Id)) continue;
            Directory.CreateDirectory(folder);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{config.Id}.asset");
            var asset = ScriptableObject.CreateInstance<TVisuals>();
            AssetDatabase.CreateAsset(asset, path);
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("<Id>k__BackingField").stringValue = config.Id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            configure?.Invoke(asset);
            EditorUtility.SetDirty(asset);
            byId.Add(config.Id, asset);
            created.Add($"{typeof(TVisuals).Name} '{config.Id}'");
        }

        return byId.Values.OrderBy(asset => asset.Id, StringComparer.Ordinal).Cast<UnityEngine.Object>().ToList();
    }

    private static void AssignModel(EntityAssets assets)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{CharacterPrefabsFolder}/{assets.Id.Replace(" ", "")}.prefab");
        if (prefab != null) assets.Model3D = prefab;
        else Debug.LogWarning($"[ContentSync] no prefab named '{assets.Id.Replace(" ", "")}' in {CharacterPrefabsFolder}; set Model3D on '{assets.Id}' by hand");
    }

    private static List<string> RegisterInScenes(Dictionary<string, IReadOnlyList<UnityEngine.Object>> lists)
    {
        var touched = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = SceneManager.GetSceneByPath(path);
            var wasLoaded = scene.isLoaded;
            if (!wasLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            var changed = false;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var bootstrap in root.GetComponentsInChildren<GameBootstrap>(true))
                changed |= Assign(bootstrap, lists);

            if (changed)
            {
                touched.Add(scene.name);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!wasLoaded) EditorSceneManager.SaveScene(scene);
            }
            if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
        }
        return touched;
    }

    private static bool Assign(GameBootstrap bootstrap, Dictionary<string, IReadOnlyList<UnityEngine.Object>> lists)
    {
        var serialized = new SerializedObject(bootstrap);
        var changed = false;
        foreach (var (name, assets) in lists)
        {
            var property = serialized.FindProperty(name);
            if (Matches(property, assets)) continue;
            property.arraySize = assets.Count;
            for (var i = 0; i < assets.Count; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];
            changed = true;
        }
        if (changed) serialized.ApplyModifiedProperties();
        return changed;
    }

    private static bool Matches(SerializedProperty property, IReadOnlyList<UnityEngine.Object> assets)
    {
        if (property.arraySize != assets.Count) return false;
        for (var i = 0; i < assets.Count; i++)
            if (property.GetArrayElementAtIndex(i).objectReferenceValue != assets[i]) return false;
        return true;
    }
}
