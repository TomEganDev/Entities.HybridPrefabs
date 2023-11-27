using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public struct CopyTransformToGameObject : IComponentData {}

[BurstCompile]
public struct CopyTransformJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction][ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
    [ReadOnly] public NativeList<Entity> Entities;
    
    public unsafe void Execute(int index, TransformAccess transform)
    {
        LocalToWorld ltw = LocalToWorldLookup[Entities[index]];
        Matrix4x4 matrix = *(UnityEngine.Matrix4x4*) &ltw;
        
        transform.SetPositionAndRotation(ltw.Position, matrix.rotation);
    }
}

[UpdateInGroup(typeof(TransformSystemGroup), OrderLast = true)]
public partial class CopyTransformToGameObjectSystem : SystemBase
{
    private EntityQuery _transformQuery;

    protected override void OnCreate()
    {
        _transformQuery = SystemAPI.QueryBuilder()
            .WithAll<CopyTransformToGameObject, UnityEngine.Transform>()
            .Build();
        
        EntityQuery runRequirementQuery = SystemAPI.QueryBuilder()
            .WithAll<CopyTransformToGameObject>()
            .WithAny<UnityEngine.Transform, UnityEngine.RectTransform>()
            .Build();

        RequireForUpdate(runRequirementQuery);
    }

    protected override void OnUpdate()
    {
        NativeList<Entity> entities = _transformQuery.ToEntityListAsync(Allocator.TempJob, out JobHandle handle);
        TransformAccessArray transforms = _transformQuery.GetTransformAccessArray();
        
        handle = JobHandle.CombineDependencies(Dependency, handle);
        handle = new CopyTransformJob
        {
            Entities = entities,
            LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(isReadOnly: true)
        }.Schedule(transforms, handle);

        entities.Dispose(handle);

        Dependency = handle;
        
        // RectTransform not handled by IJobParallelForTransform so we update here instead
        foreach ((RefRO<LocalToWorld> ltw,
                     SystemAPI.ManagedAPI.UnityEngineComponent<RectTransform> rectTransform)
                 in SystemAPI.Query<
                         RefRO<LocalToWorld>,
                         SystemAPI.ManagedAPI.UnityEngineComponent<UnityEngine.RectTransform>>()
                     .WithAll<CopyTransformToGameObject>())
        {
            rectTransform.Value.SetPositionAndRotation(ltw.ValueRO.Position, ltw.ValueRO.Rotation);
        }
    }
}