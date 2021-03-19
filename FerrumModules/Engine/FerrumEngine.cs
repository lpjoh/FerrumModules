using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Crossfrog.FerrumEngine.Modules;

namespace Crossfrog.FerrumEngine
{
    public class FerrumEngine : Game
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

        private const string TextureDirectory = "Textures";

        public FerrumEngine(
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
                ChangeScene(CurrentScene);
        }

        public SceneType ChangeScene<SceneType>(SceneType scene) where SceneType : Scene
        {
            CurrentScene = scene;
            scene.Engine = this;
            scene.Init();
            return scene;
        }

        protected override void Initialize()
        {
            base.Initialize();
            InitGame();
            Input.UpdateActionStates();
        }

        public virtual void InitGame() { }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(
                GraphicsDevice,
                BufferWidth,
                BufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            DirectoryInfo textureDir = new DirectoryInfo(Content.RootDirectory + "/" + TextureDirectory);
            FileInfo[] textureFiles = textureDir.GetFiles();

            foreach (var file in textureFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file.Name);

                var texture = Content.Load<Texture2D>(TextureDirectory + "/" + fileName);
                Assets.AddTexture(texture, fileName);
            }

            UpdateRenderSize();
            LoadGameContent();
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
            CurrentScene.Update(delta);
            UpdateGame(delta);

            Input.UpdateActionStates();
        }
        public virtual void UpdateGame(float delta) { }
        private Matrix GetViewMatrix(Camera camera)
        {
            var cameraPosition = -(camera.GlobalPositionNoOffset + camera.PositionOffset);

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
        public Rectangle GetVisibleAreaBox(Matrix viewportMatrix)
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
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var camera = CurrentScene.Camera;
            var viewMatrix = GetViewMatrix(camera);
            camera.BoundingBox = GetVisibleAreaBox(viewMatrix);

            GraphicsDevice.SetRenderTarget(_renderTarget);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, viewMatrix);

            GraphicsDevice.Clear(CurrentScene.BackgroundColor);
            CurrentScene.Render(_spriteBatch);
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
