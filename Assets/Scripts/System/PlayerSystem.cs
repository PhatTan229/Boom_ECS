using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    partial struct SetAnimationJob : IJobEntity
    {
        public Entity player;
        [NativeDisableParallelForRestriction] public BufferLookup<AnimationStateBuffer> stateLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<SpriteAnimation> animaitonLookup;
        [ReadOnly] public BufferLookup<Child> childLookup;
        public FixedString32Bytes stateName;
        public float deltaTime;
        [NativeDisableParallelForRestriction] public NativeHashMap<FixedString32Bytes, AnimationData> outputData;

        void Execute([ChunkIndexInQuery] int index)
        {
            for (int i = 0; i < childLookup[player].Length; i++)
            {
                var child = childLookup[player][i].Value;
                if (animaitonLookup.HasComponent(child))
                {
                    PlayAnimation(child, animaitonLookup[child], stateName);
                    return;
                }
            }
        }

        private void PlayAnimation(Entity entity, SpriteAnimation animation, FixedString32Bytes stateName)
        {
            var allState = stateLookup[entity];
            for (int i = 0; i < allState.Length; i++)
            {
                if(allState[i].state.name == stateName || (stateName == Utils.FixString32_Emty && allState[i].state.isDefault))
                {
                    var data = allState[i];
                    animation.UpdateAnimation(ref data.state, deltaTime);
                    allState[i] = data;
                }
                outputData[allState[i].state.name] = allState[i].state;
            }
            animaitonLookup[entity] = animation;
        }
    }

    private Entity player;
    private BufferLookup<AnimationStateBuffer> stateLookup;
    private BufferLookup<Child> childLookup;
    private ComponentLookup<SpriteAnimation> animaitonLookup;
    private NativeHashMap<FixedString32Bytes, AnimationData> data;

    public void OnStartRunning(ref SystemState state)
    {
        player = SystemAPI.GetSingletonEntity<Player>();
        stateLookup = SystemAPI.GetBufferLookup<AnimationStateBuffer>();
        childLookup = SystemAPI.GetBufferLookup<Child>();
        animaitonLookup = SystemAPI.GetComponentLookup<SpriteAnimation>();
        var states = Utils.GetBufferInChildren(player, childLookup, stateLookup);
        data = new NativeHashMap<FixedString32Bytes, AnimationData>(states.Length, Allocator.Persistent);
        var job = new SetAnimationJob()
        {
            player = player,
            stateLookup = stateLookup,
            animaitonLookup = animaitonLookup,
            childLookup = childLookup,
            deltaTime = SystemAPI.Time.DeltaTime,
            stateName = Utils.FixString32_Emty,
            outputData = data

        };
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    public void OnUpdate(ref SystemState state)
    {
        OnTrigger(ref state);

        var input = SystemAPI.GetSingletonRW<InputStorage>();
        if (math.all(input.ValueRO.direction == float3.zero)) return;

        stateLookup.Update(ref state);
        childLookup.Update(ref state);
        animaitonLookup.Update(ref state);

        var stateName = Utils.FixString32_Emty;
        if (math.all(input.ValueRO.direction == Direction.Up)) stateName = Utils.FixString32(nameof(Direction.Up));
        else if (math.all(input.ValueRO.direction == Direction.Down)) stateName = Utils.FixString32(nameof(Direction.Down));
        else if (math.all(input.ValueRO.direction == Direction.Left)) stateName = Utils.FixString32(nameof(Direction.Left));
        else stateName = Utils.FixString32(nameof(Direction.Right));

        var job = new SetAnimationJob()
        {
            player = player,
            stateLookup = stateLookup,
            animaitonLookup = animaitonLookup,
            childLookup = childLookup,
            deltaTime = SystemAPI.Time.DeltaTime,
            stateName = stateName,
            outputData = data
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);  
    }

    public void OnStopRunning(ref SystemState state)
    {
        data.Dispose();
    }
}
