# Cards v0 Data Schema

本文件定义 `data/cards/cards_v0.csv` 和 `data/statuses/statuses_v0.csv` 的字段规范。

**重要说明**: 本文件中的字段是**设计表达**，不是运行时脚本表达式。`battle-core` 线程在实现时，应将这些字段作为参考，通过代码逻辑实现对应效果，**不应尝试直接 eval 或解析这些表达式**。

## 卡牌数据表 (cards_v0.csv)

### 字段清单

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | string | 是 | 卡牌唯一标识符，小写英文 + 下划线 |
| `name` | string | 是 | 卡牌显示名称 |
| `type` | string | 是 | 卡牌类型：`Attack` / `Skill` / `Power` |
| `target` | string | 是 | 目标类型：`Enemy` / `Player` / `AllEnemies` |
| `energy_cost` | int | 是 | 能量消耗，0 表示不消耗 |
| `dice_cost` | int | 是 | 骰子消耗，0 表示不消耗 |
| `dice_type` | string | 否 | 骰子类型，`Any` 表示任意类型 |
| `damage_formula` | string | 否 | 伤害计算公式（设计表达，见下文） |
| `effect_condition` | string | 否 | 效果触发条件（设计表达，见下文） |
| `effect_description` | string | 否 | 效果描述（设计表达，见下文） |
| `description` | string | 是 | 卡牌完整描述（中文） |
| `test_cases` | string | 否 | 测试用例简要说明 |
| `animation_key` | string | 否 | 演出类型键，例如 `energy_blade`；只影响表现，不影响结算 |
| `mimic_style` | string | 否 | 拟态风格，例如 `blade` / `guard` / `data` / `void` |
| `dice_vfx_key` | string | 否 | 骰子演出键，例如 `default_dice_roll` / `high_roll_flash` |
| `hit_vfx_key` | string | 否 | 命中反馈键，例如 `enemy_core_hit` / `shield_break` |
| `impact_level` | string | 否 | 打击强度：`light` / `medium` / `heavy` / `finisher` |

### 字段详细说明

#### id

- 格式: 小写英文单词，用下划线分隔
- 示例: `energy_strike`, `break_core`, `quick_strike`
- 用途: 代码中引用卡牌的唯一标识

#### name

- 格式: 英文名称，首字母大写
- 示例: `EnergyStrike`, `BreakCore`
- 用途: 代码中静态属性名，显示名称可另外配置

#### type

- 可选值:
  - `Attack`: 攻击卡，造成伤害
  - `Skill`: 技能卡，非伤害效果
  - `Power`: 能力卡，持续被动效果（v0 暂未使用）

#### target

- 可选值:
  - `Enemy`: 目标为敌人
  - `Player`: 目标为玩家
  - `AllEnemies`: 目标为所有敌人（v0 暂未使用）

#### energy_cost

- 数值范围: 0 ~ N（N 为玩家最大能量）
- 含义: 打出卡牌需要消耗的能量数量

#### dice_cost

- 数值范围: 0 ~ N（N 为玩家当前骰子池数量）
- 含义: 打出卡牌需要消耗的骰子数量

#### dice_type

- 当前值: `Any`（任意类型）
- 预留字段: 未来可扩展为特定骰子类型（如火骰、冰骰等）

#### damage_formula

**这是设计表达，不是运行时脚本！**

- 格式: 自然语言描述的公式
- 含义: 说明伤害如何计算
- 实现方式: battle-core 线程通过代码实现对应逻辑

**公式语法说明**:

| 符号 | 含义 | 示例 |
|------|------|------|
| `dice` | 单骰子结果 | `dice + 2` |
| `dice1`, `dice2` | 多骰子结果（按消耗顺序） | `(dice1 + dice2) × 1.5` |
| `floor(...)` | 向下取整 | `floor((dice1 + dice2) × 1.5)` |
| 固定数值 | 固定伤害值 | `8` |

**示例**:

| damage_formula | 含义 | 代码实现逻辑 |
|----------------|------|-------------|
| `dice + 2` | 骰子值加 2 | `dice.Value + 2` |
| `8` | 固定 8 伤害 | `8` |
| `floor((dice1 + dice2) × 1.5)` | 双骰子之和乘以 1.5，向下取整 | `Mathf.FloorToInt((dice1.Value + dice2.Value) * 1.5f)` |
| `(dice1 + dice2) + 6` | 双骰子之和加 6 | `dice1.Value + dice2.Value + 6` |
| `dice + 1` | 骰子值加 1 | `dice.Value + 1` |
| `dice + 3` | 骰子值加 3 | `dice.Value + 3` |

