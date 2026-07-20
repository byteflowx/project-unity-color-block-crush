using App.Scripts.Gameplay.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace App.Scripts.Gameplay.EditorTools
{
    public static class GameplaySceneBuilder
    {
        private const string SCENE_PATH = "Assets/App/Scenes/Gameplay.unity";
        private const string BOOT_SCENE_PATH = "Assets/App/Scenes/Boot.unity";
        private const string BLOCK_MATERIAL_PATH = "Assets/App/Art/Materials/Block.mat";
        private const string THEME_PATH = "Assets/App/Art/UI/GameplayWorldSpace.tss";
        private const string UNIT_LABEL_UXML_PATH = "Assets/App/Art/UI/UnitLabel.uxml";
        private const string UNIT_PANEL_PATH = "Assets/App/Art/UI/UnitLabelPanel.asset";
        private const string PREFAB_FOLDER = "Assets/App/Art/Prefabs";
        private const string ROOT_NAME = "[Gameplay]";
        private const string ROOT_SCOPE_TYPE = "App.Scripts.Composition.RootLifetimeScope";

        [MenuItem("Tools/Color Block Crush/Build Gameplay Scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

            Material blockMaterial = AssetDatabase.LoadAssetAtPath<Material>(BLOCK_MATERIAL_PATH);
            PanelSettings unitPanel = EnsureUnitPanelSettings();
            VisualTreeAsset unitLabelAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UNIT_LABEL_UXML_PATH);

            RemoveExistingRoot(scene);
            Camera camera = ConfigureCamera();
            ConfigureLight();

            GameObject root = new GameObject(ROOT_NAME);
            GameObject boardRoot = CreateChild(root, "BoardRoot");
            GameObject queueRoot = CreateChild(root, "QueueRoot");
            GameObject projectileRoot = CreateChild(root, "ProjectileRoot");
            EnsurePrefabFolder();

            BlockView blockPrefab = SavePrefab(BuildBlockTemplate(blockMaterial), "Block").GetComponent<BlockView>();
            UnitView unitPrefab = SavePrefab(BuildUnitTemplate(blockMaterial, unitPanel, unitLabelAsset), "Unit").GetComponent<UnitView>();
            ProjectileView projectilePrefab = SavePrefab(BuildProjectileTemplate(blockMaterial), "Projectile").GetComponent<ProjectileView>();
            SlotMarkerView slotMarkerPrefab = SavePrefab(BuildSlotMarkerTemplate(blockMaterial), "SlotMarker").GetComponent<SlotMarkerView>();
            ParticleSystem destroyEffect = BuildDestroyEffect(root, blockMaterial);

            GameplayLayout layout = root.AddComponent<GameplayLayout>();
            GameplayPalette palette = root.AddComponent<GameplayPalette>();
            GameplayInput input = root.AddComponent<GameplayInput>();
            GameplayPresenter presenter = root.AddComponent<GameplayPresenter>();
            GameplayLifetimeScope scope = root.AddComponent<GameplayLifetimeScope>();

            SetReference(layout, "_camera", camera);
            SetReference(input, "_camera", camera);

            SerializedObject presenterObject = new SerializedObject(presenter);
            AssignProperty(presenterObject, "_layout", layout);
            AssignProperty(presenterObject, "_palette", palette);
            AssignProperty(presenterObject, "_input", input);
            AssignProperty(presenterObject, "_blockPrefab", blockPrefab);
            AssignProperty(presenterObject, "_unitPrefab", unitPrefab);
            AssignProperty(presenterObject, "_projectilePrefab", projectilePrefab);
            AssignProperty(presenterObject, "_slotMarkerPrefab", slotMarkerPrefab);
            AssignProperty(presenterObject, "_destroyEffect", destroyEffect);
            AssignProperty(presenterObject, "_boardRoot", boardRoot.transform);
            AssignProperty(presenterObject, "_queueRoot", queueRoot.transform);
            AssignProperty(presenterObject, "_projectileRoot", projectileRoot.transform);
            presenterObject.ApplyModifiedPropertiesWithoutUndo();

            SetReference(scope, "_presenter", presenter);
            AssignParentScope(scope);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorSceneManager.OpenScene(BOOT_SCENE_PATH, OpenSceneMode.Single);

            Debug.Log("[CBC] Gameplay scene rebuilt. Editor returned to Boot so Play always starts from the boot flow.");
        }

        private static void RemoveExistingRoot(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == ROOT_NAME)
                    Object.DestroyImmediate(roots[i]);
            }
        }

        private static Camera ConfigureCamera()
        {
            Camera camera = Camera.main;

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.orthographic = false;
            camera.fieldOfView = 55f;
            camera.transform.position = new Vector3(0f, 0f, -30f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.21f, 0.43f);
            camera.nearClipPlane = 1f;
            camera.farClipPlane = 200f;

            return camera;
        }

        private static void ConfigureLight()
        {
            Light light = Object.FindFirstObjectByType<Light>();

            if (light == null)
            {
                GameObject lightObject = new GameObject("Directional Light");
                light = lightObject.AddComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(28f, -38f, 0f);
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        private static GameObject CreateCube(GameObject parent, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent.transform, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;

            Object.DestroyImmediate(cube.GetComponent<BoxCollider>());

            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return cube;
        }

        private static GameObject BuildBlockTemplate(Material material)
        {
            GameObject blockObject = new GameObject("Block");
            GameObject covered = CreateCube(blockObject, "Covered", material, new Vector3(0f, 0f, 0.09f), new Vector3(0.96f, 0.96f, 0.9f));
            GameObject top = CreateCube(blockObject, "Top", material, Vector3.zero, new Vector3(0.8f, 0.8f, 0.9f));

            BlockView view = blockObject.AddComponent<BlockView>();

            SerializedObject serialized = new SerializedObject(view);
            AssignProperty(serialized, "_topTransform", top.transform);
            AssignProperty(serialized, "_topRenderer", top.GetComponent<MeshRenderer>());
            AssignProperty(serialized, "_coveredObject", covered);
            AssignProperty(serialized, "_coveredRenderer", covered.GetComponent<MeshRenderer>());
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return blockObject;
        }

        private static GameObject BuildUnitTemplate(Material material, PanelSettings panel, VisualTreeAsset labelAsset)
        {
            GameObject unitObject = new GameObject("Unit");
            GameObject body = CreateChild(unitObject, "Body");
            GameObject hull = CreateCube(body, "Hull", material, Vector3.zero, new Vector3(0.98f, 0.82f, 0.6f));
            GameObject barrel = CreateCube(body, "Barrel", material, new Vector3(0f, 0.55f, 0f), new Vector3(0.2f, 0.36f, 0.2f));

            GameObject labelObject = CreateChild(unitObject, "Charges");
            labelObject.transform.localPosition = new Vector3(0f, 0f, -0.55f);

            UIDocument document = labelObject.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = labelAsset;
            document.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
            document.worldSpaceSize = new Vector2(140f, 140f);

            UnitView view = unitObject.AddComponent<UnitView>();

            SerializedObject serialized = new SerializedObject(view);
            AssignProperty(serialized, "_bodyRenderer", hull.GetComponent<MeshRenderer>());
            AssignProperty(serialized, "_barrelRenderer", barrel.GetComponent<MeshRenderer>());
            AssignProperty(serialized, "_labelDocument", document);
            AssignProperty(serialized, "_bodyTransform", body.transform);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return unitObject;
        }

        private static GameObject BuildProjectileTemplate(Material material)
        {
            GameObject projectileObject = new GameObject("Projectile");
            CreateCube(projectileObject, "Shell", material, Vector3.zero, new Vector3(0.16f, 0.28f, 0.16f));

            TrailRenderer trail = projectileObject.AddComponent<TrailRenderer>();
            trail.time = 0.12f;
            trail.startWidth = 0.14f;
            trail.endWidth = 0f;
            trail.material = material;
            trail.numCapVertices = 2;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.receiveShadows = false;

            ProjectileView view = projectileObject.AddComponent<ProjectileView>();

            SerializedObject serialized = new SerializedObject(view);
            AssignProperty(serialized, "_trail", trail);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return projectileObject;
        }

        private static GameObject BuildSlotMarkerTemplate(Material material)
        {
            GameObject markerObject = new GameObject("SlotMarker");
            GameObject plate = CreateCube(markerObject, "Plate", material, new Vector3(0f, 0f, 0.35f), new Vector3(1.18f, 1.18f, 0.12f));

            SlotMarkerView view = markerObject.AddComponent<SlotMarkerView>();

            SerializedObject serialized = new SerializedObject(view);
            AssignProperty(serialized, "_renderer", plate.GetComponent<MeshRenderer>());
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return markerObject;
        }

        private static ParticleSystem BuildDestroyEffect(GameObject parent, Material material)
        {
            GameObject effectObject = CreateChild(parent, "DestroyEffect");
            ParticleSystem system = effectObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = system.main;
            main.duration = 1f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 4.5f;
            main.startSize = 0.18f;
            main.gravityModifier = 2.2f;
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return system;
        }

        private static PanelSettings EnsureUnitPanelSettings()
        {
            PanelSettings existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(UNIT_PANEL_PATH);

            if (existing != null)
                return existing;

            PanelSettings panel = ScriptableObject.CreateInstance<PanelSettings>();
            panel.renderMode = PanelRenderMode.WorldSpace;
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(THEME_PATH);
            panel.scaleMode = PanelScaleMode.ConstantPixelSize;
            panel.scale = 1f;

            AssetDatabase.CreateAsset(panel, UNIT_PANEL_PATH);
            AssetDatabase.SaveAssets();

            return panel;
        }

        private static void EnsurePrefabFolder()
        {
            if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
                AssetDatabase.CreateFolder("Assets/App/Art", "Prefabs");
        }

        private static GameObject SavePrefab(GameObject source, string prefabName)
        {
            string path = $"{PREFAB_FOLDER}/{prefabName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return prefab;
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            AssignProperty(serialized, propertyName, value);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignProperty(SerializedObject serialized, string propertyName, Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogError($"[CBC] Serialized property '{propertyName}' not found on {serialized.targetObject.GetType().Name}");
                return;
            }

            property.objectReferenceValue = value;
        }

        private static void AssignParentScope(GameplayLifetimeScope scope)
        {
            SerializedObject serialized = new SerializedObject(scope);
            SerializedProperty parentReference = serialized.FindProperty("parentReference");

            if (parentReference == null)
            {
                Debug.LogError("[CBC] LifetimeScope.parentReference not found");
                return;
            }

            SerializedProperty typeName = parentReference.FindPropertyRelative("TypeName");

            if (typeName == null)
            {
                Debug.LogError("[CBC] ParentReference.TypeName not found");
                return;
            }

            typeName.stringValue = ROOT_SCOPE_TYPE;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"[CBC] parent scope set to '{scope.parentReference.TypeName}' (resolved type: {scope.parentReference.Type})");
        }
    }
}
