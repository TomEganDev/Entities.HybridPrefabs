using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HybridPrefabRegistration))]
public class HybridPrefabRegistrationInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Gather all"))
        {
            HybridPrefabRegistration registration = (HybridPrefabRegistration)target;
            registration.Prefabs.Clear();
            
            string[] guids = AssetDatabase.FindAssets("t:prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                HybridPrefab hybridPrefab = prefab.GetComponent<HybridPrefab>();
                if (hybridPrefab != null)
                {
                    registration.Prefabs.Add(hybridPrefab);
                }
            }
            
            EditorUtility.SetDirty(registration);
        }

        if (GUILayout.Button("Open HybridPrefab Window"))
        {
            HybridPrefabWindow.OpenWindow();
        }
        
        base.OnInspectorGUI();
    }
}