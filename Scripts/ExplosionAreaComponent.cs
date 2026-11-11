using Godot;
using System.Collections.Generic;

// Область взрыва поражает каждую цель только один раз.
[GlobalClass]
public partial class ExplosionAreaComponent : DamagingComponent
{
    private readonly HashSet<Node3D> _damaged = new();

    public override void ApplyDamage(Node3D target)
    {
        if (!CanHit(target) || _damaged.Contains(target) || GameManager.Instance.GameOver)
            return;
        _damaged.Add(target);
        DealDamage(target);
    }
}
