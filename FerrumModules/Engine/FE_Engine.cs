using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Crossfrog.Ferrum.Engine.Entities;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine
{
    public class FE_Engine : Game
    {
        protected readonly GraphicsDeviceManager _graphics;
        protected SpriteBatch _spriteBatch;
        protected RenderTarget2D _renderTarget;

        protected Scene CurrentScene;

        public int BufferWidth { get; private set; }
        public int BufferHeight { get; private set; }
        private int blitWidth, blitHeight;

        public float FPS
        {
            set
            {
                IsFixedTimeStep = true;
                TargetElapsedTime = TimeSpan.FromSeconds(1.0f / value);
            }
        }

        public FE_Engine(
            int bufferWidth = 1280,
            int bufferHeight = 720,
            float windowScaleX = 1.0f,
            float windowScaleY = 1.0f,
            Scene startingScene = null,
            string windowName = "Ferrum Engine")
        {
            BufferWidth = bufferWidth;
            BufferHeight = bufferHeight;

            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = (int)(BufferWidth * windowScaleX),
                PreferredBackBufferHeight = (int)(BufferHeight * windowScaleY)
            };

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnWindowResize;

            Window.Title = windowName;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            if (startingScene == null)
                ChangeScene(new Scene());
            else
                ChangeScene(startingScene);
        }

        public SceneType ChangeScene<SceneType>(SceneType scene) where SceneType : Scene
        {
            foreach (var t in Assets.Textures.Values)
                t.Dispose();
            Assets.Textures.Clear();
            CurrentScene = scene;
            scene.Engine = this;
            scene.Init();
            return scene;
        }
        protected override void Initialize()
        {
            base.Initialize();
            Assets.Engine = this;
            InitGame();
            Input.UpdateActionStates();
        }
        public virtual void InitGame() { }
#if DEBUG
        private Texture2D PhysicsDebugTexture;
#endif
        protected override void LoadContent()
        {
            TileMap.ObjectNamespace = GetType().Namespace;
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(
                GraphicsDevice,
                BufferWidth,
                BufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

#if DEBUG
            PhysicsDebugTexture = new Texture2D(GraphicsDevice, 1, 1);
            PhysicsDebugTexture.SetData(new Color[] { Color.White });
#endif
            UpdateRenderSize();
            LoadGameContent();
        }
        public T LoadAsset<T>(string directory)
        {
            return Content.Load<T>(directory);
        }
        
        protected override void UnloadContent()
        {
            base.UnloadContent();
            _spriteBatch.Dispose();
        }
        public virtual void LoadGameContent() { }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var e in CurrentScene.EntitiesToBeDeleted) e.Parent?.RemoveObjectFromList(e.Parent.Children, e);
            CurrentScene.EntitiesToBeDeleted.Clear();
            foreach (var c in CurrentScene.ComponentsToBeDeleted) c.Parent?.RemoveObjectFromList(c.Parent.Components, c);
            CurrentScene.ComponentsToBeDeleted.Clear();

            CurrentScene.PreCollision();
            CurrentScene.Update(delta);
            UpdateGame(delta);

            Input.UpdateActionStates();
        }
        public virtual void UpdateGame(float delta) { }
        private Matrix GetViewMatrix(Camera camera)
        {
            var cameraPosition = -camera.GlobalPosition;

            var matrix =
                Matrix.CreateTranslation(new Vector3(cameraPosition.X, cameraPosition.Y, 0)) *
                Matrix.CreateRotationZ(camera.AngleOffset) *
                Matrix.CreateScale(camera.Zoom);

            if (camera.Centered)
            {
                var cameraOffset = new Vector2(BufferWidth, BufferHeight) / 2;
                matrix *= Matrix.CreateTranslation(new Vector3(cameraOffset.X, cameraOffset.Y, 0));
            }

            return matrix;
        }
        private Rectangle GetVisibleAreaBox(Matrix viewportMatrix)
        {
            var invertedMatrix = Matrix.Invert(viewportMatrix);
            var tl = Vector2.Transform(Vector2.Zero, invertedMatrix);
            var tr = Vector2.Transform(new Vector2(BufferWidth, 0), invertedMatrix);
            var bl = Vector2.Transform(new Vector2(0, BufferHeight), invertedMatrix);
            var br = Vector2.Transform(new Vector2(BufferWidth, BufferHeight), invertedMatrix);
            var min = new Vector2(
                MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
            var max = new Vector2(
                MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }
        public Rectangle VisibleAreaBox { get; private set; }
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var camera = CurrentScene.Camera;
            var viewMatrix = GetViewMatrix(camera);
            VisibleAreaBox = GetVisibleAreaBox(viewMatrix);

            GraphicsDevice.SetRenderTarget(_renderTarget);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, viewMatrix);

            GraphicsDevice.Clear(CurrentScene.BackgroundColor);
            CurrentScene.Render(_spriteBatch);
#if DEBUG
            if (Input.ActionPressed("ShowPhysics")) RenderPhysicsDebug(_spriteBatch);
#endif
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);

            _spriteBatch.Draw(_renderTarget, new Rectangle(
                _graphics.PreferredBackBufferWidth / 2 - (blitWidth / 2),
                _graphics.PreferredBackBufferHeight / 2 - (blitHeight / 2),
                blitWidth,
                blitHeight),
                Color.White);

            _spriteBatch.End();
        }
#if DEBUG
        public static float DebugOpacity = 0.1f;
        public static Color DebugBoxColor = new Color(Color.Navy, DebugOpacity);
        public static Color DebugVertexColor = Color.Aqua;
        public static Vector2 DebugVertexOrigin = new Vector2(0.5f, 0.5f);
        public static Vector2 DebugVertexScale = new Vector2(2, 2);
        public void RenderPhysicsDebug(SpriteBatch spriteBatch)
        {
            foreach (var body in CurrentScene.PhysicsWorld)
            {
                spriteBatch.Draw(PhysicsDebugTexture, body.BoundingBox, DebugBoxColor);
                
                foreach (var vertex in body.GlobalVertices)
                    spriteBatch.Draw(PhysicsDebugTexture, vertex, null, DebugVertexColor, MathHelper.PiOver4, DebugVertexOrigin, DebugVertexScale, SpriteEffects.None, 0.0f);
            }
        }
#endif
        public void OnWindowResize(object sender, EventArgs e)
        {
            UpdateRenderSize();
        }

        public void UpdateRenderSize()
        {
            if ((float)_renderTarget.Height / _renderTarget.Width > (float)_graphics.PreferredBackBufferHeight / _graphics.PreferredBackBufferWidth)
            {
                blitHeight = _graphics.PreferredBackBufferHeight;
                blitWidth = blitHeight * _renderTarget.Width / _renderTarget.Height;
                return;
            }
            blitWidth = _graphics.PreferredBackBufferWidth;
            blitHeight = blitWidth * _renderTarget.Height / _renderTarget.Width;
        }
    }
}
