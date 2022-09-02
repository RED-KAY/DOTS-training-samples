using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class ParticleBloodSettledAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Lifetime = 1f;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ParticleSize { Size = transform.localScale });
        dstManager.AddComponentData(entity, new NonUniformScale { Value = 1f});
        dstManager.AddComponentData(entity, new ParticleLifetime { MaxLifetime = Lifetime });
        dstManager.AddComponentData(entity, new ParticleShrinkOverLifetime {  });
    }
}
