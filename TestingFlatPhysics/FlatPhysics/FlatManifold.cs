using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public readonly struct FlatManifold
    {
        public readonly FlatBody BodyA;
        public readonly FlatBody BodyB;
        public readonly FlatVector Normal;
        public readonly float Depth;
        public readonly FlatVector Contact1;
        public readonly FlatVector Contact2;
        public readonly int ContactCount;

        public FlatManifold(FlatBody bodyA, FlatBody bodyB, FlatVector normal, float depth, FlatVector contact1, FlatVector contact2, int contactCount)
        {
            BodyA = bodyA;
            BodyB = bodyB;
            Normal = normal;
            Depth = depth;
            Contact1 = contact1;
            Contact2 = contact2;
            ContactCount = contactCount;
        }
    }
}
