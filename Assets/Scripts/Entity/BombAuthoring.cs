using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct InTrigger : IBufferElementData
{
    public Entity value;
}

public struct Bomb : IComponentData
{
    public float lifeTime;
}

public class BombAuthoring : MonoBehaviour
{
    public float lifeTime;

    class BombBaker : Baker<BombAuthoring>
    {
        public override void Bake(BombAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bomb() { lifeTime = authoring.lifeTime });
            AddBuffer<InTrigger>(entity);
        }
    }
}
