
// The Model. All property notify when their values change
using UniRx;

public class Enemy
{
    public ReactiveProperty<long> CurrentHp { get; private set; }

    public IReadOnlyReactiveProperty<bool> IsDead { get; private set; }

    public Enemy(int initialHp)
    {
        // Declarative Property
        CurrentHp = new ReactiveProperty<long>((long)initialHp);
        IsDead = CurrentHp.Select(x => x <= 0).ToReactiveProperty();
    }
}