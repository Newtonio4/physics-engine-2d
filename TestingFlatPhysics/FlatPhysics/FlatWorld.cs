using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public sealed class FlatWorld
    {
        public static readonly float MinBodySize = 0.01f * 0.01f;
        public static readonly float MaxBodySize = 64f * 64f;

        public static readonly float MinDensity = 0.2f;
        public static readonly float MaxDensity = 21.4f;

        public static readonly int MinIterations = 1;
        public static readonly int MaxIterations = 128;

        private List<FlatBody> bodyList = new List<FlatBody>();
        private FlatVector gravity;
        private List<FlatManifold> contactList = new List<FlatManifold>();

        public int BodyCount
        {
            get { return bodyList.Count; }
        }

        public FlatWorld()
        {
            this.gravity = new FlatVector(0f, -9.81f);
        }

        public void AddBody(FlatBody flatBody)
        {
            this.bodyList.Add(flatBody);
        }

        public bool RemoveBody(FlatBody flatBody)
        {
            return this.bodyList.Remove(flatBody);
        }

        public bool GetBody(int index, out FlatBody body)
        {
            body = null;

            if (index < 0 || index > bodyList.Count)
            {
                return false;
            }

            body = bodyList[index];
            return true;
        }

        public void Step(float time, int iterations)
        {
            iterations = FlatMath.Clamp(iterations, MinIterations, MaxIterations);

            for (int current = 0; current < iterations; current++)
            {
                // Movement
                for (int i = 0; i < this.bodyList.Count; i++)
                {
                    bodyList[i].Step(time, gravity, iterations);
                }

                contactList.Clear();

                // Collision
                for (int i = 0; i < bodyList.Count - 1; i++)
                {
                    FlatBody bodyA = bodyList[i];

                    for (int j = i + 1; j < bodyList.Count; j++)
                    {
                        FlatBody bodyB = bodyList[j];

                        if (bodyA.IsStatic && bodyB.IsStatic)
                            continue;

                        if (!Collisions.IntersectAABBs(bodyA.GetAABB(), bodyB.GetAABB()))
                            continue;

                        if (Collisions.Collide(bodyA, bodyB, out FlatVector normal, out float depth))
                        {
                            SeparateBodies(bodyA, bodyB, normal * depth);
                            Collisions.FindContactPoints(bodyA, bodyB, out FlatVector contact1, out FlatVector contact2, out int contactCount);
                            FlatManifold contact = new FlatManifold(bodyA, bodyB, normal, depth, contact1, contact2, contactCount);
                            contactList.Add(contact);
                        }
                    }
                }

                for (int i = 0; i < contactList.Count; i++)
                {
                    FlatManifold contact = contactList[i];
                    ResolveCollision(in contact);
                }
            }
        }

        private void SeparateBodies(FlatBody bodyA, FlatBody bodyB, FlatVector mtv)
        {
            if (bodyA.IsStatic)
            {
                bodyB.Move(mtv);
            }
            else if (bodyB.IsStatic)
            {
                bodyA.Move(-mtv);
            }
            else
            {
                bodyA.Move(-mtv / 2f);
                bodyB.Move(mtv / 2f);
            }
        }

        public void ResolveCollision(in FlatManifold contact)
        {
            FlatBody bodyA = contact.BodyA;
            FlatBody bodyB = contact.BodyB;
            FlatVector normal = contact.Normal;
            float depth = contact.Depth;
            FlatVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (FlatMath.Dot(relativeVelocity, normal) > 0f)
                return;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float j = -(1f + e) * FlatMath.Dot(relativeVelocity, normal);
            j /= (bodyA.InvMass) + (bodyB.InvMass);

            FlatVector impuls = j * normal;

            bodyA.LinearVelocity -= bodyA.InvMass * impuls;
            bodyB.LinearVelocity += bodyB.InvMass * impuls;
        }
    }
}