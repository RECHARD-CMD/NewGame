using Godot;
using System.Collections.Generic;

public partial class BattleManager : Node
{
    public PlayerState Player;
    public EnemyState Enemy;
    public int Turn = 1;
    public bool IsPlayerTurn = true;
    public DiceRoller DiceRoller;
    
    public override void _Ready()
    {
        DiceRoller = new DiceRoller();
        if (Player == null || Enemy == null)
        {
            InitializeBattle();
        }
    }
    
    public void InitializeBattle()
    {
        var player = new PlayerState(DiceRoller);
        var enemy = new EnemyState("TrainingBeast", 20);
        enemy.CurrentIntent.SetAttack(14);
        
        InitializeBattle(player, enemy, DiceRoller);
    }
    
    public void InitializeBattle(PlayerState player, EnemyState enemy, DiceRoller diceRoller = null)
    {
        DiceRoller = diceRoller ?? DiceRoller ?? new DiceRoller();
        Player = player;
        Enemy = enemy;
        Turn = 1;
        IsPlayerTurn = true;
        Player.DiceRoller = DiceRoller;
        Player.Hand.Clear();
        Player.DicePool.Clear();

        DrawInitialHand();
        StartPlayerTurn();
    }
    
    private void DrawInitialHand()
    {
        Player.Hand.Add(new CardInstance(CardData.EnergyStrike));
        Player.Hand.Add(new CardInstance(CardData.QuickStrike));
        Player.Hand.Add(new CardInstance(CardData.VulnerableStrike));
    }
    
    public void StartPlayerTurn()
    {
        IsPlayerTurn = true;
        
        int energyBefore = Player.Energy;
        Player.RestoreEnergy(Player.MaxEnergy);
        
        Player.RefreshDicePool();
        
        var cardPool = new List<CardData> { CardData.EnergyStrike, CardData.BreakCore, CardData.QuickStrike, CardData.VulnerableStrike, CardData.CriticalHit };
        
        while (Player.Hand.Count < 3)
        {
            int index = (Turn + Player.Hand.Count) % cardPool.Count;
            Player.Hand.Add(new CardInstance(cardPool[index]));
        }
        
        EmitSignal(SignalName.PlayerTurnStarted, Turn);
        EmitSignal(SignalName.BattleLog, $"回合 {Turn} 开始");
        EmitSignal(SignalName.BattleLog, $"恢复 Energy: {energyBefore} → {Player.Energy}");
        EmitSignal(SignalName.BattleLog, $"获得骰池: {Player.DiceCount}d{Player.DiceSides}");
    }
    
    public bool TryPlayCard(CardInstance card)
    {
        if (!IsPlayerTurn)
            return false;
        
        if (!Player.CanPlayCard(card))
            return false;
        
        Player.ConsumeEnergy(card.Data.EnergyCost);
        
        DiceInstance consumedDice = null;
        if (card.Data.DiceCost > 0)
        {
            consumedDice = Player.ConsumeNextDice();
            if (consumedDice == null)
                return false;
        }
        
        Player.PlayCard(card);
        
        int baseDamage = card.CalculateDamage(consumedDice);
        
        if (card.Data.ModifyDamage != null && consumedDice != null)
        {
            baseDamage = card.Data.ModifyDamage(card, consumedDice, baseDamage);
        }
        
        int finalDamage = Enemy.TakeDamage(baseDamage);
        int vulnerableBeforeEffect = Enemy.GetVulnerableStacks();
        
        if (card.Data.ApplyEffect != null)
        {
            card.Data.ApplyEffect(card, consumedDice, Enemy);
        }
        
        int vulnerableAdded = Mathf.Max(0, Enemy.GetVulnerableStacks() - vulnerableBeforeEffect);
        
        int diceResult = consumedDice?.Value ?? 0;
        EmitSignal(SignalName.CardPlayed, card.Data.Id, finalDamage, diceResult, vulnerableAdded);
        
        EmitSignal(SignalName.BattleLog, $"打出 {card.Data.Name}");
        EmitSignal(SignalName.BattleLog, $"消耗 Energy: {card.Data.EnergyCost}");
        if (card.Data.DiceCost > 0)
        {
            EmitSignal(SignalName.BattleLog, $"掷骰结果: {diceResult}");
        }
        EmitSignal(SignalName.BattleLog, $"造成伤害: {finalDamage}");
        
        if (vulnerableAdded > 0)
        {
            EmitSignal(SignalName.BattleLog, $"施加破甲: {vulnerableAdded} 层");
        }
        
        if (!Enemy.IsAlive())
        {
            EmitSignal(SignalName.BattleWon);
            EmitSignal(SignalName.BattleLog, "胜利!");
            return true;
        }
        
        return true;
    }
    
