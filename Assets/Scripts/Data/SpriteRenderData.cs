using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.VisualScripting;
using UnityEngine;

public class SpriteRenderData : IDisposable
{
    public static SpriteRenderData Instance;

    private RenderMeshArray renderMeshArray;
    private RenderMeshDescription description;

    public SpriteRenderData(NativeList<RefRW<SpriteRenderInfo>> infos)
    {
        Instance = this;
        description = new RenderMeshDescription(
               shadowCastingMode: UnityEngine.Rendering.ShadowCastingMode.Off,
               receiveShadows: false
        );
        var meshes = new List<Mesh>();
        var material = new List<Material>();
        foreach (var info in infos) 
        {
            if(!meshes.Contains(info.ValueRW.mesh.Value)) meshes.Add(info.ValueRW.mesh.Value);
            info.ValueRW.meshIndex = meshes.IndexOf(info.ValueRW.mesh.Value);
            if(!material.Contains(info.ValueRW.material.Value)) material.Add(info.ValueRW.material.Value);
            info.ValueRW.materialIndex = material.IndexOf(info.ValueRW.material.Value);
        }
        Debug.Log($"Mesh Count: {meshes.ToArray().Length}, Material Count: {material.ToArray().Length}");
        renderMeshArray = new RenderMeshArray(material.ToArray(), meshes.ToArray());
    }

    public void SetRenderInfo(RefRW<SpriteRenderInfo> info, Entity entity, EntityCommandBuffer ecb)
    {
        Debug.Log($"SetRenderInfo: materialIndex={info.ValueRO.materialIndex}, meshIndex={info.ValueRO.meshIndex}, entity={entity.Index}");
        RenderMeshUtility.AddComponents(entity, Utils.EntityManager, description, renderMeshArray, new MaterialMeshInfo() { Material = info.ValueRO.materialIndex, Mesh = info.ValueRO.meshIndex});
    }
    public void Dispose()
    {
        Instance = null;
    }

}
