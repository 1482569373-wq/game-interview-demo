using InterviewDemo.CameraRig;
using InterviewDemo.Combat;
using InterviewDemo.Core;
using InterviewDemo.Player;
using InterviewDemo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace InterviewDemo.Gameplay
{
    /// <summary>
    /// 战斗演示场景入口。
    /// 第二阶段开始，这个入口负责把第一阶段的静态占位场景装配成可操作的最小战斗 Demo：
    /// 1. 给玩家占位体添加生命值、角色控制器和移动/攻击/闪避逻辑；
    /// 2. 给敌人占位体添加生命值和训练靶反馈；
    /// 3. 给相机添加俯视角跟随；
    /// 4. 给 Canvas 添加基础 HUD 和中文操作说明。
    /// 这样做的好处是：场景仍然保持简单，玩法初始化顺序也能在一个入口里清楚讲给面试官。
    /// </summary>
    public sealed class CombatDemoBootstrap : MonoBehaviour
    {
        private const string PlayerObjectName = "玩家占位";
        private const string EnemyObjectName = "敌人占位";
        private const string CameraObjectName = "俯视战斗相机";
        private const string CanvasObjectName = "战斗 UI Canvas";
        private const string InstructionTextName = "战斗说明";
        private const float PlayerMaxHealth = 100f;
        private const float EnemyMaxHealth = 80f;

        /// <summary>
        /// 场景是否已经初始化完成。
        /// Start 只会执行一次，但保留保护位可以防止未来异步重进或手动调用初始化时重复装配组件。
        /// </summary>
        private bool _isInitialized;

        private void Start()
        {
            InitializeCombatDemo();
        }

        /// <summary>
        /// 初始化战斗演示。
        /// 这里按“先数据与对象、再表现与 UI”的顺序装配，避免 HUD 找不到玩家生命值、相机找不到跟随目标。
        /// </summary>
        private void InitializeCombatDemo()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            GameObject playerObject = FindOrCreateCapsule(PlayerObjectName, new Vector3(0f, 1f, 0f), new Color(0.18f, 0.55f, 0.95f));
            GameObject enemyObject = FindOrCreateCapsule(EnemyObjectName, new Vector3(4f, 1f, 3f), new Color(0.9f, 0.2f, 0.18f));
            Camera gameplayCamera = FindOrCreateCamera();

            Health playerHealth = ConfigurePlayer(playerObject, gameplayCamera);
            Health enemyHealth = ConfigureEnemy(enemyObject);

            ConfigureCamera(gameplayCamera, playerObject.transform);
            ConfigureHud(playerHealth, enemyHealth, playerObject.GetComponent<TopDownPlayerController>());
            UpdateSceneInstructionText();

            Debug.Log($"进入第二阶段战斗演示，当前版本：{GameVersion.Current}。WASD 移动，鼠标朝向，左键/J 攻击，空格/右键闪避。");
        }

        private static Health ConfigurePlayer(GameObject playerObject, Camera gameplayCamera)
        {
            playerObject.transform.position = new Vector3(0f, 1f, 0f);

            CapsuleCollider capsuleCollider = playerObject.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                // CharacterController 自带胶囊碰撞体。
                // 禁用原始 CapsuleCollider 可以避免玩家自己和自己发生碰撞抖动。
                capsuleCollider.enabled = false;
            }

            CharacterController characterController = playerObject.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = playerObject.AddComponent<CharacterController>();
            }

            characterController.height = 2f;
            characterController.radius = 0.45f;
            characterController.center = Vector3.zero;
            characterController.stepOffset = 0.25f;
            characterController.slopeLimit = 45f;

            Health health = playerObject.GetComponent<Health>();
            if (health == null)
            {
                health = playerObject.AddComponent<Health>();
            }

            health.Configure(PlayerMaxHealth);

            TopDownPlayerController playerController = playerObject.GetComponent<TopDownPlayerController>();
            if (playerController == null)
            {
                playerController = playerObject.AddComponent<TopDownPlayerController>();
            }

            playerController.Configure(gameplayCamera, health);
            return health;
        }

        private static Health ConfigureEnemy(GameObject enemyObject)
        {
            enemyObject.transform.position = new Vector3(4f, 1f, 3f);

            CapsuleCollider capsuleCollider = enemyObject.GetComponent<CapsuleCollider>();
            if (capsuleCollider == null)
            {
                capsuleCollider = enemyObject.AddComponent<CapsuleCollider>();
            }

            capsuleCollider.enabled = true;
            capsuleCollider.height = 2f;
            capsuleCollider.radius = 0.5f;
            capsuleCollider.center = Vector3.zero;

            Health health = enemyObject.GetComponent<Health>();
            if (health == null)
            {
                health = enemyObject.AddComponent<Health>();
            }

            health.Configure(EnemyMaxHealth);

            DemoEnemyTarget target = enemyObject.GetComponent<DemoEnemyTarget>();
            if (target == null)
            {
                target = enemyObject.AddComponent<DemoEnemyTarget>();
            }

            target.Configure(health);
            return health;
        }

        private static void ConfigureCamera(Camera gameplayCamera, Transform playerTransform)
        {
            if (gameplayCamera == null)
            {
                return;
            }

            TopDownCameraFollow follow = gameplayCamera.GetComponent<TopDownCameraFollow>();
            if (follow == null)
            {
                follow = gameplayCamera.gameObject.AddComponent<TopDownCameraFollow>();
            }

            follow.Configure(playerTransform);
        }

        private static void ConfigureHud(Health playerHealth, Health enemyHealth, TopDownPlayerController playerController)
        {
            GameObject canvasObject = GameObject.Find(CanvasObjectName);
            if (canvasObject == null)
            {
                canvasObject = new GameObject(CanvasObjectName);
            }

            BasicCombatHud hud = canvasObject.GetComponent<BasicCombatHud>();
            if (hud == null)
            {
                hud = canvasObject.AddComponent<BasicCombatHud>();
            }

            hud.Initialize(playerHealth, enemyHealth, playerController);
        }

        private static void UpdateSceneInstructionText()
        {
            GameObject instructionObject = GameObject.Find(InstructionTextName);
            if (instructionObject == null)
            {
                return;
            }

            Text instructionText = instructionObject.GetComponent<Text>();
            if (instructionText != null)
            {
                instructionText.text = "第二阶段：已接入移动、鼠标朝向、普通攻击、闪避、生命值和基础 HUD";
            }
        }

        private static Camera FindOrCreateCamera()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                return camera;
            }

            GameObject cameraObject = GameObject.Find(CameraObjectName);
            if (cameraObject == null)
            {
                cameraObject = new GameObject(CameraObjectName);
            }

            camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.10f);

            if (cameraObject.GetComponent<AudioListener>() == null)
            {
                cameraObject.AddComponent<AudioListener>();
            }

            return camera;
        }

        private static GameObject FindOrCreateCapsule(string objectName, Vector3 position, Color color)
        {
            GameObject gameObject = GameObject.Find(objectName);
            if (gameObject == null)
            {
                gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                gameObject.name = objectName;
            }

            gameObject.transform.position = position;

            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            return gameObject;
        }
    }
}
