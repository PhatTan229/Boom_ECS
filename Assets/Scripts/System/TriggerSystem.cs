using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public struct OnTriggerData : IComponentData
{
    public NativeHashSet<PhysicEntityPair> _enter;
    public NativeHashSet<PhysicEntityPair> _current;
    public NativeHashSet<PhysicEntityPair> _previous;
}

[BurstCompile]
public partial struct TriggerSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    struct PlayerTriggerEventJob : ITriggerEventsJob
    {
        public NativeHashSet<PhysicEntityPair> EnterTrigger;
        public NativeHashSet<PhysicEntityPair> CurrentTrigger;
        public NativeHashSet<PhysicEntityPair> PreviousTrigger;
        //[ReadOnly] public ComponentLookup<Player> lookup;

        public void Execute(TriggerEvent triggerEvent)
        {
            //if (!lookup.HasComponent(triggerEvent.EntityA) && !lookup.HasComponent(triggerEvent.EntityB)) return;

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
        state.EntityManager.AddComponent<OnTriggerData>(state.SystemHandle);
        state.EntityManager.SetComponentData(state.SystemHandle, new OnTriggerData()
        {
            _enter = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent),
            _current = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent),
            _previous = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent),
        });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var data = state.EntityManager.GetComponentDataRW<OnTriggerData>(state.SystemHandle);

        data.ValueRW._current.Clear();

        var job = new PlayerTriggerEventJob()
        {
            EnterTrigger = data.ValueRW._enter,
            CurrentTrigger = data.ValueRW._current,
            PreviousTrigger = data.ValueRW._previous,
            //lookup = SystemAPI.GetComponentLookup<Player>(),
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        state.Dependency.Complete();
    
        data.ValueRW._enter.Clear();
        data.ValueRW._previous.Clear();
        data.ValueRW._previous.UnionWith(data.ValueRW._current);
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        //_enter.Dispose();
        //_current.Dispose();
        //_previous.Dispose();
    }
}
