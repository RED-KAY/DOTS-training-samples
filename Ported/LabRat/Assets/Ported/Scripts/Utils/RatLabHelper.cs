using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Ported.Scripts.Utils
{
    [BurstCompile]
    public struct RatLabHelper
    {
        public static MovementDirection NextDirection(MovementDirection currentDirection)
        {
            switch (currentDirection)
            {
                case MovementDirection.North:
                    return MovementDirection.East;
                case MovementDirection.South:
                    return MovementDirection.West;
                case MovementDirection.East:
                    return MovementDirection.South;
                case MovementDirection.West:
                    return MovementDirection.North;
            }

            return MovementDirection.North;
        }
        
        /*
         *  public enum MovementDirection
            {
                North = 0,
                East = 1,
                South = 2,
                West = 3
            }
         */
        
        public static void DirectionToVector(out float2 output, MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.North:
                    output.x = 0;
                    output.y = -1;
                    break;
                case MovementDirection.South:
                    output.x = 0;
                    output.y = 1;
                    break;
                case MovementDirection.East:
                    output.x = 1;
                    output.y = 0;
                    break;
                case MovementDirection.West:
                    output.x = -1;
                    output.y = 0;
                    break;
                default:
                    output = float2.zero;
                    break;
            }
        }
        
        public static bool CollidesAABB(in float2 a, in float2 b, in float2 aSize, in float2 bSize)
        {
            float2 aHalfSize = aSize / 2;
            float2 aTopLeft = a - aHalfSize;
            float2 aBottomRight = a + aHalfSize;
                
            float2 bHalfSize = bSize / 2;
            float2 bTopLeft = b - bHalfSize;
            float2 bBottomRight = b + bHalfSize;

            if (aTopLeft.x >= bTopLeft.x && aBottomRight.x <= bBottomRight.x && aTopLeft.y >= bTopLeft.y && aBottomRight.y <= bBottomRight.y)
            {
                return true;
            }

            return false;
        }
    }
}