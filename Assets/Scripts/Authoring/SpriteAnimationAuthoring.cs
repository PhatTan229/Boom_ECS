using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public struct AnimationStateBuffer : IBufferElementData
{
    public AnimationData state;
}

[MaterialProperty("_Index")]
public struct SpriteAnimationUpdate : IComponentData
{
    public float Value;
}

public struct SpriteAnimation : IComponentData
{
    public UnityObjectRef<Material> material;
    public FixedString32Bytes currentSate;
    public readonly int row;
    public readonly int col;
    public int index;
    public float elapsedTime;

    public SpriteAnimation(Material material, int row, int col)
    {
        this.material = material;
        this.row = row;
        this.col = col;
        currentSate = Utils.FixString32_Emty;
        elapsedTime = 0;
        index = 0;
    }

    public void UpdateAnimation(ref AnimationData data, float deltaTime)
    {
        index = data.currentFrame;
        currentSate = data.name;
        var interval = 1 / data.fps;
        elapsedTime += deltaTime;
        if (elapsedTime < interval) return;
        elapsedTime -= interval;
        if (index < data.endFrame) index++;
        else index = data.startFrame;
        data.currentFrame = index;
    }

    //public void SetValue()
    //{
    //    material.Value.SetFloat("_YIndex", _YIndex);
    //    material.Value.SetFloat("_XIndex", _XIndex);
    //}
}

[RequireComponent(typeof(SpriteRenderAuthoring))]
public class SpriteAnimationAuthoring : MonoBehaviour
{
    [Serializable]
    public class AnimationDataCreate
    {
        public string stateName;
        public int[] frames;
        public float fps;
        public bool defaultState;
        public StateMachineScript stateMachinescript;

        public AnimationDataCreate(string stateName, int[] frames, int fps)
        {
            this.stateName = stateName;
            this.frames = frames;
            this.fps = fps;
        }
    }

    public SpriteRenderAuthoring spriteAuthoring;
    public AnimationDataCreate[] animationStates;
    public int row;
    public int col;
    class SpriteAnimationBaker : Baker<SpriteAnimationAuthoring>
    {
        public override void Bake(SpriteAnimationAuthoring authoring)
        {
            if(authoring.spriteAuthoring == null) authoring.spriteAuthoring = authoring.GetComponent<SpriteRenderAuthoring>();
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpriteAnimation(authoring.spriteAuthoring.material, authoring.row, authoring.col));
            //authoring.GetAniamtionStates();
            var buffer = AddBuffer<AnimationStateBuffer>(entity);
            foreach (var item in authoring.animationStates)
            {
                buffer.Add(new AnimationStateBuffer() { state = new AnimationData(Utils.FixString32(item.stateName), item.frames[0], item.frames[item.frames.Length - 1], item.fps, item.defaultState) });
            }
            AddComponent<SpriteAnimationUpdate>(entity);
        }
    }

    public void GetAniamtionStates()
    {
        col = (int)spriteAuthoring.material.GetFloat("_Collum");
        row = (int)spriteAuthoring.material.GetFloat("_Row");
        animationStates = new AnimationDataCreate[row];
        var arr = Enumerable.Range(0, col * row).ToArray();

        var index = 0;
        for (int i = 0; i < row; i++)
        {
            var frames = new int[col];
            for (int j = 0; j < frames.Length; j++)
            {
                frames[j] = arr[index];
                index++;
            }
            animationStates[i] = new AnimationDataCreate("", frames, 0);
        }
    }
}
