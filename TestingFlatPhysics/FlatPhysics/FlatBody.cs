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
        private float rotation;
        private float rotationVelocity;

        private FlatVector force;

        public readonly float Density;
        public readonly float Mass;
        public readonly float Restitution;
        public readonly float Area;

        public readonly bool IsStatic;

        public readonly float Radius;
        public readonly float Width;
        public readonly float Height;
        public readonly float InvMass;

        public readonly ShapeType ShapeType;

        private readonly FlatVector[] vertices = new FlatVector[4];
        public readonly int[] Triangles = new int[6];
        private FlatVector[] transformedVertices = new FlatVector[4];
        private FlatAABB aabb;

        private bool transformUpdateRequired;
        private bool aabbUpdateRequired;

        public FlatVector Position
        {
            get { return position; }
        }

        public FlatVector LinearVelocity
        {
            get { return linearVelocity; }
            set { linearVelocity = value; }
        }

        private FlatBody(FlatVector position, float density, float mass, float restitution, float area, bool isStatic, float radius, float width, float height, ShapeType shapeType)
        {
            this.position = position;
            this.linearVelocity = FlatVector.Zero;
            this.rotation = 0;
            this.rotationVelocity = 0;

            this.force = FlatVector.Zero;

            this.Density = density;
            this.Mass = mass;
            this.Restitution = restitution;
            this.Area = area;

            this.IsStatic = isStatic;

            this.Radius = radius;
            this.Width = width;
            this.Height = height;

            this.ShapeType = shapeType;

            if (!isStatic)
                this.InvMass = 1 / this.Mass;
            else
                this.InvMass = 0f;

            if (this.ShapeType is ShapeType.Box)
            {
                this.vertices = CreateBoxVertices(this.Width, this.Height);
                this.Triangles = CreateBoxTriangles();
                this.transformedVertices = new FlatVector[this.vertices.Length];
            }

            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        private FlatVector[] CreateBoxVertices(float width, float height)
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

        private static int[] CreateBoxTriangles()
        {
            int[] tri = new int[6];
            tri[0] = 0;
            tri[1] = 1;
            tri[2] = 2;
            tri[3] = 0;
            tri[4] = 2;
            tri[5] = 3;

            return tri;
        }

        public FlatVector[]? GetTransformVertices()
        {
            if (this.transformUpdateRequired)
            {
                FlatTransform transform = new FlatTransform(this.position, this.rotation);

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
            this.rotation += this.rotationVelocity * time;

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
            this.rotation += angle;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void AddForce(FlatVector amount)
        {
            this.force = amount;
        }

        public static bool CreateCircleBody(float radius, FlatVector position, float density, bool isStatic, float restitution, out FlatBody body, out string errorMessage)
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

            body = new FlatBody(position, density, density * area, restitution, area, isStatic, radius, 0f, 0f, ShapeType.Circle);
            return true;
        }

        public static bool CreateBoxBody(float width, float height, FlatVector position, float density, bool isStatic, float restitution, out FlatBody body, out string errorMessage)
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

            body = new FlatBody(position, density, density * area, restitution, area, isStatic, 0f, width, height, ShapeType.Box);
            return true;
        }
    }
}
