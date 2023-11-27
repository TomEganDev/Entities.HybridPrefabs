using Unity.Entities;
using UnityEngine;

public class CopyTransformToGameObjectAuthoring : MonoBehaviour
{
    private class Baker : Baker<CopyTransformToGameObjectAuthoring>
    {
        public override void Bake(CopyTransformToGameObjectAuthoring authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent<CopyTransformToGameObject>(entity);
        }
    }
}