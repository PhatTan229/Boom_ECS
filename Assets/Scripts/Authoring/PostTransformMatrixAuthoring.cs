using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PostTransformMatrixAuthoring : MonoBehaviour
{
    class PostTransformMatrixAuthoringBaker : Baker<PostTransformMatrixAuthoring>
    {
        public override void Bake(PostTransformMatrixAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PostTransformMatrix());
        }
    }
}
