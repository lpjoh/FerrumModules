using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FerrumModules.Engine
{
    public class FE_Engine : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private readonly SpriteEffects _spriteBatchEffects;

        protected FE_Scene CurrentScene;

        public FE_Engine(FE_Scene startingScene = null, int windowSizeX = 1280, int windowSizeY = 720, string windowName = "Ferrum Engine")
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = windowSizeX,
                PreferredBackBufferHeight = windowSizeY
            };

            Window.AllowUserResizing = true;
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
        }

        public virtual void InitGame() { }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadGameContent();
            InitGame();
        }

        public virtual void LoadGameContent() { }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateGame(delta);

            CurrentScene.Update(delta);

            base.Update(gameTime);
        }

        public virtual void UpdateGame(float delta) { }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            var camera = CurrentScene.Camera;
            var originalCameraPosition = camera.Position;
            
            camera.Position -= new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2)
                / CurrentScene.Camera.Scale;

            camera.BoundingBox = new Rectangle(
                (int)camera.Position.X,
                (int)camera.Position.Y,
                (int)(_graphics.PreferredBackBufferWidth / camera.Scale.X + camera.Scale.X),
                (int)(_graphics.PreferredBackBufferHeight / camera.Scale.Y + camera.Scale.Y));

            CurrentScene.Render(_spriteBatch, _spriteBatchEffects);

            if (camera != null) camera.Position = (Vector2)originalCameraPosition;

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
