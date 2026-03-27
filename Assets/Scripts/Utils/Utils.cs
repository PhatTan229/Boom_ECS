using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public static partial class Utils
{
    //Field
    public static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
    public static FixedString32Bytes FixString32_Emty => new FixedString32Bytes();

    //Mathf
    public static float2 ToFloat2(this Vector2 vector) => MathfUtils.ToFloat2(vector);
    public static float2 ToFloat2(this Vector3 vector) => MathfUtils.ToFloat2(vector);
    public static float3 ToFloat3(this Vector2 vector) => MathfUtils.ToFloat3(vector);
    public static float3 ToFloat3(this Vector3 vector) => MathfUtils.ToFloat3(vector);
    public static bool IsEqual(this float3 origin, float3 other) => MathfUtils.IsEqual(origin, other);

    //Fixed String
    public static FixedString32Bytes FixString32(string str) => FixedStringUtils.FixString32(str);
    public static FixedString64Bytes FixString64(string str) => FixedStringUtils.FixString64(str);

    //Entity
    public static Entity CreateSingleton<T>(string name) where T : unmanaged, IComponentData => EntityUtils.CreateSingleton<T>(name);
    public static Entity CreateSingleton<T>(EntityManager entityManager, string name) where T : unmanaged, IComponentData => EntityUtils.CreateSingleton<T>(entityManager, name);

    //Component
    public static T GetComponentDataInChildren<T>(Entity entity, out Entity child) where T : unmanaged, IComponentData => ComponentUtils.GetComponentDataInChildren<T>(entity, out child);
    public static T GetComponentDataInChildren<T>(Entity entity, EntityManager entityManager, out Entity child) where T : unmanaged, IComponentData => ComponentUtils.GetComponentDataInChildren<T>(entity, entityManager, out child);
    public static T GetComponentDataInChildren<T>(Entity entity, BufferLookup<Child> childLookup, ComponentLookup<T> lookup, out Entity child) where T : unmanaged, IComponentData => ComponentUtils.GetComponentDataInChildren<T>(entity, childLookup, lookup, out child);
    public static T GetComponentDataInChildren<T>(Entity entity, BufferLookup<Child> childLookup, ComponentLookup<T> lookup) where T : unmanaged, IComponentData => ComponentUtils.GetComponentDataInChildren<T>(entity, childLookup, lookup);
    public static void SetComponentDataInChildren<T>(Entity entity, T value) where T : unmanaged, IComponentData => ComponentUtils.SetComponentDataInChildren(entity, value);
    public static void SetComponentDataInChildren<T>(Entity entity, T value, EntityManager entityManager) where T : unmanaged, IComponentData => ComponentUtils.SetComponentDataInChildren(entity, value, entityManager);
    public static void SetComponentDataInChildren<T>(Entity entity, T value, EntityManager entityManager, Action<Entity> onSet) where T : unmanaged, IComponentData => ComponentUtils.SetComponentDataInChildren(entity, value, entityManager, onSet);
    
    //Dynamic Buffer
    public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity) where T : unmanaged, IBufferElementData => DynamicBufferUtils.GetBufferInChildren<T>(entity);
    public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity, EntityManager entityManager) where T : unmanaged, IBufferElementData => DynamicBufferUtils.GetBufferInChildren<T>(entity, entityManager);
    public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity, BufferLookup<Child> childLookup, BufferLookup<T> bufferLookup) where T : unmanaged, IBufferElementData => DynamicBufferUtils.GetBufferInChildren<T>(entity, childLookup, bufferLookup);
    public static T GetBufferElement<T>(this DynamicBuffer<T> buffer, Func<T, bool> finder) where T : unmanaged, IBufferElementData => DynamicBufferUtils.GetBufferElement(buffer, finder);
    public static bool ContainsEx<T>(this DynamicBuffer<T> buffer, T element) where T : unmanaged, IBufferElementData => DynamicBufferUtils.ContainsEx(buffer, element);
    public static bool ContainsEx<T>(this DynamicBuffer<T> buffer, T element, out int index) where T : unmanaged, IBufferElementData => DynamicBufferUtils.ContainsEx(buffer, element, out index);
    
    //Common
    public static List<int> GetUniqueRandomNumbers(int min, int max, int n) => CommonUtils.GetUniqueRandomNumbers(min, max, n);
}
