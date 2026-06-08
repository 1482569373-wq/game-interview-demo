using InterviewDemo.Core;
using InterviewDemo.Gameplay;
using InterviewDemo.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InterviewDemo.EditorTools
{
    /// <summary>
    /// 第一阶段场景生成工具。
    /// 这个工具只在 Unity Editor 内运行，用来把项目从“代码和目录骨架”推进到“可打开、可浏览的基础场景”。
    /// 选择用 Editor 脚本生成场景，是为了保证场景对象创建过程可重复、可追踪，也方便面试时说明工程初始化方式。
    /// </summary>
    public static class PhaseOneSceneBuilder
    {
        private const string RootFolder = "Assets/_InterviewDemo";
        private const string SceneFolder = RootFolder + "/Scenes";
        private const string MaterialFolder = RootFolder + "/Materials";
        private const string MainMenuScenePath = SceneFolder + "/MainMenu.unity";
        private const string CombatDemoScenePath = SceneFolder + "/CombatDemo.unity";

        /// <summary>
        /// 命令行和菜单都可以调用的入口。
        /// 命令行调用示例：
        /// Unity.exe -batchmode -projectPath 项目路径 -executeMethod InterviewDemo.EditorTools.PhaseOneSceneBuilder.BuildAll
        /// </summary>
        [MenuItem("Interview Demo/Build Phase 1 Scenes")]
        public static void BuildAll()
        {
            EnsureFolders();
            CreateMainMenuScene();
            CreateCombatDemoScene();
            RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("第一阶段基础场景生成完成。");
        }

        /// <summary>
        /// 确保工具会写入的资源目录存在。
        /// 目录创建是幂等操作，重复执行不会破坏已有文件。
        /// </summary>
        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "_InterviewDemo");
            EnsureFolder(RootFolder, "Scenes");
            EnsureFolder(RootFolder, "Materials");
        }

        /// <summary>
        /// 创建中文主菜单场景。
        /// 场景包含相机、灯光、EventSystem、Canvas、标题、版本号、开始按钮和退出按钮。
        /// </summary>
        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = SceneNames.MainMenu;

            CreateCamera("主菜单相机", new Vector3(0f, 1.2f, -8f), Quaternion.Euler(8f, 0f, 0f), Color.HSVToRGB(0.58f, 0.25f, 0.18f));
            CreateDirectionalLight("主菜单主光", new Vector3(35f, -25f, 0f), 1.1f);
            CreateEventSystem();
            CreateGameBootstrap();

            Canvas canvas = CreateCanvas("主菜单 Canvas");
            CreateText(canvas.transform, "标题", "面试 Demo：俯视动作 RPG", 42, new Vector2(0f, 135f), new Vector2(760f, 80f), TextAnchor.MiddleCenter);
            CreateText(canvas.transform, "版本号", $"当前版本：{GameVersion.Current}", 22, new Vector2(0f, 70f), new Vector2(420f, 44f), TextAnchor.MiddleCenter);
            CreateText(canvas.transform, "说明", "第一阶段：项目骨架、基础场景、流程入口", 20, new Vector2(0f, 25f), new Vector2(620f, 42f), TextAnchor.MiddleCenter);

            GameObject controllerObject = new GameObject("主菜单控制器");
            MainMenuController controller = controllerObject.AddComponent<MainMenuController>();
            CreateButton(canvas.transform, "开始游戏按钮", "开始游戏", new Vector2(0f, -45f), controller.StartGame);
            CreateButton(canvas.transform, "退出按钮", "退出游戏", new Vector2(0f, -115f), controller.QuitGame);

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        /// <summary>
        /// 创建基础战斗演示场景。
        /// 第一阶段重点是建立可浏览的空间、相机、光照、玩家占位和 UI 提示，不在这里塞入复杂玩法。
        /// </summary>
        private static void CreateCombatDemoScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = SceneNames.CombatDemo;

            CreateCamera("俯视战斗相机", new Vector3(0f, 11f, -9f), Quaternion.Euler(52f, 0f, 0f), new Color(0.08f, 0.09f, 0.10f));
            CreateDirectionalLight("战斗主光", new Vector3(50f, -35f, 20f), 1.2f);
            CreateEventSystem();

            Material groundMaterial = CreateMaterial("PrototypeGround", new Color(0.18f, 0.24f, 0.20f));
            Material playerMaterial = CreateMaterial("PrototypePlayer", new Color(0.18f, 0.55f, 0.95f));
            Material enemyMaterial = CreateMaterial("PrototypeEnemy", new Color(0.90f, 0.20f, 0.18f));

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "战斗地面";
            ground.transform.localScale = new Vector3(2.4f, 1f, 2.4f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "玩家占位";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.GetComponent<Renderer>().sharedMaterial = playerMaterial;

            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "敌人占位";
            enemy.transform.position = new Vector3(4f, 1f, 3f);
            enemy.GetComponent<Renderer>().sharedMaterial = enemyMaterial;

            GameObject bootstrap = new GameObject("战斗流程入口");
            bootstrap.AddComponent<CombatDemoBootstrap>();

            Canvas canvas = CreateCanvas("战斗 UI Canvas");
            CreateText(canvas.transform, "战斗标题", "战斗 Demo 场景", 34, new Vector2(0f, 165f), new Vector2(520f, 60f), TextAnchor.MiddleCenter);
            CreateText(canvas.transform, "战斗说明", "下一阶段将接入玩家移动、攻击、闪避和生命值", 22, new Vector2(0f, 112f), new Vector2(720f, 44f), TextAnchor.MiddleCenter);

            EditorSceneManager.SaveScene(scene, CombatDemoScenePath);
        }

        /// <summary>
        /// 将两个核心场景登记到 Build Settings，保证后续打包时主菜单是第一个场景。
        /// </summary>
        private static void RegisterBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(CombatDemoScenePath, true)
            };
        }

        private static void CreateGameBootstrap()
        {
            GameObject bootstrap = new GameObject("游戏启动入口");
            bootstrap.AddComponent<GameBootstrap>();
        }

        private static Camera CreateCamera(string name, Vector3 position, Quaternion rotation, Color backgroundColor)
        {
            GameObject cameraObject = new GameObject(name);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.SetPositionAndRotation(position, rotation);
            camera.backgroundColor = backgroundColor;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            AudioListener listener = cameraObject.AddComponent<AudioListener>();
            listener.enabled = true;
            return camera;
        }

        private static void CreateDirectionalLight(string name, Vector3 eulerAngles, float intensity)
        {
            GameObject lightObject = new GameObject(name);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = intensity;
            light.transform.rotation = Quaternion.Euler(eulerAngles);
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas(string name)
        {
            GameObject canvasObject = new GameObject(name);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(260f, 52f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.42f, 0.72f, 0.95f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            CreateText(buttonObject.transform, "按钮文字", label, 24, Vector2.zero, rectTransform.sizeDelta, TextAnchor.MiddleCenter);
            return button;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                existing.color = color;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string folder = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}

