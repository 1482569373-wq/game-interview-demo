namespace InterviewDemo.Core
{
    /// <summary>
    /// 统一维护场景名称，避免按钮逻辑、流程控制和构建设置里到处散落字符串。
    /// 这个类很小，但能在面试中体现一个基础工程习惯：跨模块共享的标识集中管理。
    /// </summary>
    public static class SceneNames
    {
        /// <summary>
        /// 中文主菜单场景，负责展示项目名称、版本号、开始按钮和退出按钮。
        /// </summary>
        public const string MainMenu = "MainMenu";

        /// <summary>
        /// 核心战斗演示场景，后续承载玩家、敌人、波次、UI 和胜负结算流程。
        /// </summary>
        public const string CombatDemo = "CombatDemo";
    }
}

