using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class PlayerStateMachine : IStateMachine
{
    public FixedString32Bytes stateName;
    public void OnStateEnter(AnimationData state)
    {
        Debug.Log("OnStateEnter");
    }
    public void OnStateUpdate(AnimationData state)
    {
        Debug.Log("OnStateUpdate");
    }

    public void OnStateExit(AnimationData state)
    {
        Debug.Log("OnStateExit");
    }
}

public class StateMachine : IComponentData
{
    public List<IStateMachine> stateMachine;
}

[RequireComponent(typeof(SpriteAnimationAuthoring))]
public class PlayerStateMachineAuthoring : MonoBehaviour
{
    public SpriteAnimationAuthoring spriteAnimation;
    class PlayerStateMachineBaker : Baker<PlayerStateMachineAuthoring>
    {
        public override void Bake(PlayerStateMachineAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            if (authoring.spriteAnimation == null) authoring.spriteAnimation = authoring.GetComponent<SpriteAnimationAuthoring>();
            var stateMachines = new List<IStateMachine>();
            foreach (var item in authoring.spriteAnimation.animationStates)
            {
                stateMachines.Add(new PlayerStateMachine() { stateName = item.stateName } );
            }
            AddComponentObject(entity, new StateMachine() { stateMachine = stateMachines});
        }
    }
}
