using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct SpriteAnimationSystem : ISystem
{
    public void OnUpdate(ref SystemState state) 
    {
        foreach (var item in SystemAPI.Query<RefRW<SpriteAnimation>>())
        {
            //item.ValueRW.SetValue();
        }
    }
}
