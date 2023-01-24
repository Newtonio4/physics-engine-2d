using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Flat;
using Flat.Graphics;
using System;
using Flat.Input;
using FlatPhysics;
using System.Collections.Generic;

using FlatMath = FlatPhysics.FlatMath;
using System.Reflection.Metadata.Ecma335;

namespace TestingFlatPhysics
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private Screen screen;
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;

        private List<FlatBody> bodyList;
        private Color[] colors;

        private Vector2[] vertexBuffer = new Vector2[4];

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            this.graphics.SynchronizeWithVerticalRetrace = true;

            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = true;

            const double UpdatesPerSecond = 60d;
            this.TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round((double)TimeSpan.TicksPerSecond / UpdatesPerSecond));

            int bodyCount = 10;
            bodyList = new List<FlatBody>(bodyCount);
            colors = new Color[bodyCount];

            for (int i = 0; i < bodyCount; i++)
            {
                //int type = new Random().Next(2);
                int type = 1;
                float posX = new Random().NextSingle() * 40 - 20;
                float posY = new Random().NextSingle() * 30 - 15;

                FlatBody body = null;


                if (type == (int)ShapeType.Circle)
                {
                    if(!FlatBody.CreateCircleBody(1f, new FlatVector(posX, posY), 2f, false, 0.5f, out body, out string errorMessaage))
                    {
                        throw new Exception(errorMessaage);
                    }

                }
                else if (type == (int)ShapeType.Box)
                {
                    if (!FlatBody.CreateBoxBody(2f, 2f, new FlatVector(posX, posY), 2f, false, 0.5f, out body, out string errorMessaage))
                    {
                        throw new Exception(errorMessaage);
                    }
                }

                this.bodyList.Add(body);
                this.colors[i] = RandomHelper.RandomColor();
            }
        }

        protected override void Initialize()
        {
            FlatUtil.SetRelativeBackBufferSize(this.graphics, 0.85f);

            this.screen = new Screen(this, 1280, 768);
            this.sprites = new Sprites(this);
            this.shapes = new Shapes(this);
            this.camera = new Camera(this.screen);
            this.camera.Zoom = 20;

            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            FlatKeyboard keyboard = FlatKeyboard.Instance;
            FlatMouse mouse = FlatMouse.Instance;

            keyboard.Update();
            mouse.Update();

            if (keyboard.IsKeyAvailable)
            {
                if (keyboard.IsKeyClicked(Keys.Escape))
                {
                    this.Exit();
                }

                if (keyboard.IsKeyClicked(Keys.Q))
                {
                    this.camera.IncZoom();
                }

                if (keyboard.IsKeyClicked(Keys.E))
                {
                    this.camera.DecZoom();
                }

                float dx = 0f;
                float dy = 0f;
                float speed = 8f;

                if (keyboard.IsKeyDown(Keys.W)) dy++;
                if (keyboard.IsKeyDown(Keys.A)) dx--;
                if (keyboard.IsKeyDown(Keys.S)) dy--;
                if (keyboard.IsKeyDown(Keys.D)) dx++;

                if (dx != 0f || dy != 0f)
                {
                    FlatVector direction = FlatMath.Normalize(new FlatVector(dx, dy));
                    FlatVector velocity = direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    this.bodyList[0].Move(velocity);
                }
            }

            foreach (var body in this.bodyList)
            {
                body.Rotate(MathF.PI / 2f * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            //for (int i = 0; i < bodyList.Count - 1; i++)
            //{
            //    for (int j = i + 1; j < bodyList.Count; j++)
            //    {
            //        if (Collisions.IntersectCircles(
            //            bodyList[i].Position, bodyList[i].Radius,
            //            bodyList[j].Position, bodyList[j].Radius,
            //            out FlatVector normal, out float depth))
            //        {
            //            bodyList[i].Move(-normal * depth / 2);
            //            bodyList[j].Move(normal * depth / 2);
            //        }  
            //    }
            //}

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            this.GraphicsDevice.Clear(new Color(50, 60, 70));

            this.shapes.Begin(this.camera);
            // SHAPES ---
            for (int i = 0; i < bodyList.Count; i++)
            {
                FlatBody body = bodyList[i];
                if (body.ShapeType == ShapeType.Circle)
                {
                    shapes.DrawCircleFill(FlatConverter.ToVector2(body.Position), body.Radius, 64, colors[i]);
                    shapes.DrawCircle(FlatConverter.ToVector2(body.Position), body.Radius, 64, Color.White);
                }
                else if (body.ShapeType == ShapeType.Box)
                {
                    FlatConverter.ToVector2Array(body.GetTransformVertices(), ref this.vertexBuffer);
                    shapes.DrawPolygonFill(this.vertexBuffer, body.Triangles, this.colors[i]);
                    shapes.DrawPolygon(this.vertexBuffer, Color.White);
                }
            }
            // SHAPES ---
            this.shapes.End();

            this.screen.Unset();
            this.screen.Present(this.sprites);

            base.Draw(gameTime);
        }
    }
}