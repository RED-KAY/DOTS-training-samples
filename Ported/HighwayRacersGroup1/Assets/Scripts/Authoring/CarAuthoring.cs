using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class CarAuthoring : MonoBehaviour
{
    class CarBaker : Baker<CarAuthoring>
    {
        public override void Bake(CarAuthoring authoring)
        {
            AddComponent<CarData>();
            AddComponent<URPMaterialPropertyBaseColor>();
        }
    }
}

// todo: rename to "Car"
struct CarData : IComponentData
{
    public int SegmentID;
    public float SegmentDistance;
    public int Lane;
    public float Speed;
    public float OvertakeSpeed;
    
    public float DistanceToCarInFront;
    public float CarInFrontSpeed;
}
