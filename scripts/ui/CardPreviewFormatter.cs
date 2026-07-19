using Godot;

public static class CardPreviewFormatter
{
    public static string FormatName(CardData data)
    {
        return data.Name;
    }
    
    public static string FormatType(CardData data)
    {
        return $"{data.Category} / {data.Subtype}";
    }
    
    public static string FormatTarget(CardData data)
    {
        return data.Target.ToString();
    }
    
    public static string FormatEnergyCost(CardData data)
    {
        return $"Energy: {data.EnergyCost}";
    }
    
    public static string FormatDiceCost(CardData data)
    {
        if (data.DiceCost == 0)
            return "Dice: 无需";
        string diceType = !string.IsNullOrEmpty(data.DiceType) && data.DiceType != "Any" ? $" ({data.DiceType})" : "";
        return $"Dice: {data.DiceCost}{diceType}";
    }
    
    public static string FormatDamageRange(CardData data, int diceSides)
    {
        if (data.DamageFormula == null)
            return "";
        
        int minDamage, maxDamage;
        data.GetDamageRange(diceSides, out minDamage, out maxDamage);
        
        if (minDamage == maxDamage)
            return $"伤害: {minDamage}";
        return $"伤害: {minDamage}~{maxDamage}";
    }
    
    public static string FormatShieldValue(CardData data)
    {
        if (data.Subtype != CardSubtype.Defense || data.ShieldValue <= 0)
            return "";
        return $"护盾: {data.ShieldValue}";
    }
    
    public static string FormatBuffDebuff(CardData data)
    {
        if (data.Subtype == CardSubtype.PositiveBuff)
        {
            if (!data.AppliedBuffType.HasValue) return "";
            string duration = data.Duration > 0 ? $" / 持续: {data.Duration}回合" : "";
            return $"增益: {data.AppliedBuffType.Value} ({data.EffectAmount}){duration}";
        }
        if (data.Subtype == CardSubtype.NegativeBuff)
        {
            if (!data.AppliedDebuffType.HasValue) return "";
            string duration = data.Duration > 0 ? $" / 持续: {data.Duration}回合" : "";
            return $"减益: {data.AppliedDebuffType.Value} ({data.EffectAmount}){duration}";
        }
        return "";
    }
    
    public static string FormatConsumable(CardInstance card)
    {
        if (card.Data.Subtype == CardSubtype.BattleLevelConsumable)
        {
            return $"效果: Energy恢复 {card.Data.EffectAmount} / 本场剩余: {card.RemainingUses}次";
        }
        if (card.Data.Subtype == CardSubtype.GameLevelConsumable)
        {
            return $"效果: HP恢复 {card.Data.EffectAmount} / 全局剩余: {card.RemainingUses}次";
        }
        return "";
    }
    
    public static string FormatEquipment(CardData data)
    {
        if (data.Subtype != CardSubtype.Equipment) return "";
        if (!data.EquipSlot.HasValue) return "";
        string duration = data.Duration > 0 ? $" / 持续: {data.Duration}场" : "";
        return $"装备槽: {data.EquipSlot.Value} / 加成: +{data.EffectAmount}{duration}";
    }
    
    public static string FormatCurse(CardInstance card)
    {
        if (card.Data.Subtype != CardSubtype.Curse) return "";
        
        string duration = card.Data.CurseDuration == CurseDurationType.Temporary ? "临时" : "永久";
        string trigger = card.Data.CurseTrigger.ToString();
        int effectAmount = card.Data.CurseEffectAmount;
        float disappearChance = card.Data.CurseDisappearChance * 100;
        float strengthenChance = card.Data.CurseStrengthenChance * 100;
        int stacks = card.CurseStacks;
        
        return $"类型: {duration} / 触发: {trigger} ({effectAmount}/回合) / 层数: {stacks} / 概率: {disappearChance}%消失 {strengthenChance}%强化";
    }
    
    public static string FormatEffectSummary(CardData data, CardInstance card, int diceSides)
    {
        var parts = new System.Collections.Generic.List<string>();
        
        if (data.DamageFormula != null)
        {
            parts.Add(FormatDamageRange(data, diceSides));
        }
        
        if (data.Subtype == CardSubtype.Defense && data.ShieldValue > 0)
        {
            parts.Add(FormatShieldValue(data));
        }
        
        if (data.Subtype == CardSubtype.PositiveBuff || data.Subtype == CardSubtype.NegativeBuff)
        {
            parts.Add(FormatBuffDebuff(data));
        }
        
        if (data.Subtype == CardSubtype.BattleLevelConsumable || data.Subtype == CardSubtype.GameLevelConsumable)
        {
            parts.Add(FormatConsumable(card));
        }
        
        if (data.Subtype == CardSubtype.Equipment)
        {
            parts.Add(FormatEquipment(data));
        }
        
        if (data.Subtype == CardSubtype.Curse)
        {
            parts.Add(FormatCurse(card));
        }
        
        if (data.AppliedBuffType.HasValue && data.Subtype != CardSubtype.PositiveBuff)
        {
            parts.Add($"增益: {data.AppliedBuffType.Value} ({data.EffectAmount})");
        }
        
        if (data.AppliedDebuffType.HasValue && data.Subtype != CardSubtype.NegativeBuff)
        {
            parts.Add($"减益: {data.AppliedDebuffType.Value} ({data.EffectAmount})");
        }
        
        if (parts.Count == 0)
        {
            return "效果: 无";
        }
        
        return string.Join("\n", parts);
    }
    
    public static string FormatCostSummary(CardData data)
    {
        return $"{FormatEnergyCost(data)}  {FormatDiceCost(data)}";
    }
}