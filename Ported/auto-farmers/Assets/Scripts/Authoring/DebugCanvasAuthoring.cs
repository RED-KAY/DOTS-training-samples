using Unity.Entities;
using UnityEngine;

namespace AutoFarmers.Tools
{
    public class DebugCanvasAuthoring : MonoBehaviour
    {
        public class DebugCanvasBaker : Baker<DebugCanvasAuthoring>
        {
            public override void Bake(DebugCanvasAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent<DebugCanvas>(entity);
            }
        }
    }

    public struct DebugCanvas : IComponentData
    {
        public int _value;
    }
}