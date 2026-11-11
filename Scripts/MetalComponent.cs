using Godot;

// Цена хранится в сцене башни; награда - в сцене башни или врага.
[GlobalClass]
public partial class MetalComponent : Node
{
    [Export] public int BuildCost { get; set; } = 30;
    [Export] public int Reward { get; set; } = 8;

    public override void _Ready()
    {
        GameManager.Instance.RegisterObject(GetParent<Node3D>(), Reward);
    }
}
