using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct SpriteAnimationSystem : ISystem
{
    public void OnUpdate(ref SystemState state) 
    {
        foreach (var (update, animation) in SystemAPI.Query<RefRW<SpriteAnimationUpdate>, RefRO<SpriteAnimation>>())
        {
            update.ValueRW.Value = (float)animation.ValueRO.index;
        }
    }
}
