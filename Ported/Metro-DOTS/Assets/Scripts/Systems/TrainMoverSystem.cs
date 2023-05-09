using Components;
using Metro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct TrainMoverSystem  : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
    }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;
        var track = SystemAPI.GetSingletonBuffer<TrackPoint>();
        var config = SystemAPI.GetSingleton<Config>();

        var q = em.CreateEntityQuery(typeof(TrainIDComponent));
        foreach (var entity in q.ToEntityArray(Allocator.Temp))
        {
            var transform = em.GetComponentData<LocalTransform>(entity);
            var train = em.GetComponentData<TrainIDComponent>(entity);

        // foreach (var (transform, train) in
        //          SystemAPI.Query<RefRW<LocalTransform>, RefRW<TrainIDComponent>>())
        // {

            if (em.IsComponentEnabled<EnRouteComponent>(entity))
            {
                float3 position = transform.Position - train.Offset;
                int currentIndex = train.TrackPointIndex;
                float totalDistance = config.MaxTrainSpeed * SystemAPI.Time.DeltaTime;
                bool forward = train.Forward;

                while (totalDistance > 0)
                {
                    int nextIndex = forward ? currentIndex + 1 : currentIndex - 1;

                    float3 currentPoint = track[currentIndex].Value;
                    float3 nextPoint = track[nextIndex].Value;
                    float distanceToNextPoint = math.distance(nextPoint, position);
                    float3 directionToNextPoint = math.normalize(nextPoint - currentPoint);

                    if (totalDistance >= distanceToNextPoint)
                    {
                        totalDistance -= distanceToNextPoint;
                        position = nextPoint;

                        currentIndex = nextIndex;
                        if (forward && currentIndex + 1 >= track.Length)
                            forward = false;
                        else if (!forward && currentIndex == 0)
                            forward = true;
                        
                        em.SetComponentEnabled<LoadingComponent>(entity, true);
                        em.SetComponentEnabled<EnRouteComponent>(entity, false);
                        var loadingComp = em.GetComponentData<LoadingComponent>(entity);
                        loadingComp.duration = 0;
                        em.SetComponentData(entity, loadingComp);
                        break;
                    }
                    else
                    {
                        float3 movement = directionToNextPoint * totalDistance;
                        var newPos = position + movement;
                        position = newPos;
                        totalDistance = 0;
                    }
                }

                train.Forward = forward;
                train.TrackPointIndex = currentIndex;
                transform.Position = position + train.Offset;
            }
            
            else if (em.IsComponentEnabled<LoadingComponent>(entity))
            {
                var loadingComp = em.GetComponentData<LoadingComponent>(entity);
                loadingComp.duration += SystemAPI.Time.DeltaTime;

                if (loadingComp.duration >= 2.0f)
                {
                    em.SetComponentEnabled<LoadingComponent>(entity, false);
                    em.SetComponentEnabled<EnRouteComponent>(entity, true);
                }
                em.SetComponentData(entity, loadingComp);
            }

            em.SetComponentData(entity, transform);
            em.SetComponentData(entity, train);
        }
        
    }
}
