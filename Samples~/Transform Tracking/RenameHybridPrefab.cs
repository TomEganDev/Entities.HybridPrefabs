using Unity.Entities;

public class RenameHybridPrefab : PooledBehaviour
{
    public string NewName;
    
    public override void OnPoolInstantiate(Entity entity, World world, EntityCommandBuffer commandBuffer)
    {
        if (!string.IsNullOrEmpty(NewName) && NewName != name)
        {
            name = NewName;
        }
    }
}
