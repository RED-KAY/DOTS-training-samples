using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ConfigAuthoring : MonoBehaviour
{
    #region Prefabs
    
    public GameObject bucketPrefab;
    public GameObject botPrefab;
    public GameObject flameCellPrefab;
    public GameObject waterPrefab;
    
    #endregion
    
    #region Bot Params
    
    [Range(0.0001f,1f)]
    public float botSpeed = 0.1f;
    
    [Range(0.001f,1f)]
    public float waterCarryEffect = 0.5f;

    public int numOmnibots = 0;
    
    #endregion
    
    #region Fire Params

    [Tooltip("How many random fires do you want to battle?")]
    public int startingFireCount = 1;
    [Tooltip("How high the flames reach at max temperature")]
    public float maxFlameHeight = 0.1f;
    [Tooltip("Size of an individual flame. Full grid will be (rows * cellSize)")]
    public float cellSize = 0.05f;
    [Tooltip("How many cells WIDE the simulation will be")]
    public int numRows = 20;
    [Tooltip("How many cells DEEP the simulation will be")]
    public int numColumns = 20;
    [Tooltip("When temperature reaches *flashpoint* the cell is on fire")]
    public float flashpoint = 0.5f;
    [Tooltip("How far does heat travel? Note: Higher heat radius significantly increases CPU usafge")]
    public int heatRadius = 1;
    [Tooltip("How fast will adjascent cells heat up?")]
    public float heatTransferRate = 0.7f;
    
    [Range(0.0001f, 2f)]
    public float fireSimUpdateRate = 0.5f;

    #endregion

    #region Water Params

    [Range(1,5)]
    [Tooltip("Number of cells affected by a bucket of water")]
    public int splashRadius = 3;
    [Tooltip("Water bucket reduces fire temperature by this amount")]
    public float coolingStrength = 1f;
    [Tooltip("Splash damage of water bucket. (1 = no loss of power over distance)")]
    public float coolingStrengthFalloff = 0.75f;
    [Tooltip("Water sources will refill by this amount per second")]
    public float refillRate = 0.0001f;
    [Range(0, 100)]
    public int totalBuckets = 3;
    [Tooltip("How much water does a bucket hold?")]
    public float bucketCapacity = 3f;
    [Tooltip("Buckets fill up by this much per second")]
    public float bucketFillRate = 0.01f;
    [Tooltip("Visual scale of bucket when EMPTY (no effect on water capacity)")]
    public float bucketSizeEmpty = 0.2f;
    [Tooltip("Visual scale of bucket when FULL (no effect on water capacity)")]
    public float bucketSizeFull = 0.4f;
    
    #endregion
    
    
    class Baker : Baker <ConfigAuthoring> 
    {

        public override void Bake(ConfigAuthoring authoring)
        {
            AddComponent(new Config
            {
                bucketPrefab = GetEntity(authoring.bucketPrefab),
                botPrefab = GetEntity(authoring.botPrefab),
                flameCellPrefab = GetEntity(authoring.flameCellPrefab),
                waterPrefab = GetEntity(authoring.waterPrefab),
                botSpeed =  authoring.botSpeed,
                waterCarryEffect = authoring.waterCarryEffect,
                numOmnibots = authoring.numOmnibots,
                startingFireCount = authoring.startingFireCount,
                maxFlameHeight = authoring.maxFlameHeight,
                cellSize = authoring.cellSize,
                numRows = authoring.numRows,
                numColumns = authoring.numColumns,
                flashpoint = authoring.flashpoint,
                heatRadius = authoring.heatRadius,
                heatTransferRate = authoring.heatTransferRate,
                fireSimUpdateRate = authoring.fireSimUpdateRate,
                splashRadius = authoring.splashRadius,
                coolingStrength = authoring.coolingStrength,
                coolingStrengthFalloff = authoring.coolingStrengthFalloff,
                refillRate = authoring.refillRate,
                totalBuckets = authoring.totalBuckets,
                bucketCapacity = authoring.bucketCapacity,
                bucketFillRate = authoring.bucketFillRate,
                bucketSizeEmpty = authoring.bucketSizeEmpty,
                bucketSizeFull = authoring.bucketSizeFull
            });
        }
    }

    public struct Config : IComponentData
    {
    
        public Entity bucketPrefab;
        public Entity botPrefab;
        public Entity flameCellPrefab;
        public Entity waterPrefab;
        public float botSpeed;
        public float waterCarryEffect;
        public int numOmnibots;
        public int startingFireCount;
        public float maxFlameHeight;
        public float cellSize;
        public int numRows;
        public int numColumns;
        public float flashpoint;
        public int heatRadius;
        public float heatTransferRate;
        public float fireSimUpdateRate;
        public int splashRadius;
        public float coolingStrength;
        public float coolingStrengthFalloff;
        public float refillRate;
        public int totalBuckets;
        public float bucketCapacity;
        public float bucketFillRate;
        public float bucketSizeEmpty;
        public float bucketSizeFull;
    }
}
