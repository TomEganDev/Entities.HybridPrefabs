using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

public class SetWorldUI : MonoBehaviour
{
    private class Baker : Baker<SetWorldUI>
    {
        public override void Bake(SetWorldUI authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Renderable);
            AddComponent<SetWorldUITag>(entity);
        }
    }
}

public struct SetWorldUITag : IComponentData {}

public partial class SetWorldUITextSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach ((RefRO<LocalToWorld> ltw,
                     SystemAPI.ManagedAPI.UnityEngineComponent<Text> label)
                 in SystemAPI.Query<
                         RefRO<LocalToWorld>,
                         SystemAPI.ManagedAPI.UnityEngineComponent<Text>>()
                     .WithAll<SetWorldUITag>())
        {
            label.Value.text = $"X = {ltw.ValueRO.Position.x:N2}";
        }
    }
}