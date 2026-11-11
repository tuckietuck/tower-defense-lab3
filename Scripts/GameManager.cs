using Godot;
using System.Collections.Generic;

// Единственный менеджер находится на корневом узле Level, не в Autoload.
public partial class GameManager : Node3D
{
    public static GameManager Instance { get; private set; }
    [Export] public int StartMetal { get; set; } = 100;
    [Export] public float MessageDuration { get; set; } = 2.5f;
    public int Metal { get; private set; }
    public bool GameOver { get; private set; }
    public Node3D Core { get; private set; }
    public string Result { get; private set; } = "";
    public string Message { get; private set; } = "";
    private double _messageTime;
    private readonly Dictionary<Node3D, int> _objects = new();

    // _EnterTree вызывается раньше _Ready дочерних компонентов.
    public override void _EnterTree()
    {
        Instance = this;
        Metal = StartMetal;
    }

    public override void _Ready()
    {
        Core = GetNode<Node3D>("Core");
        Core.GetNode<HealthComponent>("HealthComponent").Died += () => EndGame("Ядро уничтожено");
        GetNode<HealthComponent>("Player/HealthComponent").Died += () => EndGame("Игрок погиб");
        ShowMessage("Защищай ядро! B - строительство, 1/2/3 - тип башни.");
    }

    // Учитываем удаление со сцены, но не выдаём награду при завершении игры.
    public void RegisterObject(Node3D body, int reward)
    {
        _objects[body] = reward;
        body.TreeExiting += () =>
        {
            if (_objects.Remove(body, out int value) && !GameOver && !IsQueuedForDeletion())
                Metal += value;
        };
    }

    public bool SpendMetal(int amount)
    {
        if (Metal < amount)
        {
            ShowMessage("Не хватает металла!");
            return false;
        }
        Metal -= amount;
        return true;
    }

    public void ShowMessage(string message)
    {
        Message = message;
        _messageTime = MessageDuration;
    }

    public void EndGame(string reason)
    {
        if (GameOver)
            return;
        GameOver = true;
        Result = reason + ". Нажми R, чтобы начать заново.";
    }

    public override void _Process(double delta)
    {
        _messageTime -= delta;
        if (_messageTime <= 0)
            Message = "";
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.PhysicalKeycode == Key.R)
        {
            GameOver = true;
            GetTree().ReloadCurrentScene();
        }
    }
}
