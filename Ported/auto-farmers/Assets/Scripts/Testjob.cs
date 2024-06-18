using Unity.Burst;
using Unity.Entities;

namespace AutoFarmers.Farm
{
    public partial struct Testjob : IJobEntity
    {
        public int m_TileId;
        public bool m_Found;
        
        private void Execute(ref Tile tile)
        {

        }
    }
}