using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ParticleData : IComponentData
{
    public float lifeTime { get; private set; }
    public float currentLifeTime;
    public float3 position;

    public ParticleData(float lifeTime)
    {
        this.lifeTime = lifeTime + 0.2f;
        currentLifeTime = lifeTime;
        position = new float3(1f, 1f, 1f) * -99f;
    }

    public void ResetLifeTime(float3 position)
    {
        currentLifeTime = lifeTime;
        this.position = position;
    }
}

public class ParticleSystemRef : IComponentData
{
    public ParticleSystem particleSystem;

    public ParticleSystemRef() { }

    public ParticleSystemRef(ParticleSystem particleSystem)
    {
        this.particleSystem = particleSystem;
    }
}

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemEntityAuthoring : MonoBehaviour
{
    class ParticleSystemEntityBaker : Baker<ParticleSystemEntityAuthoring>
    {
        public override void Bake(ParticleSystemEntityAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var particle = authoring.GetComponent<ParticleSystem>();
            AddComponentObject(entity, new ParticleSystemRef(particle));
            var lifeTime = 0f;
            foreach (var item in particle.GetComponentsInChildren<ParticleSystem>())
            {
                lifeTime += item.main.duration;
            }
            AddComponent(entity, new ParticleData(lifeTime));
        }
    }
}
