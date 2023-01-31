using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public static class Collisions
    {
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

        public static bool IntersectCirclePolygon(FlatVector circleCenter, float circleRadius, FlatVector[] vertices, out FlatVector normal, out float depth)
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

            FlatVector polygonCenter = FindArithmeticMean(vertices);

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

        public static bool IntersectPolygons(FlatVector[] verticesA, FlatVector[] verticesB, out FlatVector normal, out float depth)
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

            FlatVector centerA = FindArithmeticMean(verticesA);
            FlatVector centerB = FindArithmeticMean(verticesB);

            FlatVector direction = centerB - centerA;

            if (FlatMath.Dot(direction, normal) < 0)
                normal = -normal;

            return true;
        }

        private static FlatVector FindArithmeticMean(FlatVector[] vertices)
        {
            float sumX = 0f;
            float sumY = 0f;

            foreach (FlatVector vertex in vertices)
            {
                sumX += vertex.X;
                sumY += vertex.Y;
            }
            
            return new FlatVector(sumX / vertices.Length, sumY / vertices.Length);
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
