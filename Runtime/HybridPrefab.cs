using UnityEngine;

public class HybridPrefab : MonoBehaviour
{
    public ushort PrefabId;
    public int DefaultPoolSize;
    public bool CopyTransformToPrefab;
    public Component[] ComponentsToAdd;
    public PooledBehaviour[] Callbacks;
}