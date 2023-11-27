using System.Collections.Generic;
using UnityEngine;

public class HybridPrefabRegistration : MonoBehaviour
{
    public List<HybridPrefab> Prefabs;
    
    private void Awake()
    {
        foreach (HybridPrefab prefab in Prefabs)
        {
            HybridPrefabPool.RegisterPrefab(prefab);
        }
    }
}