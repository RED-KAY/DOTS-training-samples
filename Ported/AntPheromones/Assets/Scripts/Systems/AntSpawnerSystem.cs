﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Core;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;


public class AntSpawnerSystem : SystemBase
{
    private EntityCommandBufferSystem bufferSystem;
    protected override void OnCreate()
    {
        bufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = bufferSystem.CreateCommandBuffer();

        var random = new Unity.Mathematics.Random(1234);

        Entities
            .WithAll<AntSpawner>()
            .ForEach((Entity entity, in AntSpawner spawner) =>
            {
                // Destroying the current entity is a classic ECS pattern,
                // when something should only be processed once then forgotten.
                ecb.DestroyEntity(entity);

                for (int i = 0; i < spawner.AntCount; ++i)
                {
                    var instance = ecb.Instantiate(spawner.AntPrefab);
                    var translation = new Translation { Value = new float3(0, 0, 0) };
                    var heading = new AntHeading { Degrees = random.NextFloat() * 360f };

                    ecb.SetComponent(instance, heading);
                    ecb.SetComponent(instance, translation);
                }
            }).Schedule();

        bufferSystem.AddJobHandleForProducer(Dependency);
    }
}
