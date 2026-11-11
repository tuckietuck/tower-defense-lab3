using Godot;

// При попадании создаёт отдельную область урона большого радиуса.
[GlobalClass]
public partial class ExplosionComponent : DamagingComponent
{
    [Export] public PackedScene ExplosionScene { get; set; }
    [Export] public float Radius { get; set; } = 3.5f;

    public override void ApplyDamage(Node3D target)
    {
        if (!StartSpecialHit(target))
            return;
        Node3D explosion = ExplosionScene.Instantiate<Node3D>();
        DamagingComponent area = explosion.GetNode<DamagingComponent>("DamagingComponent");
        area.Damage = Damage;
        area.TargetGroup = TargetGroup;
        area.CollisionMask = CollisionMask;
        SphereShape3D shape = new SphereShape3D();
        shape.Radius = Radius;
        area.GetNode<CollisionShape3D>("CollisionShape3D").Shape = shape;
        explosion.GetNode<MeshInstance3D>("Visual").Scale = Vector3.One * Radius;
        GetTree().CurrentScene.AddChild(explosion);
        explosion.GlobalPosition = GlobalPosition;
        // Первая цель получает урон сразу; остальные - при входе в область.
        area.ApplyDamage(target);
        DestroySource();
    }
}
