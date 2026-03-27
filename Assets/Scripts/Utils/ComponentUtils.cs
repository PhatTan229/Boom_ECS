using System;
using Unity.Entities;
using Unity.Transforms;

public static partial class Utils
{
    static class ComponentUtils
    {
        public static T GetComponentDataInChildren<T>(Entity entity, out Entity child) where T : unmanaged, IComponentData
        {
            return GetComponentDataInChildren<T>(entity, EntityManager, out child);
        }

        public static T GetComponentDataInChildren<T>(Entity entity, EntityManager entityManager, out Entity child) where T : unmanaged, IComponentData
        {
            child = Entity.Null;
            var children = entityManager.GetBuffer<Child>(entity);
            for (int i = 0; i < children.Length; i++)
            {
                if (entityManager.HasComponent<T>(children[i].Value))
                {
                    child = children[i].Value;
                    return entityManager.GetComponentData<T>(children[i].Value);
                }
            }
            return default;
        }

        public static T GetComponentDataInChildren<T>(Entity entity, BufferLookup<Child> childLookup, ComponentLookup<T> lookup, out Entity child) where T : unmanaged, IComponentData
        {
            child = Entity.Null;
            var children = childLookup[entity];
            for (int i = 0; i < children.Length; i++)
            {
                if (lookup.HasComponent(children[i].Value))
                {
                    child = children[i].Value;
                    return lookup[children[i].Value];
                }
            }
            return default;
        }

        public static T GetComponentDataInChildren<T>(Entity entity, BufferLookup<Child> childLookup, ComponentLookup<T> lookup) where T : unmanaged, IComponentData
        {
            var children = childLookup[entity];
            for (int i = 0; i < children.Length; i++)
            {
                if (lookup.HasComponent(children[i].Value))
                {
                    return lookup[children[i].Value];
                }
            }
            return default;
        }


        public static void SetComponentDataInChildren<T>(Entity entity, T value) where T : unmanaged, IComponentData
        {
            SetComponentDataInChildren<T>(entity, value, EntityManager);
        }

        public static void SetComponentDataInChildren<T>(Entity entity, T value, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            var children = entityManager.GetBuffer<Child>(entity);
            for (int i = 0; i < children.Length; i++)
            {
                if (entityManager.HasComponent<T>(children[i].Value))
                {
                    var component = entityManager.GetComponentData<T>(children[i].Value);
                    entityManager.SetComponentData<T>(children[i].Value, value);
                }
            }
        }

        public static void SetComponentDataInChildren<T>(Entity entity, T value, EntityManager entityManager, Action<Entity> onSet) where T : unmanaged, IComponentData
        {
            var children = entityManager.GetBuffer<Child>(entity);
            for (int i = 0; i < children.Length; i++)
            {
                if (entityManager.HasComponent<T>(children[i].Value))
                {
                    var component = entityManager.GetComponentData<T>(children[i].Value);
                    entityManager.SetComponentData<T>(children[i].Value, value);
                    onSet?.Invoke(children[i].Value);
                }
            }
        }
    }
}
