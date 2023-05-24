using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BloodAuthoring : MonoBehaviour
{
    public float decayRate = 1f;
}

public class BloodBaker : Baker<BloodAuthoring>
{
    public override void Bake(BloodAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new GravityComponent());
        AddComponent(entity, new VelocityComponent());
        AddComponent(entity, new BloodComponent());

        AddComponent(entity, new DecayComponent
        { 
            DecayRate = authoring.decayRate
        });
        SetComponentEnabled<DecayComponent>(entity, false);
    }
}