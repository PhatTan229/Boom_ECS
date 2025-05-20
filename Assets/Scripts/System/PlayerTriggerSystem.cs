using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
public partial struct PlayerTriggerSystem : ISystem, ISystemStartStop, IOnTrigger
{
    struct PlayerTriggerSystemData : IComponentData
    {
        public NativeHashSet<PhysicEntityPair> _enter;
        public NativeHashSet<PhysicEntityPair> _current;
        public NativeHashSet<PhysicEntityPair> _previous;
    }

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


    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        state.EntityManager.AddComponent<PlayerTriggerSystemData>(state.SystemHandle);
        state.EntityManager.SetComponentData(state.SystemHandle, new PlayerTriggerSystemData()
        {
            _enter = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent),
            _current = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent),
            _previous = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent),
        });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var data = state.EntityManager.GetComponentDataRW<PlayerTriggerSystemData>(state.SystemHandle);

        data.ValueRW._current.Clear();

        var job = new PlayerTriggerEventJob()
        {
            EnterTrigger = data.ValueRW._enter,
            CurrentTrigger = data.ValueRW._current,
            PreviousTrigger = data.ValueRW._previous,
            lookup = SystemAPI.GetComponentLookup<Player>(),
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        state.Dependency.Complete();

        foreach (var pair in data.ValueRW._enter) 
        {
            OnEnter(ref state, pair);
        }

        foreach (var pair in data.ValueRW._current)
        {
            OnStay(ref state, pair);
        }

        foreach (var pair in data.ValueRW._previous)
        {
            if (!data.ValueRW._current.Contains(pair)) OnExit(ref state, pair);
        }

        data.ValueRW._enter.Clear();
        data.ValueRW._previous.Clear();
        data.ValueRW._previous.UnionWith(data.ValueRW._current);
    }

    public void OnEnter(ref SystemState state, PhysicEntityPair entityPair)
    {
        Debug.Log("Enter");

        if (SystemAPI.HasComponent<Enemy>(entityPair.EntityA) || SystemAPI.HasComponent<Enemy>(entityPair.EntityB))
        {
            var playerEntity = SystemAPI.HasComponent<Player>(entityPair.EntityA) ? entityPair.EntityA : entityPair.EntityB;
            
            //var refPlayer = SystemAPI.GetComponentRW<Player>(playerEntity);
            //var currentStat = SystemAPI.GetComponentRW<StatData>(playerEntity);
            //refPlayer.ValueRW.TakeDamge(currentStat.ValueRW.currentStat, 1f);
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
    public void OnStopRunning(ref SystemState state)
    {
        //_enter.Dispose();
        //_current.Dispose();
        //_previous.Dispose();
    }
}
