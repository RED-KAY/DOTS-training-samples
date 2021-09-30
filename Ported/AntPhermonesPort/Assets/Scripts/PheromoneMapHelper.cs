﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PheromoneMapHelper
{
    public GridHelper grid;
    public DynamicBuffer<PheromoneMap> pheromoneMap;

    public bool IsInitialized()
    {
        return pheromoneMap.Length > 0;
    }

    public PheromoneMapHelper(DynamicBuffer<PheromoneMap> _cellmap, int _gridSize, float _worldSize)
    {
        pheromoneMap = _cellmap;
        grid = new GridHelper(_gridSize, _worldSize);
    }

    public void InitPheromoneMap()
    {
        pheromoneMap.Length = grid.gridDimLength * grid.gridDimLength;

        // UNCOMMENT TO GET A TEXTURE TEST
        //for (int x = 0; x < gridSize / 2; x++)
        //{
        //    for (int y = 0; y < gridSize / 2; ++y)
        //    {
        //        pheromoneMap.ElementAt(y * gridSize + x).intensity = new float4(0, 0.2f, 0, 1);
        //    }
        //}

        //for (int x = gridSize / 2; x < gridSize; x++)
        //{
        //    for (int y = gridSize / 2; y < gridSize; ++y)
        //    {
        //        pheromoneMap.ElementAt(y * gridSize + x).intensity = new float4(0, 0.6f, 0, 1);
        //    }
        //}

        //// Optimize
        for (int i = 0; i < pheromoneMap.Length; ++i)
        {
            pheromoneMap.ElementAt(i).intensity = new float4(0, 0, 0, 1);
        }
    }

    public float GetPheromoneIntensityFrom2DPos(float2 xy)
    {
        int index = grid.GetNearestIndex(xy);
        if (index < 0 || index > pheromoneMap.Length)
        {
            //Debug.LogError(string.Format("[Cell Map] Position is outside cell map {0}, {1}", xy.x, xy.y));
            return -1;
        }

        return pheromoneMap[index].intensity.x;
    }

    public void Set(int x, int y, float state)
    {
        pheromoneMap.ElementAt(y * grid.gridDimLength + x).intensity = state;
    }

    public float Get(int x, int y)
    {
        return pheromoneMap[y * grid.gridDimLength + x].intensity.x;
    }

    public void IncrementIntensity(float2 position, float increaseValue, float debugLineTime = 0)
    {
        int index = grid.GetNearestIndex(position);
        if (index == -1 || index >= pheromoneMap.Length)
            return;

        float4 currentValue = pheromoneMap[index].intensity;

        pheromoneMap.ElementAt(index).intensity = new float4(
            math.clamp(currentValue.x + increaseValue, 0, 1), 0, 0, 1
        );

        if(debugLineTime != 0) grid.DrawDebugRay(index, new Color(1, 0, 0.2f, 1), debugLineTime);
    }

    public void DecrementIntensity(float decreaseValue)
    {
        for(int i = 0; i<pheromoneMap.Length; i++)
        {
            var newIntensity = pheromoneMap[i].intensity.x - decreaseValue;
            if (newIntensity < 0)
                newIntensity = 0;

            pheromoneMap.ElementAt(i).intensity.x = newIntensity;
        }
    }
}
