using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct PlayerStateMachine : IBufferElementData, IStateMachine
{
    public FixedString32Bytes stateName;
    public void OnStateEnter(AnimationState state)
    {
        Debug.Log("OnStateEnter");
    }
    public void OnStateUpdate(AnimationState state)
    {
        Debug.Log("OnStateUpdate");
    }

    public void OnStateExit(AnimationState state)
    {
        Debug.Log("OnStateExit");
    }
}

[RequireComponent(typeof(SpriteAnimationAuthoring))]
public class PlayerStateMachineAuthoring : MonoBehaviour
{
    public SpriteAnimationAuthoring animation;
    class PlayerStateMachineBaker : Baker<PlayerStateMachineAuthoring>
    {
        public override void Bake(PlayerStateMachineAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var buffer = AddBuffer<PlayerStateMachine>(entity);
            if(authoring.animation == null) authoring.animation = authoring.GetComponent<SpriteAnimationAuthoring>();
            foreach (var item in authoring.animation.animationStates)
            {
                buffer.Add(new PlayerStateMachine() { stateName = item.name });
            }
        }
    }
}
