using Unity.Entities;
using Unity.Mathematics;

public struct Bee : IComponentData
{
    public float3 OcillateOffset;
    public float3 Target;
    public Entity heldResource;
    public BEESTATE beeState;

    public enum BEESTATE
    {
        IDLE,
        FORAGE,
        CARRY,
        ATTACK
    };
}
