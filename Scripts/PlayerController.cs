using Godot;

// WASD двигает игрока, мышь задаёт направление оружия на плоскости поля.
[GlobalClass]
public partial class PlayerController : Node
{
    [Export] public float Speed { get; set; } = 6.0f;
    [Export] public float MapLimit { get; set; } = 18.5f;
    private CharacterBody3D _body;
    private Node3D _visual;

    public override void _Ready()
    {
        _body = GetParent<CharacterBody3D>();
        _visual = _body.GetNode<Node3D>("MuzzleVisual");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.GameOver)
            return;
        Vector3 direction = Vector3.Zero;
        if (Input.IsPhysicalKeyPressed(Key.W)) direction.Z -= 1;
        if (Input.IsPhysicalKeyPressed(Key.S)) direction.Z += 1;
        if (Input.IsPhysicalKeyPressed(Key.A)) direction.X -= 1;
        if (Input.IsPhysicalKeyPressed(Key.D)) direction.X += 1;
        _body.Velocity = direction.Normalized() * Speed;
        _body.MoveAndSlide();
        Vector3 position = _body.Position;
        position.X = Mathf.Clamp(position.X, -MapLimit, MapLimit);
        position.Z = Mathf.Clamp(position.Z, -MapLimit, MapLimit);
        _body.Position = position;

        Camera3D camera = GetViewport().GetCamera3D();
        Vector2 mouse = GetViewport().GetMousePosition();
        Vector3 rayStart = camera.ProjectRayOrigin(mouse);
        Vector3 rayDirection = camera.ProjectRayNormal(mouse);
        Plane plane = new Plane(Vector3.Up, _visual.GlobalPosition.Y);
        Vector3? point = plane.IntersectsRay(rayStart, rayDirection);
        if (point.HasValue && _visual.GlobalPosition.DistanceSquaredTo(point.Value) > 0.01f)
            _visual.LookAt(point.Value, Vector3.Up);
    }
}
