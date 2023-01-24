using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        public readonly float Density;
        public readonly float Mass;
        public readonly float Restitution;
        public readonly float Area;

        public readonly bool IsStatic;

        public readonly float Radius;
        public readonly float Width;
        public readonly float Height;

        public readonly ShapeType ShapeType;

        public FlatVector Position
        {
            get { return position; }
        }

        private FlatBody(FlatVector position, float density, float mass, float restitution, float area, bool isStatic, float radius, float width, float height, ShapeType shapeType)
        {
            this.position = position;
            this.Density = density;
            this.Mass = mass;
            this.Restitution = restitution;
            this.Area = area;
            this.IsStatic = isStatic;
            this.Radius = radius;
            this.Width = width;
            this.Height = height;
            this.ShapeType = shapeType;
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
