using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.EventSystems.EventTrigger;

public class SpriteRenderData : IDisposable
{
    public static SpriteRenderData Instance;

    private RenderMeshArray renderMeshArray;
    private RenderMeshDescription description;

    private Dictionary<Material, BatchMaterialID> materialIds = new Dictionary<Material, BatchMaterialID>();
    private Dictionary<Mesh, BatchMeshID> meshIds = new Dictionary<Mesh, BatchMeshID>();

    public SpriteRenderData(NativeList<RefRW<SpriteRenderInfo>> infos)
    {
        Instance = this;
        description = new RenderMeshDescription(
               shadowCastingMode: ShadowCastingMode.Off,
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

        var egs = Utils.EntityManager.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
        foreach (var item in meshes)
        {
            var batchId = egs.RegisterMesh(item);
            meshIds.Add(item, batchId);
        }

        foreach (var item in material)
        {
            var batchId = egs.RegisterMaterial(item);
            materialIds.Add(item, batchId);
        }

        //Debug.Log($"Mesh Count: {meshes.ToArray().Length}, Material Count: {material.ToArray().Length}");
        renderMeshArray = new RenderMeshArray(material.ToArray(), meshes.ToArray());
    }

    public void SetRenderInfo(RefRW<SpriteRenderInfo> info, Entity entity)
    {
        var materialMeshInfo = new MaterialMeshInfo()
        {
            Material = 0,
            MaterialID = materialIds[info.ValueRW.material.Value],
            Mesh = 0,
            MeshID = meshIds[info.ValueRW.mesh.Value]
        };
        RenderMeshUtility.AddComponents(entity, Utils.EntityManager, description, materialMeshInfo);
        Utils.EntityManager.AddSharedComponentManaged(entity, renderMeshArray);
    }

    public void Dispose()
    {
        Instance = null;
    }

}
