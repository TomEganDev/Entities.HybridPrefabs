using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes
// ReSharper disable Unity.BurstLoadingManagedType

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(TeardownHybridPrefabSystem))]
public partial class InstantiateHybridPrefabSystem : SystemBase
{
    private struct EntityComponentPair
    {
        public Entity Entity;
        public UnityEngine.Component Component;
    }

    private List<EntityComponentPair> _deferredComponentAddBuffer;

    protected override void OnCreate()
    {
        _deferredComponentAddBuffer = new List<EntityComponentPair>(128);
        
        EntityQuery query = SystemAPI.QueryBuilder()
            .WithAll<LinkedHybridPrefabData, LocalToWorld>()
            .WithNone<LinkedHybridInstanceData>()
            .Build();
        RequireForUpdate(query);
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        
        foreach ((RefRO<LinkedHybridPrefabData> prefabDataRef,
                     RefRO<LocalToWorld> ltwRef,
                     Entity entity)
                 in SystemAPI.Query<
                         RefRO<LinkedHybridPrefabData>,
                         RefRO<LocalToWorld>>()
                     .WithNone<LinkedHybridInstanceData>()
                     .WithEntityAccess())
        {
            LinkedHybridPrefabData prefabData = prefabDataRef.ValueRO;
            LocalToWorld ltw = ltwRef.ValueRO;
            
            HybridPrefab instance = HybridPrefabPool.GetInstance(prefabData.PrefabId);
#if UNITY_EDITOR
            instance.name = $"{prefabData.PrefabId}:{instance.GetInstanceID()} {World.Name}";
#endif
            
            if (prefabData.EntityNeedsTransform)
            {
                //commandBuffer.AddComponent(entity, instance.transform);
                _deferredComponentAddBuffer.Add(new EntityComponentPair
                {
                    Entity = entity,
                    Component = instance.transform
                });
                instance.transform.SetLocalPositionAndRotation(ltw.Position, ltw.Rotation);
            }
            
            for (int i = 0; i < instance.Callbacks.Length; i++)
            {
                instance.Callbacks[i].OnPoolInstantiate(entity, World, commandBuffer);
            }
                
            commandBuffer.AddComponent(entity, new LinkedHybridInstanceData
            {
                InstanceId = instance.GetInstanceID()
            });

            for (int i = 0; i < instance.ComponentsToAdd.Length; i++)
            {
                // NOTE: This doesn't work as component gets added with ComponentType UnityEngine.Component
                //commandBuffer.AddComponent(entity, instance.ComponentsToAdd[i]);
                
                // so we collect all the components to add later once out of the loop
                // (so we can make structural changes)
                _deferredComponentAddBuffer.Add(new EntityComponentPair
                {
                    Entity = entity,
                    Component = instance.ComponentsToAdd[i]
                });
            }
        }
        
        commandBuffer.Playback(EntityManager);

        foreach (EntityComponentPair pair in _deferredComponentAddBuffer)
        {
            EntityManager.AddComponentObject(pair.Entity, pair.Component);
        }
        _deferredComponentAddBuffer.Clear();
    }
}