using Godot;

// Удаляет временный эффект, например сферу взрыва.
[GlobalClass]
public partial class LifetimeComponent : Node
{
    [Export] public float Lifetime { get; set; } = 0.35f;
    private double _time;

    public override void _Process(double delta)
    {
        _time += delta;
        if (_time >= Lifetime)
            GetParent().QueueFree();
    }
}
