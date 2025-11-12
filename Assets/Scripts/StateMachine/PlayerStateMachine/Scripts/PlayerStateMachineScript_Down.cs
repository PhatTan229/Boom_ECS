using System;
using UnityEngine;

[Serializable]
public class PlayerStateMachine_Down : IStateMachine
{
    public void OnStateEnter(AnimationData state)
    {
        //Debug.Log("Start Moving Down");
    }
    public void OnStateUpdate(AnimationData state)
    {
        //Debug.Log("Moving Down");
    }

    public void OnStateExit(AnimationData state)
    {
        //Debug.Log("Stop Moving Down");
    }
}

[CreateAssetMenu(fileName = "Down", menuName = "StateMachines/Player/Down")]
public class PlayerStateMachineScript_Down : StateMachineScript
{
    public override IStateMachine StateMachine
    {
        get
        {
            if (stateMachine == null) stateMachine = new PlayerStateMachine_Down();
            return stateMachine;
        }
    }
}