using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Rendering;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct SpriteRenderSystem : ISystem, ISystemStartStop
{
    public void OnStartRunning(ref SystemState state)
    {
        var list = new NativeList<RefRW<SpriteRenderInfo>>(Allocator.Temp);
        foreach (var info in SystemAPI.Query<RefRW<SpriteRenderInfo>>())
        {
            list.Add(info);
        }
        SpriteRenderData.Instance = new SpriteRenderData(list);
        list.Dispose();
        var toSet = new NativeList<Entity>(Allocator.Temp);

        foreach (var (_, entity) in SystemAPI.Query<RefRW<SpriteRenderInfo>>().WithEntityAccess())
        {
            toSet.Add(entity);
        }

        //var lookup = state.GetComponentLookup<SpriteRenderInfo>();
        foreach (var item in toSet)
        {
            var info = SystemAPI.GetComponentRW<SpriteRenderInfo>(item);
            SpriteRenderData.Instance.SetRenderInfo(info, item);
        }
        toSet.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        var query = SystemAPI.QueryBuilder()
            .WithAll<LinkedEntityGroup>()
            .Build();

        var entities = query.ToEntityArray(Allocator.Temp);
        if(entities.Length == 0)
        {
            entities.Dispose();
            return;
        }
        var updateEntity = new NativeList<Entity>(Allocator.Temp);

        foreach (var entity in entities)
        {
            var children = SystemAPI.GetBuffer<LinkedEntityGroup>(entity);
            for (int i = 0; i < children.Length; i++)
            {
                if (SystemAPI.HasComponent<SpriteRenderInfo>(children[i].Value) && !state.EntityManager.HasComponent<RenderMeshArray>(children[i].Value))
                {
                    updateEntity.Add(children[i].Value);                 
                }
            }    
        }

        foreach (var item in updateEntity)
        {
            var spriteInfo = SystemAPI.GetComponentRW<SpriteRenderInfo>(item);
            SpriteRenderData.Instance.RegisterData(spriteInfo);
            SpriteRenderData.Instance.SetRenderInfo(spriteInfo, item);
        }
        entities.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
