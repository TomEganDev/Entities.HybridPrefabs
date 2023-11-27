# Hybrid Prefabs

## Setup

Add this package by using the 'Add package from git URL...' + menu option from the Unity package manager.

```json
https://github.com/TomEganDev/Entities.HybridPrefabs.git
```

## How To Use

See included samples or...

1. Create GameObject prefab with HybridPrefab component
2. Create Entity prefab with LinkHybridPrefab authoring component that references the hybrid prefab
3. Add a HybridPrefabCollection to your scene that contains all hybrid prefabs
4. Entities with LinkHybridPrefab will now automatically pair themselves to a hybrid prefab instance when instantiated, the hybrid prefab will also be returned to the pool when the entity is destroyed


## Hybrid Prefabs
![Hybrid Prefab Component Preview](./Documentation~/images/HybridPrefabs.png)
#### PrefabId
A unique ushort identifier. These can be manually managed and assigned or you can use the HybridPrefab window (button at top of inspector or via toolbar at HybridPrefabs > Open Window)
#### DefaultPoolSize
How many instances of this hybrid prefab get pre-allocated when calling HybridPrefabPool.RegisterPrefab() (This is called by the HybridPrefabCollection component).
#### CopyTransformToPrefab
Adds a CopyTransformToGameObject IComponentData to the linked entity, this makes the HybridPrefab instance follow the entities transform.
#### ComponentsToAdd
An array of all components that will be added to the linked entity as component object references. This allows systems to process Monobehaviour components.
#### Callbacks
An array of all PooledBehaviours that need to have their callbacks run. Currently this is used to run OnPoolInstantiate(Entity, World, EntityCommandBuffer) which should be used for initialization behaviour.
Using the 'Gather Instantiate callbacks' button gets all PooledBehaviours in the prefab hierarchy into this array using GetComponentsInChildren.


### PooledBehaviour

PooledBehaviour is an abstract component that inherits IOnPoolInstantiate so it receives the OnPoolInstantiate(Entity, World) callback. Create new components inheriting from Pooledbehaviour to implement custom initialization behaviour such as resetting animations or storing entity references in Monobehaviour components.


### HybridPrefabCollection

This is a simple component that calls HybridPrefabPool.RegisterPrefab on every HybridPrefab in HybridPrefabCollection.Prefabs during Awake(). All HybridPrefabs must be registered before they can be used.