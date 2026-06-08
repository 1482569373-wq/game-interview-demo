using InterviewDemo.Core;
using UnityEngine;

namespace InterviewDemo.Gameplay
{
    /// <summary>
    /// 战斗演示场景入口。
    /// 第一阶段只保留结构占位，后续会在这里按固定顺序初始化玩家、敌人刷怪、UI 和结算流程。
    /// 将战斗场景入口单独拆出，是为了让主菜单流程和战斗流程保持清晰边界。
    /// </summary>
    public sealed class CombatDemoBootstrap : MonoBehaviour
    {
        /// <summary>
        /// 场景是否已经初始化完成。
        /// 后续所有 Update 逻辑都应先检查初始化状态，避免场景对象还没准备好就运行玩法规则。
        /// </summary>
        private bool _isInitialized;

        private void Start()
        {
            InitializeCombatDemo();
        }

        /// <summary>
        /// 初始化战斗演示。
        /// 目前只输出日志；第二阶段会接入玩家出生点、相机、基础 UI 和输入系统。
        /// </summary>
        private void InitializeCombatDemo()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            Debug.Log($"进入战斗演示场景，当前版本：{GameVersion.Current}");
        }
    }
}

