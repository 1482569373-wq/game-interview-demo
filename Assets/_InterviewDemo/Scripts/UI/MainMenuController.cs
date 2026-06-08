using InterviewDemo.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InterviewDemo.UI
{
    /// <summary>
    /// 主菜单按钮控制器。
    /// 这个脚本只处理“用户点了哪个按钮”这类 UI 输入，不直接管理战斗规则，
    /// 这样可以让 UI 层保持轻量，后续替换菜单表现时不会影响核心玩法代码。
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        /// <summary>
        /// 开始游戏按钮调用。
        /// 后续正式搭好 CombatDemo 场景后，这里会进入完整战斗流程。
        /// </summary>
        public void StartGame()
        {
            SceneManager.LoadScene(SceneNames.CombatDemo);
        }

        /// <summary>
        /// 退出按钮调用。
        /// 在编辑器中只输出日志，避免误关编辑器；在正式构建中关闭程序。
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            Debug.Log("编辑器模式下点击了退出按钮，正式构建中会关闭游戏。");
#else
            Application.Quit();
#endif
        }
    }
}

