public interface IStateMachine
{
    public void OnStateEnter(AnimationData state);
    public void OnStateUpdate(AnimationData state);
    public void OnStateExit(AnimationData state);
}