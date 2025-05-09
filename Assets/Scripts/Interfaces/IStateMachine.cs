public interface IStateMachine
{
    public void OnStateEnter(AnimationState state);
    public void OnStateUpdate(AnimationState state);
    public void OnStateExit(AnimationState state);
}