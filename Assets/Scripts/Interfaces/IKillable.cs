public interface IKillable
{
    public float MaxHp {  get; set; }
    public float Hp { get; set; }
    public void TakeDamge(float damge);
    public void Die();
}
