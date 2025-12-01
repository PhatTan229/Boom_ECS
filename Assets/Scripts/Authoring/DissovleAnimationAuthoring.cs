using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

[MaterialProperty("_Fade")]
public struct DissovleFade : IComponentData
{
    public float Value;
}

public struct DissovleModifier : IComponentData
{
    public float dissovleSpeed;
}


[RequireComponent(typeof(SpriteRenderAuthoring))]
public class DissovleAnimationAuthoring : MonoBehaviour
{
    public SpriteRenderAuthoring spriteAuthoring;
    public float dissovleSpeed;

    class DissovleAnimaitonBaker : Baker<DissovleAnimationAuthoring>
    {
        public override void Bake(DissovleAnimationAuthoring authoring)
        {
            if (authoring.spriteAuthoring == null) authoring.spriteAuthoring = authoring.GetComponent<SpriteRenderAuthoring>();
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DissovleFade());
            AddComponent(entity, new DissovleModifier() { dissovleSpeed = authoring.dissovleSpeed });
        }
    }
}
