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

[MaterialProperty("_FadeColor")]
public struct DissovelColor : IComponentData
{
    public Color Value;

    public DissovelColor(Color c)
    {
        Value = c;
    }
}

[RequireComponent(typeof(SpriteRenderAuthoring))]
public class DissovleAnimationAuthoring : MonoBehaviour
{
    public SpriteRenderAuthoring spriteAuthoring;
    public Color fadeColor;

    class DissovleAnimaitonBaker : Baker<DissovleAnimationAuthoring>
    {
        public override void Bake(DissovleAnimationAuthoring authoring)
        {
            if (authoring.spriteAuthoring == null) authoring.spriteAuthoring = authoring.GetComponent<SpriteRenderAuthoring>();
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DissovelColor(authoring.fadeColor));
            AddComponent(entity, new DissovleFade());
        }
    }
}
