using Godot;

// Пуля отвечает только за полёт. Урон находится в дочернем компоненте.
public partial class Bullet : Node3D
{
    [Export] public float Speed { get; set; } = 17.0f;
    [Export] public float Lifetime { get; set; } = 3.0f;
    private Vector3 _direction;
    private double _time;

    public void Initialize(Vector3 direction, string targetGroup)
    {
        _direction = direction.Normalized();
        DamagingComponent damage = GetNode<DamagingComponent>("DamagingComponent");
        damage.TargetGroup = targetGroup;
        damage.CollisionMask = targetGroup == "Enemies" ? 2u : 1u;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.GameOver)
            return;
        GlobalPosition += _direction * Speed * (float)delta;
        _time += delta;
        if (_time >= Lifetime)
            QueueFree();
    }
}
