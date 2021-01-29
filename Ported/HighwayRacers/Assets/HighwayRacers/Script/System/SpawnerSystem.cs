﻿using NUnit.Framework.Constraints;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;


public class SpawnerSystem : SystemBase
{

    public static float4 LowVelocityColor = new float4(1,0,0,1);
    public static float4 AmericanColors = new float4(0,1,0,1);
    public static float4 EuropeanColors = new float4(0,0,1,1);

  static float3 MapToRoundedCorners(float t, float radius)
  {
      float R = 0.29f;
        float straight = 1.0f - 2.0f * R;
        float curved = (2.0f * math.PI * R) * 0.25f;
        float total = straight + curved;
        float tls = math.saturate(straight/total);
        float tlr = math.saturate(curved/total);

        int q = (int)(t * 4.0f);

        float x = 0;
        float y = 0;
        float a = 0;

        if(q == 0)
        {
            float n = t * 4.0f;
            x = R;
            y = math.lerp(R, 1.0f - R, math.saturate(n/tls));

            a = 0.5f * math.PI * math.saturate((n - tls)/tlr);
            x -= math.cos(a) * R;
            y += math.sin(a) * R;
        }
        else if(q == 1)
        {
            float n = (t - 0.25f) * 4.0f;
            y = 1.0f - R;
            x = math.lerp(R, 1.0f - R, math.saturate(n/tls));

            a = 0.5f * math.PI * math.saturate((n - tls)/tlr);
            y += math.cos(a) * R;
            x += math.sin(a) * R;
            a += math.PI/2.0f;
        }
        else if(q == 2)
        {
            float n = (t - 0.5f) * 4.0f;
            x = 1.0f - R;
            y = math.lerp(1.0f - R, R, math.saturate(n/tls));

            a = 0.5f * math.PI * math.saturate((n - tls)/tlr);
            x += math.cos(a) * R;
            y -= math.sin(a) * R;
            a -= math.PI;
        }
        else
        {
            float n = (t - 0.75f) * 4.0f;
            y = R;
            x = math.lerp(1.0f - R, R, math.saturate(n/tls));

            a = 0.5f * math.PI * math.saturate((n - tls)/tlr);
            y -= math.cos(a) * R;
            x -= math.sin(a) * R;
            a -= math.PI/2.0f;
        }

        x -= 0.5f;
        y -= 0.5f;
        x *= radius;
        y *= radius;
        return new float3(x,y,a);
    }

    public static readonly float TrackCirconference = ((CarMovementSystem.TrackRadius/2.0f) * 4.0f)/182.0f;
    public static readonly float MinimumVelocity = 0.035f/TrackCirconference;
    public static readonly float MaximumVelocity = 0.075f/TrackCirconference;

    private EntityQuery RequirePropagation;
    private TrackOccupancySystem m_TrackOccupancySystem;
    private TrackSettings CurrentTrackSettings;
    private int CurrentSpawnCount;

    protected override void OnCreate()
    {
        m_TrackOccupancySystem = World.GetExistingSystem<TrackOccupancySystem>();
    }

