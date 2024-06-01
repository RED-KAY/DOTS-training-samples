using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using AntPheromones.Authoring;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace AntPheromones.Systems
{
    [BurstCompile]
    public partial struct MainSimulationSystem : ISystem, ISystemStartStop
    {
        private Config m_Config;
        private Random m_Random;

        private NativeArray<Obstacle> obstacles;
        private NativeArray<NativeArray<Obstacle>> obstacleBuckets;
        private NativeArray<Entity> pheromoneEntities;

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
            rotationMatrixLookup.Dispose();
            pheromoneEntities.Dispose();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityManager entityManager = state.EntityManager;
            Debug.Log("Simulation updating...");
            SimulationLogic(ref state);
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

            ants = new NativeArray<Entity>(antCount, Allocator.Persistent);
            for (int i = 0; i < antCount; i++)
            {
                var antComponent = new Ant() { 
                    m_Position = new float2(m_Random.NextFloat(-5f, 5f) + mapSize * .5f, m_Random.NextFloat(-5f, 5f) + mapSize * .5f),
                    m_TransformationMatrix = new float4x4(),
                    m_Scale = antSize
                };

                var entity = state.EntityManager.CreateEntity(typeof(Ant));
                state.EntityManager.AddComponentData(entity, antComponent);

                ants[i] = entity;
            }

            rotationMatrixLookup = new NativeArray<float4x4>(rotationResolution, Allocator.Persistent);
            for (int i = 0; i < rotationResolution; i++)
            {
                float angle = (float)i / rotationResolution;
                angle *= 360f;
                rotationMatrixLookup[i] = float4x4.TRS(float3.zero, quaternion.Euler(0f, 0f, angle), antSize);
            }

            pheromoneEntities = new NativeArray<Entity>(mapSize * mapSize, Allocator.Persistent);
            for (int i = 0; i < pheromoneEntities.Length; i++)
            {
                var pheromone = new Pheromone()
                {
                    m_Amount = 0.0f
                };

                var entity = state.EntityManager.CreateEntity(typeof(Pheromone));
                state.EntityManager.SetComponentData(entity, pheromone);
                pheromoneEntities[i] = entity;
            }
        }

        NativeArray<Obstacle> emptyBucket;
        private NativeArray<Entity> ants;
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

        float4x4 GetRotationMatrix(float angle)
        {
            angle /= math.PI * 2f;
            angle -= math.floor(angle);
            angle *= m_Config.m_RotationResolution;
            return rotationMatrixLookup[((int)angle) % m_Config.m_RotationResolution];
        }

        int PheromoneIndex(int x, int y)
        {
            return x + y * m_Config.m_MapSize;
        }

        void DropPheromones(float2 position, float strength, ref SystemState state)
        {
            int mapSize = m_Config.m_MapSize;
            int x = (int)math.floor(position.x);
            int y = (int)math.floor(position.y);
            if (x < 0 || y < 0 || x >= mapSize || y >= mapSize)
            {
                return;
            }

            int index = PheromoneIndex(x, y);

            var pheromoneEntity = pheromoneEntities[index];
            
            var pheromone = state.EntityManager.GetComponentData<Pheromone>(pheromoneEntity);
            //var pheromone = SystemAPI.GetComponent<Pheromone>(pheromoneEntity);
            pheromone.m_Amount += (m_Config.m_TrailAddSpeed * strength * SystemAPI.Time.DeltaTime) * (1f - pheromone.m_Amount);
            if(pheromone.m_Amount > 1f)
            {
                pheromone.m_Amount = 1f;
            }

            //SystemAPI.SetComponent(pheromoneEntity, pheromone);
            state.EntityManager.SetComponentData(pheromoneEntity, pheromone);
        }

        float PheromoneSteering(Ant ant, float distance, ref SystemState state)
        {
            float output = 0;
            int mapSize = m_Config.m_MapSize;

            for (int i = -1; i <= 1; i += 2)
            {
                float angle = ant.m_FacingAngle + i * math.PI * .25f;
                float testX = ant.m_Position.x + math.cos(angle) * distance;
                float testY = ant.m_Position.y + math.sin(angle) * distance;

                if (testX < 0 || testY < 0 || testX >= mapSize || testY >= mapSize)
                {

                }
                else
                {
                    int index = PheromoneIndex((int)testX, (int)testY);
                    var pheromoneEntity = pheromoneEntities[index];
                    var pheromone = state.EntityManager.GetComponentData<Pheromone>(pheromoneEntity);
                    float value = pheromone.m_Amount;
                    output += value * i;
                }
            }
            return math.sign(output);
        }

        int WallSteering(Ant ant, float distance)
        {
            int output = 0;
            int mapSize = m_Config.m_MapSize;

            for (int i = -1; i <= 1; i += 2)
            {
                float angle = ant.m_FacingAngle + i * math.PI * .25f;
                float testX = ant.m_Position.x + math.cos(angle) * distance;
                float testY = ant.m_Position.y + math.sin(angle) * distance;

                if (testX < 0 || testY < 0 || testX >= mapSize || testY >= mapSize)
                {

                }
                else
                {
                    int value = GetObstacleBucket(testX, testY).Length;
                    if (value > 0)
                    {
                        output -= i;
                    }
                }
            }
            return output;
        }

        bool Linecast(float2 point1, float2 point2)
        {
            float dx = point2.x - point1.x;
            float dy = point2.y - point1.y;
            float dist = math.sqrt(dx * dx + dy * dy);

            int stepCount = (int)math.ceil(dist * .5f);
            for (int i = 0; i < stepCount; i++)
            {
                float t = (float)i / stepCount;
                if (GetObstacleBucket(point1.x + dx * t, point1.y + dy * t).Length > 0)
                {
                    return true;
                }
            }

            return false;
        }
        
        void SimulationLogic(ref SystemState state)
        {
            for (int i = 0; i < ants.Length; i++)
            {
                //Debug.Log("**************");
                Ant ant = SystemAPI.GetComponent<Ant>(ants[i]);
                float targetSpeed = m_Config.m_AntSpeed;

                ant.m_FacingAngle += m_Random.NextFloat(-m_Config.m_RandomSteering, m_Config.m_RandomSteering);

                float pheroSteering = PheromoneSteering(ant, 3f, ref state);
                int wallSteering = WallSteering(ant, 1.5f);
                ant.m_FacingAngle += pheroSteering * m_Config.m_PheromoneSteerStrength;
                ant.m_FacingAngle += wallSteering * m_Config.m_WallSteerStrength;

                targetSpeed *= 1f - (math.abs(pheroSteering) + math.abs(wallSteering)) / 3f;

                ant.m_Speed += (targetSpeed - ant.m_Speed) * m_Config.m_AntAccel;

                float2 targetPos;
                int index1 = i / m_Config.m_InstancesPerBatch;
                int index2 = i % m_Config.m_InstancesPerBatch;
                if (ant.m_HasFood == false)
                {
                    targetPos = resourcePosition;

                    //antColors[index1][index2] += ((Vector4)searchColor * ant.brightness - antColors[index1][index2]) * .05f;
                }
                else
                {
                    targetPos = colonyPosition;
                    //antColors[index1][index2] += ((Vector4)carryColor * ant.brightness - antColors[index1][index2]) * .05f;
                }
                if (Linecast(ant.m_Position, targetPos) == false)
                {
                    //Color color = Color.green;
                    float targetAngle = math.atan2(targetPos.y - ant.m_Position.y, targetPos.x - ant.m_Position.x);
                    if (targetAngle - ant.m_FacingAngle > math.PI)
                    {
                        ant.m_FacingAngle += math.PI * 2f;
                        //color = Color.red;
                    }
                    else if (targetAngle - ant.m_FacingAngle < -math.PI)
                    {
                        ant.m_FacingAngle -= math.PI * 2f;
                        //color = Color.red;
                    }
                    else
                    {
                        if (math.abs(targetAngle - ant.m_FacingAngle) < math.PI * .5f)
                            ant.m_FacingAngle += (targetAngle - ant.m_FacingAngle) * m_Config.m_GoalSteerStrength;
                    }

                    //Debug.DrawLine(ant.position/mapSize,targetPos/mapSize,color);
                }
                if (math.lengthsq(ant.m_Position - targetPos) < 4f * 4f)
                {
                    ant.m_HasFood = !ant.m_HasFood;
                    ant.m_FacingAngle += math.PI;
                }

                float vx = math.cos(ant.m_FacingAngle) * ant.m_Speed;
                float vy = math.sin(ant.m_FacingAngle) * ant.m_Speed;
                float ovx = vx;
                float ovy = vy;

                if (ant.m_Position.x + vx < 0f || ant.m_Position.x + vx > m_Config.m_MapSize)
                {
                    vx = -vx;
                }
                else
                {
                    ant.m_Position.x += vx;
                }
                if (ant.m_Position.y + vy < 0f || ant.m_Position.y + vy > m_Config.m_MapSize)
                {
                    vy = -vy;
                }
                else
                {
                    ant.m_Position.y += vy;
                }

                float dx, dy, dist;

                NativeArray<Obstacle> nearbyObstacles = GetObstacleBucket(ant.m_Position);
                for (int j = 0; j < nearbyObstacles.Length; j++)
                {
                    Obstacle obstacle = nearbyObstacles[j];
                    dx = ant.m_Position.x - obstacle.m_Position.x;
                    dy = ant.m_Position.y - obstacle.m_Position.y;
                    float sqrDist = dx * dx + dy * dy;
                    if (sqrDist < m_Config.m_ObstacleRadius * m_Config.m_ObstacleRadius)
                    {
                        dist = math.sqrt(sqrDist);
                        dx /= dist;
                        dy /= dist;
                        ant.m_Position.x = obstacle.m_Position.x + dx * m_Config.m_ObstacleRadius;
                        ant.m_Position.y = obstacle.m_Position.y + dy * m_Config.m_ObstacleRadius;

                        vx -= dx * (dx * vx + dy * vy) * 1.5f;
                        vy -= dy * (dx * vx + dy * vy) * 1.5f;
                    }
                }

                //Debug.Log("ant>position: " + ant.m_Position);
                     
                float inwardOrOutward = -m_Config.m_OutwardStrength;
                float pushRadius = m_Config.m_MapSize * .4f;
                if (ant.m_HasFood)
                {
                    //Debug.Log("ant hasFood.");

                    inwardOrOutward = m_Config.m_InwardStrength;
                    pushRadius = m_Config.m_MapSize;
                }
                dx = colonyPosition.x - ant.m_Position.x;
                dy = colonyPosition.y - ant.m_Position.y;
                dist = math.sqrt(dx * dx + dy * dy);
                inwardOrOutward *= 1f - math.clamp(dist / pushRadius, 0, 1);
                vx += dx / dist * inwardOrOutward;
                vy += dy / dist * inwardOrOutward;

                if (ovx != vx || ovy != vy)
                {
                    ant.m_FacingAngle = math.atan2(vy, vx);
                }

                //if (ant.holdingResource == false) {
                //float excitement = 1f-Mathf.Clamp01((targetPos - ant.position).magnitude / (mapSize * 1.2f));
                float excitement = .3f;
                if (ant.m_HasFood)
                {
                    excitement = 1f;
                }
                excitement *= ant.m_Speed / m_Config.m_AntSpeed;
                DropPheromones(ant.m_Position, excitement, ref state);
                //}

                float4x4 matrix = GetRotationMatrix(ant.m_FacingAngle);
                matrix.c3.x = ant.m_Position.x / m_Config.m_MapSize;
                matrix.c3.y = ant.m_Position.y / m_Config.m_MapSize;
                ant.m_TransformationMatrix = matrix;

                state.EntityManager.SetComponentData(ants[i], ant);

                //matrices[i / m_Config.m_InstancesPerBatch][i % m_Config.m_InstancesPerBatch] = matrix;
            }

            for (int x = 0; x < m_Config.m_MapSize; x++)
            {
                for (int y = 0; y < m_Config.m_MapSize; y++)
                {
                    int index = PheromoneIndex(x, y);
                    var entity = pheromoneEntities[index];
                    var pheromone = SystemAPI.GetComponent<Pheromone>(entity);
                    pheromone.m_Amount *= m_Config.m_TrailDecay;
                    SystemAPI.SetComponent(entity, pheromone);
                }
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

    public struct Pheromone : IComponentData { public float m_Amount; }
}