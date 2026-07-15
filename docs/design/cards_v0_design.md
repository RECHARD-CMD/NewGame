# Cards v0 Design

本文件定义第一批 10 张测试卡牌的设计规格。所有卡牌遵循 battle_core_contract.md 中的核心规则。

## 设计原则

1. 每张卡牌都应有清晰的决策点：是否消耗骰子、是否消耗能量、是否触发骰点效果。
2. 骰点影响结果但不导致卡牌完全失效。
3. 低骰点有基础效果，高骰点有额外奖励。
4. 第一批优先覆盖 Attack 和 Skill；Power 类型保留到有持续被动机制后再进入数据。
5. 覆盖多种机制：伤害、状态、能量恢复、骰子生成。

## 卡牌列表

### 1. EnergyStrike（能量打击）

- **Id**: `energy_strike`
- **类型**: Attack
- **目标**: Enemy
- **Energy Cost**: 1
- **Dice Cost**: 1
- **描述**: 消耗 1 Energy 和 1 枚默认骰；打出时掷骰，造成 **骰点 + 2** 伤害。
- **设计目的**: 基础攻击卡，展示骰子与伤害的基础联动。
- **伤害范围**: 3~8 (d6)
- **测试验证**:
  - Fixed Roll = 1: 伤害 3
  - Fixed Roll = 6: 伤害 8
- **实现批次**: 第一批（必须实现）

### 2. BreakCore（破核）

- **Id**: `break_core`
- **类型**: Attack
- **目标**: Enemy
- **Energy Cost**: 3
- **Dice Cost**: 1
- **描述**: 消耗 3 Energy 和 1 枚默认骰；造成 **8** 伤害；**骰点 >= 5** 时施加 **2 层破甲**。
- **设计目的**: 高消耗攻击卡，展示骰点触发状态效果。
- **伤害范围**: 8~8；破甲是后续攻击收益，本次攻击不消耗新施加的破甲
- **测试验证**:
  - Fixed Roll = 4: 伤害 8，不施加破甲
  - Fixed Roll = 5: 伤害 8，施加 2 层破甲
- **实现批次**: 第一批（必须实现）

### 3. QuickStrike（速击）

- **Id**: `quick_strike`
- **类型**: Attack
- **目标**: Enemy
- **Energy Cost**: 0
- **Dice Cost**: 0
- **描述**: 无需消耗；造成 **4** 点固定伤害。
- **设计目的**: 零消耗攻击卡，测试无骰子无能量消耗的卡牌机制。
- **伤害范围**: 4~4
- **测试验证**:
  - 无需骰子即可打出
  - 无需能量即可打出
  - 固定造成 4 伤害
- **实现批次**: 第二批（建议实现）

### 4. DoubleStrike（双重打击）

- **Id**: `double_strike`
- **类型**: Attack
- **目标**: Enemy
- **Energy Cost**: 2
- **Dice Cost**: 2
- **描述**: 消耗 2 Energy 和 2 枚默认骰；造成 **floor((骰点1 + 骰点2) × 1.5)** 伤害。
- **设计目的**: 多骰子消耗攻击卡，展示多骰子联动机制。
- **伤害范围**: 3~18 (2d6)
- **测试验证**:
  - 需要 2 枚骰子才能打出
  - 双骰子结果相加后乘以 1.5，并向下取整
- **实现批次**: 第三批（扩展实现）

### 5. DefensivePosture（防御姿态）

- **Id**: `defensive_posture`
- **类型**: Skill
- **目标**: Player
- **Energy Cost**: 1
- **Dice Cost**: 0
- **描述**: 消耗 1 Energy；恢复 **3** Energy。
- **设计目的**: 低消耗技能卡，测试能量恢复机制。
- **效果**: 净消耗 -2 Energy
- **测试验证**:
  - 消耗 1 Energy，恢复 3 Energy
  - 玩家 Energy 增加 2
- **实现批次**: 第三批（扩展实现）

### 6. DiceBoost（骰子充能）

- **Id**: `dice_boost`
- **类型**: Skill
- **目标**: Player
- **Energy Cost**: 2
- **Dice Cost**: 0
- **描述**: 消耗 2 Energy；获得 **1** 枚额外骰子（本回合可用）。
- **设计目的**: 技能卡，测试额外骰子生成机制。
- **效果**: 本回合骰子数量 +1
- **测试验证**:
  - 消耗 2 Energy
  - 玩家骰子池增加 1 枚
- **实现批次**: 第三批（扩展实现）

### 7. PowerStrike（强力打击）

- **Id**: `power_strike`
- **类型**: Attack
- **目标**: Enemy
- **Energy Cost**: 4
- **Dice Cost**: 2
- **描述**: 消耗 4 Energy 和 2 枚默认骰；造成 **(骰点1 + 骰点2) + 6** 伤害；**骰点总和 >= 9** 时额外造成 **5** 伤害。
- **设计目的**: 高消耗多骰子攻击卡，展示复杂骰点触发条件。
- **伤害范围**: 8~23 (2d6)
- **测试验证**:
  - 需要 2 枚骰子和 4 Energy
  - 双骰子之和 >= 9 时额外加 5 伤害
- **实现批次**: 第三批（扩展实现）

### 8. VulnerableStrike（破甲打击）

- **Id**: `vulnerable_strike`
- **类型**: Attack
- **目标**: Enemy
- **Energy Cost**: 2
- **Dice Cost**: 1
- **描述**: 消耗 2 Energy 和 1 枚默认骰；造成 **骰点 + 1** 伤害；**骰点 >= 3** 时施加 **1 层破甲**。
- **设计目的**: 中等消耗攻击卡，较低门槛施加破甲。
- **伤害范围**: 2~7 (d6)
- **测试验证**:
  - Fixed Roll = 2: 伤害 3，不施加破甲
  - Fixed Roll = 3: 伤害 4，施加 1 层破甲
