﻿using FlatPhysics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Flat.Graphics;

namespace TestingFlatPhysics
{
    public static class FlatConverter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(FlatVector v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static void ToVector2Array(FlatVector[] src, ref Vector2[] dst)
        {
            if (src == null || src.Length != dst.Length)
            {
                dst = new Vector2[src.Length];
            }

            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = new Vector2(src[i].X, src[i].Y);
            }
        }
    }
}