    public void EndPlayerTurn()
    {
        IsPlayerTurn = false;
        EmitSignal(SignalName.PlayerTurnEnded);
        
        CallDeferred(nameof(ExecuteEnemyTurn));
    }
    
    public void ExecuteEnemyTurn()
    {
        if (!Enemy.IsAlive())
            return;
        
        switch (Enemy.CurrentIntent.Type)
        {
            case EnemyIntent.IntentType.Attack:
                ExecuteEnemyAttack();
                if (!Player.IsAlive())
                    return;
                break;
            case EnemyIntent.IntentType.Defend:
                Enemy.Shield += Enemy.CurrentIntent.Value;
                EmitSignal(SignalName.BattleLog, $"{Enemy.Name} 获得护盾: {Enemy.CurrentIntent.Value}");
                break;
            case EnemyIntent.IntentType.Buff:
                EmitSignal(SignalName.BattleLog, $"{Enemy.Name} 使用增益: {Enemy.CurrentIntent.Value}（暂未实现具体效果）");
                break;
            case EnemyIntent.IntentType.Debuff:
                EmitSignal(SignalName.BattleLog, $"{Enemy.Name} 使用减益: {Enemy.CurrentIntent.Value}（暂未实现具体效果）");
                break;
        }
        
        int vulnerableBefore = Enemy.GetVulnerableStacks();
        Enemy.EndTurn();
        int vulnerableAfter = Enemy.GetVulnerableStacks();
        
        if (vulnerableBefore > vulnerableAfter)
        {
            EmitSignal(SignalName.BattleLog, $"破甲减少: {vulnerableBefore} → {vulnerableAfter}");
        }
        
        Turn++;
        StartPlayerTurn();
    }
    
    private void ExecuteEnemyAttack()
    {
        int damage = Enemy.CurrentIntent.Value;
        if (damage <= 0)
        {
            EmitSignal(SignalName.BattleLog, $"{Enemy.Name} 不行动");
            return;
        }
        
        int energyBefore = Player.Energy;
        int hpBefore = Player.Hp;
        Player.TakeDamage(damage);
        
        EmitSignal(SignalName.EnemyAttacked, damage, energyBefore, Player.Energy, hpBefore, Player.Hp);
        EmitSignal(SignalName.BattleLog, $"{Enemy.Name} 攻击: {damage}");
        EmitSignal(SignalName.BattleLog, $"Energy: {energyBefore} → {Player.Energy}");
        EmitSignal(SignalName.BattleLog, $"HP: {hpBefore} → {Player.Hp}");
        
        if (!Player.IsAlive())
        {
            EmitSignal(SignalName.BattleLost);
            EmitSignal(SignalName.BattleLog, "失败!");
        }
    }
    
    public void SkipTurn()
    {
        if (IsPlayerTurn)
        {
            EndPlayerTurn();
        }
    }
    
    [Signal] public delegate void PlayerTurnStartedEventHandler(int turn);
    [Signal] public delegate void PlayerTurnEndedEventHandler();
    [Signal] public delegate void CardPlayedEventHandler(string cardId, int damage, int diceResult, int vulnerableAdded);
    [Signal] public delegate void EnemyAttackedEventHandler(int damage, int energyBefore, int energyAfter, int hpBefore, int hpAfter);
    [Signal] public delegate void BattleWonEventHandler();
    [Signal] public delegate void BattleLostEventHandler();
    [Signal] public delegate void BattleLogEventHandler(string message);
}
