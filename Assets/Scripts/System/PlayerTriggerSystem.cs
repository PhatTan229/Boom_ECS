using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
public partial struct PlayerTriggerSystem : ISystem, IOnTrigger
{
    [BurstCompile]
    struct PlayerTriggerEventJob : ITriggerEventsJob
    {
        public NativeHashSet<PhysicEntityPair> EnterTrigger;
        public NativeHashSet<PhysicEntityPair> CurrentTrigger;
        public NativeHashSet<PhysicEntityPair> PreviousTrigger;
        [ReadOnly] public ComponentLookup<Player> lookup;

        public void Execute(TriggerEvent triggerEvent)
        {
            if (!lookup.HasComponent(triggerEvent.EntityA) && !lookup.HasComponent(triggerEvent.EntityB)) return;

            var pair = new PhysicEntityPair(triggerEvent);

            if (!PreviousTrigger.Contains(pair))
            {
                EnterTrigger.Add(pair);
            }

            CurrentTrigger.Add(pair);
        }
    }

    public NativeHashSet<PhysicEntityPair> _enter;
    public NativeHashSet<PhysicEntityPair> _current;
    public NativeHashSet<PhysicEntityPair> _previous;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _enter = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent);
        _current = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent);
        _previous = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _current.Clear();

        var job = new PlayerTriggerEventJob()
        {
            EnterTrigger = _enter,
            CurrentTrigger = _current,
            PreviousTrigger = _previous,
            lookup = SystemAPI.GetComponentLookup<Player>(),
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        state.Dependency.Complete();

        foreach (var pair in _enter) 
        {
            OnEnter(ref state, pair);
        }

        foreach (var pair in _current)
        {
            OnStay(ref state, pair);
        }

        foreach (var pair in _previous)
        {
            if (!_current.Contains(pair)) OnExit(ref state, pair);
        }

        _enter.Clear();
        _previous.Clear();
        _previous.UnionWith(_current);
    }

    public void OnEnter(ref SystemState state, PhysicEntityPair entityPair)
    {
        Debug.Log("Enter");

        if (SystemAPI.HasComponent<Enemy>(entityPair.EntityA) || SystemAPI.HasComponent<Enemy>(entityPair.EntityB))
        {
            var refPlayer = SystemAPI.HasComponent<Player>(entityPair.EntityA) ? 
                SystemAPI.GetComponentRW<Player>(entityPair.EntityA) : SystemAPI.GetComponentRW<Player>(entityPair.EntityB);
            refPlayer.ValueRW.TakeDamge(1f);
        }
    }

    public void OnStay(ref SystemState state, PhysicEntityPair entityPair)
    {
        Debug.Log("Stay");
    }

    public void OnExit(ref SystemState state, PhysicEntityPair entityPair)
    {
        Debug.Log("Exit");
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        _current.Dispose();
        _previous.Dispose();
    }
}
