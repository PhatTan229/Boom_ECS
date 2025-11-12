using System;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class PlayerStateMachine_Up : IStateMachine
{
    public void OnStateEnter(AnimationData state)
    {
        //Debug.Log("Start Moving Up");
    }
    public void OnStateUpdate(AnimationData state)
    {
        //Debug.Log("Moving Up");
    }

    public void OnStateExit(AnimationData state)
    {
        //Debug.Log("Stop Moving Up");
    }
}

[CreateAssetMenu(fileName = "Up", menuName = "StateMachines/Player/Up")]
public class PlayerStateMachineScript_Up : StateMachineScript
{
    public override IStateMachine StateMachine {
        get 
        {
            if (stateMachine == null) stateMachine = new PlayerStateMachine_Up();
            return stateMachine;
        } 
    }
}
