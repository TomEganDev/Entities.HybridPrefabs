using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SimpleEntityMovement : MonoBehaviour
{
    [Tooltip("First waypoint is transform origin, this is second waypoint in local space")]
    public Vector3 LocalMovementWaypoint;

    public float Speed = 1f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.TransformPoint(LocalMovementWaypoint), 0.1f);
    }

    private class Baker : Baker<SimpleEntityMovement>
    {
        public override void Bake(SimpleEntityMovement authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            Transform authoringTransform = authoring.transform;
            AddComponent(entity, new SimpleEntityMovementData
            {
                Speed = authoring.Speed,
                PointA = authoringTransform.position,
                PointB = authoringTransform.TransformPoint(authoring.LocalMovementWaypoint)
            });
        }
    }
}

public struct SimpleEntityMovementData : IComponentData
{
    public float Speed;
    public float3 PointA;
    public float3 PointB;
}

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SimpleEntityMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        SimpleEntityMovementJob job = new SimpleEntityMovementJob
        {
            ElapsedTime = (float)SystemAPI.Time.ElapsedTime
        };
        job.ScheduleParallel();
    }

    [BurstCompile]
    private partial struct SimpleEntityMovementJob : IJobEntity
    {
        public float ElapsedTime;
        
        private void Execute(ref LocalTransform transform, in SimpleEntityMovementData movement)
        {
            float t = math.remap(-1f, 1f, 0f, 1f, math.sin(ElapsedTime * movement.Speed));
            transform.Position = math.lerp(movement.PointA, movement.PointB, t);
        }
    }
}