    protected override void OnUpdate()
    {
        // Propagate color from parent to child entities
        // We do this every frame because we change the color of the cars

        // A "ComponentDataFromEntity" allows random access to a component type from a job.
        // This much slower than accessing the components from the current entity via the
        // lambda parameters.
        var cdfe = GetComponentDataFromEntity<URPMaterialPropertyBaseColor>();

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        Entities
            // Random access to components for writing can be a race condition.
            // Here, we know for sure that prefabs don't share their entities.
            // So explicitly request to disable the safety system on the CDFE.
            .WithNativeDisableContainerSafetyRestriction(cdfe)
            .WithStoreEntityQueryInField(ref RequirePropagation)
            .ForEach((in DynamicBuffer<LinkedEntityGroup> group
                , in URPMaterialPropertyBaseColor color) =>
            {
                for (int i = 1; i < group.Length; ++i)
                {
                    cdfe[group[i].Value] = color;
                }
            }).ScheduleParallel();


        // The code below only runs when the UI updates


        if (UIValues.GetModified() == CurrentTrackSettings.Iteration && CurrentTrackSettings.Iteration > 0)
        {
            return;
        }

        CurrentTrackSettings = UIValues.GetTrackSettings();
        if (CurrentSpawnCount == CurrentTrackSettings.CarCount)
        {
            return;
        }

        CurrentSpawnCount = CurrentTrackSettings.CarCount;
        
        var carCount = CurrentSpawnCount;

        var random = new Random(1234);
        uint laneCount = m_TrackOccupancySystem.LaneCount;
        uint tilesPerLane = TrackOccupancySystem.TilesPerLane;
        float minimumVelocity = MinimumVelocity;
        float maximumVelocity = MaximumVelocity;


        float4 americanColors = AmericanColors;
        float4 europeanColors = EuropeanColors;

        if (TrackOccupancySystem.ShowDebugTiles)
        {
            Entities
                .ForEach((Entity entity, in Spawner spawner) =>
                {
                    for (uint j = 0; j < laneCount; ++j)
                    {
                        for (uint i = 0; i < tilesPerLane; ++i)
                        {
                            var tile = ecb.Instantiate(spawner.TilePrefab);

                            float laneRadius = (CarMovementSystem.TrackRadius + (j * CarMovementSystem.LaneWidth));

                            float t = (float)i/(float)tilesPerLane;
                            float3 spawnPosition = MapToRoundedCorners(t, laneRadius);

                            spawnPosition.x += (CarMovementSystem.TrackRadius)/2.0f + 2.75f;
                            spawnPosition.y += (CarMovementSystem.TrackRadius)/4.0f - 6.0f;


                            var translation = new Translation {Value = new float3(spawnPosition.x, 0, spawnPosition.y)};
                            ecb.SetComponent(tile, translation);

                            ecb.SetComponent(tile, new URPMaterialPropertyBaseColor
                            {
                                Value = new float4(0.5f, 0.5f, 0.5f, 1.0f)
                            });

                            ecb.SetComponent(tile, new TileDebugColor
                            {
                                laneId = j,
                                tileId = i
                            });

                        }
                    }
                }).Run();
        }

        
        Entities.ForEach((Entity e, in CarMovement cm) =>
        {
            ecb.DestroyEntity(e);
        }).Run();
            
        
        
        Entities
            .ForEach((Entity entity, in Spawner spawner) =>
            {
                // Destroying the current entity is a classic ECS pattern,
                // when something should only be processed once then forgotten.

                for (uint i = 0; i < carCount; ++i)
                {
                    var vehicle = ecb.Instantiate(spawner.CarPrefab);
                    var translation = new Translation {Value = new float3(0, 0, 0)};
                    ecb.SetComponent(vehicle, translation);

                    // If the driver profile is set to random, pick one of
                    // the profiles for the new car.
                    DriverProfile profile = spawner.DriverProfile;
                    if (profile == DriverProfile.Random)
                    {
                        profile = random.NextBool() ? DriverProfile.American : DriverProfile.European;
                    }
                    
                    float4 carColor = profile == DriverProfile.American ? americanColors : europeanColors;
                    
                    ecb.SetComponent(vehicle, new URPMaterialPropertyBaseColor
                    {
                        Value = carColor
                    });

                    uint currentLane = i % laneCount;
                    float velocity = random.NextFloat(minimumVelocity, maximumVelocity);
                    ecb.SetComponent(vehicle, new CarMovement
                    {
                        // todo here we ar enot smart enough. Two cars might end up in the same tile.
                        // This means they can drive through each other.
                        Offset = (float) i / spawner.CarCount,
                        Lane = currentLane,

                        LaneOffset = (float)currentLane,
                        Velocity = velocity,
                        CurrentVelocity = velocity,
                        LaneSwitchCounter = 0,
                        Profile = profile
                    });

                }
            }).Run();

        ecb.Playback(EntityManager);
    }
}