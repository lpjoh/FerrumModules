using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FerrumModules.Engine
{
    public class Engine : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private readonly SpriteEffects _spriteBatchEffects;

        protected Scene CurrentScene;

        private float renderWidth, renderHeight;

        private const string TextureDirectory = "Textures";

        public Engine(
            int displayBufferWidth = 1280,
            int displayBufferHeight = 720,
            Scene startingScene = null,
            string windowName = "Ferrum Engine")
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = displayBufferWidth,
                PreferredBackBufferHeight = displayBufferHeight
            };

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnWindowResize;

            Window.Title = windowName;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _spriteBatchEffects = new SpriteEffects();

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
        }

        public virtual void InitGame() { }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
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

            var windowDimensions = new Vector2(_renderTarget.Width, _renderTarget.Height) / camera.Zoom;
            var windowRectRotated = Rotation.RotatedRectSizeByCenter(windowDimensions, camera.AngleOffset);

            var cameraBoxSize = new Vector2(windowRectRotated.X, windowRectRotated.Y);
            var cameraBoxPosition = camera.PositionOffset + camera.GlobalPositionNoOffset - (cameraBoxSize / 2);
            
            var cameraHalfWindowOffset = Rotation.Rotate(windowDimensions / camera.GlobalScale / 2, -camera.AngleOffset);
            if (camera.Centered)
            {
                camera.PositionOffset -= cameraHalfWindowOffset;
            }
            else
            {
                cameraBoxPosition += cameraHalfWindowOffset;
            }

            camera.BoundingBox = new Rectangle(
                (int)cameraBoxPosition.X,
                (int)cameraBoxPosition.Y,
                (int)cameraBoxSize.X,
                (int)cameraBoxSize.Y);

            GraphicsDevice.SetRenderTarget(_renderTarget);
            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            CurrentScene.Render(_spriteBatch, _spriteBatchEffects);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

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
