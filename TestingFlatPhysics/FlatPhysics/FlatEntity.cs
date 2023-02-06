using Flat;
using Flat.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public sealed class FlatEntity
    {
        public readonly FlatBody Body;
        public readonly Color Color;

        public FlatEntity(FlatBody body)
        {
            this.Body = body;
            this.Color = RandomHelper.RandomColor();
        }

        public FlatEntity(FlatBody body, Color color)
        {
            this.Body = body;
            this.Color = color;
        }

        public FlatEntity(FlatWorld world, float radius, bool isStatic, FlatVector position)
        {
            if (!FlatBody.CreateCircleBody(radius, 1f, isStatic, 0.5f, out FlatBody body, out string errorMessage))
                throw new Exception(errorMessage);

            body.MoveTo(position);
            this.Body = body;
            world.AddBody(this.Body);
            this.Color = RandomHelper.RandomColor();
        }

        public FlatEntity(FlatWorld world, float width, float height, bool isStatic, FlatVector position)
        {
            if (!FlatBody.CreateBoxBody(width, height, 1f, isStatic, 0.5f, out FlatBody body, out string errorMessage))
                throw new Exception(errorMessage);

            body.MoveTo(position);
            this.Body = body;
            world.AddBody(this.Body);
            this.Color = RandomHelper.RandomColor();
        }

        public void Draw(Shapes shapes)
        {
            Vector2 position = new Vector2(Body.Position.X, Body.Position.Y);

            if (Body.ShapeType== ShapeType.Circle)
            {
                shapes.DrawCircleFill(position, Body.Radius, 64, Color);
                shapes.DrawCircle(position, Body.Radius, 64, Color.White);
            }
            else if (Body.ShapeType == ShapeType.Box)
            {
                shapes.DrawBoxFill(position, Body.Width, Body.Height, Body.Angle, Color);
                shapes.DrawBox(position, Body.Width, Body.Height, Body.Angle, Color.White);
            }
        }
    }
}
