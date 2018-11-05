public interface IDamageReceiver
{
    bool OnDamage(Target target);
}

public enum Target
{
    PLAYER,
    ENEMY
}
