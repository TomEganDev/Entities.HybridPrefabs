using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HybridPrefabWindow : EditorWindow
{
    private Dictionary<ushort, List<HybridPrefab>> _registry = new();

    [MenuItem("HybridPrefabs/Open Window")]
    public static void OpenWindow()
    {
        GetWindow<HybridPrefabWindow>();
    }
    
    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            FindPrefabs();
        }

        bool needsRefresh = false;
        if (RegistryHasConflicts() && GUILayout.Button("Fix all conflicts"))
        {
            FixAllConflicts();
            needsRefresh = true;
        }
        
        foreach (KeyValuePair<ushort, List<HybridPrefab>> kvp in _registry)
        {
            ushort id = kvp.Key;
            List<HybridPrefab> prefabs = kvp.Value;
            foreach (HybridPrefab prefab in prefabs)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"id:{id}");
                GUI.enabled = false;
                EditorGUILayout.ObjectField(prefab, typeof(HybridPrefab), false);
                GUI.enabled = true;
                if (prefabs.Count > 1 && GUILayout.Button("Fix conflict"))
                {
                    prefab.PrefabId = GetNextId();
                    EditorUtility.SetDirty(prefab);
                    needsRefresh = true;
                }
                GUILayout.EndHorizontal();
            }
        }

        if (needsRefresh)
        {
            FindPrefabs();
        }
    }

    private bool RegistryHasConflicts()
    {
        foreach (KeyValuePair<ushort, List<HybridPrefab>> kvp in _registry)
        {
            if (kvp.Value.Count > 1)
            {
                return true;
            }
        }

        return false;
    }

    private void FixAllConflicts()
    {
        FindPrefabs();

        HashSet<ushort> assignments = new HashSet<ushort>();
        List<HybridPrefab> needsNewIds = new List<HybridPrefab>();
        foreach (KeyValuePair<ushort, List<HybridPrefab>> kvp in _registry)
        {
            ushort id = kvp.Key;
            assignments.Add(kvp.Key);
            for (int i = 1; i < kvp.Value.Count; i++)
            {
                needsNewIds.Add(kvp.Value[i]);
            }
        }

        foreach (HybridPrefab prefab in needsNewIds)
        {
            ushort newId = GetNextId(assignments);
            prefab.PrefabId = newId;
            assignments.Add(newId);
            EditorUtility.SetDirty(prefab);
        }
    }
    
    private void FindPrefabs()
    {
        _registry.Clear();

        string[] prefabGuids = AssetDatabase.FindAssets("t:prefab");
        List<HybridPrefab> hybridPrefabs = new List<HybridPrefab>();
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            HybridPrefab hybridPrefab = prefab.GetComponent<HybridPrefab>();
            
            if (hybridPrefab == null)
            {
                continue;
            }
            
            hybridPrefabs.Add(hybridPrefab);
        }

        foreach (HybridPrefab prefab in hybridPrefabs)
        {
            bool isConflict = _registry.TryGetValue(prefab.PrefabId, out List<HybridPrefab> prefabsForId);
            if (!isConflict)
            {
                prefabsForId = new List<HybridPrefab>();
                _registry.Add(prefab.PrefabId, prefabsForId);
            }
            
            prefabsForId.Add(prefab);
        }
    }

    private ushort GetNextId()
    {
        if (_registry.Count == ushort.MaxValue)
        {
            Debug.LogError($"HybridPrefab limit of {ushort.MaxValue} reached, either prune assets or change id datatype");
            return 0;
        }
        
        ushort nextId = 0;
        while (_registry.ContainsKey(nextId))
        {
            nextId++;
        }

        return nextId;
    }

    private ushort GetNextId(HashSet<ushort> assigned)
    {
        if (assigned.Count == ushort.MaxValue)
        {
            Debug.LogError($"HybridPrefab limit of {ushort.MaxValue} reached, either prune assets or change id datatype");
            return 0;
        }

        ushort nextId = 0;
        while (assigned.Contains(nextId))
        {
            nextId++;
        }

        return nextId;
    }
}
