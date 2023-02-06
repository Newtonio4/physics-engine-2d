using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public enum ShapeType
    {
        Circle = 0,
        Box = 1,
    }
    public sealed  class FlatBody
    {
        private FlatVector position;
        private FlatVector linearVelocity;
        private float angle;
        private float angularVelocity;
        private FlatVector force;

        public readonly ShapeType ShapeType;
        public readonly float Density;
        public readonly float Mass;
        public readonly float InvMass;
        public readonly float Restitution;
        public readonly float Area;
        public readonly bool IsStatic;
        public readonly float Radius;
        public readonly float Width;
        public readonly float Height;
        public readonly float Inertia;
        public readonly float InvInertia;

        private readonly FlatVector[] vertices;
        private FlatVector[] transformedVertices;
        private FlatAABB aabb;

        private bool transformUpdateRequired;
        private bool aabbUpdateRequired;

        public FlatVector Position
        {
            get { return position; }
        }

        public float Angle
        {
            get { return angle; }
        }

        public FlatVector LinearVelocity
        {
            get { return linearVelocity; }
            set { linearVelocity = value; }
        }

        private FlatBody(float density, float mass, float inertia, float restitution, float area, bool isStatic, float radius, float width, float height, FlatVector[] vertices, ShapeType shapeType)
        {
            this.position = FlatVector.Zero;
            this.linearVelocity = FlatVector.Zero;
            this.angle = 0;
            this.angularVelocity = 0;
            this.force = FlatVector.Zero;

            this.ShapeType = shapeType;
            this.Density = density;
            this.Mass = mass;
            this.InvMass = mass > 0f ? 1f / mass : 0f;
            this.Inertia = inertia;
            this.InvInertia = inertia > 0f ? 1f / inertia : 0f;
            this.Restitution = restitution;
            this.Area = area;
            this.IsStatic = isStatic;
            this.Radius = radius;
            this.Width = width;
            this.Height = height;


            if (this.ShapeType is ShapeType.Box)
            {
                this.vertices = vertices;
                this.transformedVertices = new FlatVector[this.vertices.Length];
            }
            else
            {
                this.vertices = null;
                this.transformedVertices = null;
            }

            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        private float CalculateRotationalInertia()
        {
            if (ShapeType == ShapeType.Circle)
            {
                return (1f / 2f) * Mass * Radius * Radius;
            }
            else if (ShapeType == ShapeType.Box)
            {
                return (1f / 12f) * Mass * (Width * Width + Height * Height);
            }
            else
            {
                throw new ArgumentException("Wrong ShapeType");
            }
        }

        private static FlatVector[] CreateBoxVertices(float width, float height)
        {
            float left = -width / 2f;
            float right = left + width;
            float bottom = -height / 2f;
            float top = bottom + height;

            return new FlatVector[]
            {
                new FlatVector(left, top),
                new FlatVector(right, top),
                new FlatVector(right, bottom),
                new FlatVector(left, bottom)
            };
        }

        /*private static int[] CreateBoxTriangles()
        {
            int[] tri = new int[6];
            tri[0] = 0;
            tri[1] = 1;
            tri[2] = 2;
            tri[3] = 0;
            tri[4] = 2;
            tri[5] = 3;

            return tri;
        }*/

        public FlatVector[]? GetTransformVertices()
        {
            if (this.transformUpdateRequired)
            {
                FlatTransform transform = new FlatTransform(this.position, this.angle);

                for (int i = 0; i < this.vertices.Length; i++)
                    this.transformedVertices[i] = FlatVector.Transform(this.vertices[i], transform);

                this.transformUpdateRequired = false;
            }

            return this.transformedVertices;
        }

        public FlatAABB GetAABB()
        {
            if (aabbUpdateRequired)
            {
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;

                if (this.ShapeType is ShapeType.Box)
                {
                    FlatVector[] vertices = GetTransformVertices();

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        FlatVector v = vertices[i];

                        if (v.X < minX) minX = v.X;
                        if (v.X > maxX) maxX = v.X;
                        if (v.Y < minY) minY = v.Y;
                        if (v.Y > maxY) maxY = v.Y;
                    }
                }
                else if (this.ShapeType is ShapeType.Circle)
                {
                    minX = this.position.X - this.Radius;
                    minY = this.position.Y - this.Radius;
                    maxX = this.position.X + this.Radius;
                    maxY = this.position.Y + this.Radius;
                }
                else
                {
                    throw new ArgumentException("Unknown ShapeType.");
                }

                aabb = new FlatAABB(minX, minY, maxX, maxY);
                aabbUpdateRequired = false;
            }

            return aabb;
        }

        internal void Step(float time, FlatVector gravity, int iterations)
        {
            if (IsStatic)
                return;

            time /= (float)iterations;

            this.linearVelocity += gravity * time;
            this.position += this.linearVelocity * time;
            this.angle += this.angularVelocity * time;

            this.force = FlatVector.Zero;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void Move(FlatVector amount)
        {
            this.position += amount;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void MoveTo(FlatVector position)
        {
            this.position = position;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void Rotate(float angle)
        {
            this.angle += angle;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void RotateTo(float angle)
        {
            this.angle = angle;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void AddForce(FlatVector amount)
        {
            this.force = amount;
        }

        public static bool CreateCircleBody(float radius, float density, bool isStatic, float restitution, out FlatBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            float area = radius * radius * MathF.PI;

            if (area < FlatWorld.MinBodySize || area > FlatWorld.MaxBodySize)
            {
                errorMessage = $"Minimum body size = {FlatWorld.MinBodySize}, Maximum body size = {FlatWorld.MaxBodySize}";
                return false;
            }

            if (density < FlatWorld.MinDensity || density > FlatWorld.MaxDensity)
            {
                errorMessage = $"Minimum body density = {FlatWorld.MinDensity}, Maximum density size = {FlatWorld.MaxDensity}";
                return false;
            }

            restitution = FlatMath.Clamp(restitution, 0f, 1f);

            float mass;
            float inertia;

            if (!isStatic)
            {
                mass = density * area;
                inertia = (1f / 2f) * mass * radius * radius;
            }
            else
            {
                mass = 0f;
                inertia = 0f;
            }

            body = new FlatBody(density, mass, inertia, restitution, area, isStatic, radius, 0f, 0f, null, ShapeType.Circle);
            return true;
        }

        public static bool CreateBoxBody(float width, float height, float density, bool isStatic, float restitution, out FlatBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            float area = width * height;

            if (area < FlatWorld.MinBodySize || area > FlatWorld.MaxBodySize)
            {
                errorMessage = $"Minimum body size = {FlatWorld.MinBodySize}, Maximum body size = {FlatWorld.MaxBodySize}";
                return false;
            }

            if (density < FlatWorld.MinDensity || density > FlatWorld.MaxDensity)
            {
                errorMessage = $"Minimum body density = {FlatWorld.MinDensity}, Maximum density size = {FlatWorld.MaxDensity}";
                return false;
            }

            restitution = FlatMath.Clamp(restitution, 0f, 1f);

            float mass;
            float inertia;

            if (!isStatic)
            {
                mass = density * area;
                inertia = (1f / 12f) * mass * (width * width + height * height);
            }
            else
            {
                mass = 0f;
                inertia = 0f;
            }

            body = new FlatBody(density, mass, inertia, restitution, area, isStatic, 0f, width, height, CreateBoxVertices(width, height), ShapeType.Box);
            return true;
        }
    }
}
