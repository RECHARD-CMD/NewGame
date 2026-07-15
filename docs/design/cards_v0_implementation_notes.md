# Cards v0 Implementation Notes

本文件为 battle-core 线程提供明确的实现交接说明，指出每张卡牌需要的核心系统扩展点。

## 现有核心能力

当前 `CardData` 支持：
- 单骰子伤害公式: `DamageFormula(CardInstance, DiceInstance)`
- 单骰子条件效果: `ApplyEffect(CardInstance, DiceInstance, EnemyState)`

## 需要扩展的核心能力

### 1. 多骰输入支持

**涉及卡牌**: `double_strike`、`power_strike`

**当前问题**: `DamageFormula` 和 `ApplyEffect` 只接收单个 `DiceInstance`。

**扩展方案**:
- 需要支持接收 `List<DiceInstance>` 参数
- 结算时按卡牌 `DiceCost` 依次消耗骰子
- 所有骰子在打出瞬间同时掷出

**优先级**: 第三批

### 2. 非伤害 Skill 效果

**涉及卡牌**: `defensive_posture`、`dice_boost`、`energy_surge`

**当前问题**: `CardData` 没有独立的非伤害效果执行机制。

**扩展方案**:
- 在 `CardData` 中增加 `ApplySkillEffect(CardInstance, DiceInstance, PlayerState)` 委托
- Skill 类型卡牌不经过伤害结算流程
- Skill 效果直接作用于玩家状态

**优先级**: 第三批

### 3. 条件后效（伤害修改类）

**涉及卡牌**: `critical_hit`

**当前问题**: 暴击需要在伤害计算后、伤害施加前修改最终伤害值。

**扩展方案**:
- 在 `ApplyEffect` 执行时机前增加 `ModifyDamage` 回调
- 或在 `DamageFormula` 中支持条件判断
- 结算顺序：计算基础伤害 → 应用伤害修改 → 施加破甲加成 → 造成伤害

**优先级**: 第二批

### 4. 条件后效（状态施加类）

**涉及卡牌**: `break_core`、`vulnerable_strike`

**当前问题**: 当前系统已有 `ApplyEffect`，但需要确认结算顺序。

**确认事项**:
- 先造成本次伤害（含已存在的破甲加成）
- 再施加新状态
- 新施加的状态不被本次攻击消耗

**优先级**: 第一批（`break_core`）、第二批（`vulnerable_strike`）

### 5. 取整规则

**涉及卡牌**: `double_strike`

**规则**: 所有除法和乘法结果使用 **floor**（向下取整）。

**示例**:
- (1+1) × 1.5 = 3.0 → floor → 3
- (6+6) × 1.5 = 18.0 → floor → 18
- (3+4) × 1.5 = 10.5 → floor → 10

## 三轮实现范围

### 第一批（必须实现）

| 卡牌 | 需要的核心能力 | 说明 |
|------|---------------|------|
| `energy_strike` | 单骰伤害公式 | 已有，直接实现 |
| `break_core` | 单骰条件状态施加 | 需要 `ApplyEffect` 中判断 dice >= 5 |

**验收标准**:
- Fixed Roll = 1 时，EnergyStrike 伤害为 3
- Fixed Roll = 6 时，EnergyStrike 伤害为 8
- Fixed Roll = 4 时，BreakCore 伤害为 8，不施加破甲
- Fixed Roll = 5 时，BreakCore 伤害为 8，施加 2 层破甲
- 破甲在后续攻击中正确增加伤害；破甲层数在敌人回合结束时减少 1 层

### 第二批（建议实现）

| 卡牌 | 需要的核心能力 | 说明 |
|------|---------------|------|
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

| 卡牌 | 需要的核心能力 | 说明 |
|------|---------------|------|
| `double_strike` | 双骰伤害公式 + floor | 需要支持多骰子输入和取整 |
| `power_strike` | 双骰伤害公式 + 条件额外伤害 | 需要支持多骰子输入和骰子总和判断 |
| `defensive_posture` | 非伤害 Skill（能量恢复） | 需要新增 Skill 效果机制 |
| `dice_boost` | 非伤害 Skill（骰子生成） | 需要新增 Skill 效果机制 |
| `energy_surge` | 非伤害 Skill（骰子换能量） | 需要新增 Skill 效果机制 |

