using System.Diagnostics;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using Debug = UnityEngine.Debug;

namespace AutoFarmers.Farm
{

    [BurstCompile, UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct FarmSystem : ISystem, ISystemStartStop
    {
        private Farm m_Farm;
        private Farmer m_FirstFarmer;
        private NativeArray<Farmer> m_Farmers;
        private NativeArray<Tile> m_Tiles;

        private Random m_Random;

        private int m_TotalTileCount;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Farm>();


        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            m_Farm = SystemAPI.GetSingleton<Farm>();
            m_Random = Random.CreateFromIndex(123);

            m_TotalTileCount = m_Farm.m_Size.x * m_Farm.m_Size.y;
            m_Tiles = new NativeArray<Tile>(m_TotalTileCount, Allocator.Persistent);

            for (int i = 0; i < m_Farm.m_Size.x; i++)
            {
                for (int j = 0; j < m_Farm.m_Size.y; j++)
                {
                    var entity = state.EntityManager.CreateEntity();
                    int tileId = i * m_Farm.m_Size.y + j;
                    var tile = new Tile()
                    {
                        m_Id = tileId,
                        m_Position = new float3(j, 0f, -i),
                        m_Status = TileStatus.None
                    };
                    state.EntityManager.AddComponentData(entity, tile);
                    m_Tiles[tileId] = tile;
                }
            }

            //SetTileStatus(2, TileStatus.Planted, ref state);

            NativeList<int> rockTileIds;


            int currentRockPercent = GetCurrentRockPercent(ref state);
            int rockId = 0;
            while (currentRockPercent < m_Farm.m_RockPercentage)
            {
                int width = m_Random.NextInt(1, 5);
                int height = m_Random.NextInt(1, 5);
                int rockX = m_Random.NextInt(0, m_Farm.m_Size.x - width);
                int rockY = m_Random.NextInt(0, m_Farm.m_Size.y - height);

                bool notValid = false;

                int totalTiles = width * height;
                rockTileIds = new NativeList<int>(totalTiles, Allocator.Temp);

                for (int index = 0; index < totalTiles; index++)
                {
                    int i = rockX + (index / height); 
                    int j = rockY + (index % height);
                    int tileId = i * m_Farm.m_Size.y + j;
                    var getTileStatusJob = new GetTileJob()
                    {
                        m_TileIdToSearch = tileId,
                        m_Handle = state.EntityManager.GetComponentTypeHandle<Tile>(true),
                        m_Found = new NativeReference<bool>(Allocator.Domain),
                        m_Status = new NativeReference<TileStatus>(Allocator.Domain),
                        m_Tile = new NativeReference<Tile>(Allocator.Domain)
                    };

                    var entityQuery = state.GetEntityQuery(ComponentType.ReadOnly<Tile>());
                    var jobHandle = getTileStatusJob.ScheduleParallel(entityQuery, state.Dependency);
                    state.Dependency = jobHandle;
                    //var jobHandle = GetTile(tileId, out GetTileJob getTileStatusJob, ref state);
                    jobHandle.Complete();

                    bool found = getTileStatusJob.m_Found.Value;
                    TileStatus status = getTileStatusJob.m_Status.Value;
                    getTileStatusJob.m_Found.Dispose();
                    getTileStatusJob.m_Status.Dispose();
                    getTileStatusJob.m_Tile.Dispose();
                    if (found && status == TileStatus.None)
                    {
                        rockTileIds.Add(tileId);
                    }
                    else
                    {
                        notValid = true;
                        break;
                    }

                }

                if (!notValid)
                {
                    var rock = new Rock {
                        m_RockId = rockId,
                        m_Scale = new float3(1, 0.25f, 1),
                        m_Health = 30f
                    };

                    var builder = new BlobBuilder(Allocator.Temp);
                    ref PerRockInfoAsset perRockInfo = ref builder.ConstructRoot<PerRockInfoAsset>();
                    BlobBuilderArray<PerRockInfo> arrayBuilder = builder.Allocate(ref perRockInfo.m_Rocks, rockTileIds.Length);

                    float3 center = float3.zero;
                    int i = 0;
                    //TODO: optimize these 2 loops!
                    foreach (var tileId in rockTileIds)
                    {
                        var setTileStatusJobHandle = SetTileStatus(tileId, TileStatus.Rock, out SetTileStatusJob setTileStatusJob, ref state);
                        state.Dependency = setTileStatusJobHandle;
                        setTileStatusJobHandle.Complete();

                        foreach (var tile in SystemAPI.Query<RefRW<Tile>>())
                        {
                            if(tile.ValueRO.m_Id == tileId)
                            {
                                arrayBuilder[i] = new PerRockInfo
                                {
                                    m_Position = tile.ValueRO.m_Position + new float3(0, 0.25f, 0f),
                                    m_TileId = tileId
                                };

                                center.x += tile.ValueRO.m_Position.x;
                                center.y += tile.ValueRO.m_Position.y;
                                center.z += tile.ValueRO.m_Position.z;

                                //Debug.Log("pos: " + arrayBuilder[i].m_Position + " tile: " + arrayBuilder[i].m_TileId);
                                i++;
                            }
                        }
                        //Debug.Log("tileId:" + tileId);
                    }

                    center = center / rockTileIds.Length;

                    rock.m_BlobRef = builder.CreateBlobAssetReference<PerRockInfoAsset>(Allocator.Persistent);
                    rock.m_Center = center;
                    builder.Dispose();
                    var entity = state.EntityManager.CreateEntity();
                    state.EntityManager.AddComponentData(entity, rock);
                    rockId++;
                }
                rockTileIds.Dispose();

                currentRockPercent = GetCurrentRockPercent(ref state);
            }
        }


        [BurstCompile]
        private JobHandle SetTileStatus(int targetTileId, TileStatus statusToSet, out SetTileStatusJob setTileJob, ref SystemState state)
        {
            setTileJob = new SetTileStatusJob()
            {
                m_TargetTileID = targetTileId,
                m_StatusToSet = statusToSet,
                m_Handle = state.EntityManager.GetComponentTypeHandle<Tile>(false),
            };

            var entityQuery = state.GetEntityQuery(ComponentType.ReadOnly<Tile>());

            //var handle = getTileJob.ScheduleParallel(state.EntityManager.CreateEntityQuery(typeof(Tile)), state.Dependency);
            var jobHandle = setTileJob.ScheduleParallel(entityQuery, state.Dependency);
            return jobHandle;
        }

        private int GetCurrentRockPercent(ref SystemState state)
        {
            int currentPercent = 0;
            float currentCoverage = GetCurrentRockCoverage(ref state);
            currentPercent = (int)math.ceil((currentCoverage * 100) / m_TotalTileCount);
            return currentPercent;
        }

        private int GetCurrentRockCoverage(ref SystemState state)
        {
            int currentCoverage = 0;
            foreach (var tileEntity in SystemAPI.Query<RefRO<Tile>>())
            {
                if (tileEntity.ValueRO.m_Status != TileStatus.Rock) continue;

                currentCoverage++;
            }
            return currentCoverage;
        }


        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            m_Tiles.Dispose();
            //m_Farmers.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var damageRocksjob = new DamageRocksJob()
            {
                _ecb = ecb.AsParallelWriter()
            };

            var handle = damageRocksjob.ScheduleParallel(state.Dependency);
            state.Dependency = handle;
            handle.Complete();
        }

