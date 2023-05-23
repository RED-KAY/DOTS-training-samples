using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct MovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, velocity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<VelocityComponent>>())
        {
            transform.ValueRW.Position += velocity.ValueRO.Velocity * SystemAPI.Time.DeltaTime;
        }
    }
}