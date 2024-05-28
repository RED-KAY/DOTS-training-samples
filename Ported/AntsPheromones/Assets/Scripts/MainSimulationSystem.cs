using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using AntPheromones.Authoring;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using System.Diagnostics;
//using Debug = UnityEngine.Debug;
using System.Collections.Generic;

namespace AntPheromones.Systems
{
    [BurstCompile]
    public partial struct MainSimulationSystem : ISystem, ISystemStartStop
    {
        private Config m_Config;
        private Random m_Random;

        private NativeArray<Obstacle> obstacles;
        private NativeArray<NativeArray<Obstacle>> obstacleBuckets;

        float2 colonyPosition;
        float2 resourcePosition;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            //Debug.Log("OnCreate...");
            state.RequireForUpdate<Config>();

        }

        public void OnStartRunning(ref SystemState state)
        {
            //Debug.Log("OnStartRunning...");
            //Debug.Log("OnStartRunning...");
            m_Config = SystemAPI.GetSingleton<Config>();
            m_Random = new Random(123);

            Init(ref state);
            GenerateObstacles(ref state);
            SetupColonyAndResource(ref state);
        }

        public void OnStopRunning(ref SystemState state)
        {
            for (int i = 0; i < obstacleBuckets.Length; i++)
            {
                if (obstacleBuckets[i].IsCreated)
                {
                    obstacleBuckets[i].Dispose();
                }
            }
            obstacleBuckets.Dispose();

            obstacles.Dispose();
            ants.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityManager entityManager = state.EntityManager;
        }

        private void GenerateObstacles(ref SystemState state)
        {
            NativeList<Obstacle> output = new NativeList<Obstacle>(Allocator.Temp);

            // List<Obstacle> output = new List<Obstacle>();
            int obstacleRingCount = m_Config.m_ObstacleRingCount;
            int mapSize = m_Config.m_MapSize;
            float obstacleRadius = m_Config.m_ObstacleRadius;
            float obstaclesPerRing = m_Config.m_ObstaclesPerRing;
            int bucketResolution = m_Config.m_BucketResolution;

            for (int i = 1; i <= obstacleRingCount; i++)
            {
                float ringRadius = (i / (obstacleRingCount + 1f)) * (mapSize * .5f);
                float circumference = ringRadius * 2f * math.PI;
                int maxCount = (int)math.ceil(circumference / (2f * obstacleRadius) * 2f);
                int offset = m_Random.NextInt(0, maxCount);

                int holeCount = m_Random.NextInt(1, 3);
                for (int j = 0; j < maxCount; j++)
                {
                    float t = (float)j / maxCount;
                    if ((t * holeCount) % 1f < obstaclesPerRing)
                    {
                        float angle = (j + offset) / (float)maxCount * (2f * math.PI);

                        float2 pos = new float2(mapSize * .5f + math.cos(angle) * ringRadius, mapSize * .5f + math.sin(angle) * ringRadius);
                        Obstacle obstacle = new Obstacle
                        {
                            m_Position = pos / mapSize,
                            m_Scale = new float3(m_Config.m_ObstacleRadius * 2f, m_Config.m_ObstacleRadius * 2f, 1f) / mapSize,
                            m_Rotation = quaternion.identity,
                            m_Radius = obstacleRadius,
                        };
                        //Debug.Log("output created? " + output.IsCreated);
                        if (!output.IsCreated)
                        {
                            output = new NativeList<Obstacle>(Allocator.Temp);
                        }
      
                        output.Add(obstacle);
                        var entity = state.EntityManager.CreateEntity();
                        state.EntityManager.AddComponentData(entity, obstacle);

                        //Debug.DrawRay(obstacle.position / mapSize,-Vector3.forward * .05f,Color.green,10000f);
                    }
                }
            }

            obstacles = new NativeArray<Obstacle>(output.Length, Allocator.Persistent);
            obstacles = output.ToArray(Allocator.Persistent);

            output.Dispose();

            NativeArray<NativeList<Obstacle>> tempObstacleBuckets = new NativeArray<NativeList<Obstacle>>(bucketResolution * bucketResolution, Allocator.Temp);

            for (int i = 0; i < bucketResolution * bucketResolution; i++)
            {
                tempObstacleBuckets[i] = new NativeList<Obstacle>(Allocator.Temp);
            }

            for (int i = 0; i < obstacles.Length; i++)
            {
                float2 pos = obstacles[i].m_Position;
                float radius = obstacles[i].m_Radius;
                for (int x = (int)math.floor((pos.x - radius) / (float)mapSize * (float)bucketResolution); x <= (int)math.floor((pos.x + radius) / (float)mapSize * (float)bucketResolution); x++)
                {
                    if (x < 0 || x >= bucketResolution)
                    {
                        continue;
                    }
                    for (int y = (int)math.floor((pos.y - radius) / (float)mapSize * (float)bucketResolution); y <= (int)math.floor((pos.y + radius) / (float)mapSize * (float)bucketResolution); y++)
                    {
                        if (y < 0 || y >= bucketResolution)
                        {
                            continue;
                        }
                        tempObstacleBuckets[x+y*bucketResolution].Add(obstacles[i]);
                    }
                }
            }

            

            obstacleBuckets = new NativeArray<NativeArray<Obstacle>>(bucketResolution*bucketResolution, Allocator.Persistent);
            for (int i = 0; i < bucketResolution; i++)
            {
                obstacleBuckets[i] = tempObstacleBuckets[i].ToArray(Allocator.Persistent);
                tempObstacleBuckets[i].Dispose();

            }

            tempObstacleBuckets.Dispose();
        }

