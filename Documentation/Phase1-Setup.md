# 第一阶段执行记录

## 已完成

- 创建 Unity 项目基础目录。
- 添加 `Packages/manifest.json`，声明后续会使用的基础 Unity 包。
- 添加 `ProjectSettings/ProjectVersion.txt`，目标版本为 `6000.4.8f1`。
- 添加 `ProjectSettings/EditorBuildSettings.asset`，预登记 `MainMenu` 与 `CombatDemo` 两个场景。
- 添加主菜单和战斗场景占位文件。
- 添加基础 C# 脚本，并在代码中写入中文注释。
- 添加 README、CHANGELOG、VERSION、AGENTS 和 Git 忽略规则。

## 当前阻塞

- 本机 `E:\Unity\Hub\Editor\6000.4.8f1` 目录存在，但未找到 `Unity.exe`。
- 已尝试运行本机安装包和 Unity Hub headless 安装命令，仍未检测到编辑器本体。
- 后续需要先修复 Unity 编辑器安装，再用 Unity 打开工程生成正式场景对象、Canvas、相机、灯光和 `.meta` 文件。

## 下一步

- 修复或重新安装 Unity 编辑器。
- 用 Unity 打开项目，让 Package Manager 解析依赖。
- 创建正式 `MainMenu` 场景对象：相机、灯光、Canvas、标题、开始按钮、退出按钮、版本号文本。
- 创建正式 `CombatDemo` 场景对象：地面、相机、灯光、玩家出生点、战斗入口对象。
- 检查 Console，确保没有编译错误。

