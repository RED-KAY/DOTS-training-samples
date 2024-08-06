using AutoFarmers.Farm;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateAfter(typeof(FarmSystem))]
public partial struct FarmerSystem : ISystem, ISystemStartStop
{
    //TODO: 1. Farmer movement system.
    //TODO: 2. Spawning a Farmer.

    private Farm m_Farm;
    private Random m_Random;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Farm>();
    }
    
    public void OnUpdate(ref SystemState state)
    {

    }

    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnStartRunning(ref SystemState state)
    {
        m_Farm = SystemAPI.GetSingleton<Farm>();
        m_Random = Random.CreateFromIndex(123);

        //SPAWNING FIRST FARMER
        var farmerEntity = SpawnFarmer(ref state);
        state.EntityManager.AddComponent<FirstFarmer>(farmerEntity);
    }

    private Entity SpawnFarmer(ref SystemState state)
    {
        var getEmptyTilesJob = new GetEmptyTiles
        {
            m_EmptyTiles = new NativeList<Tile>(Allocator.TempJob)
        };
        var getEmptyTilesJobHandle = getEmptyTilesJob.Schedule(state.Dependency);
        getEmptyTilesJobHandle.Complete();

        Tile spawnTile = getEmptyTilesJob.m_EmptyTiles[m_Random.NextInt(0, getEmptyTilesJob.m_EmptyTiles.Length)];
        var farmerEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(farmerEntity, new Farmer { m_CurrentTile = spawnTile, m_Position = spawnTile.m_Position });
        return farmerEntity;
    }

    public void OnStopRunning(ref SystemState state)
    {
        
    }
}