using Unity.Entities;
using UnityEngine;

public class DestroyInTimeAuthoring : MonoBehaviour
{
    public float TimeToDestroy = 1f;
    
    private class Baker : Baker<DestroyInTimeAuthoring>
    {
        public override void Bake(DestroyInTimeAuthoring authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
            AddComponent(entity, new DestroyInTime { TimeToDestroy = authoring.TimeToDestroy });
        }
    }
}