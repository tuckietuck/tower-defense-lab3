using Godot;

// Бесконечные волны: число врагов понемногу увеличивается.
[GlobalClass]
public partial class EnemySpawner : Node
{
    [Export] public PackedScene CrawlerScene { get; set; }
    [Export] public PackedScene ShooterScene { get; set; }
    [Export] public float FirstWaveDelay { get; set; } = 12;
    [Export] public float WaveInterval { get; set; } = 15;
    [Export] public float SpawnEdge { get; set; } = 18;
    [Export] public int StartEnemyCount { get; set; } = 3;
    [Export] public int MaxEnemiesPerWave { get; set; } = 12;
    public int Wave { get; private set; }
    public double TimeToWave { get; private set; }
    private readonly RandomNumberGenerator _random = new();

    public override void _Ready()
    {
        _random.Randomize();
        TimeToWave = FirstWaveDelay;
    }

    public override void _Process(double delta)
    {
        if (GameManager.Instance.GameOver)
            return;
        TimeToWave -= delta;
        if (TimeToWave > 0)
            return;
        Wave++;
        int count = Mathf.Min(StartEnemyCount + Wave - 1, MaxEnemiesPerWave);
        for (int i = 0; i < count; i++)
        {
            PackedScene scene = i % 3 == 2 ? ShooterScene : CrawlerScene;
            Node3D enemy = scene.Instantiate<Node3D>();
            Vector3 position = new Vector3(_random.RandfRange(-SpawnEdge, SpawnEdge), 0, -SpawnEdge);
            position = position.Rotated(Vector3.Up, _random.RandiRange(0, 3) * Mathf.Pi / 2);
            GetTree().CurrentScene.AddChild(enemy);
            enemy.GlobalPosition = position;
        }
        GameManager.Instance.ShowMessage($"Волна {Wave}");
        TimeToWave = WaveInterval;
    }
}
