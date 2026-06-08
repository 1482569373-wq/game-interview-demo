using UnityEngine;

namespace InterviewDemo.Core
{
    /// <summary>
    /// 游戏启动入口。
    /// 当前阶段只负责输出版本信息，后续会扩展为统一初始化点：
    /// 1. 初始化全局流程状态；
    /// 2. 注册 UI、输入和战斗系统；
    /// 3. 明确控制初始化顺序，减少 Awake/Start 隐式顺序带来的问题。
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        /// <summary>
        /// 初始化保护位。
        /// 后续如果多个场景都挂载 Bootstrap，可以用它避免重复初始化全局系统。
        /// </summary>
        private bool _isInitialized;

        private void Awake()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            Debug.Log($"面试 Demo 启动，当前版本：{GameVersion.Current}");
        }
    }
}

