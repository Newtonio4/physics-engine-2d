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

namespace TestingFlatPhysics
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private Screen screen;
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;

        private FlatWorld world;

        private Color[] colors;
        private Color[] outlineColors;

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
        }

        protected override void Initialize()
        {
            FlatUtil.SetRelativeBackBufferSize(this.graphics, 0.85f);

            this.screen = new Screen(this, 1280, 768);
            this.sprites = new Sprites(this);
            this.shapes = new Shapes(this);
            this.camera = new Camera(this.screen);
            this.camera.Zoom = 20;

            int bodyCount = 30;
            this.world = new FlatWorld();
            this.colors = new Color[bodyCount];
            this.outlineColors = new Color[bodyCount];

            for (int i = 0; i < bodyCount; i++)
            {
                int type = new Random().Next(2);
                float posX = new Random().NextSingle() * 40 - 20;
                float posY = new Random().NextSingle() * 30 - 15;

                FlatBody body = null;


                if (type == (int)ShapeType.Circle)
                {
                    if (!FlatBody.CreateCircleBody(1f, new FlatVector(posX, posY), 2f, false, 0.5f, out body, out string errorMessaage))
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

                this.world.AddBody(body);
                this.colors[i] = RandomHelper.RandomColor();
                this.outlineColors[i] = Color.White;
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Keys

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
                float forceMagnitude = 50f;

                if (keyboard.IsKeyDown(Keys.W)) dy++;
                if (keyboard.IsKeyDown(Keys.A)) dx--;
                if (keyboard.IsKeyDown(Keys.S)) dy--;
                if (keyboard.IsKeyDown(Keys.D)) dx++;

                if (!this.world.GetBody(0, out FlatBody body))
                    throw new Exception("No body!");

                if (dx != 0f || dy != 0f)
                {
                    FlatVector forceDirection = FlatMath.Normalize(new FlatVector(dx, dy));
                    FlatVector force = forceDirection * forceMagnitude;
                    body.AddForce(force);
                }

                if (keyboard.IsKeyDown(Keys.R))
                {
                    body.Rotate(MathF.PI / 2f * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }

            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            WrapScreen();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            this.GraphicsDevice.Clear(new Color(50, 60, 70));

            this.shapes.Begin(this.camera);

            // SHAPES ---
            for (int i = 0; i < this.world.BodyCount; i++)
            {
                if(!this.world.GetBody(i, out FlatBody body))
                    throw new Exception("No body!");

                if (body.ShapeType == ShapeType.Circle)
                {
                    shapes.DrawCircleFill(FlatConverter.ToVector2(body.Position), body.Radius, 64, colors[i]);
                    shapes.DrawCircle(FlatConverter.ToVector2(body.Position), body.Radius, 64, Color.White);
                }
                else if (body.ShapeType == ShapeType.Box)
                {
                    FlatConverter.ToVector2Array(body.GetTransformVertices(), ref this.vertexBuffer);
                    shapes.DrawPolygonFill(this.vertexBuffer, body.Triangles, this.colors[i]);
                    shapes.DrawPolygon(this.vertexBuffer, this.outlineColors[i]);
                }
            }
            // SHAPES ---

            this.shapes.End();

            this.screen.Unset();
            this.screen.Present(this.sprites);

            base.Draw(gameTime);
        }

        private void WrapScreen()
        {
            this.camera.GetExtents(out Vector2 camMin, out Vector2 camMax);

            float viewWidth = camMax.X - camMin.X;
            float viewHeight = camMax.Y - camMin.Y;

            for (int i = 0; i < world.BodyCount; i++)
            {
                if (!this.world.GetBody(i, out FlatBody body))
                {
                    throw new Exception("Game1 - WrapScreen");
                }

                if (body.Position.X < camMin.X) body.MoveTo(body.Position + new FlatVector(viewWidth, 0f));
                if (body.Position.X > camMax.X) body.MoveTo(body.Position - new FlatVector(viewWidth, 0f));
                if (body.Position.Y < camMin.Y) body.MoveTo(body.Position + new FlatVector(0f, viewHeight));
                if (body.Position.Y > camMax.Y) body.MoveTo(body.Position - new FlatVector(0f, viewHeight));
            }
        }
    }
}