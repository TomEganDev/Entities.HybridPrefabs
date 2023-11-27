using Unity.Entities;
using UnityEngine;

public class HybridParticleBehaviour : PooledBehaviour
{
    public ParticleSystem RootParticleSystem;
    
    public override void OnPoolInstantiate(Entity entity, World world, EntityCommandBuffer commandBuffer)
    {
        RootParticleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
        RootParticleSystem.Play(withChildren: true);
    }
}