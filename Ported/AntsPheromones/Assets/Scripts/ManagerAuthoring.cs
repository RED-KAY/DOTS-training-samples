using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AntPheromones.Authoring
{
    public class ManagerAuthoring : MonoBehaviour
    {
        [Header("Rendering")]
        public Material m_BasePheromoneMaterial;
        public Renderer m_PheromoneRenderer;
        public Material m_AntMaterial;
        public Material m_ObstacleMaterial;
        public Material m_ResourceMaterial;
        public Material m_ColonyMaterial;
        public Mesh m_AntMesh;
        public Mesh m_ObstacleMesh;
        public Mesh m_ColonyMesh;
        public Mesh m_ResourceMesh;
        public Color m_SearchColor;
        public Color m_CarryColor;

        [Header("Config")]
        public int m_AntCount;
        public int m_MapSize = 128;
        public int m_BucketResolution;
        public float3 m_AntSize;
        public float m_AntSpeed;
        [Range(0f, 1f)]
        public float m_AntAccel;
        public float m_TrailAddSpeed;
        [Range(0f, 1f)]
        public float m_TrailDecay;
        public float m_RandomSteering;
        public float m_PheromoneSteerStrength;
        public float m_WallSteerStrength;
        public float m_GoalSteerStrength;
        public float m_OutwardStrength;
        public float m_InwardStrength;
        public int m_RotationResolution = 360;
        public int m_ObstacleRingCount;
        [Range(0f, 1f)]
        public float m_ObstaclesPerRing;
        public float m_ObstacleRadius;

        public class ManagerBaker : Baker<ManagerAuthoring>
        {
            public override void Bake(ManagerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                Debug.Log($"Baking entity: {entity}");
                var config = new Config()
                {
                    m_AntCount = authoring.m_AntCount,
                    m_MapSize = authoring.m_MapSize,
                    m_BucketResolution = authoring.m_BucketResolution,
                    m_AntSize = authoring.m_AntSize,
                    m_AntSpeed = authoring.m_AntSpeed,
                    m_AntAccel = authoring.m_AntAccel,
                    m_TrailAddSpeed = authoring.m_TrailAddSpeed,
                    m_TrailDecay = authoring.m_TrailDecay,
                    m_RandomSteering = authoring.m_RandomSteering,
                    m_PheromoneSteerStrength = authoring.m_PheromoneSteerStrength,
                    m_WallSteerStrength = authoring.m_WallSteerStrength,
                    m_GoalSteerStrength = authoring.m_GoalSteerStrength,
                    m_OutwardStrength = authoring.m_OutwardStrength,
                    m_InwardStrength = authoring.m_InwardStrength,
                    m_RotationResolution = authoring.m_RotationResolution,
                    m_ObstacleRingCount = authoring.m_ObstacleRingCount,
                    m_ObstaclesPerRing = authoring.m_ObstaclesPerRing,
                    m_ObstacleRadius = authoring.m_ObstacleRadius
                };



                var drawingData = new DrawingDataTest()
                {
                    m_BasePheromoneMaterial = authoring.m_BasePheromoneMaterial,
                    m_PheromoneRenderer = authoring.m_PheromoneRenderer,
                    m_AntMaterial = authoring.m_AntMaterial,
                    m_ObstacleMaterial = authoring.m_ObstacleMaterial,
                    m_ResourceMaterial = authoring.m_ResourceMaterial,
                    m_ColonyMaterial = authoring.m_ColonyMaterial,
                    m_AntMesh = authoring.m_AntMesh,
                    m_ObstacleMesh = authoring.m_ObstacleMesh,
                    m_ColonyMesh = authoring.m_ColonyMesh,
                    m_ResourceMesh = authoring.m_ResourceMesh,
                    m_SearchColor = authoring.m_SearchColor,
                    m_CarryColor = authoring.m_CarryColor
                };

                AddComponent(entity, config);
                Debug.Log("Added Config component");

                AddSharedComponentManaged(entity, drawingData);
                Debug.Log("Added DrawingDataTest shared component");
            }
        }
    }

    public struct Ant : IComponentData
    {
        public float2 m_Position;
        public bool m_HasFood;
        public float m_FacingAngle;
        public float m_Speed;
        public float m_Brightness;
    }

    public struct Config : IComponentData
    {
        public int m_AntCount;
        public int m_MapSize;
        public int m_BucketResolution;
        public float3 m_AntSize;
        public float m_AntSpeed;
        public float m_AntAccel;
        public float m_TrailAddSpeed;
        public float m_TrailDecay;
        public float m_RandomSteering;
        public float m_PheromoneSteerStrength;
        public float m_WallSteerStrength;
        public float m_GoalSteerStrength;
        public float m_OutwardStrength;
        public float m_InwardStrength;
        public int m_RotationResolution;
        public int m_ObstacleRingCount;
        public float m_ObstaclesPerRing;
        public float m_ObstacleRadius;
    }

    public struct DrawingDataTest : ISharedComponentData, IEquatable<DrawingDataTest>
    {
        public Material m_BasePheromoneMaterial;
        public Renderer m_PheromoneRenderer;
        public Material m_AntMaterial;
        public Material m_ObstacleMaterial;
        public Material m_ResourceMaterial;
        public Material m_ColonyMaterial;
        public Mesh m_AntMesh;
        public Mesh m_ObstacleMesh;
        public Mesh m_ColonyMesh;
        public Mesh m_ResourceMesh;
        public Color m_SearchColor;
        public Color m_CarryColor;

        public bool Equals(DrawingDataTest other)
        {
            return m_BasePheromoneMaterial == other.m_BasePheromoneMaterial &&
                   m_PheromoneRenderer == other.m_PheromoneRenderer &&
                   m_AntMaterial == other.m_AntMaterial &&
                   m_ObstacleMaterial == other.m_ObstacleMaterial &&
                   m_ResourceMaterial == other.m_ResourceMaterial &&
                   m_ColonyMaterial == other.m_ColonyMaterial &&
                   m_AntMesh == other.m_AntMesh &&
                   m_ObstacleMesh == other.m_ObstacleMesh &&
                   m_ColonyMesh == other.m_ColonyMesh &&
                   m_ResourceMesh == other.m_ResourceMesh &&
                   m_SearchColor.Equals(other.m_SearchColor) &&
                   m_CarryColor.Equals(other.m_CarryColor);
        }

        public override int GetHashCode()
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (m_BasePheromoneMaterial != null ? m_BasePheromoneMaterial.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_PheromoneRenderer != null ? m_PheromoneRenderer.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_AntMaterial != null ? m_AntMaterial.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_ObstacleMaterial != null ? m_ObstacleMaterial.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_ResourceMaterial != null ? m_ResourceMaterial.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_ColonyMaterial != null ? m_ColonyMaterial.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_AntMesh != null ? m_AntMesh.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_ObstacleMesh != null ? m_ObstacleMesh.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_ColonyMesh != null ? m_ColonyMesh.GetHashCode() : 0);
            hashCode = hashCode * 23 + (m_ResourceMesh != null ? m_ResourceMesh.GetHashCode() : 0);
            hashCode = hashCode * 23 + m_SearchColor.GetHashCode();
            hashCode = hashCode * 23 + m_CarryColor.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}

