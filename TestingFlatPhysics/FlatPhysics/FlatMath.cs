using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public static class FlatMath
    {
        public static float Clamp(float value, float min, float max)
        {
            if (min == max) return value;
            if (min > max) throw new ArgumentException("Min value should be less than max value.");
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (min == max) return value;
            if (min > max) throw new ArgumentException("Min value should be less than max value.");
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float LengthSquared(FlatVector v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        public static float DistanceSquared(FlatVector a, FlatVector b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public static float Length(FlatVector v)
        {
            return MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        public static float Distance(FlatVector a, FlatVector b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public static FlatVector Normalize(FlatVector v)
        {
            float length = Length(v);
            return new FlatVector(v.X / length, v.Y / length);
        }

        public static float Dot(FlatVector a, FlatVector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static float Cross(FlatVector a, FlatVector b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static bool NearlyEqual(float a, float b)
        {
            return MathF.Abs(a - b) < 0.001;
        }

        public static bool NearlyEqual(FlatVector a, FlatVector b)
        {
            return NearlyEqual(a.X, b.X) && NearlyEqual(a.Y, b.Y);
        }
    }
}
