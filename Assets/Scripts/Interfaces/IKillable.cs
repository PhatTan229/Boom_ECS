using Unity.Entities;

public interface IKillable
{
    public void TakeDamge(RefRW<StatData> stat, float damge);
    public void Die();
}
