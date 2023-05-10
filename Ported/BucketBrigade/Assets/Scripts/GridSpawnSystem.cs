using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public partial struct GridSpawnSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<Grid>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        state.Enabled = false;

        var config = SystemAPI.GetSingleton<Grid>();
        state.EntityManager.Instantiate(config.FirePrefab, config.GridSize * config.GridSize, Allocator.Temp);
        var pos = config.GridOrigin;
        int i = 0;
        foreach (var (trans, burn) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Burner>>().WithAll<Fire>()) {
            trans.ValueRW.Scale = 1;
            trans.ValueRW.Position.x = pos.x + (i % config.GridSize) * 1.05f;
            trans.ValueRW.Position.y = pos.y - 2.0f;    // Candidate for some global grid config min-Y value
            trans.ValueRW.Position.z = pos.z + (i / config.GridSize) * 1.05f;
            i++;
        }
    }
}