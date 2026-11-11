using Godot;

// Общий урон для пули, контактной атаки и специальных эффектов.
[GlobalClass]
public partial class DamagingComponent : Area3D
{
    [Export] public int Damage { get; set; } = 2;
    [Export] public string TargetGroup { get; set; } = "Enemies";
    [Export] public bool OneShot { get; set; } = true;
    [Export] public float HitInterval { get; set; } = 1.0f;
    private double _cooldown;
    private bool _used;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (OneShot || GameManager.Instance.GameOver)
            return;
        _cooldown -= delta;
        if (_cooldown > 0)
            return;
        var bodies = GetOverlappingBodies();
        if (bodies.Count == 0)
            return;
        foreach (Node3D body in bodies)
            ApplyDamage(body);
        _cooldown = HitInterval;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (OneShot || _cooldown <= 0)
        {
            ApplyDamage(body);
            if (!OneShot)
                _cooldown = HitInterval;
        }
    }

    protected bool CanHit(Node3D target)
    {
        return GodotObject.IsInstanceValid(target) && !target.IsQueuedForDeletion()
            && target.IsInGroup(TargetGroup)
            && target.GetNodeOrNull<HealthComponent>("HealthComponent") != null;
    }

    public virtual void ApplyDamage(Node3D target)
    {
        if (!CanHit(target) || GameManager.Instance.GameOver || (OneShot && _used))
            return;
        DealDamage(target);
        if (OneShot)
            DestroySource();
    }

    // Потомки тоже используют этот метод: формула урона остаётся в одном месте.
    protected void DealDamage(Node3D target)
    {
        if (CanHit(target))
            target.GetNode<HealthComponent>("HealthComponent").TakeDamage(Damage);
    }

    protected void DestroySource()
    {
        _used = true;
        GetParent().QueueFree();
    }

    // Защита от повторного срабатывания в одном физическом кадре.
    protected bool StartSpecialHit(Node3D target)
    {
        if (_used || !CanHit(target) || GameManager.Instance.GameOver)
            return false;
        _used = true;
        return true;
    }
}
