# 更新记录

## v0.1.0 - 第一阶段项目骨架

- 初始化 Unity 项目目录结构。
- 添加 Git 忽略规则，排除 Unity 生成目录和本地 IDE 文件。
- 添加版本文件 `VERSION`。
- 添加中文 README，说明项目目标、当前状态和后续方向。
- 添加 Unity `Packages/manifest.json` 与基础 `ProjectSettings` 文件。
- 添加主菜单和战斗 Demo 场景占位文件。
- 添加基础脚本：
  - `GameVersion`：统一暴露版本号。
  - `SceneNames`：集中维护场景名称。
  - `GameBootstrap`：后续作为流程初始化入口。
  - `MainMenuController`：主菜单按钮逻辑入口。
  - `CombatDemoBootstrap`：战斗场景初始化入口。
- 记录 Unity 编辑器安装阻塞：当前未检测到 `Unity.exe`。

