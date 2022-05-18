using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
partial struct BucketSpawningSystem : ISystem
{

    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<Config>();
    }
    
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var tileGridConfig = SystemAPI.GetSingleton<TileGridConfig>();
        var config = SystemAPI.GetSingleton<Config>();

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        var allocator = state.WorldUnmanaged.UpdateAllocator.ToAllocator;
        var buckets = CollectionHelper.CreateNativeArray<Entity>(config.bucketCount, allocator);
        ecb.Instantiate(config.bucketPrefab, buckets);

        var random = new Random((uint)UnityEngine.Random.Range(1, 100000));
        foreach (var bucket in buckets)
        {
            var randomRow = random.NextInt(0, tileGridConfig.Size);
            var randomColumn = random.NextInt(0, tileGridConfig.Size);
            
            ecb.SetComponent(bucket, new Translation { Value = new float3(randomRow * tileGridConfig.CellSize, 0, randomColumn * tileGridConfig.CellSize) });
        }
        
        //only meant to run once, so disable afterwards. 
        state.Enabled = false;
    }
}