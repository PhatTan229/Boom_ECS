using System;
using UnityEngine;

[Serializable]
public class PlayerStateMachine_Left : IStateMachine
{
    public void OnStateEnter(AnimationData state)
    {
        Debug.Log("Start Moving Left");
    }
    public void OnStateUpdate(AnimationData state)
    {
        Debug.Log("Moving Left");
    }

    public void OnStateExit(AnimationData state)
    {
        Debug.Log("Stop Moving Left");
    }
}

[CreateAssetMenu(fileName = "Left", menuName = "StateMachines/Player/Left")]
public class PlayerStateMachineScript_Left : StateMachineScript
{
    public override IStateMachine StateMachine
    {
        get
        {
            if (stateMachine == null) stateMachine = new PlayerStateMachine_Left();
            return stateMachine;
        }
    }
}
