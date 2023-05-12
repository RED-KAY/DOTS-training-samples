using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct PheromonesSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AntData>();
        state.RequireForUpdate<PheromoneBufferElement>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var globalSettings = SystemAPI.GetSingleton<GlobalSettings>();
        var pheromoneBufferElement = SystemAPI.GetSingletonBuffer<PheromoneBufferElement>();
        
        var pheromoneDropJob = new PheromoneDropJob
        {
            GlobalSettings = globalSettings,
            Pheromones = pheromoneBufferElement,
            DeltaTime = SystemAPI.Time.DeltaTime,
        };

        state.Dependency = pheromoneDropJob.ScheduleParallel(state.Dependency);
        state.Dependency.Complete(); // works like await

        var pheromoneDecreaseJob = new PheromoneDecreaseJob
        {
            Pheromones = pheromoneBufferElement.AsNativeArray(),
            TrailDecay = 1 - (globalSettings.TrailDecay * SystemAPI.Time.DeltaTime)
        };
        
        state.Dependency = pheromoneDecreaseJob.Schedule(pheromoneBufferElement.Length, 1000, state.Dependency);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    
    [BurstCompile]
    public partial struct PheromoneDecreaseJob : IJobParallelFor
    {
        public float TrailDecay;
        public NativeArray<PheromoneBufferElement> Pheromones;
        
        [BurstCompile]
        public void Execute(int index)
        {
            var value = Pheromones[index].Value;
            if (value.x > 1f)
            {
                value.x = 1;
            }
            
            value.x *= TrailDecay;
            Pheromones[index] = value;
        }
    }
    
    [BurstCompile]
    public partial struct PheromoneDropJob : IJobEntity
    {
        public GlobalSettings GlobalSettings;
        public float DeltaTime;

        [NativeDisableParallelForRestriction]
        public DynamicBuffer<PheromoneBufferElement> Pheromones;
        
        [BurstCompile]
        void Execute(ref AntData ant, ref LocalTransform localTransform)
        {
            float excitement = GlobalSettings.RegularExcitement;
          //  float maxPheromones = 1;
            if (ant.HoldingResource) 
            {
                excitement = GlobalSettings.TrailExcitement;
         //       maxPheromones = 1;
            }
            excitement *= ant.Speed / GlobalSettings.AntSpeed;
            int x = (int)math.floor(localTransform.Position.x);
            int y = (int)math.floor(localTransform.Position.y);
            
            if (x < 0 || y < 0 || x >= GlobalSettings.MapSizeX || y >= GlobalSettings.MapSizeY)
                return;
            
            int index = PheromoneIndex(x , y, GlobalSettings.MapSizeX);

          // if (Pheromones[index] < maxPheromones)
          // {
          var value = Pheromones[index].Value;
          value.x += (GlobalSettings.TrailAddSpeed * excitement * DeltaTime) * (1f - Pheromones[index].Value.x);
          Pheromones[index] = value;
          //  }
        }
    }
    
    public static int PheromoneIndex(int x, int y, int mapSizeX)
    {
        return x + y * mapSizeX;
    }
    
    public static int PheromoneIndexClamp(int x, int y, int mapSizeX, int mapSizeY)
    {
        x = math.clamp(x, 0, mapSizeX - 1);
        y = math.clamp(y, 0, mapSizeY - 1);
        return x + y * mapSizeX;
    }
}