        private void SetupColonyAndResource(ref SystemState state)
        {
            int mapSize = m_Config.m_MapSize;
            colonyPosition = (float2.zero+1) * mapSize * .5f;
            float2 pos = colonyPosition / mapSize;
            //colonyMatrix = float4x4.TRS(new float3(pos.x, pos.y, 0f), quaternion.identity, new float3(4f, 4f, .1f) / mapSize);

            Colony colony = new Colony()
            {
                m_Position = pos,
                m_Rotation = quaternion.identity,
                m_Scale = new float3(4f, 4f, .1f) / mapSize
            };

            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, colony);

            float resourceAngle = m_Random.NextFloat() * 2f * math.PI;
            resourcePosition = (float2.zero+1) * mapSize * .5f + new float2(math.cos(resourceAngle) * mapSize * .475f, math.sin(resourceAngle) * mapSize * .475f);
            pos = resourcePosition / mapSize;
            //resourceMatrix = float4x4.TRS(new float3(pos.x, pos.y, 0f), quaternion.identity, new float3(4f, 4f, .1f) / mapSize);

            Resource resource = new Resource()
            {
                m_Position = pos,
                m_Rotation = quaternion.identity,
                m_Scale = new float3(4f, 4f, .1f) / mapSize
            };

            entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, resource);
        }

        private void Init(ref SystemState state)
        {
            int antCount = m_Config.m_AntCount;
            float3 antSize = m_Config.m_AntSize;
            int mapSize = m_Config.m_MapSize;
            int rotationResolution = m_Config.m_RotationResolution;

            ants = new NativeArray<Ant>(antCount, Allocator.Persistent);
            for (int i = 0; i < antCount; i++)
            {
                ants[i] = new Ant() { 
                    m_Position = new float2(m_Random.NextFloat(-5f, 5f) + mapSize * .5f, m_Random.NextFloat(-5f, 5f) + mapSize * .5f)
                };

                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, ants[i]);
            }

            rotationMatrixLookup = new NativeArray<float4x4>(rotationResolution, Allocator.Temp);
            for (int i = 0; i < rotationResolution; i++)
            {
                float angle = (float)i / rotationResolution;
                angle *= 360f;
                rotationMatrixLookup[i] = float4x4.TRS(float3.zero, quaternion.Euler(0f, 0f, angle), antSize);
            }
        }

        NativeArray<Obstacle> emptyBucket;
        private NativeArray<Ant> ants;
        private NativeArray<float4x4> rotationMatrixLookup;

        NativeArray<Obstacle> GetObstacleBucket(float2 pos)
        {
            return GetObstacleBucket(pos.x, pos.y);
        }
        NativeArray<Obstacle> GetObstacleBucket(float posX, float posY)
        {
            int x = (int)(posX / m_Config.m_MapSize * m_Config.m_BucketResolution);
            int y = (int)(posY / m_Config.m_MapSize * m_Config.m_BucketResolution);
            if (x < 0 || y < 0 || x >= m_Config.m_BucketResolution || y >= m_Config.m_BucketResolution)
            {
                return emptyBucket;
            }
            else
            {
                return obstacleBuckets[x+y*m_Config.m_BucketResolution];
            }
        }
    }

    public struct Obstacle : IComponentData
    {
        public float2 m_Position;
        public quaternion m_Rotation;
        public float3 m_Scale;
        public float m_Radius;
        public int m_BatchIndex;
    }

    public struct Colony : IComponentData
    {
        public float2 m_Position;
        public quaternion m_Rotation;
        public float3 m_Scale;
    }

    public struct Resource : IComponentData
    {
        public float2 m_Position;
        public quaternion m_Rotation;
        public float3 m_Scale;
    }
}