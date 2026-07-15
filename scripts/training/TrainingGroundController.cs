using Godot;

public partial class TrainingGroundController : Node
{
    private BattleManager _battleManager;
    private BattleLogPanel _battleLogPanel;
    
    private LineEdit _playerHpInput;
    private LineEdit _playerEnergyInput;
    private LineEdit _diceCountInput;
    private LineEdit _diceSidesInput;
    private LineEdit _fixedRollInput;
    
    private Button _rollModeRandomBtn;
    private Button _rollModeFixedBtn;
    
    private Button _enemyDummyBtn;
    private Button _enemyBeastBtn;
    
    private LineEdit _enemyHpInput;
    private LineEdit _enemyAttackInput;
    private LineEdit _enemyShieldInput;
    private LineEdit _enemyIntentValueInput;
    
    private Button _enemyIntentAttackBtn;
    private Button _enemyIntentDefendBtn;
    private Button _enemyIntentBuffBtn;
    private Button _enemyIntentDebuffBtn;
    
    private Button _resetButton;
    
    private TrainingConfig _config = new TrainingConfig();
    
    public override void _Ready()
    {
        _battleManager = GetNode<BattleManager>("BattleManager");
        _battleLogPanel = GetNode<BattleLogPanel>("BattleLogPanel");
        
        _playerHpInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/PlayerHpRow/PlayerHpInput");
        _playerEnergyInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/PlayerEnergyRow/PlayerEnergyInput");
        _diceCountInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/DiceCountRow/DiceCountInput");
        _diceSidesInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/DiceSidesRow/DiceSidesInput");
        _fixedRollInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/FixedRollRow/FixedRollInput");
        
        _rollModeRandomBtn = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/RollModeRow/RollModeRandomBtn");
        _rollModeFixedBtn = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/RollModeRow/RollModeFixedBtn");
        
        _enemyDummyBtn = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyRow/EnemyDummyBtn");
        _enemyBeastBtn = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyRow/EnemyBeastBtn");
        
        _enemyHpInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyHpRow/EnemyHpInput");
        _enemyAttackInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyAttackRow/EnemyAttackInput");
        _enemyShieldInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyShieldRow/EnemyShieldInput");
        _enemyIntentValueInput = GetNode<LineEdit>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyIntentValueRow/EnemyIntentValueInput");
        
        _enemyIntentAttackBtn = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyIntentRow1/EnemyIntentAttackBtn");
        _enemyIntentDefendBtn = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyIntentRow1/EnemyIntentDefendBtn");
        _enemyIntentBuffBtn = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyIntentRow2/EnemyIntentBuffBtn");
        _enemyIntentDebuffBtn = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/EnemyIntentRow2/EnemyIntentDebuffBtn");
        
        _resetButton = GetNode<Button>("ConfigPanel/ConfigScroll/ConfigContainer/ResetButton");
        
        _playerHpInput.Text = _config.PlayerHp.ToString();
        _playerEnergyInput.Text = _config.PlayerEnergy.ToString();
        _diceCountInput.Text = _config.DiceCount.ToString();
        _diceSidesInput.Text = _config.DiceSides.ToString();
        _fixedRollInput.Text = _config.FixedRollValue.ToString();
        
        _enemyHpInput.Text = _config.EnemyHp.ToString();
        _enemyAttackInput.Text = _config.EnemyAttack.ToString();
        _enemyShieldInput.Text = _config.EnemyShield.ToString();
        _enemyIntentValueInput.Text = _config.EnemyIntentValue.ToString();
        
        UpdateRollModeButtons();
        UpdateEnemyButtons();
        UpdateEnemyIntentButtons();
        UpdateEnemyControlsVisibility();
        
        _rollModeRandomBtn.Pressed += () => SetRollMode(RollMode.Random);
        _rollModeFixedBtn.Pressed += () => SetRollMode(RollMode.Fixed);
        
        _enemyDummyBtn.Pressed += () => SetEnemyType(EnemyType.TrainingDummy);
        _enemyBeastBtn.Pressed += () => SetEnemyType(EnemyType.TrainingBeast);
        
        _enemyIntentAttackBtn.Pressed += () => SetEnemyIntent(EnemyIntentType.Attack);
        _enemyIntentDefendBtn.Pressed += () => SetEnemyIntent(EnemyIntentType.Defend);
        _enemyIntentBuffBtn.Pressed += () => SetEnemyIntent(EnemyIntentType.Buff);
        _enemyIntentDebuffBtn.Pressed += () => SetEnemyIntent(EnemyIntentType.Debuff);
        
        _resetButton.Pressed += OnResetBattle;
        
        _battleManager.BattleLog += _battleLogPanel.AddLog;
        
        ResetBattle();
    }
    
    private void SetRollMode(RollMode mode)
    {
        _config.RollMode = mode;
        UpdateRollModeButtons();
    }
    
    private void SetEnemyType(EnemyType type)
    {
        _config.EnemyType = type;
        
        if (type == EnemyType.TrainingBeast && _config.EnemyIntent == EnemyIntentType.None)
        {
            _config.EnemyIntent = EnemyIntentType.Attack;
        }
        
        UpdateEnemyButtons();
        UpdateEnemyControlsVisibility();
    }
    
    private void UpdateEnemyControlsVisibility()
    {
        bool isDummy = _config.EnemyType == EnemyType.TrainingDummy;
        
        _enemyAttackInput.Editable = !isDummy;
        _enemyAttackInput.Modulate = isDummy ? Colors.Gray : Colors.White;
        
        _enemyShieldInput.Editable = !isDummy;
        _enemyShieldInput.Modulate = isDummy ? Colors.Gray : Colors.White;
        
        if (isDummy)
        {
            _config.EnemyIntent = EnemyIntentType.None;
        }
        
        UpdateEnemyIntentButtons();
    }
    
