using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    public GameObject Prefab;
    public float TimePerSpawn;
    public bool InstantSpawn = true;

    private class Baker : Baker<TestSpawner>
    {
        public override void Bake(TestSpawner authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
            AddComponent(entity, new TestSpawnerData
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.None),
                SpawnCooldown = authoring.TimePerSpawn,
                SpawnTimer = authoring.InstantSpawn ? 0 : authoring.TimePerSpawn
            });
        }
    }
}

public struct TestSpawnerData : IComponentData
{
    public Entity Prefab;
    public float SpawnCooldown;
    public float SpawnTimer;
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TestSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer commandBuffer = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (RefRW<TestSpawnerData> spawnerRef in SystemAPI.Query<RefRW<TestSpawnerData>>())
        {
            ref TestSpawnerData spawner = ref spawnerRef.ValueRW;

            spawner.SpawnTimer -= deltaTime;
            if (spawner.SpawnTimer <= 0)
            {
                spawner.SpawnTimer += spawner.SpawnCooldown;

                commandBuffer.Instantiate(spawner.Prefab);
            }
        }
    }
}