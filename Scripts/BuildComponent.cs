using Godot;

// Призрак перед игроком показывает место и возможность постройки.
[GlobalClass]
public partial class BuildComponent : Node
{
    [Export] public Godot.Collections.Array<PackedScene> TowerScenes { get; set; } = new();
    [Export] public float BuildDistance { get; set; } = 3.0f;
    [Export] public float MapLimit { get; set; } = 18.0f;
    [Export] public float FreeRadius { get; set; } = 1.6f;
    public bool IsBuilding { get; private set; }
    public int SelectedType { get; private set; }
    public int SelectedCost { get; private set; }
    private Node3D _player;
    private Node3D _visual;
    private Node3D _ghost;
    private StandardMaterial3D _ghostMaterial;
    private readonly string[] _names = { "Обычная", "Взрывная", "Замораживающая" };

    public override void _Ready()
    {
        _player = GetParent<Node3D>();
        _visual = _player.GetNode<Node3D>("MuzzleVisual");
        SelectType(0);
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo || GameManager.Instance.GameOver)
            return;
        if (key.PhysicalKeycode == Key.Key1) SelectType(0);
        if (key.PhysicalKeycode == Key.Key2) SelectType(1);
        if (key.PhysicalKeycode == Key.Key3) SelectType(2);
        if (key.PhysicalKeycode == Key.Escape)
        {
            IsBuilding = false;
            _ghost.Visible = false;
        }
        if (key.PhysicalKeycode == Key.B)
        {
            if (!IsBuilding)
            {
                IsBuilding = true;
                _ghost.Visible = true;
            }
            else
                TryBuild();
        }
    }

    private void SelectType(int index)
    {
        SelectedType = index;
        // Экземпляр пока не в дереве: его скрипты и оружие не запускаются.
        Node3D sample = TowerScenes[index].Instantiate<Node3D>();
        SelectedCost = sample.GetNode<MetalComponent>("MetalComponent").BuildCost;
        _ghost?.QueueFree();
        _ghost = new Node3D();
        _ghostMaterial = new StandardMaterial3D();
        _ghostMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        CopyMeshes(sample, _ghost);
        sample.Free();
        GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, _ghost);
        _ghost.Visible = IsBuilding;
    }

    // Копируются только меши, поэтому у призрака нет коллизий и компонентов.
    private void CopyMeshes(Node source, Node3D destination)
    {
        foreach (Node child in source.GetChildren())
        {
            if (child is not Node3D spatial)
                continue;
            Node3D copy;
            if (child is MeshInstance3D mesh)
            {
                MeshInstance3D meshCopy = new MeshInstance3D();
                meshCopy.Mesh = mesh.Mesh;
                meshCopy.MaterialOverride = _ghostMaterial;
                copy = meshCopy;
            }
            else
                copy = new Node3D();
            destination.AddChild(copy);
            copy.Transform = spatial.Transform;
            CopyMeshes(child, copy);
        }
    }

    public override void _Process(double delta)
    {
        if (!IsBuilding || !_ghost.IsInsideTree())
            return;
        _ghost.GlobalPosition = GetBuildPosition();
        bool allowed = IsPlaceFree(_ghost.GlobalPosition) && GameManager.Instance.Metal >= SelectedCost;
        _ghostMaterial.AlbedoColor = allowed ? new Color(0.2f, 1, 0.4f, 0.45f)
            : new Color(1, 0.15f, 0.15f, 0.45f);
    }

    private Vector3 GetBuildPosition()
    {
        Vector3 direction = -_visual.GlobalBasis.Z;
        direction.Y = 0;
        Vector3 point = _player.GlobalPosition + direction.Normalized() * BuildDistance;
        point.Y = 0;
        return point;
    }

    private bool IsPlaceFree(Vector3 point)
    {
        if (Mathf.Abs(point.X) > MapLimit || Mathf.Abs(point.Z) > MapLimit)
            return false;
        SphereShape3D shape = new SphereShape3D();
        shape.Radius = FreeRadius;
        PhysicsShapeQueryParameters3D query = new PhysicsShapeQueryParameters3D();
        query.Shape = shape;
        query.Transform = new Transform3D(Basis.Identity, point + Vector3.Up);
        query.CollisionMask = 3;
        return _player.GetWorld3D().DirectSpaceState.IntersectShape(query).Count == 0;
    }

    private void TryBuild()
    {
        if (!IsPlaceFree(GetBuildPosition()))
        {
            GameManager.Instance.ShowMessage("Место занято или находится за границей поля.");
            return;
        }
        if (!GameManager.Instance.SpendMetal(SelectedCost))
            return;
        Node3D tower = TowerScenes[SelectedType].Instantiate<Node3D>();
        GetTree().CurrentScene.AddChild(tower);
        tower.GlobalPosition = GetBuildPosition();
        IsBuilding = false;
        _ghost.Visible = false;
        GameManager.Instance.ShowMessage("Башня построена.");
    }

    public string GetSelectionText()
    {
        return $"{SelectedType + 1}: {_names[SelectedType]} ({SelectedCost} металла)";
    }
}
