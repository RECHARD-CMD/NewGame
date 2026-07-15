# Battle Core Contract v0.1

本文件定义当前项目战斗核心的最小接口与并行开发边界。训练场、正式战斗、测试场景和后续 demo 都必须调用同一套战斗核心，不允许各自实现伤害、骰子、状态或 Energy 承伤规则。

## 目标

- 保证训练场和正式战斗共享同一套规则。
- 避免多个 AI 或多个分支重复实现战斗结算。
- 为并行开发提供稳定接口。

## 当前核心规则

- Energy 同时是出牌资源和护盾。
- 玩家每回合获得未揭示骰池。
- 需要骰子的卡牌在打出时自动消耗最左侧可用骰子。
- 被消耗的骰子在出牌瞬间掷出。
- 骰点影响结算结果，不影响卡牌是否完全生效。
- 敌人攻击先扣玩家 Energy，Energy 不足时扣 HP。
- 如果卡牌描述为“造成伤害，并在骰点满足条件时施加状态”，默认先造成本次伤害，再施加状态。
- 新施加的状态不应被本次攻击立即消耗。

## 当前交互规则

- 单击卡牌：显示预览。
- 再次单击当前预览卡牌：取消预览。
- 双击卡牌：尝试打出。
- 不能打出的卡牌仍可预览，但双击不结算。
- 当前不做手动选骰、拖拽绑骰或提前显示骰点。

## 当前核心对象

- `BattleManager`：战斗流程、回合、出牌、敌人行动和信号。
- `PlayerState`：玩家 HP、Energy、DicePool、Hand。
- `EnemyState`：敌人 HP、Intent、Statuses。
- `CardData`：卡牌静态数据与效果委托。
- `CardInstance`：战斗中的卡牌实例。
- `DiceInstance`：本回合未揭示骰子，出牌时掷出并消耗。
- `DiceRoller`：随机或固定掷骰。
- `StatusInstance` / `StatusType`：状态类型与层数。

## 最小接口

当前代码接口以实际 C# 为准，概念上应保持以下能力：

```text
InitializeBattle()
InitializeBattle(PlayerState player, EnemyState enemy, DiceRoller diceRoller = null)
StartPlayerTurn()
TryPlayCard(CardInstance card)
EndPlayerTurn()
ExecuteEnemyTurn()
SkipTurn()
```

战斗日志通过信号输出：

```text
BattleLog(string message)
```

训练场可以订阅日志，但不能复制战斗结算。

## 当前测试卡

### EnergyStrike

- Id：`energy_strike`
- Cost：1 Energy
- DiceCost：1
- 效果：打出时掷 1 枚默认骰，造成 `骰点 + 2` 伤害。

### BreakCore

- Id：`break_core`
- Cost：3 Energy
- DiceCost：1
- 基础效果：造成 8 伤害。
- 骰点触发：如果骰点 >= 5，造成伤害后施加 2 层破甲。

## 当前状态

### Vulnerable / 破甲

- 敌人每层破甲使受到的攻击伤害 +1。
- 当前 demo 中，破甲在敌人回合结束时减少 1 层。
- 如果某张卡在本次攻击后施加破甲，新破甲不能被本次攻击立即消耗。

## 当前测试敌人

### TrainingDummy

- HP：999
- Intent：不行动
- 用途：测试卡牌、骰子、伤害和状态。

### TrainingBeast

- HP：20
- Intent：固定攻击
- 用途：测试 Energy 承伤、HP 承伤、失败状态。

## 并行开发边界

### battle-core 线程可以改

```text
scripts/battle/
scripts/core/
```

### training-ground 线程可以改

```text
scenes/training/
scripts/training/
scripts/ui/BattleLogPanel.cs
```

### cards-v0-design 线程可以改

```text
docs/design/
docs/balance/
data/cards/
data/statuses/
```

### battle-log-and-tests 线程可以改

```text
docs/specs/
docs/tests/
```

## 验收要求

修改战斗核心、训练场或卡牌结算后，至少验证：

```text
dotnet build --no-restore
Godot 能加载当前主场景
```

训练场中还应人工验证：

- Fixed Roll = 1 时，EnergyStrike 伤害为 3。
- Fixed Roll = 6 时，EnergyStrike 伤害为 8。
- Fixed Roll = 5/6 时，BreakCore 造成 8 伤害后施加 2 层破甲。
- 后续攻击会因破甲增加伤害。
- 敌人回合结束时，破甲减少 1 层。
- TrainingBeast 攻击时，玩家 Energy 先减少，Energy 不足再扣 HP。
