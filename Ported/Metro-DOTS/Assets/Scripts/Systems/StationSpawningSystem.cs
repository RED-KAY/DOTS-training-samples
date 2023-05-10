using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct StationSpawningSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<StationConfig>();
    }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var stationConfig = SystemAPI.GetSingleton<StationConfig>();
        var stations = CollectionHelper.CreateNativeArray<Entity>(stationConfig.NumStations, Allocator.Temp);

        var em = state.EntityManager;
        em.Instantiate(stationConfig.StationEntity, stations);

        // TODO Make random posiioning work properly
        var random = Random.CreateFromIndex(12314);

        int stationIdCount = 0;
        float accumulatedValue = 0;
        foreach (var (transform, stationID) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<StationIDComponent>>())
        {
            /*
            var val = random.NextFloat();
            float3 offset = new float3(val * stationConfig.Spacing, 0, 0);

            transform.ValueRW.Position = new float3(i * stationConfig.Spacing, 0, 0) + offset;
            i++;
            */

            stationID.ValueRW.StationID = stationIdCount;
            stationIdCount++;

            var val = random.NextFloat();
            accumulatedValue += (math.max(val * stationConfig.Spacing, 10)) + stationConfig.StationWidth;// + width of the platform
            float3 offset = new float3(accumulatedValue, 0, 0);
            transform.ValueRW.Position = offset;
        }

        var trackArchetype = em.CreateArchetype(typeof(TrackIDComponent), typeof(Track));
        var trackEntityA = em.CreateEntity(trackArchetype);
        var trackEntityB = em.CreateEntity(trackArchetype);
#if UNITY_EDITOR
        em.SetName(trackEntityA, "TrackEntityA");
        em.SetName(trackEntityB, "TrackEntityB");
#endif
        var TrackPointBufferA = em.AddBuffer<TrackPoint>(trackEntityA);
        var TrackPointBufferB = em.AddBuffer<TrackPoint>(trackEntityB);
        TrackPointBufferA = em.GetBuffer<TrackPoint>(trackEntityA);

        int i = 0;
        var halfStationLength = 20;
        float3 trackPointOffsetFromCenter = new float3(halfStationLength, 0, 0);
        foreach (var (transform, stationEntity) in
            SystemAPI.Query<RefRO<LocalTransform>>()
            .WithAll<StationIDComponent>()
            .WithEntityAccess())
        {
            bool isEnd = i == 0 || i == stationConfig.NumStations - 1;
            TrackPointBufferA.Add(new TrackPoint { Position = stationConfig.TrackACenter + transform.ValueRO.Position - trackPointOffsetFromCenter, Station = Entity.Null });
            TrackPointBufferA.Add(new TrackPoint { Position = stationConfig.TrackACenter + transform.ValueRO.Position, IsEnd = isEnd, IsStation = true, Station = stationEntity});
            TrackPointBufferA.Add(new TrackPoint { Position = stationConfig.TrackACenter + transform.ValueRO.Position + trackPointOffsetFromCenter, Station = Entity.Null  });
            
            TrackPointBufferB.Add(new TrackPoint { Position = stationConfig.TrackBCenter + transform.ValueRO.Position - trackPointOffsetFromCenter, Station = Entity.Null  });
            TrackPointBufferB.Add(new TrackPoint { Position = stationConfig.TrackBCenter + transform.ValueRO.Position, IsEnd = isEnd, IsStation = true, Station = stationEntity });
            TrackPointBufferB.Add(new TrackPoint { Position = stationConfig.TrackBCenter + transform.ValueRO.Position + trackPointOffsetFromCenter, Station = Entity.Null  });
            i++;
        }

        float sleeperSpacing = 1;

        foreach (var trackBuffer in new[] { TrackPointBufferA, TrackPointBufferB })
        {
            for (int j = 0; j < trackBuffer.Length - 1; j++)
            {
                float distance = math.distance(trackBuffer[j].Position, trackBuffer[j + 1].Position);
                var numberOfSleepers = (int)(distance / sleeperSpacing);

                var sleepers = CollectionHelper.CreateNativeArray<Entity>(numberOfSleepers, Allocator.Temp);
                em.Instantiate(stationConfig.TrackEntity, sleepers);

                for (int k = 0; k < sleepers.Length; k++)
                {
                    float3 pos = math.lerp(trackBuffer[j].Position, trackBuffer[j + 1].Position, (float)k / sleepers.Length);
                    LocalTransform lc = new LocalTransform
                    {
                        Position = pos,
                        Scale = 1f,
                        Rotation = quaternion.RotateY(math.PI / 2)
                    };
                    em.SetComponentData<LocalTransform>(sleepers[k], lc);
                }
            }
        }

        // TODO Add random spawn points between 3-5 for same number of carriadges
        // var random = Random.CreateFromIndex(12314);
        // var val = random.NextFloat();
        int numQueuePoints = stationConfig.NumStations * (stationConfig.NumQueingPoints /** 2*/);
        var queuePoints = CollectionHelper.CreateNativeArray<Entity>(numQueuePoints, Allocator.Temp);

        EntityArchetype queueArchetype = em.CreateArchetype(typeof(QueueComponent), typeof(LocalTransform), typeof(QueuePassengers));
