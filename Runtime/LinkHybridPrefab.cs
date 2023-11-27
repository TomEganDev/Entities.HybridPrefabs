using Unity.Entities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LinkHybridPrefab))]
public class LinkHybridPrefabInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LinkHybridPrefab link = (LinkHybridPrefab)target;
        if (link.Prefab != null)
        {
            if (link.PreviewInstance != null && GUILayout.Button("Stop Preview"))
            {
                DestroyImmediate(link.PreviewInstance.gameObject);
            }
            else if(link.PreviewInstance == null && GUILayout.Button("Start Preview"))
            {
                link.PreviewInstance = Instantiate(link.Prefab, link.transform);
                link.PreviewInstance.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }
    }
}
#endif

public class LinkHybridPrefab : MonoBehaviour
{
#if UNITY_EDITOR
    [System.NonSerialized]
    public HybridPrefab PreviewInstance;
#endif
    
    public HybridPrefab Prefab;

    private class Baker : Baker<LinkHybridPrefab>
    {
        public override void Bake(LinkHybridPrefab authoring)
        {
            if (authoring.Prefab == null)
            {
                return;
            }

            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Renderable);
            
            AddComponent(entity, new LinkedHybridPrefabData
            {
                PrefabId = authoring.Prefab.PrefabId,
                EntityNeedsTransform = authoring.Prefab.CopyTransformToPrefab,
            });

            if (authoring.Prefab.CopyTransformToPrefab)
            {
                AddComponent<CopyTransformToGameObject>(entity);
            }
        }
    }
}

public struct LinkedHybridPrefabData : IComponentData
{
    public ushort PrefabId;
    public bool EntityNeedsTransform;
}

public struct LinkedHybridInstanceData : ICleanupComponentData
{
    public int InstanceId;
}