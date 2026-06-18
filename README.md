# HighPerformanceBlacksmith

BlacksmithFramework 的高性能重写版本。围绕《打铁》(Blacksmith) 规则构建的可扩展对战框架，基于 .NET 8/9。

**核心改进**：将判定管线中的"数据"与"行为"解耦，从 Resolution 模式转变为 AnalyzableData + AnalyzerRegistry 模式。经过简单测试，VS 调试模式下约有 **9-10 倍**提速。

## 与 BlacksmithFramework 的关键差异

| 维度 | BlacksmithFramework (BF) | HighPerformanceBlacksmith (HPB) |
|---|---|---|
| 数据模型 | `IResolution` — 数据自带 Execute 委托 | `IAnalyzableData` — 纯数据 + AnalyzerKey 字符串 |
| 防御基类 | `DefenseBase` — 子类实现 `Work()` 方法 | `DefenseEntity` — 纯数据声明，逻辑在 Analyzer 中 |
| 判定阶段 | 11 阶段 | 9 阶段（移除 OnEffectSwaping、OnAttackSwaping） |
| 效果写入 | `WriteEffect(..., Action<...> effectAction)` | `WriteEffect(..., string analyzerKey)` |
| 攻击挂载 | `WithFree(AttackStage, Action<...>)` | `WithRuntime(AttackStage.CEValue, string analyzerKey)` |
| 编译 | `Compile(Judger)` | `Compile(JudgeRuleManager?)` |
| 分析器注册 | 无 | `AnalyzerRegistry` + `[IsAnalyzer]` |
| Power 类型 | float | int（攻击/防御），float（资源） |

详见 [架构升级说明](./Documents/架构升级说明.md)。

## 项目结构

| 项目 | 说明 |
|---|---|
| `Clap/ClapSourceGenerators` | Roslyn 增量源生成器，编译时生成技能注册和分析器注册代码。 |
| `BlacksmithCore` | 核心引擎。领域模型、技能 DSL、判定引擎、动态规则、AI 策略、Mod 加载器、基础单元（时钟、状态变量等）。 |
| `BlacksmithClient` | 本地运行入口。ASP.NET Core 本地站点，托管前端。 |
| `BlacksmithServer` | 多人服务器。 |
| `ModExamples` | 示例 Mod —— 圣书、幻书、炼药锅、弩、武僧、先知、酒杯。 |

所有项目目标框架为 `net8.0`（AIPVPPlatform 为 `net9.0`）。

## 运行方式

```powershell
# 发布纯净版（不含 Mod）
.\BlacksmithPure.cmd

# 发布带示例 Mod 版
.\BlacksmithWithMods.cmd

# 运行
.\BlacksmithPure\BlacksmithClient.exe
# 或
.\BlacksmithWithMods\BlacksmithClient.exe
```

```bash
# Linux 服务器
bash BlacksmithServer.sh
```

`.cmd` 脚本需要 .NET 8.0 SDK，执行 `dotnet publish -c Release`。Mod DLL 通过 `.blacksmith/mod.json` 配置加载。

## 对战模式

| 模式 | 说明 |
|---|---|
| **Manual** | 双方技能均由前端手动输入。 |
| **BloodSigil** | 启发式规则 AI。 |
| **General** | 基于 MCTS 搜索的通用 AI。 |

## 内置职业

| 职业 | 状态 | 说明 |
|---|---|---|
| **Common** | ✅ 已迁移 | 通用技能，可转职到所有其他职业 |
| **Cannon** | ✅ 已迁移 | 钢炮。高物理伤害，穿甲弹 |
| **Driver** | ✅ 已迁移 | 驱动器。被动真实伤减，时空资源转换 |
| **Warlock** | ✅ 已迁移 | 术士。魔法职业，多回合延迟攻击 |
| **BloodSigil** | ✅ 已迁移 | 鲜血印记。以生命换取伤害与吸血 |
| **Alchemy** | ✅ 已迁移 | 炼金（装备技能）。Iron 转 Gold_Iron |
| **Lancer** | ❌ 未迁移 | 战矛。纹章系统（代码已注释） |

`ModExamples/` 提供了 7 个示例 Mod（部分迁移中）。

## 迁移状态

- ✅ 核心引擎（AnalyzableData、AnalyzerRegistry、DefenseEntity、9 阶段管线）
- ✅ StandardAnalyzers（DSL + Defense 共 11 个默认分析器）
- ✅ Common、Cannon、Driver、Warlock、BloodSigil、Alchemy 职业
- ❌ Lancer 职业（代码已注释，待迁移）
- ⚠️ ModExamples（部分迁移，自定义防御/效果仍用旧模式）

## 文档导航

- [架构升级说明](./Documents/架构升级说明.md) — 从 BF 到 HPB 的完整架构变更
- [项目架构](./Documents/项目架构.md) — 面向维护者和 Mod 开发者
- [判定流程](./Documents/判定流程.md) — 9 阶段判定管线
- [Mod 基础指南](./Documents/Mod基础指南/引言.md)
- [Mod 进阶指南](./Documents/Mod进阶指南/引言.md)
- [Blacksmith 规则](./Documents/规则/BlacksmithRuleCN.md)

## 相关仓库

- [BlacksmithFramework](https://github.com/NeonNickNick/BlacksmithFramework) — 原始项目
