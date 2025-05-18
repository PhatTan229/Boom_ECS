using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public partial struct BombSystem : ISystem, ISystemStartStop, IOnTrigger
{
    struct TriggerJob : ITriggerEventsJob
    {
        public NativeHashSet<PhysicEntityPair> EnterTrigger;
        public NativeHashSet<PhysicEntityPair> CurrentTrigger;
        public NativeHashSet<PhysicEntityPair> PreviousTrigger;
        [ReadOnly] public ComponentLookup<Bomb> lookup;

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


    private NativeHashSet<PhysicEntityPair> _enter;
    private NativeHashSet<PhysicEntityPair> _current;
    private NativeHashSet<PhysicEntityPair> _previous;

    private ComponentLookup<Bomb> bombLookup;
    private ComponentLookup<Player> playerLookup;
    private ComponentLookup<Enemy> enemyLookup;
    public void OnStartRunning(ref SystemState state)
    {
        _enter = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent);
        _current = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent);
        _previous = new NativeHashSet<PhysicEntityPair>(128, Allocator.Persistent);
    }

    public void OnUpdate(ref SystemState state) 
    {
        bombLookup.Update(ref state);
        playerLookup.Update(ref state);
        enemyLookup.Update(ref state);

        _current.Clear();

        var job = new TriggerJob()
        {
            EnterTrigger = _enter,
            CurrentTrigger = _current,
            PreviousTrigger = _previous,
            lookup = bombLookup,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        state.Dependency.Complete();

        foreach (var pair in _enter)
        {
            OnEnter(ref state, pair);
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
        if(entityPair.TryGetEntity(playerLookup, out var player, out var bomb))
        {
            var buffer = state.EntityManager.GetBuffer<InTrigger>(bomb);
            var playerInTrigger = new InTrigger() { value = player };
            if(!buffer.ContainsEx(playerInTrigger))
            {
                buffer.Add(playerInTrigger);
            }
        }
    }

    public void OnExit(ref SystemState state, PhysicEntityPair entityPair)
    {
    }

    public void OnStay(ref SystemState state, PhysicEntityPair entityPair)
    {
    }

    public void OnStopRunning(ref SystemState state)
    {
        _enter.Dispose();
        _current.Dispose();
        _previous.Dispose();
    }
}
