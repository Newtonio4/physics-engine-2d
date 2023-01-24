using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public readonly struct FlatVector
    {
        public readonly float X;
        public readonly float Y;

        public static readonly FlatVector Zero = new FlatVector(0, 0);

        public FlatVector(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public static FlatVector operator +(FlatVector a, FlatVector b)
        {
            return new FlatVector(a.X + b.X, a.Y + b.Y);
        }

        public static FlatVector operator -(FlatVector a, FlatVector b)
        {
            return new FlatVector(a.X - b.X, a.Y - b.Y);
        }

        public static FlatVector operator -(FlatVector v)
        {
            return new FlatVector(-v.X, -v.Y);
        }

        public static FlatVector operator *(FlatVector v, float s)
        {
            return new FlatVector(v.X * s, v.Y * s);
        }

        public static FlatVector operator /(FlatVector v, float s)
        {
            return new FlatVector(v.X / s, v.Y / s);
        }

        public static FlatVector Transform(FlatVector v, FlatTransform transform)
        {
            /*
            float rx = transform.Cos * v.X - transform.Sin * v.Y;
            float ry = transform.Sin * v.X + transform.Cos * v.Y;

            float tx = rx + transform.PositionX;
            float ty = ry + transform.PositionY;
            */

            return new FlatVector(
                transform.Cos * v.X - transform.Sin * v.Y + transform.PositionX,
                transform.Sin * v.X + transform.Cos * v.Y + transform.PositionY);
        }

        public bool Equals(FlatVector other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is FlatVector other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return new { this.X, this.Y}.GetHashCode();
        }

        public override string ToString()
        {
            return $"X: {this.X}, Y: {this.Y}";
        }
    }
}
