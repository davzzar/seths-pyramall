using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    /// <summary>
    /// The game engine in the core of the whole game.<br/>
    /// It is a singleton class and can exist at most once.<br/>
    /// To start a game, instantiate the game engine, create game objects and add components to initialize the game and then start the game using <b>GameEngine.Run()</b>. 
    /// </summary>
    public sealed class GameEngine : Game
    {
        private static GameEngine instance;
        
        /// <summary>
        /// Gets the singleton instance of <see cref="GameEngine"/>, throws if no <see cref="GameEngine"/> was created beforehand.
        /// </summary>
        public static GameEngine Instance
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

        private readonly Stopwatch timer = new Stopwatch();

        internal RenderPipeline RenderPipeline { get; }

        internal GraphicsDeviceManager GraphicsDeviceManager { get; }

        public bool Fullscreen { set
            {
                this.GraphicsDeviceManager.IsFullScreen = value;
                this.GraphicsDeviceManager.ApplyChanges();
            } }
        public Int2 Resolution
        {
            get { return new Int2(this.GraphicsDeviceManager.PreferredBackBufferWidth, this.GraphicsDeviceManager.PreferredBackBufferHeight); 
            } 
            set
            {
                this.GraphicsDeviceManager.PreferredBackBufferWidth = value.X;
                this.GraphicsDeviceManager.PreferredBackBufferHeight = value.Y;
                this.GraphicsDeviceManager.ApplyChanges();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the render loop should be synced with the screen refresh rate.
        /// </summary>
        public bool VSync
        {
            get => this.GraphicsDeviceManager.SynchronizeWithVerticalRetrace;
            set => this.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = value;
        }

        /// <summary>
        /// Instantiates a new instance of the GameEngine type.
        /// </summary>
        /// <exception cref="InvalidOperationException">There can't exist more than one instance of GameEngine at any given time.</exception>
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
            this.GraphicsDeviceManager.PreferMultiSampling = true;

            this.IsFixedTimeStep = false;

            Graphics.Init();
        }

        /// <summary>
        /// Destroys the current GameEngine instance, effectively ending the game.
        /// </summary>
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
            PhysicsManager.Init();
            SceneManager.Init();
            UI.Initialize();
            this.RenderPipeline.Init();
        }
        
        /// <inheritdoc />
        protected override void Update(GameTime gameTime)
        {
            Time.StartNextFrame(gameTime.ElapsedGameTime.TotalSeconds);
            this.timer.Restart();
            
            SceneManager.DoUpdate();
            PhysicsManager.Step();

            this.timer.Stop();
            Time.OnFrameUpdateFinished(this.timer.Elapsed);
        }

        /// <inheritdoc />
        protected override void Draw(GameTime gameTime)
        {
            this.timer.Restart();

            this.RenderPipeline.Render();
            
            this.timer.Stop();
            Time.OnFrameDrawFinished(this.timer.Elapsed);
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
