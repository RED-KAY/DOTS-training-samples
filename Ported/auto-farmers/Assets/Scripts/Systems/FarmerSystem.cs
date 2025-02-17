namespace AutoFarmers.Farmer
{
    using AutoFarmers.Farm;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    [BurstCompile, UpdateAfter(typeof(FarmSystem))]
    public partial struct FarmerSystem : ISystem, ISystemStartStop
    {
        //TODO: 1. Farmer movement system.
        //TODO: 2. Spawning a Farmer.

        private Farm m_Farm;
        private Random m_Random;

        private EntityCommandBuffer _endSimSystemECB;

        private EntityQuery m_FarmersToMoveEQ, m_FarmersEQ;

        private int m_FarmerCount;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Farm>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            _endSimSystemECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var moveFarmerJob = new MoveFarmerJob
            {
                _ecb = _endSimSystemECB.AsParallelWriter()
            };
            var moveFarmerJobHandle = moveFarmerJob.ScheduleParallel(m_FarmersToMoveEQ, state.Dependency);

            foreach (var farmer in SystemAPI.Query<RefRO<Farmer>>())
            {

            }

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

            //CREATION OF ENTITY QUERIES
            var farmersToMoveCT = new NativeArray<ComponentType>(2, Allocator.Temp);
            farmersToMoveCT[0] = ComponentType.ReadWrite<Farmer>();
            farmersToMoveCT[1] = ComponentType.ReadOnly<FarmerMove>();
            m_FarmersToMoveEQ = state.GetEntityQuery(farmersToMoveCT);
            farmersToMoveCT.Dispose();

            var farmersCT = new NativeArray<ComponentType>(1, Allocator.Temp);
            farmersCT[0] = ComponentType.ReadOnly<Farmer>();
            m_FarmersEQ = state.GetEntityQuery(farmersCT);
            farmersCT.Dispose();
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
            state.EntityManager.AddComponentData(farmerEntity, new Farmer { m_Id = m_FarmerCount, m_CurrentTile = spawnTile, m_Position = spawnTile.m_Position });
            state.EntityManager.AddComponentData(farmerEntity, new FarmerStateMachine(){m_CurrentState = new FarmerState(){CurrentTypeId = FarmerState.TypeId.DoNothingState}});
            
            state.EntityManager.AddBuffer<FarmerState>(farmerEntity);
            var stateBuffer = state.EntityManager.GetBuffer<FarmerState>(farmerEntity, false);

            stateBuffer.Add(new DoNothingState());
            stateBuffer.Add(new MovingState() { m_Speed = 2f, m_StoppingDistance = 0.5f});
            stateBuffer.Add(new RockMiningState() { m_Damage = 3, m_HitRate = 2f});
            
            m_FarmerCount++;

            return farmerEntity;
        }

        public void OnStopRunning(ref SystemState state)
        {

        }
    }

    
    public struct FirstFarmer : IComponentData { }

    public struct Farmer : IComponentData
    {
        public int m_Id;
        public float3 m_Position;
        public Tile m_CurrentTile;
        public Tile m_TargetTile;
    }

    [InternalBufferCapacity(0)]
    public partial struct FarmerState : IBufferElementData
    {
    }
    
    public struct FarmerStateMachine : IComponentData
    {
        public FarmerState m_CurrentState;
       
        public bool IsInitialized;
        public int TransitionToStateIndex; 
    }
    
    public struct FarmerMove : IComponentData
    {
        public float3 _Location;
    }

    [BurstCompile]
    public partial struct MoveFarmerJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter _ecb;

        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int chunkIndexInQuery, Entity entity, ref Farmer farmer, ref FarmerMove farmerMove)
        {
            float3 rel = (farmerMove._Location - farmer.m_Position);
            farmer.m_Position += math.normalize(rel) * 2f;
            float distance = math.length(rel);

            if (distance >= 0.2f) {
                
                _ecb.RemoveComponent(chunkIndexInQuery, entity, ComponentType.ReadOnly<FarmerMove>());
            }
        }
    }
}
