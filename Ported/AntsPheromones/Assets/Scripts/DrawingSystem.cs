using AntPheromones.Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace AntPheromones.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class DrawingSystem : SystemBase
    {
        Texture2D pheromoneTexture;
        Material myPheromoneMaterial;

        Color[] pheromones;
        Matrix4x4[][] matrices;
        private MaterialPropertyBlock[] matProps;
        Vector4[][] antColors;
        private Config m_Config;
        private EntityQuery m_SharedComponentQuery;
        private DrawingDataTest m_DrawingDataTest;
        private Random m_Random;
        private Matrix4x4[][] m_ObstacleMatrices;
        private const int m_InstancesPerBatch = 1023;

        private Matrix4x4 colonyMatrix;
        private Matrix4x4 resourceMatrix;

        protected override void OnCreate()
        {
            RequireForUpdate<Config>();
        }

        protected override void OnStartRunning()
        {
            if (SystemAPI.TryGetSingleton<Config>(out m_Config))
            {
                m_SharedComponentQuery = GetEntityQuery(typeof(DrawingDataTest));
                if (m_SharedComponentQuery.CalculateEntityCount() > 0)
                {
                    Entity entity = m_SharedComponentQuery.GetSingletonEntity();
                    m_DrawingDataTest = EntityManager.GetSharedComponentManaged<DrawingDataTest>(entity);
                }
                m_Random = new Random(123);

                CreateBatches();
                InitializeMaterialsAndTextures();
            }
            else
            {
                Debug.LogError("Config component not found. Ensure it is added to an entity.");
            }
        }

        private void CreateBatches()
        {
            var entityQuery = GetEntityQuery(typeof(Obstacle));
            int obstacleCount = entityQuery.CalculateEntityCount();
            if (obstacleCount > 0)
            {   
                int length = (int)math.ceil((float)obstacleCount / (float)m_InstancesPerBatch);
                //Debug.Log("m_ObstacleMatrices Length: " + length);

                m_ObstacleMatrices = new Matrix4x4[length][];
                int i = 0;
                for (i = 0; i < m_ObstacleMatrices.Length; i++)
                {
                    m_ObstacleMatrices[i] = new Matrix4x4[Mathf.Min(m_InstancesPerBatch, obstacleCount - i * m_InstancesPerBatch)];
                }

                i = 0;
                int j = 0;
                foreach (var obstacle in SystemAPI.Query<RefRO<Obstacle>>())
                {
                    if(j >= m_ObstacleMatrices[i].Length)
                    {
                        if(i < m_ObstacleMatrices.Length-1){
                            i++;
                            j=0;
                        }
                        else {
                            Debug.LogError("i exceeding the length of m_ObstacleMatrices!");
                            break;
                        }
                    }
                    m_ObstacleMatrices[i][j] = Matrix4x4.TRS(new Vector3(obstacle.ValueRO.m_Position.x, obstacle.ValueRO.m_Position.y, .0f), obstacle.ValueRO.m_Rotation, obstacle.ValueRO.m_Scale);
                    j++;
                }
            }


            Colony colony = SystemAPI.GetSingleton<Colony>();
            colonyMatrix = Matrix4x4.TRS(new Vector3(colony.m_Position.x, colony.m_Position.y, 0f), colony.m_Rotation, colony.m_Scale);

            Resource resource = SystemAPI.GetSingleton<Resource>();
            resourceMatrix = Matrix4x4.TRS(new Vector3(resource.m_Position.x, resource.m_Position.y, 0f), resource.m_Rotation, resource.m_Scale);
        }

        private void InitializeMaterialsAndTextures()
        {
            int mapSize = m_Config.m_MapSize;
            int antCount = m_Config.m_AntCount;
            int instancesPerBatch = m_InstancesPerBatch;
            Renderer pheromoneRenderer = m_DrawingDataTest.m_PheromoneRenderer;
            Material basePheromoneMaterial = m_DrawingDataTest.m_BasePheromoneMaterial;

            pheromoneTexture = new Texture2D(mapSize, mapSize);
            pheromoneTexture.wrapMode = TextureWrapMode.Mirror;
            myPheromoneMaterial = new Material(basePheromoneMaterial);
            myPheromoneMaterial.mainTexture = pheromoneTexture;
            pheromoneRenderer.sharedMaterial = myPheromoneMaterial;

            pheromones = new Color[mapSize * mapSize];
            matrices = new Matrix4x4[Mathf.CeilToInt((float)antCount / instancesPerBatch)][];
            for (int i = 0; i < matrices.Length; i++)
            {
                if (i < matrices.Length - 1)
                {
                    matrices[i] = new Matrix4x4[instancesPerBatch];
                }
                else
                {
                    matrices[i] = new Matrix4x4[antCount - i * instancesPerBatch];
                }
            }
            matProps = new MaterialPropertyBlock[matrices.Length];
            antColors = new Vector4[matrices.Length][];
            for (int i = 0; i < matProps.Length; i++)
            {
                antColors[i] = new Vector4[matrices[i].Length];
                matProps[i] = new MaterialPropertyBlock();
            }
        }

        protected override void OnUpdate()
        {
            Debug.Log("Drawing System updating... ");
            // for (int i = 0; i < matrices.Length; i++)
            // {
            //     Graphics.DrawMeshInstanced(m_DrawingDataTest.m_AntMesh, 0, m_DrawingDataTest.m_AntMaterial, matrices[i], matrices[i].Length, matProps[i]);
            // }

            if (m_ObstacleMatrices != null || m_ObstacleMatrices.Length > 0)
            {
                for (int i = 0; i < m_ObstacleMatrices.Length; i++)
                {
                    
                    Graphics.DrawMeshInstanced(m_DrawingDataTest.m_ObstacleMesh, 0, m_DrawingDataTest.m_ObstacleMaterial, m_ObstacleMatrices[i]);
                }
            }

            
            Graphics.DrawMesh(m_DrawingDataTest.m_ColonyMesh, colonyMatrix, m_DrawingDataTest.m_ColonyMaterial, 0);
            Graphics.DrawMesh(m_DrawingDataTest.m_ResourceMesh, resourceMatrix, m_DrawingDataTest.m_ResourceMaterial, 0);
        }
    }
}