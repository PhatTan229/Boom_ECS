using Unity.Entities;
using UnityEngine;

public struct Killable : IComponentData
{
    public void TakeDamge(RefRW<StatData> stat, float damge)
    {
        stat.ValueRW.currentStat.HP -= damge;
        Debug.Log($"{stat.ValueRO.currentStat.name} take {damge} damage, {stat.ValueRW.currentStat.HP} remain");
        //if (stat.ValueRO.currentStat.HP <= 0) Die();
    }

    //public void Die()
    //{
    //}
}

public class KillableAuthoring : MonoBehaviour
{
    class KillableAuthoringBaker : Baker<KillableAuthoring>
    {
        public override void Bake(KillableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Killable>(entity);
        }
    }
}
