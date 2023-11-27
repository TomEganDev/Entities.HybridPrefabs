using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class CopyTransformFromGameObject : MonoBehaviour
{
    private class Baker : Baker<CopyTransformFromGameObject>
    {
        public override void Bake(CopyTransformFromGameObject authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent<CopyTransformFromGameObjectTag>(entity);
        }
    }
}

public struct CopyTransformFromGameObjectTag : IComponentData { }

[UpdateBefore(typeof(TransformSystemGroup))]
public partial class CopyTransformFromGameObjectSystem : SystemBase
{
    private EntityQuery _query;

    protected override void OnCreate()
    {
        _query = SystemAPI.QueryBuilder()
            .WithAll<CopyTransformFromGameObjectTag>()
            .WithAll<UnityEngine.Transform>()
            .Build();
        
        RequireForUpdate(_query);
    }

    protected override void OnUpdate()
    {
        foreach ((RefRW<LocalTransform> entityTransform,
                     SystemAPI.ManagedAPI.UnityEngineComponent<Transform> gameObjectTransform)
                 in SystemAPI.Query<
                     RefRW<LocalTransform>,
                     SystemAPI.ManagedAPI.UnityEngineComponent<Transform>>()
                     .WithAll<CopyTransformFromGameObjectTag>())
        {
            entityTransform.ValueRW = LocalTransform.FromPositionRotation(gameObjectTransform.Value.position,
                gameObjectTransform.Value.rotation);
        }
    }
}