- **实现批次**: 第二批（建议实现）

### 9. EnergySurge（能量涌动）

- **Id**: `energy_surge`
- **类型**: Skill
- **目标**: Player
- **Energy Cost**: 0
- **Dice Cost**: 1
- **描述**: 消耗 1 枚默认骰；恢复 **骰点 + 1** Energy。
- **设计目的**: 零能量消耗技能卡，展示骰子消耗换取能量的机制。
- **恢复范围**: 2~7 Energy (d6)
- **测试验证**:
  - 无需 Energy，需要 1 枚骰子
  - Fixed Roll = 1: 恢复 2 Energy
  - Fixed Roll = 6: 恢复 7 Energy
- **实现批次**: 第三批（扩展实现）

### 10. CriticalHit（暴击）

- **Id**: `critical_hit`
- **类型**: Attack
- **目标**: Enemy
- **Energy Cost**: 2
- **Dice Cost**: 1
- **描述**: 消耗 2 Energy 和 1 枚默认骰；造成 **骰点 + 3** 伤害；**骰点 >= 4** 时伤害翻倍。
- **设计目的**: 高风险高回报攻击卡，展示暴击机制。
- **伤害范围**: 4~18 (d6)
- **测试验证**:
  - Fixed Roll = 3: 伤害 6（不暴击）
  - Fixed Roll = 4: 伤害 14（暴击，(4+3)×2）
- **实现批次**: 第二批（建议实现）

## 实现批次划分

### 第一批（必须实现）

| 卡牌 | 核心机制 | 需要的核心能力 |
|------|----------|---------------|
| `energy_strike` | 单骰伤害 | 已有，直接实现 |
| `break_core` | 单骰条件状态施加 | 需要 `ApplyEffect` 中判断 dice >= 5 |

**验收标准**:
- Fixed Roll = 1 时，EnergyStrike 伤害为 3
- Fixed Roll = 6 时，EnergyStrike 伤害为 8
- Fixed Roll = 4 时，BreakCore 伤害为 8，不施加破甲
- Fixed Roll = 5 时，BreakCore 伤害为 8，施加 2 层破甲
- 破甲在后续攻击中正确增加伤害；破甲层数在敌人回合结束时减少 1 层

### 第二批（建议实现）

| 卡牌 | 核心机制 | 需要的核心能力 |
|------|----------|---------------|
| `quick_strike` | 零消耗固定伤害 | 需要支持 dice_cost = 0 和 energy_cost = 0 |
| `vulnerable_strike` | 单骰条件状态施加 | 需要 `ApplyEffect` 中判断 dice >= 3 |
| `critical_hit` | 单骰条件伤害翻倍 | 需要在伤害计算后应用倍率修改 |

**验收标准**:
- QuickStrike 无需骰子和能量即可打出，固定造成 4 伤害
- Fixed Roll = 2 时，VulnerableStrike 伤害为 3，不施加破甲
- Fixed Roll = 3 时，VulnerableStrike 伤害为 4，施加 1 层破甲
- Fixed Roll = 3 时，CriticalHit 伤害为 6（不暴击）
- Fixed Roll = 4 时，CriticalHit 伤害为 14（暴击，(4+3)×2）

### 第三批（扩展实现）

| 卡牌 | 核心机制 | 需要的核心能力 |
|------|----------|---------------|
| `double_strike` | 双骰伤害 + floor | 需要支持多骰子输入和取整 |
| `power_strike` | 双骰伤害 + 条件额外伤害 | 需要支持多骰子输入和骰子总和判断 |
| `defensive_posture` | 能量恢复 | 需要新增 Skill 效果机制 |
| `dice_boost` | 骰子生成 | 需要新增 Skill 效果机制 |
| `energy_surge` | 骰子换能量 | 需要新增 Skill 效果机制 |

**验收标准**:
- DoubleStrike 需要 2 枚骰子，伤害为 floor((dice1+dice2)×1.5)
- PowerStrike 需要 2 枚骰子和 4 Energy，骰子总和 >= 9 时额外加 5 伤害
- DefensivePosture 消耗 1 Energy，恢复 3 Energy（净 +2）
- DiceBoost 消耗 2 Energy，玩家骰子池 +1
- EnergySurge 消耗 1 枚骰子，恢复 dice + 1 Energy

## 机制分类

### 纯伤害卡
- EnergyStrike: 基础骰子伤害
- QuickStrike: 零消耗固定伤害
- CriticalHit: 高骰点暴击

### 伤害 + 状态卡
- BreakCore: 高消耗 + 高门槛破甲
- VulnerableStrike: 中等消耗 + 低门槛破甲

### 多骰子卡
- DoubleStrike: 双骰子联动
- PowerStrike: 双骰子 + 高门槛额外伤害

### 能量相关卡
- DefensivePosture: 能量恢复（零骰子）
- EnergySurge: 骰子换能量

### 骰子相关卡
- DiceBoost: 能量换骰子

## 与现有系统的兼容性

所有卡牌遵循 battle_core_contract.md 中的规则：

1. Energy 同时是出牌资源和护盾。
2. 需要骰子的卡牌在打出时自动消耗最左侧可用骰子。
3. 被消耗的骰子在出牌瞬间掷出。
4. 骰点影响结算结果，不影响卡牌是否完全生效。
5. 如果卡牌描述为"造成伤害，并在骰点满足条件时施加状态"，默认先造成本次伤害，再施加状态。
6. 新施加的状态不应被本次攻击立即消耗。
