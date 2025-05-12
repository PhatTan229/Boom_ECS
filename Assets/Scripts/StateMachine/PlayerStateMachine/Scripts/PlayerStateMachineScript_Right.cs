using System;
using UnityEngine;

[Serializable]
public class PlayerStateMachine_Right : IStateMachine
{
    public void OnStateEnter(AnimationData state)
    {
        Debug.Log("Start Moving Right");
    }
    public void OnStateUpdate(AnimationData state)
    {
        Debug.Log("Moving Right");
    }

    public void OnStateExit(AnimationData state)
    {
        Debug.Log("Stop Moving Right");
    }
}

[CreateAssetMenu(fileName = "Right", menuName = "StateMachines/Player/Right")]
public class PlayerStateMachineScript_Right : StateMachineScript
{
    public override IStateMachine StateMachine
    {
        get
        {
            if (stateMachine == null) stateMachine = new PlayerStateMachine_Right();
            return stateMachine;
        }
    }
}