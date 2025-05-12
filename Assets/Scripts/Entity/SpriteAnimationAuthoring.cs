using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct AnimationStateBuffer : IBufferElementData
{
    public AnimationData state;
}

public struct SpriteAnimation : IComponentData
{
    public UnityObjectRef<Material> material;
    public FixedString32Bytes currentSate;
    public readonly int row;
    public readonly int col;
    public int _XIndex;
    public int _YIndex;

    public float elapsedTime;

    public SpriteAnimation(Material material)
    {
        this.material = material;
        row = (int)material.GetFloat("_Row") - 1;
        col = (int)material.GetFloat("_Collum") - 1;
        _XIndex = 0;
        _YIndex = 0;
        currentSate = Utils.FixString32_Emty;
        elapsedTime = 0;
    }

    public void UpdateAnimation(ref AnimationData data, float deltaTime)
    {
        currentSate = data.name;
        var interval = 1 / data.fps;
        elapsedTime += deltaTime;
        if (elapsedTime < interval) return;
        elapsedTime -= interval;
        _YIndex = data.rowIndex;
        material.Value.SetFloat("_YIndex", _YIndex);

        _XIndex = (data.currentFrame + 1) % col;
        material.Value.SetFloat("_XIndex", _XIndex);
        data.currentFrame = _XIndex;
    }
}

[RequireComponent(typeof(SpriteRenderAuthoring))]
public class SpriteAnimationAuthoring : MonoBehaviour
{
    [Serializable]
    public class AnimationDataCreate
    {
        public string stateName;
        public int row;
        public int fps;
        public bool defaultState;
        public StateMachineScript stateMachinescript;

        public AnimationDataCreate(string stateName, int row, int fps)
        {
            this.stateName = stateName;
            this.row = row;
            this.fps = fps;
        }
    }

    public SpriteRenderAuthoring spriteAuthoring;
    public AnimationDataCreate[] animationStates;

    class SpriteAnimationBaker : Baker<SpriteAnimationAuthoring>
    {
        public override void Bake(SpriteAnimationAuthoring authoring)
        {
            if(authoring.spriteAuthoring == null) authoring.spriteAuthoring = authoring.GetComponent<SpriteRenderAuthoring>();
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpriteAnimation(authoring.spriteAuthoring.material));
            //authoring.GetAniamtionStates();
            var buffer = AddBuffer<AnimationStateBuffer>(entity);
            foreach (var item in authoring.animationStates)
            {
                buffer.Add(new AnimationStateBuffer() { state = new AnimationData(Utils.FixString32(item.stateName), item.row, item.fps) });
            }
        }
    }

    public void GetAniamtionStates()
    {
        var col = (int)spriteAuthoring.material.GetFloat("_Collum");
        var row = (int)spriteAuthoring.material.GetFloat("_Row");
        animationStates = new AnimationDataCreate[row];
        var arr = Enumerable.Range(0, col).ToArray();

        for (int i = 0; i < row; i++)
        {
            animationStates[i] = new AnimationDataCreate("", i, 0);
        }
    }
}
