using Unity.Entities;
using UnityEngine;

public abstract class PooledBehaviour : MonoBehaviour
{
    public abstract void OnPoolInstantiate(Entity entity, World world, EntityCommandBuffer commandBuffer);
}