using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace AutoFarmers.Farm
{
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(FarmSystem))]
    public partial class FarmDrawingSystem : SystemBase
    {
        private FarmDrawing m_Farm;

        private const int m_InstancesPerBatch = 1023;

        private Matrix4x4[][] m_FarmCells;

        protected override void OnCreate()
        {
            RequireForUpdate<FarmDrawing>();
        }

        protected override void OnStartRunning()
        {
            var query = GetEntityQuery(typeof(FarmDrawing));
            if(query.CalculateEntityCount() > 0)
            {
                m_Farm = EntityManager.GetSharedComponentManaged<FarmDrawing>(query.GetSingletonEntity());

                int totalCellCount = m_Farm.m_Size.x * m_Farm.m_Size.y;
                totalCellCount = GetEntityQuery(typeof(Tile)).CalculateEntityCount();

                m_FarmCells = new Matrix4x4[(int) math.ceil((float)totalCellCount / (float)m_InstancesPerBatch)][];

                for (int i = 0; i<m_FarmCells.Length; i++)
                {
                    m_FarmCells[i] = new Matrix4x4[math.min(m_InstancesPerBatch, totalCellCount - i * m_InstancesPerBatch)];
                }

                int r = 0, c = 0;
                float x=0, z=m_Farm.m_Size.y-1;

                foreach (var tile in SystemAPI.Query<RefRO<Tile>>())
                {
                    m_FarmCells[r][c] = Matrix4x4.TRS(new UnityEngine.Vector3(tile.ValueRO.m_Position.x, 0f, tile.ValueRO.m_Position.z), Quaternion.Euler(90f, 0f, 0f), Vector3.one);

                    if (c < m_FarmCells[r].Length - 1)
                    {
                        c++;
                    }
                    else
                    {
                        c = 0;
                        if (r < m_FarmCells.Length - 1)
                        {
                            r++;
                        }
                        else
                        {
                            break;
                        }
                    }


                    if (x < m_Farm.m_Size.x - 1)
                    {
                        x++;
                    }
                    else
                    {
                        x = 0;
                        if (z > 0)
                        {
                            z--;
                        }
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            if(m_FarmCells != null && m_FarmCells.Length > 0)
            {
                for (int i = 0; i < m_FarmCells.Length; i++)
                {
                    Graphics.DrawMeshInstanced(m_Farm.m_GroundMesh, 0, m_Farm.m_GroundMaterial, m_FarmCells[i]);
                }
            }
        }
    }
}