**验收标准**:
- DoubleStrike 需要 2 枚骰子，伤害为 floor((dice1+dice2)×1.5)
- PowerStrike 需要 2 枚骰子和 4 Energy，骰子总和 >= 9 时额外加 5 伤害
- DefensivePosture 消耗 1 Energy，恢复 3 Energy（净 +2）
- DiceBoost 消耗 2 Energy，玩家骰子池 +1
- EnergySurge 消耗 1 枚骰子，恢复 dice + 1 Energy

## 结算顺序规范

### Attack 类型卡牌

```text
1. 检查 Energy 是否足够
2. 检查 DicePool 是否有足够骰子
3. 消耗 Energy
4. 消耗指定数量的骰子（自动消耗最左侧）
5. 对所有消耗的骰子进行掷骰
6. 根据伤害公式计算基础伤害
7. 应用伤害修改效果（如暴击翻倍）
8. 记录敌人攻击前已有破甲层数，并计算破甲加成
9. 造成最终伤害（基础伤害 + 伤害修改 + 已有破甲加成）
10. 应用状态施加效果（在伤害之后）
11. 破甲不在攻击后减少；由敌人回合结束阶段统一减少 1 层
```

### Skill 类型卡牌

```text
1. 检查 Energy 是否足够
2. 检查 DicePool 是否有足够骰子
3. 消耗 Energy
4. 消耗指定数量的骰子（自动消耗最左侧）
5. 对所有消耗的骰子进行掷骰
6. 应用 Skill 效果（直接作用于玩家状态）
```

## 关键实现注意事项

### 破甲结算
- 破甲加成 = 破甲层数 × 1
- 破甲减少时机：敌人回合结束时减少 1 层
- 新施加的破甲不被本次攻击消耗
- 攻击只读取已有破甲作为伤害加成，不消耗旧破甲

### 骰子消耗
- 自动消耗最左侧可用骰子
- 消耗数量 = `CardData.DiceCost`
- 消耗后立即掷骰，不提前显示结果
- 多骰子卡需要一次性消耗所有需要的骰子

### 能量结算
- Energy 同时是出牌资源和护盾
- 出牌时先检查 Energy 是否足够
- 出牌时消耗 Energy（而不是伤害结算时）
- 敌人攻击时先扣 Energy，Energy 不足时扣 HP

### 卡牌不可打出的情况
- Energy 不足
- DicePool 中骰子数量不足
- 非玩家回合
- 卡牌已在手牌外

## 与现有代码的接口对齐

### CardData 扩展建议

```text
// 当前已有
Func<CardInstance, DiceInstance, int> DamageFormula;
Action<CardInstance, DiceInstance, EnemyState> ApplyEffect;

// 建议新增
Func<CardInstance, List<DiceInstance>, int> MultiDiceDamageFormula;
Action<CardInstance, List<DiceInstance>, EnemyState> MultiDiceApplyEffect;
Action<CardInstance, DiceInstance, PlayerState> ApplySkillEffect;
Action<CardInstance, DiceInstance, ref int> ModifyDamage;
```

### BattleManager 修改建议

```text
// TryPlayCard 需要扩展：
1. 根据 DiceCost 决定调用单骰或多骰公式
2. Skill 类型不走伤害结算流程
3. 增加伤害修改阶段（在破甲加成前）
4. 确保状态施加在伤害之后
```

## 测试优先级

1. 第一批：确保基础战斗闭环可用
2. 第二批：增加策略深度和状态机制
3. 第三批：扩展资源管理和多骰子联动

## 风险提示

- 多骰子结算需要修改 `CardData` 和 `BattleManager` 的核心接口
- Skill 效果需要新增玩家状态修改机制
- 伤害修改时机需要仔细设计，避免与破甲结算冲突
- 取整规则必须统一，避免不同卡牌使用不同取整方式
