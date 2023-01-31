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

        private List<FlatBody> bodyList = new List<FlatBody>();
        private FlatVector gravity;

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

        public void Step(float time)
        {
            // Movement
            for (int i = 0; i < this.bodyList.Count; i++)
            {
                bodyList[i].Step(time, gravity);
            }

            // Collision
            for (int i = 0; i < bodyList.Count - 1; i++)
            {
                FlatBody bodyA = bodyList[i];
                for (int j = i + 1; j < bodyList.Count; j++)
                {
                    FlatBody bodyB = bodyList[j];

                    if (bodyA.IsStatic && bodyB.IsStatic)
                        continue;

                    if (Collide(bodyA, bodyB, out FlatVector normal, out float depth))
                    {
                        if (bodyA.IsStatic)
                        {
                            bodyB.Move(normal * depth);
                        }
                        else if (bodyB.IsStatic)
                        {
                            bodyA.Move(-normal * depth);
                        }
                        else
                        {
                            bodyA.Move(-normal * depth / 2f);
                            bodyB.Move(normal * depth / 2f);
                        }


                        ResolveCollision(bodyA, bodyB, normal, depth);
                    }
                }
            }
        }

        public void ResolveCollision(FlatBody bodyA, FlatBody bodyB, FlatVector normal, float depth)
        {
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

        public bool Collide(FlatBody bodyA, FlatBody bodyB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = 0f;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectPolygons(
                        bodyA.Position, bodyA.GetTransformVertices(),
                        bodyB.Position,bodyB.GetTransformVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    bool result = Collisions.IntersectCirclePolygon(
                        bodyB.Position, bodyB.Radius,
                        bodyA.Position, bodyA.GetTransformVertices(),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            else if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectCirclePolygon(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.GetTransformVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    return Collisions.IntersectCircles(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.Radius,
                        out normal, out depth);
                }
            }

            return false;
        }
    }
}

/*
                    if (bodyA.ShapeType is ShapeType.Box && bodyB.ShapeType is ShapeType.Circle)
                        if (Collisions.IntersectCirclePolygon(bodyB.Position, bodyB.Radius, bodyA.GetTransformVertices(), out FlatVector normal, out float depth))
                        {
                            this.outlineColors[i] = Color.Red;
                            this.outlineColors[j] = Color.Red;

                            bodyA.Move(normal * depth / 2f);
                            bodyB.Move(-normal * depth / 2f);
                        }

                    if (bodyB.ShapeType is ShapeType.Box && bodyA.ShapeType is ShapeType.Circle)
                        if (Collisions.IntersectCirclePolygon(bodyA.Position, bodyA.Radius, bodyB.GetTransformVertices(), out FlatVector normal, out float depth))
                        {
                            this.outlineColors[i] = Color.Red;
                            this.outlineColors[j] = Color.Red;

                            bodyA.Move(-normal * depth / 2f);
                            bodyB.Move(normal * depth / 2f);
                        }

                    
                    if (Collisions.IntersectPolygons(bodyA.GetTransformVertices(), bodyB.GetTransformVertices(), out FlatVector normal, out float depth))
                    {
                        this.outlineColors[i] = Color.Red;
                        this.outlineColors[j] = Color.Red;

                        bodyA.Move(-normal * depth / 2f);
                        bodyB.Move(normal * depth / 2f);
                    }

                    
                    if (Collisions.IntersectCircles(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.Radius,
                        out FlatVector normal, out float depth))
                    {
                        bodyA.Move(-normal * depth / 2);
                        bodyB.Move(normal * depth / 2);
                    }
                    */
