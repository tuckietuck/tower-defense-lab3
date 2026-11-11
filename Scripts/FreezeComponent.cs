using Godot;

// Наносит урон и временно уменьшает скорость MoveComponent цели.
[GlobalClass]
public partial class FreezeComponent : DamagingComponent
{
    [Export] public float SlowMultiplier { get; set; } = 0.35f;
    [Export] public float Duration { get; set; } = 3.0f;

    public override void ApplyDamage(Node3D target)
    {
        if (!StartSpecialHit(target))
            return;
        DealDamage(target);
        target.GetNodeOrNull<MoveComponent>("MoveComponent")?.ApplySlow(SlowMultiplier, Duration);
        DestroySource();
    }
}
