using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FerrumModules.Engine
{
    public class FerrumEngine : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;

        protected Scene CurrentScene;

        public int BufferWidth { get; private set; }
        public int BufferHeight { get; private set; }

        private float renderWidth, renderHeight;

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
            BufferWidth = bufferWidth; BufferHeight = bufferHeight;

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
            CurrentScene.Engine = this;
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

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            var camera = CurrentScene.Camera;
            var originalCameraOffset = camera.PositionOffset;

            var cameraBoxSize = new Vector2(_renderTarget.Width, _renderTarget.Height) / camera.Zoom;
            var cameraHalfWindowOffset = Rotation.Rotate(cameraBoxSize, camera.AngleOffset) / 2;

            camera.BoundingBox = Rotation.RotatedRectAABB(
                cameraBoxSize,
                camera.Centered ? Vector2.Zero : Rotation.Rotate(cameraHalfWindowOffset, camera.AngleOffset),
                camera.GlobalPosition,
                -camera.AngleOffset);
            
            if (camera.Centered)
            {
                camera.PositionOffset -= cameraHalfWindowOffset;
            }

            GraphicsDevice.SetRenderTarget(_renderTarget);
            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);

            GraphicsDevice.Clear(CurrentScene.BackgroundColor);
            CurrentScene.Render(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);

            _spriteBatch.Draw(_renderTarget, new Rectangle(
                _graphics.PreferredBackBufferWidth / 2 - (int)(renderWidth / 2),
                _graphics.PreferredBackBufferHeight / 2 - (int)(renderHeight / 2),
                (int)renderWidth,
                (int)renderHeight),
                Color.White);

            _spriteBatch.End();
            camera.PositionOffset = originalCameraOffset;
        }

        public void OnWindowResize(object sender, EventArgs e)
        {
            UpdateRenderSize();
        }

        public void UpdateRenderSize()
        {
            if ((float)_renderTarget.Height / _renderTarget.Width > (float)_graphics.PreferredBackBufferHeight / _graphics.PreferredBackBufferWidth)
            {
                renderHeight = _graphics.PreferredBackBufferHeight;
                renderWidth = renderHeight * _renderTarget.Width / _renderTarget.Height;
                return;
            }
            renderWidth = _graphics.PreferredBackBufferWidth;
            renderHeight = renderWidth * _renderTarget.Height / _renderTarget.Width;
        }


    }
}
