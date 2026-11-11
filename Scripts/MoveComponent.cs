using Godot;

// Движение врага к ближайшей цели, а если её нет - к ядру.
[GlobalClass]
public partial class MoveComponent : Node
{
    [Export] public float Speed { get; set; } = 2.5f;
    [Export] public float StopDistance { get; set; } = 1.3f;
    [Export] public Color FrozenColor { get; set; } = new Color(0.35f, 0.8f, 1);
    private CharacterBody3D _body;
    private VisibilityComponent _visibility;
    private Node3D _visual;
    private double _slowTime;
    private float _speedMultiplier = 1;
    private StandardMaterial3D _material;
    private Color _normalColor;

    public override void _Ready()
    {
        _body = GetParent<CharacterBody3D>();
        _visibility = _body.GetNode<VisibilityComponent>("VisibilityComponent");
        _visual = _body.GetNode<Node3D>("MuzzleVisual");
        MeshInstance3D mesh = _body.GetNode<MeshInstance3D>("TowerBaseVisual");
        _material = (StandardMaterial3D)mesh.MaterialOverride.Duplicate();
        mesh.MaterialOverride = _material;
        _normalColor = _material.AlbedoColor;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        _speedMultiplier = Mathf.Clamp(multiplier, 0, 1);
        _slowTime = duration;
        _material.AlbedoColor = FrozenColor;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.GameOver)
            return;
        if (_slowTime > 0)
        {
            _slowTime -= delta;
            if (_slowTime <= 0)
            {
                _speedMultiplier = 1;
                _material.AlbedoColor = _normalColor;
            }
        }
        Node3D target = _visibility.GetClosestTarget();
        if (target == null)
            target = GameManager.Instance.Core;
        if (!GodotObject.IsInstanceValid(target))
            return;
        Vector3 offset = target.GlobalPosition - _body.GlobalPosition;
        offset.Y = 0;
        if (offset.Length() > 0.01f)
            _visual.LookAt(_visual.GlobalPosition + offset, Vector3.Up);
        _body.Velocity = offset.Length() > StopDistance
            ? offset.Normalized() * Speed * _speedMultiplier : Vector3.Zero;
        _body.MoveAndSlide();
    }
}
