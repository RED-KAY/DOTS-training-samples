using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AutoFarmers.Farm
{
    public class FarmAuthoring : MonoBehaviour
    {
        public Mesh m_Ground;
        public Material m_GroundMaterial;
        public int2 m_Size;
        [Range(0, 100)] public int m_RockPercentage;

        public Mesh m_FarmerMesh;
        public Material m_FarmerMaterial;

        public Mesh m_RockMesh;
        public Material m_RockMaterial;

        public GameObject m_DebugTileId;

        public class FarmBaker : Baker<FarmAuthoring>
        {
            public override void Bake(FarmAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                AddComponent(entity, new Farm()
                {
                    m_Size = authoring.m_Size,
                    m_RockPercentage = authoring.m_RockPercentage,
                    m_DebugCanvas = GetEntity(authoring.m_DebugTileId, TransformUsageFlags.Dynamic)
                });

                AddSharedComponentManaged(entity, new FarmDrawing()
                {
                    m_Size = authoring.m_Size,
                    m_GroundMesh = authoring.m_Ground,
                    m_GroundMaterial = authoring.m_GroundMaterial,
                    m_RockMesh = authoring.m_RockMesh,
                    m_RockMaterial = authoring.m_RockMaterial,
                    m_FarmerMesh = authoring.m_FarmerMesh,
                    m_FarmerMaterial = authoring.m_FarmerMaterial
                });
            }
        }
    }

    public struct Farm : IComponentData
    {
        public int2 m_Size;
        public float m_RockPercentage;
        public Entity m_DebugCanvas;
    }

    public struct FarmDrawing : IEquatable<FarmDrawing>, ISharedComponentData 
    {
        public Material m_GroundMaterial;
        public Mesh m_GroundMesh;
        public int2 m_Size;
        public Mesh m_RockMesh;
        public Material m_RockMaterial;
        public Mesh m_FarmerMesh;
        public Material m_FarmerMaterial;


        public bool Equals(FarmDrawing other)
        {
            return m_GroundMesh == other.m_GroundMesh &&
                   m_GroundMaterial == other.m_GroundMaterial &&
                   m_Size.Equals(other.m_Size) &&
                   m_RockMesh == other.m_RockMesh &&
                   m_RockMaterial == other.m_RockMaterial &&
                   m_FarmerMaterial == other.m_FarmerMaterial &&
                   m_FarmerMesh == other.m_FarmerMesh;
                  
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (m_GroundMaterial != null ? m_GroundMaterial.GetHashCode() : 0);
            hash = hash * 23 + (m_GroundMesh != null ? m_GroundMesh.GetHashCode() : 0);
            hash = hash * 23 + m_Size.GetHashCode();
            hash = hash * 23 + (m_RockMaterial != null ? m_RockMaterial.GetHashCode() : 0);
            hash = hash * 23 + (m_RockMesh != null ? m_RockMesh.GetHashCode() : 0);
            hash = hash * 23 + (m_FarmerMaterial != null ? m_FarmerMaterial.GetHashCode() : 0);
            hash = hash * 23 + (m_FarmerMesh != null ? m_FarmerMesh.GetHashCode() : 0);
            //hash = hash * 23 + (m_DebugCanvas != null ? m_DebugCanvas.GetHashCode() : 0);

            return hash;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}