#if UNITY_EDITOR
        // todo: make naming work
        for (var k = 0; k < queuePoints.Length; k++)
        {
            em.SetName(queuePoints[k], "QueueEntity");
        }
#endif
        em.CreateEntity(queueArchetype, queuePoints);

        for (int j = 0; j < queuePoints.Length; j++)
        {
            // Todo is there a better way to initialize 16 elemtns into a buffer?
            var buffer = em.GetBuffer<QueuePassengers>(queuePoints[j]);
            for (int k = 0; k < 16; k++)
            {
                buffer.Add(new QueuePassengers());
            }

            // init QueueComponent
            var qc = em.GetComponentData<QueueComponent>(queuePoints[j]);
            qc.StartIndex = 0;
            qc.EndEndex = -1;
            em.SetComponentData<QueueComponent>(queuePoints[j], qc);
        }

        float carriadgeLength = 5;

        i = 0;
        foreach (var transform in
            SystemAPI.Query<RefRO<LocalTransform>>()
            .WithAll<StationIDComponent>())
        {
            var stationsQueueBuffer = em.GetBuffer<StationQueuesElement>(stations[i]);

            for (int k = 0; k < stationConfig.NumQueingPoints; k++)
            {
                LocalTransform lc = new LocalTransform();
                float totalQueuePointsSpan = (carriadgeLength * (stationConfig.NumQueingPoints - 1)) / 2;
                lc.Position = stationConfig.TrackACenter + stationConfig.SpawnPointOffsetFromCenterPoint + transform.ValueRO.Position - new float3(totalQueuePointsSpan, 0, 0) + new float3(k * carriadgeLength, 0, 0);
                lc.Scale = 1;
                lc.Rotation = quaternion.RotateY(math.PI);
                var queuePointIndex = (i * stationConfig.NumQueingPoints) + k;
                em.SetComponentData<LocalTransform>(queuePoints[queuePointIndex], lc);

                stationsQueueBuffer.Add(new StationQueuesElement { Queue = queuePoints[queuePointIndex] });
            }
            i++;

            //for (int k = 0; k < stationConfig.NumCarriadges; k++)
            //{
            //    // make the x negative
            //}
        }

        // var tracks = CollectionHelper.CreateNativeArray<Entity>(TrackPointBuffer.Length, Allocator.Temp);
        // em.Instantiate(stationConfig.TrackEntity, tracks);

        // i = 0;
        // foreach (var transform in
        //     SystemAPI.Query<RefRW<LocalTransform>>()
        //     .WithAll<SleeperTag>())
        // {
        //     transform.ValueRW.Position = TrackPointBuffer[i].Position;
        //     transform.ValueRW.Rotation = quaternion.RotateY(math.PI / 2);
        //     i++;
        // }


        /*
        var query = SystemAPI.QueryBuilder().WithAll<StationConfig>().Build();
        var chunks = query.ToArchetypeChunkArray(Allocator.Temp);
        var localTransformHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>();
        foreach (var chunk in chunks)
        {
            var localTransformVals = chunk.GetNativeArray(localTransformHandle);
            for (int j = 0; j < chunk.Count; j++)
            {
                var v = localTransformVals[j];
            }
        }
        */

        state.Enabled = false;
    }
}
