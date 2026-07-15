using Godot;
using System.Collections.Generic;

public class EnemyState
{
    public string Name;
    public int MaxHp;
    public int Hp;
    public int Shield;
    public EnemyIntent CurrentIntent;
    public Dictionary<StatusType, int> Statuses = new Dictionary<StatusType, int>();
    
    public EnemyState(string name, int maxHp)
    {
        Name = name;
        MaxHp = maxHp;
        Hp = maxHp;
        Shield = 0;
        CurrentIntent = new EnemyIntent();
    }
    
    public int GetVulnerableStacks()
    {
        return Statuses.TryGetValue(StatusType.Vulnerable, out int stacks) ? stacks : 0;
    }
    
    public void AddVulnerable(int stacks)
    {
        if (!Statuses.ContainsKey(StatusType.Vulnerable))
        {
            Statuses[StatusType.Vulnerable] = 0;
        }
        Statuses[StatusType.Vulnerable] += stacks;
    }
    
    public void ReduceVulnerable(int stacks)
    {
        if (Statuses.ContainsKey(StatusType.Vulnerable))
        {
            Statuses[StatusType.Vulnerable] = Mathf.Max(0, Statuses[StatusType.Vulnerable] - stacks);
            if (Statuses[StatusType.Vulnerable] == 0)
            {
                Statuses.Remove(StatusType.Vulnerable);
            }
        }
    }
    
    public int CalculateDamage(int baseDamage)
    {
        int vulnerable = GetVulnerableStacks();
        return baseDamage + vulnerable;
    }
    
    public int TakeDamage(int damage)
    {
        int finalDamage = CalculateDamage(damage);
        
        if (Shield > 0)
        {
            int shieldDamage = Mathf.Min(Shield, finalDamage);
            Shield -= shieldDamage;
            finalDamage -= shieldDamage;
        }
        
        Hp -= finalDamage;
        
        return finalDamage;
    }
    
    public void EndTurn()
    {
        int vulnerable = GetVulnerableStacks();
        if (vulnerable > 0)
        {
            ReduceVulnerable(1);
        }
    }
    
    public bool IsAlive()
    {
        return Hp > 0;
    }
}

public class EnemyIntent
{
    public enum IntentType
    {
        None,
        Attack,
        Defend,
        Buff,
        Debuff
    }
    
    public IntentType Type;
    public int Value;
    public string Description;
    
    public EnemyIntent()
    {
        Type = IntentType.None;
        Value = 0;
        Description = "不行动";
    }
    
    public void SetNone()
    {
        Type = IntentType.None;
        Value = 0;
        Description = "不行动";
    }
    
    public void SetAttack(int damage)
    {
        Type = IntentType.Attack;
        Value = damage;
        Description = $"攻击 {Value}";
    }
    
    public void SetDefend(int shield)
    {
        Type = IntentType.Defend;
        Value = shield;
        Description = $"防御 {shield}";
    }
    
    public void SetBuff(int value)
    {
        Type = IntentType.Buff;
        Value = value;
        Description = $"增益 {value}";
    }
    
    public void SetDebuff(int value)
    {
        Type = IntentType.Debuff;
        Value = value;
        Description = $"减益 {value}";
    }
}