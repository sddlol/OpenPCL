# OpenPCL（PCL 外挂版）

> 基于 Plain Craft Launcher 2 的第三方开源恢复与维护项目。

[![Build](https://github.com/sddlol/OpenPCL/actions/workflows/build.yml/badge.svg)](https://github.com/sddlol/OpenPCL/actions/workflows/build.yml)
[![Pages](https://github.com/sddlol/OpenPCL/actions/workflows/pages.yml/badge.svg)](https://github.com/sddlol/OpenPCL/actions/workflows/pages.yml)
[![Release](https://img.shields.io/github/v/release/sddlol/OpenPCL?label=Release)](https://github.com/sddlol/OpenPCL/releases)
[![Stars](https://img.shields.io/github/stars/sddlol/OpenPCL?style=flat&label=Stars)](https://github.com/sddlol/OpenPCL)

<p align="center">
  <img src="docs/assets/pcl-cover.jpg" alt="OpenPCL 项目封面" width="760">
</p>

## 项目简介

OpenPCL 是一个基于 Plain Craft Launcher 2 整理、恢复并持续维护的第三方开源版本。项目致力于公开 PCL2 的源码、构建流程与发布产物，让项目更方便学习、研究、构建和二次开发。

感谢我自己，和 [@龙猫是狗(零夜 千東)](https://github.com/ltcat-dog)

Developer: [gpt-5.5](https://developers.openai.com/api/docs/models/gpt-5.5), [gpt-5.6-sol](https://developers.openai.com/api/docs/models/gpt-5.6-sol), [gpt-5.6-luna](https://developers.openai.com/api/docs/models/gpt-5.6-luna), [claude-sonnet-5](https://platform.claude.com/docs/en/about-claude/models/whats-new-sonnet-5), [尹海航](https://github.com/LTCatt)

项目主页：暂无

## 重要说明

> 原账号已丢失，请以本仓库作为当前维护、构建与发布地址。

本项目允许修改主题，与原版 PCL2 相互独立，并采用 MIT 开源协议。具体的使用、分发和二次创作规则，请以仓库中的 [LICENCE](LICENCE) 以及各第三方组件的许可证为准。

本仓库不是官方 PCL2 项目，也不代表原作者或官方社区的立场。Plain Craft Launcher、PCL、PCL2 等名称及相关权益归原作者与对应权利人所有。

## 当前状态

- **源码公开**：包含启动器主体、UI、动画、下载、Minecraft 启动、资源管理、PCLCS 与 MeloongCore 等代码。
- **构建公开**：使用 GitHub Actions 在 Windows 环境中执行 Release 构建，并上传构建产物。
- **发布公开**：发布页提供可直接下载的构建包与校验信息。
- **主题功能**：已恢复主题解锁与主题调色逻辑。
- **联机功能**：已恢复 Taowa/Terracotta 测试联机逻辑。
- **联机源码**：Terracotta v0.4.2 源码已作为第三方源码包放入 [ThirdParty/Terracotta](ThirdParty/Terracotta)。PCL2 目前默认使用已移植至 VB/.NET 的内部联机实现，不再携带或启动 `terracotta.exe`。
- **登录支持**：已增加 Token 登录功能。

## 本次更新

- **启动前校验**：启动 Minecraft 前解析 JWT Access Token 的 `exp` 字段，已过期的 Token 直接提示重新输入；非 JWT 格式的 Token 继续通过服务器验证。
- **服务端验证**：Token 登录仍会请求 `https://api.minecraftservices.com/minecraft/profile`，对失效或过期的 Token 给出明确提示。
- **输入页校验**：Token 验证按钮会先拦截空 Token 和本地已过期 Token，再发起网络验证。
- **启动界面状态**：Token 账户卡片显示到期时间与动态剩余时间，每秒刷新；无法从 Token 读取有效期时显示“未知”。
- **缓存清理**：验证成功后保存 Token 到期时间，退出 Token 登录时同步清理到期时间缓存。

## 构建

推荐在 Windows 环境中，使用 Visual Studio Build Tools、Rust 和 MSBuild 进行构建。

```powershell
git clone https://github.com/sddlol/OpenPCL.git
cd OpenPCL
git submodule update --init --recursive
.\scripts\build-embedded-codex.ps1
msbuild "Plain Craft Launcher 2.sln" /t:Restore /p:Configuration=Release /p:Platform="Any CPU"
msbuild "Plain Craft Launcher 2.sln" /m /p:Configuration=Release /p:Platform="Any CPU"
```

也可以直接查看 GitHub Actions 的构建记录和发布产物：

- [Actions](https://github.com/sddlol/OpenPCL/actions)
- [Releases](https://github.com/sddlol/OpenPCL/releases)

## Vibe Coding

PCL 内置了独立的 `Vibe Coding` GUI 和从源码编译的 Codex App Server，不依赖用户全局安装 `@openai/codex`，也不是包装 `codex exec` 输出。发布包会包含 Codex CLI、App Server、Code Mode host 和 Windows sandbox/helper 程序。它支持新建和恢复 Codex 历史对话、流式回合事件、回合中断、命令/文件修改确认，以及保存并切换自定义模型档案（模型 ID、Responses API 地址和 API Key）。API Key 仅保存到 PCL 的加密设置，并只通过子进程环境变量传入 Codex。首次进入工作区时，PCL 会写入 `.agents/skills/minecraft-agent/`，内含版本与映射检查、Mod 初始化、Mixin、渲染、输入与 GUI、网络线程、资源生命周期、构建排错和 PowerShell 5.1 等路由 Skill。

Codex 的配置、认证、会话、日志、Skills 和 MCP 状态会统一写入 PCL 的 `VibeCoding\\CodexHome` 数据目录，不读取或污染 `%USERPROFILE%\\.codex`。进入 PCL 顶部的 `Vibe Coding` 后，选择项目文件夹；在“模型配置”中保存 OpenAI Responses API 兼容地址、模型 ID 和 API Key，然后可直接切换。历史对话从该独立目录的 Codex 线程库加载，选中后恢复完整上下文。

## 致谢

感谢尹海航（龙腾猫跃 / LTCat）创建并长期维护 Plain Craft Launcher 2。PCL2 的界面、交互、启动流程、下载能力以及大量工程实现，都来自原作者与社区的长期积累。本仓库的恢复与维护工作建立在这些成果之上。

感谢 PCL2 社区、问题反馈者、贡献者和文档维护者。正是这些长期沉淀，让许多功能行为和边界得以准确还原。

感谢在恢复、构建和发布过程中使用过的工具与服务：

- Git、GitHub、GitHub CLI：用于版本管理、仓库上传、Release 发布与协作流程。
- GitHub Actions、MSBuild、Visual Studio Build Tools、.NET SDK：用于 Windows Release 构建与产物验证。
- ripgrep、Bash、Python zipfile：用于代码检索、脚本化检查和发布包整理。
- ILSpy 与 .NET 反编译/分析工具链：用于阅读既有构建产物，并对照恢复缺失逻辑。
- Terracotta / EasyTier：为测试联机模式提供参考实现与底层联机能力。Terracotta 源码按 AGPL-3.0 协议随仓库公开，EasyTier 仍作为底层联机过渡资产随构建提供。
- OpenAI Codex / Codex CLI：用于辅助代码阅读、修改、验证、提交和发布流程。
- Newtonsoft.Json、NAudio、Ookii.Dialogs.Wpf、Imazen.WebP、CacheCow、ThrottleDebounce 等第三方库：为 PCL2 的数据处理、音频、对话框、图片、缓存和交互能力提供支持。

## 留言
这个仓库会优先保持优先探索，更好服务于群众，欢迎大家提交pr和issues
