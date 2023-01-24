using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public static class Collisions
    {
        public static bool IntersectCircles(FlatVector centerA, float radiusA, FlatVector centerB, float radiusB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = 0f;

            float dist = FlatMath.Distance(centerA, centerB);

            if (dist >= radiusA + radiusB)
                return false;


            normal = centerB - centerA;
            depth = radiusA + radiusB - dist;
            return true;
        }
    }
}
