using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Flat;
using Flat.Graphics;
using System;
using Flat.Input;
using System.Diagnostics;
using FlatPhysics;
using System.Collections.Generic;
using System.Reflection.Metadata;

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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            this.graphics.SynchronizeWithVerticalRetrace = true;

            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = true;

            const double UpdatesPerSecond = 60d;
            this.TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round((double)TimeSpan.TicksPerSecond / UpdatesPerSecond));

            bodyList = new List<FlatBody>();

            for (int i = 0; i < 10; i++)
            {
                int type = new Random().Next(2);
                float posX = new Random().Next(40) - 20;
                float posY = new Random().Next(30) - 15;

                FlatBody body = null;


                if (type == (int)ShapeType.Circle)
                {
                    if(!FlatBody.CreateCircleBody(3f, new FlatVector(posX, posY), 2f, false, 0.5f, out body, out string errorMessaage))
                    {
                        throw new Exception(errorMessaage);
                    }

                }
                else if (type == (int)ShapeType.Box)
                {
                    if (!FlatBody.CreateBoxBody(3f, 3f, new FlatVector(posX, posY), 2f, false, 0.5f, out body, out string errorMessaage))
                    {
                        throw new Exception(errorMessaage);
                    }
                }

                this.bodyList.Add(body);
            }
        }

        protected override void Initialize()
        {
            FlatUtil.SetRelativeBackBufferSize(this.graphics, 0.85f);

            this.screen = new Screen(this, 1280, 768);
            this.sprites = new Sprites(this);
            this.shapes = new Shapes(this);
            this.camera = new Camera(this.screen);
            this.camera.Zoom = 5;

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

                if (keyboard.IsKeyClicked(Keys.A))
                {
                    this.camera.IncZoom();
                }

                if (keyboard.IsKeyClicked(Keys.Z))
                {
                    this.camera.DecZoom();
                }
            }

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
                    shapes.DrawCircle(FlatConverter.ToVector2(body.Position), body.Radius, 32, Color.White);
                }
                else if (body.ShapeType == ShapeType.Box)
                {
                    shapes.DrawBox(FlatConverter.ToVector2(body.Position), body.Width, body.Height, Color.Red);
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