using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace AutoFarmers.Farm
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class FarmDrawingSystem : SystemBase
    {
        private Farm _Farm;
        private FarmDrawing m_FarmDrawing;

        private const int m_InstancesPerBatch = 1023;

        private Matrix4x4[][] m_FarmCells;
        private Matrix4x4[][] m_Rocks;
        private Matrix4x4[][] m_Farmers;

        private Vector3 m_FarmerScale = new Vector3(1, 2, 1);

        protected override void OnCreate()
        {
            RequireForUpdate<FarmDrawing>();
            RequireForUpdate<Farmer>();
        }

        protected override void OnStartRunning()
        {
            var query = GetEntityQuery(typeof(FarmDrawing));
            if(query.CalculateEntityCount() > 0)
            {
                m_FarmDrawing = EntityManager.GetSharedComponentManaged<FarmDrawing>(query.GetSingletonEntity());

                int totalCellCount = m_FarmDrawing.m_Size.x * m_FarmDrawing.m_Size.y;
                totalCellCount = GetEntityQuery(typeof(Tile)).CalculateEntityCount();

                m_FarmCells = new Matrix4x4[(int) math.ceil((float)totalCellCount / (float)m_InstancesPerBatch)][];

                for (int i = 0; i<m_FarmCells.Length; i++)
                {
                    m_FarmCells[i] = new Matrix4x4[math.min(m_InstancesPerBatch, totalCellCount - i * m_InstancesPerBatch)];
                }

                int r = 0, c = 0;
                //float x=0, z=m_Farm.m_Size.y-1;

                foreach (var tile in SystemAPI.Query<RefRO<Tile>>())
                {
                    m_FarmCells[r][c] = Matrix4x4.TRS(new UnityEngine.Vector3(tile.ValueRO.m_Position.x, 0f, tile.ValueRO.m_Position.z), Quaternion.Euler(90f, 0f, 0f), Vector3.one);

                    if (c < m_FarmCells[r].Length - 1)
                        c++;
                    else
                    {
                        c = 0;
                        if (r < m_FarmCells.Length - 1)
                            r++;
                        else
                            break;
                    }
                }

            }
        }


        private void UpdateRocksMatrices()
        {
            var rocksTotalCountJob = new RocksTotalCountJob
            {
                m_Count = new NativeReference<int>(Allocator.TempJob)
            };
            var jobHandle = rocksTotalCountJob.Schedule(GetEntityQuery(ComponentType.ReadOnly<Rock>()), Dependency);
            jobHandle.Complete();

            int totalRocks = rocksTotalCountJob.m_Count.Value;
            rocksTotalCountJob.m_Count.Dispose();
            //Debug.Log("totalRocks = " + totalRocks);
            m_Rocks = new Matrix4x4[(int)math.ceil((float)totalRocks / (float)m_InstancesPerBatch)][];

            for (int i = 0; i < m_Rocks.Length; i++)
            {
                m_Rocks[i] = new Matrix4x4[math.min(m_InstancesPerBatch, totalRocks - i * m_InstancesPerBatch)];
            }

            int r =0, c = 0;
            foreach (var rock in SystemAPI.Query<RefRO<Rock>>())
            {
                for (int i = 0; i < rock.ValueRO.m_BlobRef.Value.m_Rocks.Length; i++)
                {
                    Vector3 position = new Vector3(rock.ValueRO.m_BlobRef.Value.m_Rocks[i].m_Position.x, rock.ValueRO.m_BlobRef.Value.m_Rocks[i].m_Position.y, rock.ValueRO.m_BlobRef.Value.m_Rocks[i].m_Position.z);
                    //Debug.Log(position + ", " + rock.ValueRO.m_RockId);
                    m_Rocks[r][c] = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);

                    if (c < m_Rocks[r].Length - 1)
                    {
                        c++;
                    }
                    else
                    {
                        c = 0;
                        if (r < m_Rocks.Length - 1)
                        {
                            r++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        
        private void UpdateFarmerMatrices()
        {
            var query = GetEntityQuery(typeof(Farmer));
            int count = query.CalculateEntityCount();
            int len = (int)math.ceil((float)count / (float)m_InstancesPerBatch);
            m_Farmers = new Matrix4x4[len][];
            for (int i = 0; i < m_Farmers.Length; i++)
            {
                m_Farmers[i] = new Matrix4x4[math.min(m_InstancesPerBatch, count - i * m_InstancesPerBatch)];
            }

            Debug.Log("farmers len: " + m_Farmers.Length + ", len: " + len);
            int r=0; int c=0;
            foreach (var farmer in SystemAPI.Query<RefRO<Farmer>>())
            {
                Vector3 position = new Vector3(farmer.ValueRO.m_Position.x, farmer.ValueRO.m_Position.y+1, farmer.ValueRO.m_Position.z);

                m_Farmers[r][c] = Matrix4x4.TRS(position, Quaternion.identity, m_FarmerScale);

                if (c < m_Farmers[r].Length - 1)
                {
                    c++;
                }
                else
                {
                    c = 0;
                    if (r < m_Farmers.Length - 1)
                    {
                        r++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        protected override void OnUpdate()
        {

            UpdateRocksMatrices();
            UpdateFarmerMatrices();

            if (m_FarmCells != null && m_FarmCells.Length > 0)
            {
                for (int i = 0; i < m_FarmCells.Length; i++)
                {
                    Graphics.DrawMeshInstanced(m_FarmDrawing.m_GroundMesh, 0, m_FarmDrawing.m_GroundMaterial, m_FarmCells[i]);
                }

                for (int i = 0; i < m_Rocks.Length; i++)
                {
                    Graphics.DrawMeshInstanced(m_FarmDrawing.m_RockMesh, 0, m_FarmDrawing.m_RockMaterial, m_Rocks[i]);
                }

                for (int i = 0; i < m_Farmers.Length; i++)
                {
                    Graphics.DrawMeshInstanced(m_FarmDrawing.m_FarmerMesh, 0, m_FarmDrawing.m_FarmerMaterial, m_Farmers[i]);

                }
            }
        }
    }

    [BurstCompile]
    public partial struct RocksTotalCountJob : IJobEntity
    {
        public NativeReference<int> m_Count;
        private void Execute(ref Rock rock)
        {
            m_Count.Value += rock.m_BlobRef.Value.m_Rocks.Length;
        }
    }

    [BurstCompile]
    public partial struct FarmersTotalCountJob : IJobEntity
    {
        public NativeReference<int> m_Count;

        private void Execute(ref Farmer farmer)
        {
            m_Count.Value += 1;
        }
    }
}