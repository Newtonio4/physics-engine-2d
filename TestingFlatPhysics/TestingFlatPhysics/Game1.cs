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

        private List<Color> colors;
        private List<Color> outlineColors;

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

            this.camera.GetExtents(out float left, out float right, out float bottom, out float top);

            this.colors = new List<Color>();
            this.outlineColors = new List<Color>();

            this.world = new FlatWorld();
            float padding = MathF.Abs(right - left) * 0.1f;

            if (!FlatBody.CreateBoxBody(right - left - padding * 2, 3f, new FlatVector(0, -10), 1f, true, 0.5f, out FlatBody groundBody, out string errorMessage))
                throw new Exception(errorMessage);

            this.world.AddBody(groundBody);

            this.colors.Add(Color.DarkGray);
            this.outlineColors.Add(Color.White);


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

            // Add box body
            if (mouse.IsLeftMouseButtonPressed())
            {
                float width = RandomHelper.RandomSingle(1f, 2f);
                float height = RandomHelper.RandomSingle(1f, 2f);
                FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this.screen, this.camera));

                if (!FlatBody.CreateBoxBody(width, height, mouseWorldPosition, 2f, false, 0.5f, out FlatBody body, out string errorMessage))
                    throw new Exception(errorMessage);

                world.AddBody(body);
                this.colors.Add(RandomHelper.RandomColor());
                this.outlineColors.Add(Color.White);
            }

            // Add circle body
            if (mouse.IsRightMouseButtonPressed())
            {
                float radius = RandomHelper.RandomSingle(0.75f, 1.5f);
                FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this.screen, this.camera));

                if (!FlatBody.CreateCircleBody(radius, mouseWorldPosition, 2f, false, 0.5f, out FlatBody body, out string errorMessage))
                    throw new Exception(errorMessage);

                world.AddBody(body);
                this.colors.Add(RandomHelper.RandomColor());
                this.outlineColors.Add(Color.White);
            }

            if (keyboard.IsKeyAvailable)
            {
                if (keyboard.IsKeyClicked(Keys.C))
                {
                    Console.WriteLine($"BodyCount: {world.BodyCount}");
                }

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
#if false
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
#endif
            }

            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            this.camera.GetExtents(out float left, out float right, out float bottom, out float top);

            for (int i = 0; i < world.BodyCount; i++)
            {
                if (!world.GetBody(i, out FlatBody body))
                    throw new ArgumentOutOfRangeException();

                FlatAABB box = body.GetAABB();

                if (box.Max.Y < bottom)
                {
                    world.RemoveBody(body);
                    colors.RemoveAt(i);
                    outlineColors.RemoveAt(i);
                }
            }

            //WrapScreen();

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
                    shapes.DrawCircle(FlatConverter.ToVector2(body.Position), body.Radius, 64, this.outlineColors[i]);
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