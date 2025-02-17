using Trove.PolymorphicStructs;
using Unity.Collections;
using Unity.Transforms;

namespace AutoFarmers.Farmer
{
    [PolymorphicStructInterface]
    public interface IFarmerState
    {
        void Execute();
        void OnEnter();
        void OnExit();
    }

    [PolymorphicStruct]
    public partial struct DoNothingState : IFarmerState
    {
        public void Execute()
        {
            
        }

        public void OnEnter()
        {
            
        }

        public void OnExit()
        {
            
        }
    }
    
    [PolymorphicStruct]
    public partial struct MovingState : IFarmerState
    {
        private int m_TargetDestination;
        public float m_Speed;
        public float m_StoppingDistance;
        
        public void OnEnter()
        {
            
        }
        public void Execute()
        {
            
        }

        public void OnExit()
        {
            
        }
    }
    
    [PolymorphicStruct]
    public partial struct RockMiningState : IFarmerState
    {
        private int m_RockId;
        public float m_Damage;
        public float m_HitRate;

        public void OnEnter()
        {
            
        }
        public void Execute()
        {
            
        }

        public void OnExit()
        {
            
        }
    }

    [PolymorphicStruct]
    public partial struct TilSoilState : IFarmerState
    {
        private NativeArray<int> m_Tiles;

        public void OnEnter()
        {
            
        }

        public void Execute()
        {
            
        }

        public void OnExit()
        {
            
        }
    }

    [PolymorphicStruct]
    public partial struct TaskHuntingState : IFarmerState
    {
        public float m_InitialRadius;
        public float m_MaxRadius;
        private float m_CurrentRadius;
        public float m_Increment;

        public void OnEnter()
        {
        }

        public void Execute()
        {
            
        }

        public void OnExit()
        {
            
        }
    }
    
}