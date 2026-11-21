using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class KayKitCharacterBuilder
{
    private const string Visuals = "Assets/Imports/Visuals/";
    private const string Classic = Visuals + "Models/Characters/";
    private const string Adventurers = Visuals + "kaykit-adventurers-2.0/";
    private const string Skeletons = Visuals + "kaykit-skeletons-1.1/";
    private const string Rigs = Visuals + "kaykit-rig-animations/";
    private const string MediumGeneral = Rigs + "Rig_Medium_General.fbx";
    private const string MediumMovement = Rigs + "Rig_Medium_MovementBasic.fbx";
    private const string MediumMelee = Rigs + "Rig_Medium_CombatMelee.fbx";
    private const string MediumRanged = Rigs + "Rig_Medium_CombatRanged.fbx";
    private const string LargeGeneral = Rigs + "Rig_Large_General.fbx";
    private const string LargeMovement = Rigs + "Rig_Large_MovementBasic.fbx";
    private const string ControllerFolder = Visuals + "Animations/KayKit";
    private const string BaseControllerPath = ControllerFolder + "/EntityAnimator.controller";
    private const string PrefabFolder = "Assets/Prefabs/Characters";
    private const float Scale = 0.5f;

    private enum Family { Classic, Medium, Large }

    private sealed class Spec
    {
        public string Name;
        public string Model;
        public Family Family;
        public (string Prop, bool LeftHand)[] Props = { };
    }

    private sealed class BaseController
    {
        public AnimatorController Controller;
        public AnimationClip Idle;
        public AnimationClip Run;
        public readonly Dictionary<CasterAction, AnimationClip> Actions = new();
    }

    private static readonly Dictionary<CasterAction, string> ClassicActions = new()
    {
        { CasterAction.MeleeChop, "1H_Melee_Attack_Chop" },
        { CasterAction.MeleeSlice, "1H_Melee_Attack_Slice_Horizontal" },
        { CasterAction.MeleeStab, "1H_Melee_Attack_Stab" },
        { CasterAction.HeavyChop, "2H_Melee_Attack_Chop" },
        { CasterAction.HeavySpin, "2H_Melee_Attack_Spin" },
        { CasterAction.DualStab, "Dualwield_Melee_Attack_Stab" },
        { CasterAction.BlockAttack, "Block_Attack" },
        { CasterAction.Spell, "Spellcast_Shoot" },
        { CasterAction.Summon, "Spellcast_Raise" },
        { CasterAction.Shoot, "1H_Ranged_Shoot" },
        { CasterAction.Bow, "2H_Ranged_Shoot" },
        { CasterAction.Throw, "Throw" },
        { CasterAction.UseItem, "Use_Item" },
    };

    private static readonly Dictionary<CasterAction, (string File, string Clip)> MediumActions = new()
    {
        { CasterAction.MeleeChop, (MediumMelee, "Melee_1H_Attack_Chop") },
        { CasterAction.MeleeSlice, (MediumMelee, "Melee_1H_Attack_Slice_Horizontal") },
        { CasterAction.MeleeStab, (MediumMelee, "Melee_1H_Attack_Stab") },
        { CasterAction.HeavyChop, (MediumMelee, "Melee_2H_Attack_Chop") },
        { CasterAction.HeavySpin, (MediumMelee, "Melee_2H_Attack_Spin") },
        { CasterAction.DualStab, (MediumMelee, "Melee_Dualwield_Attack_Stab") },
        { CasterAction.BlockAttack, (MediumMelee, "Melee_Block_Attack") },
        { CasterAction.Spell, (MediumRanged, "Ranged_Magic_Shoot") },
        { CasterAction.Summon, (MediumRanged, "Ranged_Magic_Summon") },
        { CasterAction.Shoot, (MediumRanged, "Ranged_1H_Shoot") },
        { CasterAction.Bow, (MediumRanged, "Ranged_Bow_Release") },
        { CasterAction.Throw, (MediumGeneral, "Throw") },
        { CasterAction.UseItem, (MediumGeneral, "Use_Item") },
    };

    private static Spec Classic1(string name) => new() { Name = name, Family = Family.Classic };

    private static Spec Medium(string name, string model, params (string, bool)[] props) =>
        new() { Name = name, Model = model, Family = Family.Medium, Props = props };

    private static Spec Large(string name, string model, params (string, bool)[] props) =>
        new() { Name = name, Model = model, Family = Family.Large, Props = props };

    private static readonly Spec[] Characters =
    {
        Classic1("Knight"),
        Classic1("Barbarian"),
        Classic1("Mage"),
        Medium("Rogue", Adventurers + "Characters/Rogue_Hooded.fbx",
            (Adventurers + "Assets/dagger.fbx", false), (Adventurers + "Assets/dagger.fbx", true)),
        Medium("Druid", Adventurers + "Characters/Druid.fbx", (Adventurers + "Assets/druid_staff.fbx", false)),
        Medium("Engineer", Adventurers + "Characters/Engineer.fbx", (Adventurers + "Assets/engineer_Wrench.fbx", false)),
        Medium("Necromancer", Skeletons + "Characters/Necromancer.fbx", (Skeletons + "Assets/Skeleton_Staff.fbx", false)),
        Medium("SkeletonMinion", Skeletons + "Characters/Skeleton_Minion.fbx", (Skeletons + "Assets/Skeleton_Axe.fbx", false)),
        Medium("Ranger", Adventurers + "Characters/Ranger.fbx", (Adventurers + "Assets/bow_withString.fbx", true)),
        Medium("SkeletonWarrior", Skeletons + "Characters/Skeleton_Warrior.fbx",
            (Skeletons + "Assets/Skeleton_Blade.fbx", false), (Skeletons + "Assets/Skeleton_Shield_Small_A.fbx", true)),
        Medium("SkeletonMage", Skeletons + "Characters/Skeleton_Mage.fbx", (Skeletons + "Assets/Skeleton_Staff.fbx", false)),
        Medium("SkeletonRogue", Skeletons + "Characters/Skeleton_Rogue.fbx", (Skeletons + "Assets/Skeleton_Crossbow.fbx", false)),
        Large("BarbarianLarge", Adventurers + "Characters/Barbarian_Large.fbx", (Adventurers + "Assets/axe_2handed_Large.fbx", false)),
        Large("SkeletonGolem", Skeletons + "Characters/Skeleton_Golem.fbx", (Skeletons + "Assets/Skeleton_Golem_Axe.fbx", false)),
    };

    private static (string File, string Clip) IdleOf(Spec spec) => spec.Family switch
    {
        Family.Classic => (Classic + spec.Name + ".fbx", "Idle"),
        Family.Medium => (MediumGeneral, "Idle_A"),
        _ => (LargeGeneral, "Idle_A"),
    };

    private static (string File, string Clip) RunOf(Spec spec) => spec.Family switch
    {
        Family.Classic => (Classic + spec.Name + ".fbx", "Running_A"),
        Family.Medium => (MediumMovement, "Running_A"),
        _ => (LargeMovement, "Running_A"),
    };

    private static (string File, string Clip)? ActionOf(Spec spec, CasterAction action) => spec.Family switch
    {
        Family.Classic when ClassicActions.TryGetValue(action, out var clip) => (Classic + spec.Name + ".fbx", clip),
        Family.Medium when MediumActions.TryGetValue(action, out var source) => source,
        _ => null,
    };

    private static readonly Dictionary<string, string> EntityModels = new() { { "Rogue", "Rogue" } };

    private static readonly string[] LoopingClips = { "Idle", "Walking", "Running" };

    [MenuItem("Tools/Build KayKit Characters")]
    private static void Build()
    {
        foreach (var file in Characters.SelectMany(spec => new[] { IdleOf(spec).File, RunOf(spec).File }).Distinct()) MarkLoops(file);
        if (!AssetDatabase.IsValidFolder(Visuals + "Animations")) AssetDatabase.CreateFolder(Visuals.TrimEnd('/'), "Animations");
        if (!AssetDatabase.IsValidFolder(ControllerFolder)) AssetDatabase.CreateFolder(Visuals + "Animations", "KayKit");
        var baseController = BuildBaseController();
        var built = new Dictionary<string, GameObject>();
        foreach (var spec in Characters)
        {
            var controller = BuildOverride(spec, baseController);
            var prefab = spec.Model == null ? Rewire(spec, controller) : BuildPrefab(spec, controller);
            if (prefab != null) built[spec.Name] = prefab;
        }
        foreach (var (entityId, prefabName) in EntityModels)
            if (built.TryGetValue(prefabName, out var prefab)) PointEntityAt(entityId, prefab);
        AssetDatabase.SaveAssets();
        Debug.Log($"[KayKit] built {built.Count} character prefabs in {PrefabFolder}");
    }

    private static void MarkLoops(string path)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
        foreach (var clip in clips)
            clip.loopTime = LoopingClips.Any(prefix => clip.name.StartsWith(prefix));
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    private static BaseController BuildBaseController()
    {
        AssetDatabase.DeleteAsset(BaseControllerPath);
        var result = new BaseController
        {
            Controller = AnimatorController.CreateAnimatorControllerAtPath(BaseControllerPath),
            Idle = FindClip((MediumGeneral, "Idle_A")),
            Run = FindClip((MediumMovement, "Running_A")),
        };
        var controller = result.Controller;
        controller.AddParameter(EntityModel.MovingParameter, AnimatorControllerParameterType.Bool);
        var machine = controller.layers[0].stateMachine;

        var idle = machine.AddState("Idle", new Vector3(300f, 0f));
        idle.motion = result.Idle;
        machine.defaultState = idle;
        var run = machine.AddState("Run", new Vector3(600f, 0f));
        run.motion = result.Run;
        Connect(idle, run, AnimatorConditionMode.If, EntityModel.MovingParameter);
        Connect(run, idle, AnimatorConditionMode.IfNot, EntityModel.MovingParameter);

        var row = 0;
        foreach (var (action, source) in MediumActions)
        {
            var clip = FindClip(source);
            result.Actions[action] = clip;
            var state = machine.AddState(action.ToString(), new Vector3(300f + 250f * (row % 3), 120f + 60f * (row / 3)));
            state.motion = clip;
            var exit = state.AddTransition(idle);
            exit.hasExitTime = true;
            exit.exitTime = 0.9f;
            exit.duration = 0.1f;
            row++;
        }
        return result;
    }

    private static AnimatorOverrideController BuildOverride(Spec spec, BaseController baseController)
    {
        var path = $"{ControllerFolder}/{spec.Name}.overrideController";
        AssetDatabase.DeleteAsset(path);
        var idle = FindClip(IdleOf(spec));
        var clips = new List<KeyValuePair<AnimationClip, AnimationClip>>
        {
            new(baseController.Idle, idle),
            new(baseController.Run, FindClip(RunOf(spec))),
        };
        foreach (var (action, placeholder) in baseController.Actions)
            clips.Add(new(placeholder, ActionOf(spec, action) is { } source ? FindClip(source) : idle));
        var overrides = new AnimatorOverrideController(baseController.Controller) { name = spec.Name };
        overrides.ApplyOverrides(clips);
        AssetDatabase.CreateAsset(overrides, path);
        return overrides;
    }

    private static void Connect(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, string parameter)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.1f;
        transition.AddCondition(mode, 0f, parameter);
    }

    private static AnimationClip FindClip((string File, string Clip) source)
    {
        var clip = AssetDatabase.LoadAllAssetsAtPath(source.File).OfType<AnimationClip>().FirstOrDefault(c => c.name == source.Clip);
        if (clip == null) Debug.LogError($"[KayKit] clip '{source.Clip}' not found in {source.File}");
        return clip;
    }

    private static GameObject Rewire(Spec spec, RuntimeAnimatorController controller)
    {
        var path = $"{PrefabFolder}/{spec.Name}.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        Configure(root, controller);
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        return prefab;
    }

    private static GameObject BuildPrefab(Spec spec, RuntimeAnimatorController controller)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(spec.Model);
        if (model == null)
        {
            Debug.LogError($"[KayKit] model not found: {spec.Model}");
            return null;
        }
        var root = (GameObject)PrefabUtility.InstantiatePrefab(model);
        root.name = spec.Name;
        root.transform.localScale = Vector3.one * Scale;
        Configure(root, controller);
        foreach (var (prop, leftHand) in spec.Props) Attach(root.transform, prop, leftHand ? "handslot.l" : "handslot.r");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{spec.Name}.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void Configure(GameObject root, RuntimeAnimatorController controller)
    {
        if (!root.TryGetComponent(out Animator animator)) animator = root.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        if (!root.TryGetComponent(out EntityModel model)) model = root.AddComponent<EntityModel>();
        var serialized = new SerializedObject(model);
        serialized.FindProperty("_animator").objectReferenceValue = animator;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Attach(Transform root, string propPath, string slotName)
    {
        var slot = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == slotName);
        var prop = AssetDatabase.LoadAssetAtPath<GameObject>(propPath);
        if (slot == null || prop == null)
        {
            Debug.LogError($"[KayKit] can't attach {propPath} to {slotName} on {root.name}");
            return;
        }
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prop, slot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }

    private static void PointEntityAt(string entityId, GameObject prefab)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:EntityAssets"))
        {
            var assets = AssetDatabase.LoadAssetAtPath<EntityAssets>(AssetDatabase.GUIDToAssetPath(guid));
            if (assets == null || assets.Id != entityId) continue;
            assets.Model3D = prefab;
            EditorUtility.SetDirty(assets);
        }
    }
}
