using System;
using Unity.Entities;
using Unity.Transforms;

public static partial class Utils
{
    static class DynamicBufferUtils
    {
        public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity) where T : unmanaged, IBufferElementData
        {
            return GetBufferInChildren<T>(entity, EntityManager);
        }

        public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity, EntityManager entityManager) where T : unmanaged, IBufferElementData
        {
            var children = entityManager.GetBuffer<Child>(entity);
            for (int i = 0; i < children.Length; i++)
            {
                if (entityManager.HasBuffer<T>(children[i].Value))
                {
                    return entityManager.GetBuffer<T>(children[i].Value);
                }
            }
            return default;
        }

        public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity, BufferLookup<Child> childLookup, BufferLookup<T> bufferLookup) where T : unmanaged, IBufferElementData
        {
            var children = childLookup[entity];
            for (int i = 0; i < children.Length; i++)
            {
                if (bufferLookup.HasBuffer(children[i].Value))
                {
                    return bufferLookup[children[i].Value];
                }
            }
            return default;
        }

        public static T GetBufferElement<T>(DynamicBuffer<T> buffer, Func<T, bool> finder) where T : unmanaged, IBufferElementData
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (finder(buffer[i]))
                {
                    return buffer[i];
                }
            }
            return default;
        }

        public static bool ContainsEx<T>(DynamicBuffer<T> buffer, T element) where T : unmanaged, IBufferElementData
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Equals(element)) return true;
            }
            return false;
        }
        public static bool ContainsEx<T>(DynamicBuffer<T> buffer, T element, out int index) where T : unmanaged, IBufferElementData
        {
            index = -1;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Equals(element))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }
    }
}
