using Godot;
using System.Collections.Generic;

// Та же идея, что в практике: Area3D запоминает цели в радиусе.
[GlobalClass]
public partial class VisibilityComponent : Area3D
{
    [Export] public string TargetGroup { get; set; } = "Enemies";
    private readonly HashSet<Node3D> _targets = new();

    public override void _Ready()
    {
        BodyEntered += body => { if (body.IsInGroup(TargetGroup)) _targets.Add(body); };
        BodyExited += body => _targets.Remove(body);
    }

    public Node3D GetClosestTarget()
    {
        Node3D closest = null;
        float bestDistance = float.PositiveInfinity;
        foreach (Node3D target in _targets)
        {
            if (!GodotObject.IsInstanceValid(target) || target.IsQueuedForDeletion())
                continue;
            float distance = GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = target;
            }
        }
        return closest;
    }
}
