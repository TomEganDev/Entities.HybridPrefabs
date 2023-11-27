using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BasicEntityMover : MonoBehaviour
{
    private class Baker : Baker<BasicEntityMover>
    {
        public override void Bake(BasicEntityMover authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent<BasicEntityMoverTag>(entity);
        }
    }
}

public struct BasicEntityMoverTag : IComponentData {}

public partial struct BasicEntityMoverSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float elapsedTime = (float)SystemAPI.Time.ElapsedTime;

        foreach (RefRW<LocalTransform> transform in
                 SystemAPI.Query<
                         RefRW<LocalTransform>>()
                     .WithAll<BasicEntityMoverTag>())
        {
            transform.ValueRW.Position = new float3(math.sin(elapsedTime), 0f, 0f);
        }
    }
}