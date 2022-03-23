using System;
using Microsoft.Xna.Framework;

namespace Engine
{
    public sealed class GameEngine : Game
    {
        private static GameEngine instance;
        
        internal static GameEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException(
                        "No game engine is currently running, use GameEngine.Start() to start a new instance.");
                }

                return instance;
            }
        }

        private bool isExiting;

        internal RenderPipeline RenderPipeline { get; }

        internal GraphicsDeviceManager GraphicsDeviceManager { get; }

        public GameEngine()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Can't create more than one instance of GameEngine, use Destroy() to remove an existing instance.");
            }

            instance = this;

            this.GraphicsDeviceManager = new GraphicsDeviceManager(this);
            this.RenderPipeline = new RenderPipeline();

            this.Content.RootDirectory = "Content";
            this.Window.AllowUserResizing = true;

            Graphics.Init();
        }

        public void Destroy()
        {
            if (this.isExiting)
            {
                return;
            }

            this.isExiting = true;
            this.Exit();
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            Time.Init();
            SceneManager.Init();
            this.RenderPipeline.Init();
        }
        
        /// <inheritdoc />
        protected override void Update(GameTime gameTime)
        {
            Time.StartNextFrame(gameTime.ElapsedGameTime.TotalSeconds);
            SceneManager.DoUpdate();
        }

        /// <inheritdoc />
        protected override void Draw(GameTime gameTime)
        {
            this.RenderPipeline.Render();
        }

        /// <inheritdoc />
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            
            instance = null;
            this.isExiting = false;
        }
    }
}
