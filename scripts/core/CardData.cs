using System;

public enum CardType
{
    Attack,
    Skill,
    Power
}

public enum TargetType
{
    Enemy,
    Player,
    AllEnemies
}

public class CardData
{
    public string Id;
    public string Name;
    public string Description;
    public CardType Type;
    public TargetType Target;
    public int EnergyCost;
    public int DiceCost;
    public string DiceType;
    public Func<CardInstance, DiceInstance, int> DamageFormula;
    public Action<CardInstance, DiceInstance, EnemyState> ApplyEffect;
    public Func<CardInstance, DiceInstance, int, int> ModifyDamage;
    
    public static CardData EnergyStrike = new CardData()
    {
        Id = "energy_strike",
        Name = "EnergyStrike",
        Description = "消耗 1 Energy 和 1 枚默认骰；打出时掷骰，造成 骰点 + 2 伤害",
        Type = CardType.Attack,
        Target = TargetType.Enemy,
        EnergyCost = 1,
        DiceCost = 1,
        DiceType = "Any",
        DamageFormula = (card, dice) => dice.Value.GetValueOrDefault() + 2
    };
    
    public static CardData BreakCore = new CardData()
    {
        Id = "break_core",
        Name = "BreakCore",
        Description = "消耗 3 Energy 和 1 枚默认骰；造成 8 伤害；骰点 >= 5 时施加 2 层破甲",
        Type = CardType.Attack,
        Target = TargetType.Enemy,
        EnergyCost = 3,
        DiceCost = 1,
        DiceType = "Any",
        DamageFormula = (card, dice) => 8,
        ApplyEffect = (card, dice, enemy) =>
        {
            if (dice.Value.GetValueOrDefault() >= 5)
            {
                enemy.AddVulnerable(2);
            }
        }
    };
    
    public static CardData QuickStrike = new CardData()
    {
        Id = "quick_strike",
        Name = "QuickStrike",
        Description = "无需消耗；造成 4 点固定伤害",
        Type = CardType.Attack,
        Target = TargetType.Enemy,
        EnergyCost = 0,
        DiceCost = 0,
        DiceType = "Any",
        DamageFormula = (card, dice) => 4
    };
    
    public static CardData VulnerableStrike = new CardData()
    {
        Id = "vulnerable_strike",
        Name = "VulnerableStrike",
        Description = "消耗 2 Energy 和 1 枚默认骰；造成 骰点 + 1 伤害；骰点 >= 3 时施加 1 层破甲",
        Type = CardType.Attack,
        Target = TargetType.Enemy,
        EnergyCost = 2,
        DiceCost = 1,
        DiceType = "Any",
        DamageFormula = (card, dice) => dice.Value.GetValueOrDefault() + 1,
        ApplyEffect = (card, dice, enemy) =>
        {
            if (dice.Value.GetValueOrDefault() >= 3)
            {
                enemy.AddVulnerable(1);
            }
        }
    };
    
    public static CardData CriticalHit = new CardData()
    {
        Id = "critical_hit",
        Name = "CriticalHit",
        Description = "消耗 2 Energy 和 1 枚默认骰；造成 骰点 + 3 伤害；骰点 >= 4 时伤害翻倍",
        Type = CardType.Attack,
        Target = TargetType.Enemy,
        EnergyCost = 2,
        DiceCost = 1,
        DiceType = "Any",
        DamageFormula = (card, dice) => dice.Value.GetValueOrDefault() + 3,
        ModifyDamage = (card, dice, baseDamage) =>
        {
            if (dice.Value.GetValueOrDefault() >= 4)
            {
                return baseDamage * 2;
            }
            return baseDamage;
        }
    };
    
    public void GetDamageRange(int diceSides, out int min, out int max)
    {
        if (DamageFormula == null)
        {
            min = 0;
            max = 0;
            return;
        }
        
        var tempCard = new CardInstance(this);
        
        if (DiceCost == 0)
        {
            min = DamageFormula(tempCard, null);
            max = min;
            return;
        }
        
        var minDice = new DiceInstance(diceSides);
        minDice.Value = 1;
        var maxDice = new DiceInstance(diceSides);
        maxDice.Value = diceSides;
        
        min = DamageFormula(tempCard, minDice);
        max = DamageFormula(tempCard, maxDice);
        
        if (ModifyDamage != null)
        {
            max = ModifyDamage(tempCard, maxDice, max);
        }
    }
}