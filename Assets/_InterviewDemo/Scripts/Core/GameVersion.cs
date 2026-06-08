namespace InterviewDemo.Core
{
    /// <summary>
    /// 项目版本号集中入口。
    /// 面试展示时，主菜单、README、更新记录和 Git 提交说明都应保持同一个版本号，
    /// 这样面试官可以很快确认当前演示包对应哪一轮开发内容。
    /// </summary>
    public static class GameVersion
    {
        /// <summary>
        /// 第一阶段版本：只包含工程骨架和基础入口脚本。
        /// 后续每完成一个阶段，都需要同步更新这里、VERSION 文件和 CHANGELOG。
        /// </summary>
        public const string Current = "v0.1.0";
    }
}

