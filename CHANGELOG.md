# 更新记录

## v0.1.1 - 同步 Unity 安装并生成基础场景

- 将项目目标 Unity 版本更新为 `6000.4.10f1`。
- 更新版本号为 `v0.1.1`。
- 添加 Editor 场景生成工具，用于生成主菜单和战斗 Demo 的基础对象。
- 通过 Unity 批处理生成主菜单、战斗 Demo 场景和三份原型材质。
- 记录 Unity 授权/Access token 警告，本次验证未发现 C# 编译错误。

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
