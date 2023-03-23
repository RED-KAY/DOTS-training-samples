﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(OmniworkerBucketSystem))]
public partial struct OmniworkerWaterSystem : ISystem
{
   private float speed;
   private float3 waterPos;
   private float arriveThreshold;

   public void OnCreate(ref SystemState state)
   {
      state.RequireForUpdate<Config>();
   }

   public void OnUpdate(ref SystemState state)
   {
      var config = SystemAPI.GetSingleton<Config>();
      speed = config.botSpeed;
      arriveThreshold = config.arriveThreshold;

      //for each omniworker that goes for the water with bucket
      foreach (var (omniworkerTag, omniworkerTransform, omniworker) in SystemAPI.Query<OmniworkerGoForWaterTag,LocalTransform>().WithEntityAccess())
      {
         float minDist = float.MaxValue;
         float dist;
         //Get closest water
         foreach (var (water,waterTransform) in SystemAPI.Query<Water,LocalTransform>())
         {
            dist = Vector3.Distance(waterTransform.Position, omniworkerTransform.Position);
            if (dist < minDist)
            {
               minDist = dist;
               waterPos = waterTransform.Position;
            }
         }
         
         float3 dir = waterPos - omniworkerTransform.Position;
         Vector3 waterPosWithoutY = new Vector3(waterPos.x, 0f, waterPos.z);
         Vector3 omniworkerPosWithoutY = new Vector3(omniworkerTransform.Position.x, 0f, omniworkerTransform.Position.z);
         dist = Vector3.Distance(waterPosWithoutY, omniworkerPosWithoutY);
         dir = Vector3.Normalize(dir);
      
         if (dist > arriveThreshold)
         {
            var omniworkerPosition = SystemAPI.GetComponent<LocalTransform>(omniworker);
            var prev = omniworkerPosition.Position.y;
            omniworkerPosition.Position += dir * SystemAPI.Time.DeltaTime * speed;
            omniworkerPosition.Position.y = prev;
            SystemAPI.SetComponent(omniworker, omniworkerPosition);
         }
         else
         {
            SystemAPI.SetComponentEnabled<OmniworkerGoForWaterTag>(omniworker, false);
            SystemAPI.SetComponentEnabled<OmniworkerGoForFireTag>(omniworker, true);
            //fill the bucket with water and change tag
            //another system for looking for fire
         }
         
      }
   }
}