        public void OnStopRunning(ref SystemState state)
        {

        }
    }


    public enum TileStatus
    {
        None,
        Tilled,
        Planted,
        Rock,
        Silo
    }

    public struct PerRockInfo
    {
        public int m_TileId;
        public float3 m_Position;
    }

    public struct PerRockInfoAsset
    {
        public BlobArray<PerRockInfo> m_Rocks;
    }

    public struct Rock : IComponentData
    {
        public int m_RockId;
        public float3 m_Center;
        //public NativeArray<int> m_TileIds;
        public float m_Health;
        //public NativeArray<float3> m_Positions;
        public float3 m_Scale;
        public BlobAssetReference<PerRockInfoAsset> m_BlobRef;
    }

    public struct RockHitTag : IComponentData {
        public bool m_Hit;
    }

    public struct RockRemoveTag : IComponentData { }

    public struct Tile : IComponentData
    {
        public int m_Id;
        public float3 m_Position;
        public TileStatus m_Status;
        public int m_RockEntityId;
    }

    public struct Farmer : IComponentData
    {
        public float3 m_Position;
        public Tile m_CurrentTile;
        public Tile m_TargetTile;
    }

    [BurstCompile]
    public partial struct DamageRocksJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter _ecb;

        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int chunkIndexInQuery, Entity entity, ref Rock rock, ref RockHitTag rockHitTag)
        {
            if (!rockHitTag.m_Hit) return;

            rock.m_Health -= 30f;
            rockHitTag.m_Hit = false;

            if (rock.m_Health <= 0f)
            {
                rock.m_Health = 0f;
                //state.EntityManager.AddComponent<RockRemoveTag>(entity);
                //this.
                //_ecb.AddComponent<RockRemoveTag>(chunkIndexInQuery, entity);
                _ecb.DestroyEntity(chunkIndexInQuery, entity);
            }
        }
    }

    [BurstCompile]
    public struct SetTileStatusJob : IJobChunk
    {
        public int m_TargetTileID;
        public ComponentTypeHandle<Tile> m_Handle;
        internal TileStatus m_StatusToSet;

        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var components = chunk.GetNativeArray(ref m_Handle);

            for (int i = 0; i < components.Length; i++)
            {
                var tile = components[i];
                if (tile.m_Id == m_TargetTileID)
                {
                    //m_FoundTile[0] = tile;
                    tile.m_Status = m_StatusToSet;
                    components[i] = tile;
                    break;
                }

            }
        }
    }

    [BurstCompile]
    public struct GetTileJob : IJobChunk
    {
        [ReadOnly] public ComponentTypeHandle<Tile> m_Handle;
        public int m_TileIdToSearch;
        [NativeDisableParallelForRestriction] public NativeReference<bool> m_Found;
        [NativeDisableParallelForRestriction] public NativeReference<TileStatus> m_Status;
        [NativeDisableParallelForRestriction] public NativeReference<Tile> m_Tile;

        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var components = chunk.GetNativeArray(ref m_Handle);
            for (int i = 0; i < components.Length; i++)
            {
                var tile = components[i];
                if (tile.m_Id == m_TileIdToSearch)
                {
                    m_Tile.Value = tile;
                    m_Found.Value = true;
                    m_Status.Value = tile.m_Status;
                    return;
                }

            }
        }
    }
}