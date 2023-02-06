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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TestingFlatPhysics
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private Screen screen;
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;
        private SpriteFont fontConsolas18;

        private FlatWorld world;

        private List<FlatEntity> entityList;
        private List<FlatEntity> entityRemovalList;

        private Stopwatch watch;

        private double totalWorldStepTime;
        private int totalBodyCount;
        private int totalSampleCount;
        private Stopwatch sampleTimer;
        private string worldStepTimeString = string.Empty;
        private string bodyCountString = string.Empty;

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
            this.Window.Position = new Point(10, 40);

            FlatUtil.SetRelativeBackBufferSize(this.graphics, 0.85f);

            this.screen = new Screen(this, 1280, 768);
            this.sprites = new Sprites(this);
            this.shapes = new Shapes(this);
            this.camera = new Camera(this.screen);
            this.camera.Zoom = 20;

            this.camera.GetExtents(out float left, out float right, out float bottom, out float top);

            this.entityList = new List<FlatEntity>();
            this.entityRemovalList = new List<FlatEntity>();

            this.world = new FlatWorld();
            float padding = MathF.Abs(right - left) * 0.1f;

            // Bodies
            if (!FlatBody.CreateBoxBody(right - left - padding * 2, 3f, 1f, true, 0.5f, out FlatBody groundBody, out string errorMessage))
                throw new Exception(errorMessage);

            groundBody.MoveTo(new FlatVector(0, -10));
            this.world.AddBody(groundBody);
            entityList.Add(new FlatEntity(groundBody, Color.DarkGray));

            if (!FlatBody.CreateBoxBody(20f, 1.5f, 1f, true, 0.5f, out FlatBody ledgeBody1, out errorMessage))
                throw new Exception(errorMessage);

            ledgeBody1.MoveTo(new FlatVector(-8f, 6.5f));
            ledgeBody1.Rotate(-0.13f);
            this.world.AddBody(ledgeBody1);
            entityList.Add(new FlatEntity(ledgeBody1, Color.DarkGray));

            if (!FlatBody.CreateBoxBody(20f, 1.5f, 1f, true, 0.5f, out FlatBody ledgeBody2, out errorMessage))
                throw new Exception(errorMessage);

            ledgeBody2.MoveTo(new FlatVector(8f, 1f));
            ledgeBody2.Rotate(0.17f);
            this.world.AddBody(ledgeBody2);
            entityList.Add(new FlatEntity(ledgeBody2, Color.DarkGray));

            watch = new Stopwatch();
            sampleTimer = new Stopwatch();
            watch.Start();
            sampleTimer.Start();


            base.Initialize();
        }

        protected override void LoadContent()
        {
            fontConsolas18 = Content.Load<SpriteFont>("Consolas 18");
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
                float width = RandomHelper.RandomSingle(1.5f, 2.5f);
                float height = RandomHelper.RandomSingle(1.5f, 2.5f);
                FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this.screen, this.camera));

                entityList.Add(new FlatEntity(world, width, height, false, mouseWorldPosition));
            }

            // Add circle body
            if (mouse.IsRightMouseButtonPressed())
            {
                float radius = RandomHelper.RandomSingle(1f, 1.5f);
                FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this.screen, this.camera));

                entityList.Add(new FlatEntity(world, radius, false, mouseWorldPosition));
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
            }

            if (this.sampleTimer.Elapsed.TotalSeconds > 1d)
            {
                bodyCountString = "BodyCount: " + Math.Round(totalBodyCount / (double)totalSampleCount, 4).ToString();
                worldStepTimeString = "StepTime: " + Math.Round(totalWorldStepTime / (double)totalSampleCount, 4).ToString();
                totalBodyCount = 0;
                totalWorldStepTime = 0;
                totalSampleCount = 0;
                sampleTimer.Restart();
            }

            watch.Restart();
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 20);
            watch.Stop();

            totalWorldStepTime += watch.Elapsed.TotalMilliseconds;
            totalBodyCount += world.BodyCount;
            totalSampleCount++;

            this.camera.GetExtents(out float left, out float right, out float bottom, out float top);

            entityRemovalList.Clear();

            for (int i = 0; i < entityList.Count; i++)
            {
                FlatEntity entity = entityList[i];
                FlatBody body = entity.Body;

                if (body.IsStatic)
                    continue;

                FlatAABB box = body.GetAABB();

                if (box.Max.Y < bottom)
                {
                    entityRemovalList.Add(entity);
                    world.RemoveBody(body);
                }
            }

            for (int i = 0; i < entityRemovalList.Count; i++)
            {
                FlatEntity entity = entityRemovalList[i];
                world.RemoveBody(entity.Body);
                entityList.Remove(entity);
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
            for (int i = 0; i < this.entityList.Count; i++)
            {
                entityList[i].Draw(shapes);
            }
            // SHAPES ---

            this.shapes.End();

            // TEXT ---

            Vector2 stringSize = fontConsolas18.MeasureString(bodyCountString);
            sprites.Begin();
            sprites.DrawString(fontConsolas18, bodyCountString, new Vector2(0, 0), Color.White);
            sprites.DrawString(fontConsolas18, worldStepTimeString, new Vector2(0, stringSize.Y), Color.White);
            sprites.End();

            // TEXT ---

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