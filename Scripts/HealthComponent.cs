using Godot;

// Хранит здоровье. О смерти сообщает менеджеру, а затем удаляет объект.
[GlobalClass]
public partial class HealthComponent : Node
{
    [Signal] public delegate void DiedEventHandler();
    [Export] public int MaxHealth { get; set; } = 10;
    public int CurrentHealth { get; private set; }
    private bool _dead;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (_dead || damage <= 0 || GameManager.Instance.GameOver)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        if (CurrentHealth == 0)
        {
            _dead = true;
            EmitSignal(SignalName.Died);
            GetParent().QueueFree();
        }
    }
}
