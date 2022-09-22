﻿using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
struct SpawnCommon
{
    public static void Spawn(int index, ref EntityCommandBuffer.ParallelWriter ECB, AABB Aabb, Entity Prefab, EntityQueryMask Mask, int InitFaction, LocalToWorldTransform InitTransform, float4 InitColor, out Entity entity)
    {
        entity = ECB.Instantiate(index, Prefab);
        var random = Random.CreateFromIndex((uint) index);

        var randomf3 = (random.NextFloat3() - new float3(0.5f, 0.5f, 0.5f)) * 2.0f; // [-1;1]

        float3 position = Aabb.Center + Aabb.Extents * randomf3;

        ECB.SetComponentEnabled<Dead>(index, entity, false);
        ECB.SetComponent(index, entity, new LocalToWorldTransform{Value = UniformScaleTransform.FromPositionRotationScale(position, InitTransform.Value.Rotation, InitTransform.Value.Scale)});
        ECB.AddSharedComponent(index, entity, new Faction{Value = InitFaction});
        ECB.AddComponentForLinkedEntityGroup(index, entity, Mask, new URPMaterialPropertyBaseColor { Value = InitColor});
    }
}

[BurstCompile]
unsafe struct FoodSpawnJob : IJobParallelFor
{
    // A regular EntityCommandBuffer cannot be used in parallel, a ParallelWriter has to be explicitly used.
    public EntityCommandBuffer.ParallelWriter ECB;
    public Entity Prefab;
    public AABB Aabb;
    public LocalToWorldTransform InitTransform;
    public EntityQueryMask Mask;
    public int InitFaction;
    public float4 InitColor;

    public void Execute(int index)
    {
        SpawnCommon.Spawn(index, ref ECB, Aabb, Prefab, Mask, InitFaction, InitTransform, InitColor, out _);
    }
}

[BurstCompile]
unsafe struct BeeSpawnJob : IJobParallelFor
{
    // A regular EntityCommandBuffer cannot be used in parallel, a ParallelWriter has to be explicitly used.
    public EntityCommandBuffer.ParallelWriter ECB;
    public Entity Prefab;
    public AABB Aabb;
    public LocalToWorldTransform InitTransform;
    public EntityQueryMask Mask;
    public int InitFaction;
    public float4 InitColor;

    public void Execute(int index)
    {
        SpawnCommon.Spawn(index, ref ECB, Aabb, Prefab, Mask, InitFaction, InitTransform, InitColor, out var entity);
        var random = Random.CreateFromIndex((uint)index);
        ECB.SetComponent<BeeProperties>(index, entity, new BeeProperties
        {
            Aggressivity = random.NextFloat(),
            BeeMode = BeeMode.Idle,
            Target = Entity.Null,
            TargetPosition = float3.zero,
        });
    }
}