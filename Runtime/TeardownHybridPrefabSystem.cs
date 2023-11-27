using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial class TeardownHybridPrefabSystem : SystemBase
{
    protected override void OnCreate()
    {
        EntityQuery query = SystemAPI.QueryBuilder()
            .WithAll<LinkedHybridInstanceData>()
            .WithNone<LinkedHybridPrefabData>()
            .Build();
        RequireForUpdate(query);
        RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(World.Unmanaged);

        foreach ((RefRO<LinkedHybridInstanceData> instanceData, Entity entity) in
                 SystemAPI.Query<
                         RefRO<LinkedHybridInstanceData>>()
                     .WithNone<LinkedHybridPrefabData>()
                     .WithEntityAccess())
        {
            HybridPrefabPool.ReturnInstance(instanceData.ValueRO.InstanceId);
            commandBuffer.RemoveComponent<LinkedHybridInstanceData>(entity);
        }
    }
}