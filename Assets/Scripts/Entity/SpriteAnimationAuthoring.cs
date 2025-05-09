using System.Linq;
using Unity.Entities;
using UnityEngine;

public struct AnimationStateBuffer : IBufferElementData
{
    public AnimationState state;
}

public struct SpriteAnimation : IComponentData
{
    public UnityObjectRef<Material> material;
    public int row;
    public int col;
    public int _XIndex;
    public int _YIndex;

    public SpriteAnimation(Material material)
    {
        this.material = material;
        row = (int)material.GetFloat("_Row");
        col = (int)material.GetFloat("_Collum");
        _XIndex = 0;
        _YIndex = 0;
    }
}

[RequireComponent(typeof(SpriteRenderAuthoring))]
public class SpriteAnimationAuthoring : MonoBehaviour
{
    public SpriteRenderAuthoring spriteAuthoring;
    public AnimationState[] animationStates;

    class SpriteAnimationBaker : Baker<SpriteAnimationAuthoring>
    {
        public override void Bake(SpriteAnimationAuthoring authoring)
        {
            if(authoring.spriteAuthoring == null) authoring.spriteAuthoring = authoring.GetComponent<SpriteRenderAuthoring>();
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpriteAnimation(authoring.spriteAuthoring.material));
            authoring.GetAniamtionStates();
            var buffer = AddBuffer<AnimationStateBuffer>(entity);
            foreach (var item in authoring.animationStates)
            {
                buffer.Add(new AnimationStateBuffer() { state = item });
            }
        }
    }

    public void GetAniamtionStates()
    {
        var col = (int)spriteAuthoring.material.GetFloat("_Collum");
        var row = (int)spriteAuthoring.material.GetFloat("_Row");
        animationStates = new AnimationState[row];
        var arr = Enumerable.Range(0, col).ToArray();

        for (int i = 0; i < row; i++)
        {
            animationStates[i] = new AnimationState("", arr);
        }
    }
}
