using Unity.Entities;

public static partial class Utils
{
    static class EntityUtils
    {
        public static Entity CreateSingleton<T>(string name) where T : unmanaged, IComponentData
        {
            var query = EntityManager.CreateEntityQuery(typeof(T));
            if (query.IsEmpty)
            {
                var entity = EntityManager.CreateSingleton<T>(name);
                return entity;
            }
            else
            {
                return query.GetSingletonEntity();
            }
        }

        public static Entity CreateSingleton<T>(EntityManager entityManager, string name) where T : unmanaged, IComponentData
        {
            var query = entityManager.CreateEntityQuery(typeof(T));
            if (query.IsEmpty)
            {
                var entity = entityManager.CreateSingleton<T>(name);
                return entity;
            }
            else
            {
                return query.GetSingletonEntity();
            }
        }
    }
}
