using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FerrumModules.Engine
{
    public class FE_Engine : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private readonly SpriteEffects _spriteBatchEffects;

        protected FE_Scene CurrentScene;

        private float renderWidth, renderHeight;

        private const string TextureDirectory = "Textures";

        public FE_Engine(
            int displayBufferWidth = 1280,
            int displayBufferHeight = 720,
            FE_Scene startingScene = null,
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
                ChangeScene(new FE_Scene());
            else
                ChangeScene(CurrentScene);
        }

        public SceneType ChangeScene<SceneType>(SceneType scene) where SceneType : FE_Scene
        {
            CurrentScene = scene;
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
                FE_Assets.AddTexture(texture, fileName);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateGame(delta);
            CurrentScene.Update(delta);
            FE_Input.UpdateActionStates();

            base.Update(gameTime);
        }

        public virtual void UpdateGame(float delta) { }

        protected override void Draw(GameTime gameTime)
        {
            var camera = CurrentScene.Camera;
            var originalCameraPosition = camera.Position;
            
            if (camera.Centered)
            {
                camera.Position -= new Vector2(
                _renderTarget.Width / 2,
                _renderTarget.Height / 2)
                / CurrentScene.Camera.Scale;
            }

            camera.BoundingBox = new Rectangle(
                (int)camera.Position.X,
                (int)camera.Position.Y,
                (int)(_renderTarget.Width / camera.Scale.X + camera.Scale.X),
                (int)(_renderTarget.Height / camera.Scale.Y + camera.Scale.Y));

            GraphicsDevice.SetRenderTarget(_renderTarget);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

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

            camera.Position = (Vector2)originalCameraPosition;

            base.Draw(gameTime);
        }

        public void OnWindowResize(Object sender, EventArgs e)
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
