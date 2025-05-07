using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorAuthoring : MonoBehaviour
{
    class AnimatorAuthoringBaker : Baker<AnimatorAuthoring>
    {
        public override void Bake(AnimatorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, authoring.GetComponent<Animator>());
        }
    }
}