    private void SetEnemyIntent(EnemyIntentType intent)
    {
        _config.EnemyIntent = intent;
        UpdateEnemyIntentButtons();
    }
    
    private void UpdateRollModeButtons()
    {
        _rollModeRandomBtn.ButtonPressed = _config.RollMode == RollMode.Random;
        _rollModeFixedBtn.ButtonPressed = _config.RollMode == RollMode.Fixed;
        _fixedRollInput.Editable = _config.RollMode == RollMode.Fixed;
        _fixedRollInput.Modulate = _config.RollMode == RollMode.Fixed ? Colors.White : Colors.Gray;
    }
    
    private void UpdateEnemyButtons()
    {
        _enemyDummyBtn.ButtonPressed = _config.EnemyType == EnemyType.TrainingDummy;
        _enemyBeastBtn.ButtonPressed = _config.EnemyType == EnemyType.TrainingBeast;
    }
    
    private void UpdateEnemyIntentButtons()
    {
        _enemyIntentAttackBtn.ButtonPressed = _config.EnemyIntent == EnemyIntentType.Attack;
        _enemyIntentDefendBtn.ButtonPressed = _config.EnemyIntent == EnemyIntentType.Defend;
        _enemyIntentBuffBtn.ButtonPressed = _config.EnemyIntent == EnemyIntentType.Buff;
        _enemyIntentDebuffBtn.ButtonPressed = _config.EnemyIntent == EnemyIntentType.Debuff;
        
        bool hasIntent = _config.EnemyIntent != EnemyIntentType.None;
        _enemyIntentAttackBtn.Visible = hasIntent;
        _enemyIntentDefendBtn.Visible = hasIntent;
        _enemyIntentBuffBtn.Visible = hasIntent;
        _enemyIntentDebuffBtn.Visible = hasIntent;
        _enemyIntentValueInput.Visible = hasIntent;
    }
    
    private void OnResetBattle()
    {
        ResetBattle();
    }
    
    public void ResetBattle()
    {
        _battleLogPanel.Clear();
        
        ParseConfig();
        
        _battleLogPanel.AddLog("=== 训练场已重置 ===");
        _battleLogPanel.AddLog($"玩家: {_config.PlayerHp} HP, {_config.PlayerEnergy} Energy");
        _battleLogPanel.AddLog($"骰子: {_config.DiceCount}d{_config.DiceSides}");
        _battleLogPanel.AddLog($"掷骰模式: {_config.RollMode}");
        if (_config.RollMode == RollMode.Fixed)
        {
            _battleLogPanel.AddLog($"固定骰点: {_config.FixedRollValue}");
        }
        _battleLogPanel.AddLog($"敌人: {_config.EnemyType}");
        _battleLogPanel.AddLog($"敌人 HP: {_config.EnemyHp}");
        _battleLogPanel.AddLog($"敌人攻击: {_config.EnemyAttack}");
        _battleLogPanel.AddLog($"敌人护盾: {_config.EnemyShield}");
        _battleLogPanel.AddLog($"敌人意图: {_config.EnemyIntent} ({_config.EnemyIntentValue})");
        
        var diceRoller = new DiceRoller
        {
            Mode = _config.RollMode,
            FixedValue = _config.FixedRollValue
        };

        var player = new PlayerState(diceRoller)
        {
            MaxHp = _config.PlayerHp,
            Hp = _config.PlayerHp,
            MaxEnergy = _config.PlayerEnergy,
            Energy = _config.PlayerEnergy,
            DiceCount = _config.DiceCount,
            DiceSides = _config.DiceSides
        };

        var enemy = TrainingEnemyFactory.CreateEnemy(_config.EnemyType, _config);
        _battleManager.InitializeBattle(player, enemy, diceRoller);
    }
    
    private void ParseConfig()
    {
        if (int.TryParse(_playerHpInput.Text, out int hp))
            _config.PlayerHp = hp;
        if (int.TryParse(_playerEnergyInput.Text, out int energy))
            _config.PlayerEnergy = energy;
        if (int.TryParse(_diceCountInput.Text, out int diceCount))
            _config.DiceCount = diceCount;
        if (int.TryParse(_diceSidesInput.Text, out int diceSides))
            _config.DiceSides = diceSides;
        if (int.TryParse(_fixedRollInput.Text, out int fixedRoll))
            _config.FixedRollValue = fixedRoll;
        
        if (int.TryParse(_enemyHpInput.Text, out int enemyHp))
            _config.EnemyHp = enemyHp;
        if (int.TryParse(_enemyAttackInput.Text, out int enemyAttack))
            _config.EnemyAttack = enemyAttack;
        if (int.TryParse(_enemyShieldInput.Text, out int enemyShield))
            _config.EnemyShield = enemyShield;
        if (int.TryParse(_enemyIntentValueInput.Text, out int intentValue))
            _config.EnemyIntentValue = intentValue;
        
        _config.ClampValues();
        
        _playerHpInput.Text = _config.PlayerHp.ToString();
        _playerEnergyInput.Text = _config.PlayerEnergy.ToString();
        _diceCountInput.Text = _config.DiceCount.ToString();
        _diceSidesInput.Text = _config.DiceSides.ToString();
        _fixedRollInput.Text = _config.FixedRollValue.ToString();
        
        _enemyHpInput.Text = _config.EnemyHp.ToString();
        _enemyAttackInput.Text = _config.EnemyAttack.ToString();
        _enemyShieldInput.Text = _config.EnemyShield.ToString();
        _enemyIntentValueInput.Text = _config.EnemyIntentValue.ToString();
    }
}