#### effect_condition

**这是设计表达，不是运行时脚本！**

- 格式: 自然语言描述的条件
- 含义: 触发额外效果的条件
- 实现方式: battle-core 线程通过代码实现对应条件判断

**条件语法说明**:

| 条件 | 含义 | 示例 |
|------|------|------|
| `dice >= N` | 单骰子结果大于等于 N | `dice >= 5`, `dice >= 3`, `dice >= 4` |
| `dice_sum >= N` | 多骰子结果之和大于等于 N | `dice_sum >= 9` |

**示例**:

| effect_condition | 含义 | 代码实现逻辑 |
|------------------|------|-------------|
| `dice >= 5` | 骰子值 >= 5 | `dice.Value >= 5` |
| `dice >= 3` | 骰子值 >= 3 | `dice.Value >= 3` |
| `dice >= 4` | 骰子值 >= 4 | `dice.Value >= 4` |
| `dice_sum >= 9` | 双骰子之和 >= 9 | `dice1.Value + dice2.Value >= 9` |

#### effect_description

**这是设计表达，不是运行时脚本！**

- 格式: 自然语言描述的效果
- 含义: 满足条件时触发的额外效果
- 实现方式: battle-core 线程通过代码实现对应效果

**效果类型说明**:

| 效果 | 含义 | 代码实现逻辑 |
|------|------|-------------|
| `施加 2 层破甲` | 给敌人添加 2 层破甲 | `enemy.AddVulnerable(2)` |
| `施加 1 层破甲` | 给敌人添加 1 层破甲 | `enemy.AddVulnerable(1)` |
| `额外造成 5 伤害` | 在基础伤害上额外加 5 | `damage += 5` |
| `伤害翻倍` | 将最终伤害乘以 2 | `damage *= 2` |
| `恢复 3 Energy` | 玩家恢复 3 能量 | `player.RestoreEnergy(3)` |
| `获得 1 枚额外骰子` | 玩家骰子池 +1 | `player.AddDice(1)` |
| `恢复 dice + 1 Energy` | 根据骰子值恢复能量 | `player.RestoreEnergy(dice.Value + 1)` |

#### description

- 格式: 中文描述
- 含义: 卡牌完整说明，包含消耗、效果和触发条件

#### test_cases

- 格式: 分号分隔的测试用例
- 含义: 简要说明关键测试场景

#### animation_key / mimic_style / dice_vfx_key / hit_vfx_key / impact_level

这些字段用于未来表现系统，不属于战斗结算字段。

设计目标：

```text
骰子通用动画
→ 拟态职业动画
→ 卡牌效果表现
→ 敌方核心受击反馈
```

示例：

| 字段 | EnergyStrike 示例 |
|------|-------------------|
| `animation_key` | `energy_blade` |
| `mimic_style` | `blade` |
| `dice_vfx_key` | `default_dice_roll` |
| `hit_vfx_key` | `enemy_core_hit` |
| `impact_level` | `light` |

实现要求：

- 这些字段不得改变 Energy、骰子、伤害、状态或目标选择规则。
- 当前 v0 CSV 可以暂不填写这些字段；真正实现动画系统时再追加到数据表末尾。
- 如果代码暂未读取这些字段，不应因此阻塞 battle-core 实现。

## 状态数据表 (statuses_v0.csv)

### 字段清单

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | string | 是 | 状态唯一标识符 |
| `name` | string | 是 | 状态显示名称 |
| `description` | string | 是 | 状态描述 |
| `stack_behavior` | string | 是 | 叠加行为：`叠加` / `覆盖` / `不叠加` |
| `duration_type` | string | 是 | 持续类型：`永久` / `回合` / `战斗` |
| `effect_type` | string | 是 | 效果类型：`伤害加成` / `防御加成` / `持续伤害` |
| `effect_value` | int | 是 | 效果数值 |
| `consume_on_attack` | bool | 是 | 受到攻击后是否减少层数；当前破甲为 `false` |
| `consume_on_turn_end` | bool | 是 | 回合结束时是否减少层数；当前破甲为 `true` |

### 字段详细说明

#### id

- 格式: 小写英文单词
- 示例: `vulnerable`

#### name

