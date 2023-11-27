using Unity.Burst;
using Unity.Entities;

public struct DestroyInTime : IComponentData
{
    public float TimeToDestroy;
}

public partial struct DestroyInTimeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach ((RefRW<DestroyInTime> timerRef, Entity entity) in
                 SystemAPI.Query<
                     RefRW<DestroyInTime>>()
                     .WithEntityAccess())
        {
            ref DestroyInTime timer = ref timerRef.ValueRW;
            
            timer.TimeToDestroy -= deltaTime;
            if (timer.TimeToDestroy <= 0)
            {
                commandBuffer.DestroyEntity(entity);
            }
        }
    }
}
