using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HybridPrefab))]
public class HybridPrefabInspector : Editor
{
    private HybridPrefab _prefabAssetRef;
    
    public override void OnInspectorGUI()
    {
        HybridPrefab prefab = (HybridPrefab)target;

        if (GUILayout.Button("Open HybridPrefab Window"))
        {
            HybridPrefabWindow.OpenWindow();
        }
        
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Gather Callbacks"))
        {
            prefab.Callbacks = prefab.GetComponentsInChildren<PooledBehaviour>();
            EditorUtility.SetDirty(prefab);
        }
    }
}