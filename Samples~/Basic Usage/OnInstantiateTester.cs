using Unity.Entities;
using UnityEngine;

public class OnInstantiateTester : PooledBehaviour
{
    private uint _count;

    public override void OnPoolInstantiate(Entity entity, World world, EntityCommandBuffer commandBuffer)
    {
        _count++;
        Debug.Log($"{name} instantiated {_count} times\nLinked Entity = {entity}", this);
    }
}
