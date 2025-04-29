using Unity.Entities;

public interface IOnTrigger
{
    void OnEnter(ref SystemState state, PhysicEntityPair entityPair);
    void OnStay(ref SystemState state, PhysicEntityPair entityPair);
    void OnExit(ref SystemState state, PhysicEntityPair entityPair);
}
