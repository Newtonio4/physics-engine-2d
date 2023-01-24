using FlatPhysics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace TestingFlatPhysics
{
    public static class FlatConverter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(FlatVector v)
        {
            return new Vector2(v.X, v.Y);
        }
    }
}
