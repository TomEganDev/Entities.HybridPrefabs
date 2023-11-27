using System.Collections.Generic;
using Unity.Assertions;
using UnityEngine;
// ReSharper disable Unity.BurstLoadingManagedType
// ReSharper disable Unity.BurstAccessingManagedIndexer
// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

public static class HybridPrefabPool
{
    private static Dictionary<ushort, HybridPrefab> _prefabLookup = new();
    private static Dictionary<ushort, Stack<HybridPrefab>> _poolLookup = new();
    private static Dictionary<int, HybridPrefab> _activeInstances = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _prefabLookup.Clear();
        _poolLookup.Clear();
        _activeInstances.Clear();
    }

    public static void OnRestart()
    {
        Initialize();
    }

    public static void RegisterPrefab(HybridPrefab prefab)
    {
        Assert.IsNotNull(prefab, "HybridPrefab was null!");

        if (_prefabLookup.TryGetValue(prefab.PrefabId, out HybridPrefab duplicatePrefab))
        {
            if (duplicatePrefab == prefab)
            {
                Debug.LogWarning($"Duplicate prefab registration for {prefab.PrefabId} ignored");
            }
            else
            {
                Debug.LogError($"HybridPrefab.Id {prefab.PrefabId} collision!");
                Debug.LogError($"{duplicatePrefab.PrefabId}:{duplicatePrefab.name} - registered", duplicatePrefab);
                Debug.LogError($"{prefab.PrefabId}:{prefab.name} - conflict", prefab);
            }

            return;
        }
        
        _prefabLookup.Add(prefab.PrefabId, prefab);
        
        // pre-allocate pool
        Stack<HybridPrefab> pool = new Stack<HybridPrefab>(prefab.DefaultPoolSize);
        _poolLookup.Add(prefab.PrefabId, pool);
        for (int i = 0; i < prefab.DefaultPoolSize; i++)
        {
            HybridPrefab instance = Object.Instantiate(prefab);
#if UNITY_EDITOR
            instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
            instance.gameObject.SetActive(false);
            pool.Push(instance);
        }
    }

    public static HybridPrefab GetInstance(ushort prefabId)
    {
        Assert.IsTrue(_prefabLookup.ContainsKey(prefabId), $"HybridPrefab {prefabId} was not registered");

        Stack<HybridPrefab> pool = _poolLookup[prefabId];
        HybridPrefab instance;
        if (pool.Count > 0)
        {
            instance = pool.Pop();
            instance.gameObject.SetActive(true);
        }
        else
        {
            HybridPrefab prefab = _prefabLookup[prefabId];
            instance = Object.Instantiate(prefab);
        }
        
#if UNITY_EDITOR
        instance.gameObject.hideFlags = HideFlags.None;
#endif

        _activeInstances.Add(instance.GetInstanceID(), instance);
        
        return instance;
    }

    public static void ReturnInstance(int instanceId)
    {
        Assert.IsTrue(_activeInstances.ContainsKey(instanceId), $"Returning untracked instance {instanceId}");

        HybridPrefab instance = _activeInstances[instanceId];
        _activeInstances.Remove(instanceId);
        Stack<HybridPrefab> pool = _poolLookup[instance.PrefabId];
        
#if UNITY_EDITOR
        instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
        instance.gameObject.SetActive(false);
        pool.Push(instance);
    }
}