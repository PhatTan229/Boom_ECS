using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Serialization;
using UnityEngine;

public abstract class StateMachineScript : ScriptableObject
{
    protected IStateMachine stateMachine;
    public abstract IStateMachine StateMachine { get; }
}

public struct StateMachineData
{
    public int id;
    public UnityObjectRef<StateMachineScript> stateMachineScript;
}

public struct StateMachineBuffer : IBufferElementData
{
    public StateMachineData StateMachineData;
}

//public class StateMachine : IComponentData
//{
//    public Dictionary<FixedString32Bytes, IStateMachine> stateMachines;
//}

[RequireComponent(typeof(SpriteAnimationAuthoring))]
public class StateMachineAuthoring : MonoBehaviour
{
    public SpriteAnimationAuthoring spriteAnimation;
    class PlayerStateMachineBaker : Baker<StateMachineAuthoring>
    {
        public override void Bake(StateMachineAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            if (authoring.spriteAnimation == null) authoring.spriteAnimation = authoring.GetComponent<SpriteAnimationAuthoring>();
            var buffer = AddBuffer<StateMachineBuffer>(entity);
            //var stateMachines = new Dictionary<FixedString32Bytes, IStateMachine>();
            foreach (var item in authoring.spriteAnimation.animationStates)
            {
                if (item.stateMachinescript == null) continue;
                //stateMachines.Add(Utils.FixString32(item.stateName), item.stateMachinescript.StateMachine);
                var data = new StateMachineData()
                {
                    id = Utils.FNV1aHash(item.stateName),
                    stateMachineScript = item.stateMachinescript
                };
                buffer.Add(new StateMachineBuffer() { StateMachineData = data });
            }
            //AddComponentObject(entity, new StateMachine() { stateMachines = stateMachines });
        }
    }
}
