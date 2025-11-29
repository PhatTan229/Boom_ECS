using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public struct Tintable : IComponentData
{
    public float fadeOutSpeed;
}

[MaterialProperty("_TintPercent")]
public struct TintColor : IComponentData
{
    public float Value;
}

public class TintColorAuthoring : MonoBehaviour
{
    public float fadeSpeed;

    class TintColorAuthoringBaker : Baker<TintColorAuthoring>
    {
        public override void Bake(TintColorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Tintable() { fadeOutSpeed = authoring.fadeSpeed });
            AddComponent(entity, new TintColor());
        }
    }
}