- 格式: 中文名称
- 示例: `破甲`

#### description

- 格式: 中文描述
- 含义: 状态效果的完整说明

#### stack_behavior

- 可选值:
  - `叠加`: 每层叠加效果
  - `覆盖`: 新状态覆盖旧状态
  - `不叠加`: 不能叠加，已有则不添加

#### duration_type

- 可选值:
  - `永久`: 直到被消耗或移除
  - `回合`: 持续指定回合数
  - `战斗`: 持续整场战斗

#### effect_type

- 当前值: `伤害加成`
- 预留值: `防御加成`, `持续伤害` 等

#### effect_value

- 数值范围: 整数
- 含义: 每层状态提供的效果数值

#### consume_on_attack

- 值: `true` / `false`
- 含义: 敌人受到攻击后是否减少 1 层。当前 demo 中破甲不在攻击后减少。

#### consume_on_turn_end

- 值: `true` / `false`
- 含义: 回合结束时是否减少 1 层。当前 demo 中破甲在敌人回合结束时减少 1 层。

## 数据使用流程

### 当前阶段（v0）

```text
1. 设计文档 (cards_v0_design.md) → 定义卡牌概念
2. 数据表 (cards_v0.csv) → 结构化存储卡牌参数
3. 测试表 (cards_v0_test_cases.csv) → 定义验收条件
4. battle-core 线程 → 根据文档和表格，用 C# 代码实现卡牌效果
5. training-ground 线程 → 根据测试表验证实现
```

### 未来阶段（数据驱动）

```text
1. 数据表 → 结构化存储卡牌参数
2. 数据加载器 → 从 CSV/JSON/Resource 加载卡牌数据
3. 表达式解析器 → 解析 damage_formula、effect_condition
4. 效果执行器 → 根据解析结果执行效果
```

**当前阶段不实现表达式解析器**，所有公式和条件都通过硬编码实现。

## 卡牌测试表 (cards_v0_test_cases.csv)

测试表中的 `expected_*_delta` 字段统一表示“结算前后变化量”：

| 字段名 | 说明 |
|--------|------|
| `expected_energy_delta` | 玩家 Energy 净变化。消耗为负数，恢复为正数 |
| `expected_dice_pool_delta` | 玩家骰池净变化。消耗为负数，新增骰子为正数 |
| `expected_damage` | 本次对敌人造成的最终伤害，包含暴击和已有破甲加成 |
| `expected_enemy_hp_delta` | 敌人 HP 净变化，通常为 `-expected_damage` |
| `expected_enemy_vulnerable_delta` | 敌人破甲层数净变化。当前 demo 中，攻击不会消耗旧破甲；新施加破甲为正数；敌人回合结束时单独测试 -1 |
| `can_play` | 是否应允许打出。为 `false` 时，各变化量应为 0 |

示例：敌人已有 2 层破甲时打出 `EnergyStrike`，Fixed Roll = 3，基础伤害为 5，已有破甲加成为 2，最终伤害为 7；攻击不会消耗旧破甲，因此 `expected_enemy_vulnerable_delta = 0`。敌人回合结束时，破甲再减少 1 层，应由独立回合结束测试覆盖。

## 与代码的映射关系

### 卡牌类型映射

| CSV type | C# CardType |
|----------|-------------|
| `Attack` | `CardType.Attack` |
| `Skill` | `CardType.Skill` |
| `Power` | `CardType.Power` |

### 目标类型映射

| CSV target | C# TargetType |
|------------|---------------|
| `Enemy` | `TargetType.Enemy` |
| `Player` | `TargetType.Player` |
| `AllEnemies` | `TargetType.AllEnemies` |

### 状态类型映射

| CSV id | C# StatusType |
|--------|---------------|
| `vulnerable` | `StatusType.Vulnerable` |

## 版本控制

- 当前版本: v0.1
- 数据格式变更时，应更新版本号
- 新增字段时，应在本文件中同步更新

## 注意事项

1. **不要尝试解析公式**: `damage_formula` 和 `effect_condition` 是给开发者看的设计文档，不是给程序解析的脚本。
2. **代码实现优先**: 当 CSV 字段与代码实现不一致时，以代码实现为准，但应尽快更新 CSV 保持一致。
3. **字段顺序**: CSV 字段顺序应保持稳定，新增字段应追加到末尾。
4. **空值处理**: 可选字段为空时，表示该卡牌没有对应效果。
