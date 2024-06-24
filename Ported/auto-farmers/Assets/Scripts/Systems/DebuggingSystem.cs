//using AutoFarmers.Farm;
//using Unity.Burst;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;


//namespace AutoFarmers.Tools
//{
//    using Farm = AutoFarmers.Farm.Farm;
//    [UpdateBefore(typeof(TransformSystemGroup)), BurstCompile]
//    public partial struct DebuggingSystem : ISystem, ISystemStartStop
//    {
//        private Farm _system;

//        [BurstCompile]
//        public void OnCreate(ref SystemState state)
//        {
//            state.RequireForUpdate<Farm>();
//            state.RequireForUpdate<Tile>();
//        }

//        [BurstCompile]
//        public void OnStartRunning(ref SystemState state)
//        {

//            _system = SystemAPI.GetSingleton<Farm>();

//            foreach (var entity in SystemAPI.Query<RefRO<Tile>>())
//            {
//                //Debug.Log("comes here");
//                var canvasInstance = state.EntityManager.Instantiate(_system.m_DebugCanvas);
                
//                var position = entity.ValueRO.m_Position + new float3(0, 0.3f, 0);
//                //EntityManager.AddComponent<LocalTransform>(canvasInstance);
//                state.EntityManager.SetComponentData<LocalTransform>(canvasInstance, new LocalTransform { Position = position, Rotation = quaternion.identity, Scale = 0.1f });

//                state.EntityManager.SetComponentData(canvasInstance, new DebugCanvas { _value = entity.ValueRO.m_Id });
//            }
//        }


//        [BurstCompile]
//        public void OnStopRunning(ref SystemState state)
//        {
            
//        }
//    }
//}