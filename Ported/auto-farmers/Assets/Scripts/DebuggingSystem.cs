using AutoFarmers.Farm;
using Unity.Entities;
using UnityEngine;

namespace AutoFarmers.Tools
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class DebuggingSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<Tile>();
        }

        protected override void OnStartRunning()
        {
            foreach (var entity in SystemAPI.Query<RefRO<Tile>>())
            {
                //Debug.Log("Tile[" + entity.ValueRO.m_Id + "]: " + entity.ValueRO.m_Status);
            }
        }

        protected override void OnUpdate() { }
    }
}