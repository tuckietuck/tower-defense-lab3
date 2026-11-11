using Godot;

// Интерфейс только показывает состояние. Строительством управляет игрок.
public partial class GameHud : CanvasLayer
{
    private Label _status;
    private Label _help;
    private Label _message;
    private Label _result;
    private HealthComponent _playerHealth;
    private HealthComponent _coreHealth;
    private BuildComponent _build;
    private EnemySpawner _spawner;

    public override void _Ready()
    {
        _status = GetNode<Label>("Status");
        _help = GetNode<Label>("Help");
        _message = GetNode<Label>("Message");
        _result = GetNode<Label>("Result");
        Node level = GetParent();
        _playerHealth = level.GetNode<HealthComponent>("Player/HealthComponent");
        _coreHealth = level.GetNode<HealthComponent>("Core/HealthComponent");
        _build = level.GetNode<BuildComponent>("Player/BuildComponent");
        _spawner = level.GetNode<EnemySpawner>("EnemySpawner");
    }

    public override void _Process(double delta)
    {
        GameManager manager = GameManager.Instance;
        if (!manager.GameOver)
        {
            _status.Text = $"Металл: {manager.Metal}    Игрок: {_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}"
                + $"    Ядро: {_coreHealth.CurrentHealth}/{_coreHealth.MaxHealth}"
                + $"    Волна: {_spawner.Wave}    До волны: {Mathf.CeilToInt((float)_spawner.TimeToWave)} с";
            _help.Text = "WASD - движение | Мышь - прицел | ЛКМ/Пробел - огонь | R - заново\n"
                + "B - призрак / построить | 1, 2, 3 - выбор | Esc - отмена\n"
                + _build.GetSelectionText() + (_build.IsBuilding ? " | Выбери свободное место и нажми B" : "");
        }
        _message.Text = manager.Message;
        _result.Text = manager.Result;
    }
}
