using System;
using System.ComponentModel;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{

    interface IPlayerControlState
    {
        IPlayerControlState Update();
        void OnEnter();
        void OnExit();
    }

    class InControlState : IPlayerControlState
    {
        public IPlayerControlState Update()
        {
            // delegate plater control to the controller component
            return null;

            // implement transition to Trapped state
            // if SandLevel > Player.pos.y && player.vel.y < 0 -> return trapped state, else null


        }

        public void OnEnter()
        {
            // gain control of the player (enable controller component)
            // perform a jump this frame.
            throw new NotImplementedException();
        }

        public void OnExit()
        {
            // disable controller component
            // Make player have no velocity
            throw new NotImplementedException();
        }
    }

    class TrappedState : IPlayerControlState
    {
        public IPlayerControlState Update()
        {
            // delegate player control to button masher
            throw new NotImplementedException();

            // implement transition to InControllState
            // if masher level fill event triggers, tansition to Incontroll
            return null;
        }

        public void OnEnter()
        {
            return;
        }

        public void OnExit()
        {
            return;
        }
    }


    public class PlayerComponent : Behaviour
    {
        private PlayerIndex playerIndex;

        private IPlayerControlState state = new InControlState();
        private bool isAlive = true;
        private bool initialized = false;

        public PlayerIndex PlayerIndex
        {
            get => playerIndex;
            set
            {
                if (value == playerIndex)
                {
                    return;
                }

                if (!Enum.IsDefined(typeof(PlayerIndex), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int) value, typeof(PlayerIndex));
                
                // Update child components when the player index is updated
                playerIndex = value;
                InputHandler.PlayerIndex = value;
                SetPlayerAnimationSprite();
            }
        }

        public bool IsAlive
        {
            get => this.isAlive;
            set
            {
                if (this.isAlive == value)
                {
                    return;
                }

                this.isAlive = value;
                
                Color color;

                if (this.isAlive)// dead -> alive
                {
                    color = Color.White;
                    // enable collision
                    Owner.GetComponentInChildren<Collider>().IsActive = true;
                    // show sprite
                    Owner.GetComponent<SpriteRenderer>().IsActive = true;
                    AddCameraControlPoint();
                }
                else// alive -> dead
                {
                    color = Color.DarkGray * 0.8f;
                    // disable colision
                    Owner.GetComponentInChildren<Collider>().IsActive = false;
                    // hide sprite after 10s( or at RoundCheck)
                    Owner.AddComponent<GoTimer>().Init(10f,() =>
                    {
                        Owner.GetComponent<SpriteRenderer>().IsActive = false;
                    });
                    RemoveCameraControlPoint();
                }
                
                var renderer = Owner.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    renderer.Color = color;
                }
            }
        }

        public void RemoveCameraControlPoint()
        {
            Owner.GetComponent<CameraControlPoint>()?.Destroy();
        }

        public void AddCameraControlPoint()
        {
            var ccp = Owner.GetOrAddComponent<CameraControlPoint>();
            ccp.Margin = new Border(5, 10, 5, 5);
        }

        public InputHandler InputHandler { get; set; }

        protected override void OnEnable()
        {
            // Note that PlayerIndex is always One when OnAwake is called. It needs to be updated whenever we update the index.
            base.OnEnable();

            if (initialized)
            {
                return;
            }

            initialized = true;

#if DEBUG
            //FOR DEBUG (updated in the PlayerControlComponent)
            var textRenderer = Owner.AddComponent<GuiTextRenderer>();
            textRenderer.PositionMode = GuiTextRenderer.ScreenPositionMode.Absolute;
            textRenderer.ScreenPosition = Vector2.UnitY * 400f;
            textRenderer.Color = Color.Yellow;
            textRenderer.FontSize = 34;
            var tracer = Owner.AddComponent<TracerRendererComponent>();
            tracer.TraceLength = 60;
#endif

            InputHandler = new InputHandler(PlayerIndex);
            Debug.Print($"Player with player index {PlayerIndex} created");
            
            var colliderGo = new GameObject("Player collider");
            colliderGo.Transform.Parent = Owner.Transform;
            colliderGo.Transform.LocalPosition = new Vector2(0, -0.17f);
            var playerCollider = colliderGo.GetOrAddComponent<CircleCollider>();
            playerCollider.Radius = 0.325f;
            playerCollider.Friction = 0.0f;


            var playerRB = Owner.GetOrAddComponent<RigidBody>();
            playerRB.IsKinematic = false;
            playerRB.FreezeRotation = true;
            playerRB.IgnoreGravity = true;

            Owner.GetOrAddComponent<GroundCheckComponent>();

            // To show when player is trapped in sand
            var timerBar = Owner.GetOrAddComponent<TimerBar>();
            timerBar.FillColor = Color.Red;
            timerBar.DepletionSpeed = 0.2f;
            timerBar.IsActive = false;

            var playerController = Owner.GetOrAddComponent<PlayerControlComponent>();
            playerController.InputHandler = InputHandler;

            var playerStates = Owner.GetOrAddComponent<PlayerStates>();
            playerStates.InputHandler = InputHandler;
            playerStates.Collider = playerCollider;

            // animator need to be created after controlComp and input handler
            var playerAnimator = Owner.GetOrAddComponent<Animator>();
            playerAnimator.Depth = Conf.Depth.Player;
            SetPlayerAnimationSprite();

            // Add hardjump haloGo
            var haloGo = new GameObject();
            haloGo.Transform.Parent = Transform;
            haloGo.Transform.LocalPosition = new Vector2(0, -.1f);
            var playerEffectAnimator = haloGo.AddComponent<Animator>();
            playerEffectAnimator.LoadFromContent("PlayerEffectAnimated");
            playerEffectAnimator.Depth = Conf.Depth.PlayerBack;
            haloGo.AddComponent<EffectAnimatorController>();
            haloGo.IsEnabled = false;

            var cameraControlPoint = Owner.GetOrAddComponent<CameraControlPoint>();
            cameraControlPoint.Margin = new Border(5, 10, 5, 5);

            var itemsManager = Owner.GetOrAddComponent<ItemManager>();
            itemsManager.inputHandler = InputHandler;

            this.IsAlive = true;

            // Subscribe to events
            // disable the timerBar when it fills up so it is not shown.
            // TODO add recharge sound cue here
            timerBar.OnFilled += () => timerBar.IsActive = false;
        }

        private void SetPlayerAnimationSprite()
        {
            var playerAnimator = Owner.GetOrAddComponent<Animator>();

            string animationTexture = PlayerIndex switch
            {
                (PlayerIndex.One) => "PlayerAnimated",
                (PlayerIndex.Two) => "PlayerAnimated2",
                (PlayerIndex.Three) => "PlayerAnimated3",
                (PlayerIndex.Four) => "PlayerAnimated4",
                _ => ""
            };

            playerAnimator.LoadFromContent("PlayerAnimated", animationTexture);
            Owner.AddComponent<PlayerAnimatorController>();
        }

        protected override void Update()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                this.IsAlive = !this.IsAlive;
            }

            base.Update();
            var newState = state.Update();

            InputHandler.UpdateState();

            if (newState == null) return;
            state.OnExit();
            this.state = newState;
            state.OnEnter();
        }

    }
}
