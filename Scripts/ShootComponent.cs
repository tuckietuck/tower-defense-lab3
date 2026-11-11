using Godot;

// Один компонент используется башнями, стрелком и игроком.
[GlobalClass]
public partial class ShootComponent : Node
{
    [Export] public PackedScene BulletScene { get; set; }
    [Export] public float ShotInterval { get; set; } = 0.65f;
    [Export] public bool PlayerControlled { get; set; }
    [Export] public string TargetGroup { get; set; } = "Enemies";
    [Export] public float SpawnOffset { get; set; } = 0.15f;
    private Node3D _visual;
    private Marker3D _muzzle;
    private VisibilityComponent _visibility;
    private double _cooldown;

    public override void _Ready()
    {
        _visual = GetParent().GetNode<Node3D>("MuzzleVisual");
        _muzzle = _visual.GetNode<Marker3D>("MuzzleMarker");
        _visibility = GetParent().GetNodeOrNull<VisibilityComponent>("VisibilityComponent");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.GameOver)
            return;
        _cooldown -= delta;
        if (PlayerControlled)
        {
            if (GetParent().GetNode<BuildComponent>("BuildComponent").IsBuilding)
                return;
            if (!Input.IsMouseButtonPressed(MouseButton.Left) && !Input.IsPhysicalKeyPressed(Key.Space))
                return;
        }
        else
        {
            Node3D target = _visibility.GetClosestTarget();
            if (target == null)
                return;
            Vector3 point = target.GlobalPosition;
            point.Y = _visual.GlobalPosition.Y;
            if (_visual.GlobalPosition.DistanceSquaredTo(point) > 0.001f)
                _visual.LookAt(point, Vector3.Up);
        }
        if (_cooldown > 0)
            return;
        Vector3 direction = -_muzzle.GlobalBasis.Z;
        Bullet bullet = BulletScene.Instantiate<Bullet>();
        bullet.Initialize(direction, TargetGroup);
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = _muzzle.GlobalPosition + direction * SpawnOffset;
        _cooldown = ShotInterval;
    }
}
