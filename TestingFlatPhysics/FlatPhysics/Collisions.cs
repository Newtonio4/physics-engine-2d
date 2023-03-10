using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public static class Collisions
    {
        public static bool IntersectAABBs(FlatAABB a, FlatAABB b)
        {
            if (a.Max.X <= b.Min.X ||
                b.Max.X <= a.Min.X ||
                a.Max.Y <= b.Min.Y ||
                b.Max.Y <= a.Min.Y)
            {
                return false;
            }

            return true;
        }

        public static void FindContactPoints(FlatBody bodyA, FlatBody bodyB, out FlatVector contact1, out FlatVector contact2, out int contactCount)
        {
            contact1 = FlatVector.Zero;
            contact2 = FlatVector.Zero;
            contactCount = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    FindPolygonsContactPoints(bodyA.GetTransformVertices(), bodyB.GetTransformVertices(), out contact1, out contact2, out contactCount);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    FindCirclePolygonContactPoint(bodyB.Position, bodyB.Radius, bodyA.Position, bodyA.GetTransformVertices(), out contact1);
                    contactCount = 1;
                }
            }
            else if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    FindCirclePolygonContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.GetTransformVertices(), out contact1);
                    contactCount = 1;
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    FindCirclesContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.Radius, out contact1);
                    contactCount = 1;
                }
            }
        }

        private static void FindCirclesContactPoint(FlatVector centerA, float radiusA, FlatVector centerB, float radiusB, out FlatVector contactPoint)
        {
            FlatVector ab = centerB - centerA;
            contactPoint = FlatMath.Normalize(ab) * radiusA + centerA;
        }

        private static void FindCirclePolygonContactPoint(FlatVector circleCenter, float circleRadius, FlatVector polygonCenter, FlatVector[] vertices, out FlatVector contactPoint)
        {
            contactPoint = FlatVector.Zero;
            float minDistanseSquared = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                FlatVector va = vertices[i];
                FlatVector vb = vertices[(i + 1) % vertices.Length];

                PointSegmentDistance(circleCenter, va, vb, out float distanseSquared, out FlatVector contact);
                if (distanseSquared < minDistanseSquared)
                {
                    minDistanseSquared = distanseSquared;
                    contactPoint = contact;
                }
            }
        }

        private static void FindPolygonsContactPoints(FlatVector[] verticesA, FlatVector[] verticesB, out FlatVector contact1, out FlatVector contact2, out int contactCount)
        {
            contact1 = FlatVector.Zero;
            contact2 = FlatVector.Zero;
            contactCount = 0;

            float minDistanseSquared = float.MaxValue;

            for (int i = 0; i < verticesA.Length; i++)
            {
                FlatVector p = verticesA[i];
                for (int j = 0; j < verticesB.Length; j++)
                {
                    FlatVector va = verticesB[j];
                    FlatVector vb = verticesB[(j + 1) % verticesB.Length];

                    PointSegmentDistance(p, va, vb, out float distanseSquared, out FlatVector contact);

                    if (FlatMath.NearlyEqual(distanseSquared, minDistanseSquared))
                    {
                        if (!FlatMath.NearlyEqual(contact, contact1))
                        {
                            contact2 = contact;
                            contactCount = 2;
                        }
                    }
                    else if (distanseSquared < minDistanseSquared)
                    {
                        minDistanseSquared = distanseSquared;
                        contact1 = contact;
                        contactCount = 1;
                    }
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                FlatVector p = verticesB[i];
                for (int j = 0; j < verticesA.Length; j++)
                {
                    FlatVector va = verticesA[j];
                    FlatVector vb = verticesA[(j + 1) % verticesA.Length];

                    PointSegmentDistance(p, va, vb, out float distanseSquared, out FlatVector contact);

                    if (FlatMath.NearlyEqual(distanseSquared, minDistanseSquared))
                    {
                        if (!FlatMath.NearlyEqual(contact, contact1))
                        {
                            contact2 = contact;
                            contactCount = 2;
                        }
                    }
                    else if (distanseSquared < minDistanseSquared)
                    {
                        minDistanseSquared = distanseSquared;
                        contact1 = contact;
                        contactCount = 1;
                    }
                }
            }
        }

        public static void PointSegmentDistance(FlatVector p, FlatVector a, FlatVector b, out float distanceSquared, out FlatVector closestPoint)
        {
            FlatVector ab = b - a;
            FlatVector ap = p - a;

            float proj = FlatMath.Dot(ap, ab);
            float abLenSq = FlatMath.LengthSquared(ab);
            float d = proj / abLenSq;

            if (d < 0f)
                closestPoint = a;
            else if (d >= 1f)
                closestPoint = b;
            else
                closestPoint = a + ab * d;

            distanceSquared = FlatMath.DistanceSquared(p, closestPoint);
        }

        public static bool Collide(FlatBody bodyA, FlatBody bodyB, out FlatVector normal, out float depth)
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
                        bodyB.Position, bodyB.GetTransformVertices(),
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

        public static bool IntersectCirclePolygon(FlatVector circleCenter, float circleRadius, FlatVector polygonCenter, FlatVector[] vertices, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = float.MaxValue;
            FlatVector axis = FlatVector.Zero;
            float axisDepth = 0f;
            float minA, minB, maxA, maxB;

            for (int i = 0; i < vertices.Length; i++)
            {
                FlatVector va = vertices[i];
                FlatVector vb = vertices[(i + 1) % vertices.Length];

                FlatVector edge = vb - va;
                axis = new FlatVector(-edge.Y, edge.X);
                axis = FlatMath.Normalize(axis);

                ProjectVertices(vertices, axis, out minA, out maxA);
                ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            FlatVector cp = vertices[FindClosestPointOnPolygon(circleCenter, vertices)];
            axis = cp - circleCenter;
            axis = FlatMath.Normalize(axis);

            ProjectVertices(vertices, axis, out minA, out maxA);
            ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

            if (minA >= maxB || minB >= maxA)
            {
                return false;
            }

            axisDepth = MathF.Min(maxB - minA, maxA - minB);

            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }

            FlatVector direction = polygonCenter - circleCenter;

            if (FlatMath.Dot(direction, normal) < 0)
                normal = -normal;

            return true;
        }

        public static bool IntersectPolygons(FlatVector centerA, FlatVector[] verticesA, FlatVector centerB, FlatVector[] verticesB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = float.MaxValue;
            FlatVector axis = FlatVector.Zero;
            float axisDepth = 0f;
            float minA, minB, maxA, maxB;

            for (int i = 0; i < verticesA.Length; i++)
            {
                FlatVector va = verticesA[i];
                FlatVector vb = verticesA[(i + 1) % verticesA.Length];

                FlatVector edge = vb - va;
                axis = new FlatVector(-edge.Y, edge.X);
                axis = FlatMath.Normalize(axis);

                ProjectVertices(verticesA, axis, out minA, out maxA);
                ProjectVertices(verticesB, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                FlatVector va = verticesB[i];
                FlatVector vb = verticesB[(i + 1) % verticesB.Length];

                FlatVector edge = vb - va;
                axis = new FlatVector(-edge.Y, edge.X);
                axis = FlatMath.Normalize(axis);

                ProjectVertices(verticesA, axis, out minA, out maxA);
                ProjectVertices(verticesB, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            FlatVector direction = centerB - centerA;

            if (FlatMath.Dot(direction, normal) < 0)
                normal = -normal;

            return true;
        }


        private static int FindClosestPointOnPolygon(FlatVector circleCenter, FlatVector[] vertices)
        {
            int result = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                float distnce = FlatMath.Distance(vertices[i], circleCenter);
                if (distnce < minDistance)
                {
                    minDistance = distnce;
                    result = i;
                }
            }

            return result;
        }

        private static void ProjectCircle(FlatVector center, float radius, FlatVector axis, out float min, out float max)
        {
            FlatVector radiusDirection = FlatMath.Normalize(axis) * radius;

            FlatVector p1 = center + radiusDirection;
            FlatVector p2 = center - radiusDirection;

            min = FlatMath.Dot(p1, axis);
            max = FlatMath.Dot(p2, axis);

            if (min > max)
            {
                float t = max;
                max = min;
                min = t;
            }
        }

        private static void ProjectVertices(FlatVector[] vertices, FlatVector axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                FlatVector v = vertices[i];
                float projection = FlatMath.Dot(v, axis);

                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }
        }

        public static bool IntersectCircles(
            FlatVector centerA, float radiusA,
            FlatVector centerB, float radiusB,
            out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = 0f;

            float distance = FlatMath.Distance(centerA, centerB);
            float radii = radiusA + radiusB;

            if (distance >= radii)
            {
                return false;
            }

            normal = FlatMath.Normalize(centerB - centerA);
            depth = radii - distance;

            return true;
        }
    }
}
