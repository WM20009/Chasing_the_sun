using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class SceneAssetBuilder
{
    private const string GeneratedAnimationsPath = ChaseTheSunProjectSettings.GeneratedRoot + "/Animations";
    private const string GeneratedControllersPath = ChaseTheSunProjectSettings.GeneratedRoot + "/Controllers";
    private const string GeneratedGameplayPrefabsPath = ChaseTheSunProjectSettings.GeneratedRoot + "/Prefabs/Gameplay";
    private const string GeneratedBackgroundPrefabsPath = ChaseTheSunProjectSettings.GeneratedRoot + "/Prefabs/Background";
    private const string GeneratedForegroundPrefabsPath = ChaseTheSunProjectSettings.GeneratedRoot + "/Prefabs/Foreground";
    private const string GeneratedPlayerPrefabsPath = ChaseTheSunProjectSettings.GeneratedRoot + "/Prefabs/Player";

    private const string PlayerControllerPath = GeneratedControllersPath + "/CTS_Player.controller";
    private const string PlayerPrefabPath = GeneratedPlayerPrefabsPath + "/CTS_Player.prefab";

    [MenuItem("Tools/Chase the Sun/Generate All Assets")]
    public static void GenerateAllAssets()
    {
        RunGenerationStep(() =>
        {
            GeneratePlayerAnimationAssets();
            GenerateBackdropPrefabs();
            GenerateGameplayPrefabs();
            GeneratePlayerPrefab();
        }, "Generated reference gameplay prefabs, reference animation assets, and the reference player prefab.");
    }

    [MenuItem("Tools/Chase the Sun/Setup Active Scene")]
    public static void SetupActiveScene()
    {
        GenerateAllAssets();
        SetupSceneContents();
    }

    [MenuItem("Tools/Chase the Sun/Generate/Player Animations")]
    public static void GeneratePlayerAnimationsOnly()
    {
        RunGenerationStep(GeneratePlayerAnimationAssets, "Generated reference player animation clips and controller.");
    }

    [MenuItem("Tools/Chase the Sun/Generate/Backdrop Prefabs")]
    public static void GenerateBackdropPrefabsOnly()
    {
        RunGenerationStep(GenerateBackdropPrefabs, "Generated reference background and foreground prefabs.");
    }

    [MenuItem("Tools/Chase the Sun/Generate/Gameplay Prefabs")]
    public static void GenerateGameplayPrefabsOnly()
    {
        RunGenerationStep(GenerateGameplayPrefabs, "Generated reference gameplay prefabs.");
    }

    [MenuItem("Tools/Chase the Sun/Generate/Player Prefab")]
    public static void GeneratePlayerPrefabOnly()
    {
        RunGenerationStep(() =>
        {
            GeneratePlayerAnimationAssets();
            GeneratePlayerPrefab();
        }, "Generated the reference player prefab.");
    }

    [MenuItem("Tools/Chase the Sun/Scene/Refresh Scene Support")]
    public static void RefreshSceneSupport()
    {
        EnsureProjectLayers();
        SetupSceneContents();
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Chase the Sun", "Refreshed the scene roots, respawn support, camera bounds, and fade overlay.", "OK");
        }
    }

    public static void BuildGeneratedAssetsBatch()
    {
        GenerateAllAssets();
    }

    public static void BuildSampleSceneBatch()
    {
        GenerateAllAssets();
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.scene");
        SetupSceneContents();
        EditorSceneManager.SaveScene(scene);
    }

    private static void RunGenerationStep(System.Action generateAction, string successMessage)
    {
        EnsureGeneratedFolders();
        EnsureProjectLayers();
        generateAction();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Chase the Sun", successMessage, "OK");
        }
    }

    [MenuItem("Tools/Chase the Sun/Fix Selection Sorting")]
    public static void FixSelectionSorting()
    {
        foreach (var selected in Selection.gameObjects)
        {
            ApplyRecommendedPresentation(selected);
        }
    }

    [MenuItem("Tools/Chase the Sun/Move Selection To Recommended Root")]
    public static void MoveSelectionToRecommendedRoot()
    {
        var roots = EnsureSceneRoots();
        foreach (var selected in Selection.transforms)
        {
            var targetRoot = ResolveRecommendedSceneRoot(selected.gameObject, roots);
            if (targetRoot == null || selected == targetRoot || IsAncestorOf(selected, targetRoot))
            {
                continue;
            }

            Undo.SetTransformParent(selected, targetRoot, "Move To Recommended Root");
        }
    }

    [MenuItem("Tools/Chase the Sun/Snap Selection To 0.25 Grid")]
    public static void SnapSelectionToQuarterGrid()
    {
        foreach (var selected in Selection.transforms)
        {
            Undo.RecordObject(selected, "Snap To Grid");
            var position = selected.position;
            position.x = Mathf.Round(position.x * 4f) / 4f;
            position.y = Mathf.Round(position.y * 4f) / 4f;
            selected.position = position;
        }
    }

    private static void EnsureGeneratedFolders()
    {
        EnsureFolder("Assets/Generated");
        EnsureFolder(ChaseTheSunProjectSettings.GeneratedRoot);
        EnsureFolder(ChaseTheSunProjectSettings.GeneratedRoot + "/Animations");
        EnsureFolder(ChaseTheSunProjectSettings.GeneratedRoot + "/Controllers");
        EnsureFolder(ChaseTheSunProjectSettings.GeneratedRoot + "/Prefabs");
        EnsureFolder(GeneratedGameplayPrefabsPath);
        EnsureFolder(GeneratedBackgroundPrefabsPath);
        EnsureFolder(GeneratedForegroundPrefabsPath);
        EnsureFolder(GeneratedPlayerPrefabsPath);
    }

    private static void EnsureProjectLayers()
    {
        var tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset").FirstOrDefault();
        if (tagManager == null)
        {
            return;
        }

        var serializedObject = new SerializedObject(tagManager);
        var layersProperty = serializedObject.FindProperty("layers");
        SetLayerName(layersProperty, 8, ChaseTheSunProjectSettings.PlayerLayer);
        SetLayerName(layersProperty, 9, ChaseTheSunProjectSettings.GroundLayer);
        SetLayerName(layersProperty, 10, ChaseTheSunProjectSettings.PushableLayer);
        SetLayerName(layersProperty, 11, ChaseTheSunProjectSettings.ClimbableLayer);
        SetLayerName(layersProperty, 12, ChaseTheSunProjectSettings.HazardLayer);
        SetLayerName(layersProperty, 13, ChaseTheSunProjectSettings.CheckpointLayer);
        SetLayerName(layersProperty, 14, ChaseTheSunProjectSettings.DecorationLayer);

        var sortingLayers = serializedObject.FindProperty("m_SortingLayers");
        EnsureSortingLayer(sortingLayers, ChaseTheSunProjectSettings.BackgroundSortingLayer, 1001);
        EnsureSortingLayer(sortingLayers, ChaseTheSunProjectSettings.GameplaySortingLayer, 1002);
        EnsureSortingLayer(sortingLayers, ChaseTheSunProjectSettings.PlayerSortingLayer, 1003);
        EnsureSortingLayer(sortingLayers, ChaseTheSunProjectSettings.ForegroundSortingLayer, 1004);
        EnsureSortingLayer(sortingLayers, ChaseTheSunProjectSettings.OverlaySortingLayer, 1005);

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void GeneratePlayerAnimationAssets()
    {
        var jumpingSprites = LoadSprites("Assets/Scenes/Sprite/Player/jumping.png");
        var clipMap = new Dictionary<string, AnimationClip>
        {
            { "Idle", CreateOrUpdateClip(GeneratedAnimationsPath + "/Idle.anim", LoadSprites("Assets/Scenes/Sprite/Player/push1.png").Take(1).ToArray(), 1f, false) },
            { "Run", CreateOrUpdateClip(GeneratedAnimationsPath + "/Run.anim", LoadSprites("Assets/Scenes/Sprite/Player/running.png"), 12f, true) },
            { "PushRight", CreateOrUpdateClip(GeneratedAnimationsPath + "/PushRight.anim", LoadSprites("Assets/Scenes/Sprite/Player/push.png"), 10f, true) },
            { "PushLeft", CreateOrUpdateClip(GeneratedAnimationsPath + "/PushLeft.anim", LoadSprites("Assets/Scenes/Sprite/Player/push_left.png"), 10f, true) },
            { "Climb", CreateOrUpdateClip(GeneratedAnimationsPath + "/Climb.anim", LoadSprites("Assets/Scenes/Sprite/Player/climb1.png"), 10f, true) },
            { "JumpStart", CreateOrUpdateClip(GeneratedAnimationsPath + "/JumpStart.anim", jumpingSprites.Take(2).ToArray(), 10f, false) },
            { "JumpRise", CreateOrUpdateClip(GeneratedAnimationsPath + "/JumpRise.anim", jumpingSprites.Skip(2).Take(1).ToArray(), 10f, false) },
            { "JumpApex", CreateOrUpdateClip(GeneratedAnimationsPath + "/JumpApex.anim", jumpingSprites.Skip(3).Take(1).ToArray(), 10f, false) },
            { "JumpFall", CreateOrUpdateClip(GeneratedAnimationsPath + "/JumpFall.anim", jumpingSprites.Skip(4).Take(1).ToArray(), 10f, false) },
            { "Land", CreateOrUpdateClip(GeneratedAnimationsPath + "/Land.anim", jumpingSprites.Skip(5).Take(1).ToArray(), 10f, false) },
            { "Dead", CreateOrUpdateClip(GeneratedAnimationsPath + "/Dead.anim", jumpingSprites.Skip(4).Take(1).ToArray(), 10f, false) }
        };

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);
        if (controller != null)
        {
            Debug.Log($"[ChaseTheSun] Skipped existing controller: {PlayerControllerPath}");
            return;
        }

        controller = AnimatorController.CreateAnimatorControllerAtPath(PlayerControllerPath);

        var stateMachine = controller.layers[0].stateMachine;
        foreach (var child in stateMachine.states.ToArray())
        {
            stateMachine.RemoveState(child.state);
        }

        AnimatorState idleState = null;
        foreach (var pair in clipMap)
        {
            var state = stateMachine.AddState(pair.Key);
            state.motion = pair.Value;
            if (pair.Key == "Idle")
            {
                idleState = state;
            }
        }

        stateMachine.defaultState = idleState;
        EditorUtility.SetDirty(controller);
    }

    private static void GenerateBackdropPrefabs()
    {
        CreateCenteredVisualPrefab(
            "Night_Background",
            "Assets/Scenes/Sprite/Background/night.png",
            GeneratedBackgroundPrefabsPath + "/Night_Background.prefab",
            ChaseTheSunProjectSettings.BackgroundSortingLayer,
            ChaseTheSunProjectSettings.DecorationLayer);

        CreateCenteredVisualPrefab(
            "TreeTrunk_Foreground",
            "Assets/Scenes/Sprite/Foreground/treetrunk.png",
            GeneratedForegroundPrefabsPath + "/TreeTrunk_Foreground.prefab",
            ChaseTheSunProjectSettings.ForegroundSortingLayer,
            ChaseTheSunProjectSettings.DecorationLayer);
    }

    private static void GenerateGameplayPrefabs()
    {
        Vector2 floorSizeMedium = new Vector2(4f, 1f);
        Vector2 floorSize2 = new Vector2(2f, 1f);
        Vector2 floorSize3 = new Vector2(3f, 1f);
        Vector2 wallSize = new Vector2(1f, 4f);
        Vector2 boxSize = new Vector2(2f, 2f);
        Vector2 spikeSize = new Vector2(1f, 0.75f);
        Vector2 ladderSize = new Vector2(1f, 5f);

        CreateStaticGroundPrefab("Medium_Ground", "Assets/Scenes/Sprite/Gameplay/Tiles/medium.png", floorSizeMedium);
        CreateStaticGroundPrefab("Platform2_Ground", "Assets/Scenes/Sprite/Gameplay/Tiles/platform2.png", floorSize2);
        CreateStaticGroundPrefab("Platform3_Ground", "Assets/Scenes/Sprite/Gameplay/Tiles/platform3.png", floorSize3);
        CreateStaticGroundPrefab("Wall_Blocker", "Assets/Scenes/Sprite/Gameplay/wall.png", wallSize);
        CreateStaticGroundPrefab("Bridge_Ground", "Assets/Scenes/Sprite/Gameplay/bridge (1).png", floorSizeMedium);
        CreateStaticGroundPrefab("Gravestone_Blocker", "Assets/Scenes/Sprite/Gameplay/gravestone.png", new Vector2(1f, 1.5f));

        CreateBrokenBridgePrefab(floorSizeMedium);
        CreatePushablePrefab("Box_Pushable", "Assets/Scenes/Sprite/Gameplay/box.png", boxSize);
        CreatePushablePrefab("BrokenBox_Pushable", "Assets/Scenes/Sprite/Gameplay/brokenbox.png", boxSize);
        CreateLadderPrefab(ladderSize);
        CreateHazardPrefab(spikeSize);
        CreateCheckpointPrefab("Campfire_Checkpoint", "Assets/Scenes/Sprite/Gameplay/campfire.png");
        CreateDecorationPrefab("Lantern_Decoration", "Assets/Scenes/Sprite/Gameplay/lantern.png");
        CreateDecorationPrefab("Vine_Decoration", "Assets/Scenes/Sprite/Gameplay/vine.png");
    }

    private static void ApplyTargetSize(GameObject root, Sprite sprite, Vector2 targetSize)
    {
        var visual = root.transform.Find("Visual");
        if (visual != null && sprite != null && sprite.bounds.size.x > 0 && sprite.bounds.size.y > 0)
        {
            visual.localScale = new Vector3(
                targetSize.x / sprite.bounds.size.x,
                targetSize.y / sprite.bounds.size.y,
                1f
            );
        }
    }

    private static void GeneratePlayerPrefab()
    {
        var idleSprite = LoadSprites("Assets/Scenes/Sprite/Player/push1.png").FirstOrDefault();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);
        if (idleSprite == null || controller == null)
        {
            return;
        }

        var root = new GameObject("CTS_Player");
        root.layer = LayerMask.NameToLayer(ChaseTheSunProjectSettings.PlayerLayer);

        var body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 4f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var capsule = root.AddComponent<CapsuleCollider2D>();
        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.offset = new Vector2(0f, 0.875f);
        capsule.size = new Vector2(0.75f, 1.75f);

        root.AddComponent<PlayerAnimationDriver>();
        root.AddComponent<PlayerController2D>();

        var visual = new GameObject("Visual");
        visual.layer = root.layer;
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = GetPlayerSpriteOffset(idleSprite);

        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = idleSprite;
        renderer.sortingLayerName = ChaseTheSunProjectSettings.PlayerSortingLayer;

        var animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        SavePrefab(root, PlayerPrefabPath);
    }

    private static void CreateStaticGroundPrefab(string prefabName, string spritePath, Vector2 targetSize)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) return;

        var root = CreateCenteredSpriteRoot(prefabName, sprite, ChaseTheSunProjectSettings.GameplaySortingLayer, ChaseTheSunProjectSettings.GroundLayer);
        ApplyTargetSize(root, sprite, targetSize); // 强制缩放视觉表现

        var collider = root.AddComponent<BoxCollider2D>();
        collider.size = targetSize; // 碰撞体匹配目标尺寸
        SavePrefab(root, GeneratedGameplayPrefabsPath + "/" + prefabName + ".prefab");
    }

    private static void CreatePushablePrefab(string prefabName, string spritePath, Vector2 targetSize)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) return;

        var root = CreateCenteredSpriteRoot(prefabName, sprite, ChaseTheSunProjectSettings.GameplaySortingLayer, ChaseTheSunProjectSettings.PushableLayer);
        ApplyTargetSize(root, sprite, targetSize);

        var collider = root.AddComponent<BoxCollider2D>();
        collider.size = targetSize;

        var body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        root.AddComponent<PushableBox>();
        SavePrefab(root, GeneratedGameplayPrefabsPath + "/" + prefabName + ".prefab");
    }

    private static void CreateLadderPrefab(Vector2 targetSize)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Scenes/Sprite/Gameplay/ladder.png");
        if (sprite == null) return;

        var root = CreateCenteredSpriteRoot("Ladder_Climbable", sprite, ChaseTheSunProjectSettings.GameplaySortingLayer, ChaseTheSunProjectSettings.ClimbableLayer);
        ApplyTargetSize(root, sprite, targetSize);

        var collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(targetSize.x * 0.5f, targetSize.y); // 梯子的判定范围稍微比视觉窄一点
        root.AddComponent<LadderZone>();
        SavePrefab(root, GeneratedGameplayPrefabsPath + "/Ladder_Climbable.prefab");
    }

    private static void CreateHazardPrefab(Vector2 targetSize)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Scenes/Sprite/Gameplay/spikes.png");
        if (sprite == null) return;

        var root = CreateCenteredSpriteRoot("Spikes_Hazard", sprite, ChaseTheSunProjectSettings.GameplaySortingLayer, ChaseTheSunProjectSettings.HazardLayer);
        ApplyTargetSize(root, sprite, targetSize);

        var collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        // 尖刺碰撞盒应更关注底部，高度给0.4即一半左右
        collider.size = new Vector2(targetSize.x * 0.9f, targetSize.y * 0.4f);
        collider.offset = new Vector2(0f, -targetSize.y * 0.5f + collider.size.y * 0.5f); // 底部对齐
        root.AddComponent<HazardZone>();
        SavePrefab(root, GeneratedGameplayPrefabsPath + "/Spikes_Hazard.prefab");
    }

    private static void CreateCheckpointPrefab(string prefabName, string spritePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            return;
        }

        var root = CreateCenteredSpriteRoot(prefabName, sprite, ChaseTheSunProjectSettings.GameplaySortingLayer, ChaseTheSunProjectSettings.CheckpointLayer);
        var collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(sprite.bounds.size.x * 0.55f, sprite.bounds.size.y * 0.75f);
        collider.offset = new Vector2(0f, sprite.bounds.extents.y * 0.05f);
        var checkpoint = root.AddComponent<CampfireCheckpoint>();
        var serializedObject = new SerializedObject(checkpoint);
        serializedObject.FindProperty("spawnOffset").vector2Value = new Vector2(0f, sprite.bounds.extents.y + 0.55f);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(root, GeneratedGameplayPrefabsPath + "/" + prefabName + ".prefab");
    }

    private static void CreateDecorationPrefab(string prefabName, string spritePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            return;
        }

        var root = CreateCenteredSpriteRoot(prefabName, sprite, ChaseTheSunProjectSettings.GameplaySortingLayer, ChaseTheSunProjectSettings.DecorationLayer);
        SavePrefab(root, GeneratedGameplayPrefabsPath + "/" + prefabName + ".prefab");
    }

    private static void CreateBrokenBridgePrefab(Vector2 targetSize)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Scenes/Sprite/Gameplay/brokenbridge (1).png");
        if (sprite == null) return;

        var root = CreateCenteredSpriteRoot("BrokenBridge_Hazard", sprite, ChaseTheSunProjectSettings.GameplaySortingLayer, ChaseTheSunProjectSettings.GroundLayer);
        ApplyTargetSize(root, sprite, targetSize);

        var left = root.AddComponent<BoxCollider2D>();
        left.size = new Vector2(targetSize.x * 0.34f, targetSize.y * 0.5f);
        left.offset = new Vector2(-targetSize.x * 0.31f, -targetSize.y * 0.07f);

        var right = root.AddComponent<BoxCollider2D>();
        right.size = new Vector2(targetSize.x * 0.34f, targetSize.y * 0.5f);
        right.offset = new Vector2(targetSize.x * 0.31f, -targetSize.y * 0.07f);

        var hazardRoot = new GameObject("GapHazard");
        hazardRoot.layer = LayerMask.NameToLayer(ChaseTheSunProjectSettings.HazardLayer);
        hazardRoot.transform.SetParent(root.transform, false);
        hazardRoot.transform.localPosition = new Vector3(0f, -targetSize.y * 0.42f, 0f);
        var hazardCollider = hazardRoot.AddComponent<BoxCollider2D>();
        hazardCollider.isTrigger = true;
        hazardCollider.size = new Vector2(targetSize.x * 0.28f, targetSize.y * 1.35f);
        hazardRoot.AddComponent<HazardZone>();

        SavePrefab(root, GeneratedGameplayPrefabsPath + "/BrokenBridge_Hazard.prefab");
    }

    private static AnimationClip CreateOrUpdateClip(string clipPath, Sprite[] sprites, float sampleRate, bool loop)
    {
        if (sprites == null || sprites.Length == 0)
        {
            return null;
        }

        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip != null)
        {
            Debug.Log($"[ChaseTheSun] Skipped existing animation clip: {clipPath}");
            return clip;
        }

        clip = new AnimationClip();
        AssetDatabase.CreateAsset(clip, clipPath);

        clip.frameRate = sampleRate;

        var spriteBinding = new EditorCurveBinding
        {
            path = string.Empty,
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };

        var spriteFrames = new ObjectReferenceKeyframe[sprites.Length];
        var frameDuration = 1f / sampleRate;
        for (var i = 0; i < sprites.Length; i++)
        {
            spriteFrames[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteFrames);
        SetOffsetCurves(clip, sprites, sampleRate);
        SetClipLoop(clip, loop, Mathf.Max(frameDuration, sprites.Length * frameDuration));
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void SetOffsetCurves(AnimationClip clip, IReadOnlyList<Sprite> sprites, float sampleRate)
    {
        var xCurve = new AnimationCurve();
        var yCurve = new AnimationCurve();
        var zCurve = new AnimationCurve();
        var frameDuration = 1f / sampleRate;

        for (var i = 0; i < sprites.Count; i++)
        {
            var offset = GetPlayerSpriteOffset(sprites[i]);
            var start = i * frameDuration;
            var end = (i + 1) * frameDuration;
            AddSteppedKey(xCurve, start, end, offset.x);
            AddSteppedKey(yCurve, start, end, offset.y);
            AddSteppedKey(zCurve, start, end, offset.z);
        }

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), "m_LocalPosition.x"), xCurve);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), "m_LocalPosition.y"), yCurve);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), "m_LocalPosition.z"), zCurve);
    }

    private static void SetClipLoop(AnimationClip clip, bool loop, float stopTime)
    {
        var serializedObject = new SerializedObject(clip);
        var settings = serializedObject.FindProperty("m_AnimationClipSettings");
        settings.FindPropertyRelative("m_LoopTime").boolValue = loop;
        settings.FindPropertyRelative("m_StopTime").floatValue = stopTime;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Sprite[] LoadSprites(string path)
    {
        return AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => ExtractTrailingIndex(sprite.name))
            .ToArray();
    }

    private static int ExtractTrailingIndex(string value)
    {
        var underscore = value.LastIndexOf('_');
        if (underscore < 0 || underscore >= value.Length - 1)
        {
            return 0;
        }

        return int.TryParse(value.Substring(underscore + 1), out var index) ? index : 0;
    }

    private static void AddSteppedKey(AnimationCurve curve, float startTime, float endTime, float value)
    {
        curve.AddKey(new Keyframe(startTime, value));
        curve.AddKey(new Keyframe(Mathf.Max(startTime, endTime - 0.0001f), value));
    }

    private static Vector3 GetPlayerSpriteOffset(Sprite sprite)
    {
        return new Vector3(0f, sprite.bounds.extents.y, 0f);
    }

    private static GameObject CreateCenteredSpriteRoot(string rootName, Sprite sprite, string sortingLayerName, string layerName)
    {
        var root = new GameObject(rootName);
        root.layer = LayerMask.NameToLayer(layerName);

        // 【核心修复】：直接将 SpriteRenderer 挂载在 Root 节点上，舍弃独立的 Visual 子节点
        // 这样后续添加 BoxCollider2D 时，它的中心和大小将自动与 Sprite 的 bounds 绑定。
        // 当你在场景中缩放物体大小时，碰撞箱会完全同步缩放。
        var renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = sortingLayerName;

        return root;
    }

    private static void CreateCenteredVisualPrefab(string prefabName, string spritePath, string prefabPath, string sortingLayerName, string layerName)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            return;
        }

        var root = CreateCenteredSpriteRoot(prefabName, sprite, sortingLayerName, layerName);
        SavePrefab(root, prefabPath);
    }

    private static void SavePrefab(GameObject root, string path)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log($"[ChaseTheSun] Skipped existing prefab: {path}");
            UnityEngine.Object.DestroyImmediate(root);
            return;
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        var segments = folderPath.Split('/');
        var current = segments[0];
        for (var i = 1; i < segments.Length; i++)
        {
            var next = current + "/" + segments[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, segments[i]);
            }

            current = next;
        }
    }

    private static void SetLayerName(SerializedProperty layersProperty, int index, string layerName)
    {
        if (index < 0 || index >= layersProperty.arraySize)
        {
            return;
        }

        layersProperty.GetArrayElementAtIndex(index).stringValue = layerName;
    }

    private static void EnsureSortingLayer(SerializedProperty sortingLayers, string layerName, int uniqueId)
    {
        for (var i = 0; i < sortingLayers.arraySize; i++)
        {
            var layer = sortingLayers.GetArrayElementAtIndex(i);
            if (layer.FindPropertyRelative("name").stringValue == layerName)
            {
                layer.FindPropertyRelative("uniqueID").intValue = uniqueId;
                return;
            }
        }

        sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
        var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
        newLayer.FindPropertyRelative("name").stringValue = layerName;
        newLayer.FindPropertyRelative("uniqueID").intValue = uniqueId;
        newLayer.FindPropertyRelative("locked").boolValue = false;
    }

    private static void ApplyRecommendedPresentation(GameObject rootObject)
    {
        ApplyRecommendedLayersRecursively(rootObject.transform);

        foreach (var renderer in rootObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.sortingLayerName = ResolveSortingLayer(renderer.gameObject);
            renderer.sortingOrder = ResolveSortingOrder(renderer.gameObject);
            EditorUtility.SetDirty(renderer);
        }

        foreach (var renderer in rootObject.GetComponentsInChildren<TilemapRenderer>(true))
        {
            renderer.sortingLayerName = ResolveSortingLayer(renderer.gameObject);
            renderer.sortingOrder = ResolveSortingOrder(renderer.gameObject);
            EditorUtility.SetDirty(renderer);
        }
    }

    private static void ApplyRecommendedLayersRecursively(Transform current)
    {
        var layerName = ResolveLayerName(current.gameObject);
        var layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex >= 0 && current.gameObject.layer != layerIndex)
        {
            Undo.RecordObject(current.gameObject, "Fix Selection Sorting");
            current.gameObject.layer = layerIndex;
            EditorUtility.SetDirty(current.gameObject);
        }

        foreach (Transform child in current)
        {
            ApplyRecommendedLayersRecursively(child);
        }
    }

    private static string ResolveSortingLayer(GameObject gameObject)
    {
        if (IsOverlayObject(gameObject))
        {
            return ChaseTheSunProjectSettings.OverlaySortingLayer;
        }

        if (gameObject.GetComponentInParent<PlayerController2D>() != null)
        {
            return ChaseTheSunProjectSettings.PlayerSortingLayer;
        }

        if (IsForegroundObject(gameObject))
        {
            return ChaseTheSunProjectSettings.ForegroundSortingLayer;
        }

        if (IsBackgroundObject(gameObject))
        {
            return ChaseTheSunProjectSettings.BackgroundSortingLayer;
        }

        return ChaseTheSunProjectSettings.GameplaySortingLayer;
    }

    private static int ResolveSortingOrder(GameObject gameObject)
    {
        return IsOverlayObject(gameObject) ? 100 : 0;
    }

    private static string ResolveLayerName(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return ChaseTheSunProjectSettings.DecorationLayer;
        }

        if (gameObject.GetComponent<PlayerController2D>() != null || gameObject.GetComponentInParent<PlayerController2D>() != null)
        {
            return ChaseTheSunProjectSettings.PlayerLayer;
        }

        if (gameObject.GetComponent<HazardZone>() != null || NameContains(gameObject.name, "Hazard") || NameContains(gameObject.name, "Spike"))
        {
            return ChaseTheSunProjectSettings.HazardLayer;
        }

        if (gameObject.GetComponent<LadderZone>() != null || NameContains(gameObject.name, "Ladder"))
        {
            return ChaseTheSunProjectSettings.ClimbableLayer;
        }

        if (gameObject.GetComponent<CampfireCheckpoint>() != null || NameContains(gameObject.name, "Checkpoint") || NameContains(gameObject.name, "Campfire"))
        {
            return ChaseTheSunProjectSettings.CheckpointLayer;
        }

        if (gameObject.GetComponent<PushableBox>() != null || NameContains(gameObject.name, "Pushable"))
        {
            return ChaseTheSunProjectSettings.PushableLayer;
        }

        var collider2D = gameObject.GetComponent<Collider2D>();
        if (collider2D != null && !collider2D.isTrigger)
        {
            return ChaseTheSunProjectSettings.GroundLayer;
        }

        return ChaseTheSunProjectSettings.DecorationLayer;
    }

    private static Transform ResolveRecommendedSceneRoot(GameObject gameObject, SceneRoots roots)
    {
        if (gameObject == null)
        {
            return null;
        }

        if (IsBackgroundObject(gameObject))
        {
            return roots.Background;
        }

        if (IsForegroundObject(gameObject))
        {
            return roots.Foreground;
        }

        if (IsOverlayObject(gameObject))
        {
            return roots.UiOverlay;
        }

        if (gameObject.GetComponent<CameraBounds2D>() != null || NameContains(gameObject.name, "CameraBounds"))
        {
            return roots.CameraBounds;
        }

        if (gameObject.GetComponent<RespawnManager>() != null || NameContains(gameObject.name, "PlayerSpawn") || NameContains(gameObject.name, "Spawn"))
        {
            return roots.Spawn;
        }

        if (gameObject.GetComponent<CampfireCheckpoint>() != null || NameContains(gameObject.name, "Checkpoint"))
        {
            return roots.Checkpoints;
        }

        if (gameObject.GetComponent<HazardZone>() != null || NameContains(gameObject.name, "Hazard") || NameContains(gameObject.name, "Spike"))
        {
            return roots.Hazards;
        }

        return roots.Gameplay;
    }

    private static bool IsAncestorOf(Transform child, Transform possibleAncestor)
    {
        var current = possibleAncestor;
        while (current != null)
        {
            if (current == child)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool IsBackgroundObject(GameObject gameObject)
    {
        return NameContains(gameObject.name, "Background") || NameContains(gameObject.transform.root.name, "Background");
    }

    private static bool IsForegroundObject(GameObject gameObject)
    {
        return NameContains(gameObject.name, "Foreground") || NameContains(gameObject.transform.root.name, "Foreground");
    }

    private static bool IsOverlayObject(GameObject gameObject)
    {
        return gameObject.GetComponentInParent<Canvas>() != null
            || NameContains(gameObject.name, "Fade")
            || NameContains(gameObject.name, "Overlay")
            || NameContains(gameObject.transform.root.name, "UIOverlay");
    }

    private static bool NameContains(string value, string token)
    {
        return value.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static SceneRoots EnsureSceneRoots()
    {
        return new SceneRoots
        {
            Background = GetOrCreateRoot("Background"),
            Gameplay = GetOrCreateRoot("Gameplay"),
            Checkpoints = GetOrCreateRoot("Checkpoints"),
            Hazards = GetOrCreateRoot("Hazards"),
            Foreground = GetOrCreateRoot("Foreground"),
            Spawn = GetOrCreateRoot("Spawn"),
            CameraBounds = GetOrCreateRoot("CameraBounds"),
            UiOverlay = GetOrCreateRoot("UIOverlay")
        };
    }

    private static Transform GetOrCreateRoot(string rootName)
    {
        var root = GameObject.Find(rootName);
        if (root == null)
        {
            root = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Scene Root");
        }

        return root.transform;
    }

    private static FadeOverlay EnsureFadeOverlay(Transform uiOverlayRoot)
    {
        var canvasTransform = uiOverlayRoot.Find("FadeCanvas");
        if (canvasTransform == null)
        {
            var canvasObject = new GameObject("FadeCanvas");
            canvasTransform = canvasObject.transform;
            canvasTransform.SetParent(uiOverlayRoot, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingLayerName = ChaseTheSunProjectSettings.OverlaySortingLayer;
            canvas.sortingOrder = 100;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        var fadeTransform = canvasTransform.Find("Fade");
        if (fadeTransform == null)
        {
            var fadeObject = new GameObject("Fade");
            fadeTransform = fadeObject.transform;
            fadeTransform.SetParent(canvasTransform, false);
            var rect = fadeObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = fadeObject.AddComponent<Image>();
            image.color = Color.black;
        }

        var group = fadeTransform.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = fadeTransform.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
        }

        var overlay = fadeTransform.GetComponent<FadeOverlay>();
        if (overlay == null)
        {
            overlay = fadeTransform.gameObject.AddComponent<FadeOverlay>();
        }

        return overlay;
    }

    private static RespawnManager EnsureRespawnManager(Transform spawnRoot)
    {
        var managerTransform = spawnRoot.Find("RespawnManager");
        if (managerTransform == null)
        {
            var managerObject = new GameObject("RespawnManager");
            managerTransform = managerObject.transform;
            managerTransform.SetParent(spawnRoot, false);
        }

        var manager = managerTransform.GetComponent<RespawnManager>();
        if (manager == null)
        {
            manager = managerTransform.gameObject.AddComponent<RespawnManager>();
        }

        return manager;
    }

    private static Transform EnsureSpawnMarker(Transform spawnRoot)
    {
        var marker = spawnRoot.Find("PlayerSpawn");
        if (marker == null)
        {
            var markerObject = new GameObject("PlayerSpawn");
            marker = markerObject.transform;
            marker.SetParent(spawnRoot, false);
        }

        return marker;
    }

    private static void AssignRespawnReferences(RespawnManager respawnManager, Transform spawnPoint, FadeOverlay overlay)
    {
        var serializedObject = new SerializedObject(respawnManager);
        serializedObject.FindProperty("defaultSpawnPoint").objectReferenceValue = spawnPoint;
        serializedObject.FindProperty("fadeOverlay").objectReferenceValue = overlay;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(respawnManager);
    }

    private static CameraBounds2D EnsureCameraBounds(Transform cameraBoundsRoot)
    {
        var bounds = cameraBoundsRoot.GetComponent<CameraBounds2D>();
        if (bounds == null)
        {
            bounds = cameraBoundsRoot.gameObject.AddComponent<CameraBounds2D>();
        }

        var collider = cameraBoundsRoot.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = cameraBoundsRoot.gameObject.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        collider.size = new Vector2(24f, 12f);
        return bounds;
    }

    private static void EnsureMainCamera(CameraBounds2D bounds)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();
        }

        var follow = camera.GetComponent<CameraFollow2D>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<CameraFollow2D>();
        }

        var serializedObject = new SerializedObject(follow);
        serializedObject.FindProperty("cameraBounds").objectReferenceValue = bounds;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetupSceneContents()
    {
        var roots = EnsureSceneRoots();
        var overlay = EnsureFadeOverlay(roots.UiOverlay);
        var respawnManager = EnsureRespawnManager(roots.Spawn);
        var spawnPoint = EnsureSpawnMarker(roots.Spawn);
        AssignRespawnReferences(respawnManager, spawnPoint, overlay);
        var bounds = EnsureCameraBounds(roots.CameraBounds);
        EnsureMainCamera(bounds);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        if (!Application.isBatchMode)
        {
            Selection.activeGameObject = roots.Gameplay.gameObject;
        }
    }

    private struct SceneRoots
    {
        public Transform Background;
        public Transform Gameplay;
        public Transform Checkpoints;
        public Transform Hazards;
        public Transform Foreground;
        public Transform Spawn;
        public Transform CameraBounds;
        public Transform UiOverlay;
    }
}
