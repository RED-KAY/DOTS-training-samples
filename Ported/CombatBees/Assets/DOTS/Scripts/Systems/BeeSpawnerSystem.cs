using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[UpdateInGroup(typeof(SpawnGroup))]
[UpdateBefore(typeof(BeeUpdateGroup))]

public class BeeSpawnerSystem : SystemBase
{
    private EntityCommandBufferSystem EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        EntityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = EntityCommandBufferSystem.CreateCommandBuffer();
        var random = Utils.GetRandom();

        var yellowBase = GetSingletonEntity<YellowBase>();
        var yellowBaseAABB = EntityManager.GetComponentData<Bounds>(yellowBase).Value;

        var blueBase = GetSingletonEntity<BlueBase>();
        var blueBaseAABB = EntityManager.GetComponentData<Bounds>(blueBase).Value;

        var arena = GetSingletonEntity<IsArena>();
        var arenaAABB = EntityManager.GetComponentData<Bounds>(arena).Value;

        var spawnerEntity = GetSingletonEntity<BeeSpawner>(); 
        var beeSpawner = GetComponent<BeeSpawner>(spawnerEntity);
        var numberOfBees = beeSpawner.BeeCountFromResource;

        var explosion = GetComponent<Explosion>(spawnerEntity);

        Entities
            .WithAll<IsResource, OnCollision>()
            .WithNone<LifeSpan>()
            .ForEach((Entity entity, in Translation translation) =>
            {
                var baseEntity = Entity.Null;
                
                if (yellowBaseAABB.Contains(translation.Value))
                {
                    baseEntity = yellowBase;
                }
                else if (blueBaseAABB.Contains(translation.Value))
                {
                    baseEntity = blueBase;
                }

                if (baseEntity != Entity.Null)
                {
                    var explosionInstance = ecb.Instantiate(explosion.ExplosionPrefab);
                    ecb.SetComponent(explosionInstance, new Translation
                    {
                        Value = translation.Value
                    });

                    ecb.AddComponent<LifeSpan>(explosionInstance, new LifeSpan
                    {
                        Value = 4f
                    });
                    

                    for (int i = 0; i < numberOfBees; ++i)
                    {
                        var instance = ecb.Instantiate(beeSpawner.BeePrefab);
                        ecb.SetComponent(instance, GetComponent<Team>(baseEntity));

                        var minSpeed = random.NextFloat(0, beeSpawner.MinSpeed);
                        var maxSpeed = random.NextFloat(0, beeSpawner.MaxSpeed);

                        var randomPointOnBase = Utils.BoundedRandomPosition(arenaAABB, ref random);


                        ecb.SetComponent(instance, new Velocity
                        {
                            Value = math.normalize(randomPointOnBase - translation.Value) * maxSpeed
                        });

                        ecb.SetComponent(instance, new TargetPosition
                        {
                            Value = randomPointOnBase
                        });

                        ecb.SetComponent(instance, new Speed
                        {
                            MaxSpeedValue = maxSpeed,
                            MinSpeedValue = minSpeed
                        });
                        
                        ecb.SetComponent(instance, new Translation
                        {
                            Value = translation.Value
                        });

                        ecb.SetComponent(instance, new URPMaterialPropertyBaseColor
                        {
                            Value = GetComponent<URPMaterialPropertyBaseColor>(baseEntity).Value
                        });

                        var aggression = random.NextFloat(0, 1);
                        ecb.SetComponent(instance, new Aggression
                        {
                            Value = aggression
                        });
                    }

                    ecb.AddComponent<LifeSpan>(entity);
                }
            }).Schedule();

        EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
