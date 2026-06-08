using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InterviewDemo.EditorTools
{
    /// <summary>
    /// Unity 编辑器启动时的 Demo 场景引导器。
    /// 这个类只在编辑器内生效，目的是解决“打开项目后看到空白默认场景，不知道 Demo 在哪里”的体验问题。
    /// 它不会参与正式构建，也不会影响运行时玩法逻辑。
    /// </summary>
    [InitializeOnLoad]
    public static class DemoSceneAutoOpener
    {
        /// <summary>
        /// 面试 Demo 的主入口场景。
        /// 这里使用资源路径而不是场景名称，是为了避免未来出现同名场景时误打开错误内容。
        /// </summary>
        private const string MainMenuScenePath = "Assets/_InterviewDemo/Scenes/MainMenu.unity";

        /// <summary>
        /// 每个 Unity 编辑器会话只自动尝试一次。
        /// 这样既能让第一次打开项目时直接看到 Demo，也不会在用户后续切换场景时反复打断操作。
        /// </summary>
        private const string SessionOpenedKey = "InterviewDemo.DemoSceneAutoOpener.OpenedInCurrentSession";

        static DemoSceneAutoOpener()
        {
            // Unity 刚加载程序集时，资源数据库和场景状态可能还没有完全稳定。
            // 使用 delayCall 延后一帧执行，可以避免在导入脚本或刷新资源时误判当前场景。
            EditorApplication.delayCall += TryOpenMainMenuWhenProjectStarts;
        }

        /// <summary>
        /// 在项目打开后的安全时机尝试切到主菜单。
        /// 只有当前场景看起来是 Unity 默认未保存场景时才会自动打开，避免覆盖用户手动打开的工作场景。
        /// </summary>
        private static void TryOpenMainMenuWhenProjectStarts()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            if (SessionState.GetBool(SessionOpenedKey, false))
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorApplication.isCompiling)
            {
                // 首次打开项目时经常会先触发脚本编译。
                // 编译期间场景状态可能还在变化，所以延后一帧继续等待，而不是直接放弃自动打开。
                EditorApplication.delayCall += TryOpenMainMenuWhenProjectStarts;
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath) == null)
            {
                Debug.LogWarning($"没有找到 Demo 主菜单场景，自动打开已跳过：{MainMenuScenePath}");
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!ShouldReplaceCurrentScene(activeScene))
            {
                return;
            }

            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            SessionState.SetBool(SessionOpenedKey, true);
            Debug.Log($"已自动打开面试 Demo 主菜单场景：{MainMenuScenePath}");
        }

        /// <summary>
        /// 判断当前场景是否可以被主菜单替换。
        /// Unity 新建项目或首次打开时常见的是未保存场景，里面最多只有 Main Camera 和 Directional Light；
        /// 这种场景没有面试展示价值，可以安全切换到 Demo 主菜单。
        /// </summary>
        private static bool ShouldReplaceCurrentScene(Scene activeScene)
        {
            if (activeScene.path == MainMenuScenePath)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(activeScene.path))
            {
                return false;
            }

            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            if (rootObjects.Length == 0)
            {
                return true;
            }

            return rootObjects.All(IsDefaultUnityStartupObject);
        }

        /// <summary>
        /// 识别 Unity 默认空场景自带对象。
        /// 如果场景里已经出现玩家、UI、地形或其他自定义对象，就说明用户可能在编辑内容，脚本不应该自动切场景。
        /// </summary>
        private static bool IsDefaultUnityStartupObject(GameObject rootObject)
        {
            return rootObject.name == "Main Camera"
                || rootObject.name == "Directional Light"
                || rootObject.name == "Global Volume";
        }
    }
}
