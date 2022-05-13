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

                if (this.isAlive)
                {
                    color = Color.White;
                    var ccp = Owner.GetOrAddComponent<CameraControlPoint>();
                    ccp.Margin = new Border(5, 10, 5, 5);
                }
                else
                {
                    color = Color.DarkGray * 0.8f;
                    Owner.GetComponent<CameraControlPoint>()?.Destroy();
                }
                
                var renderer = Owner.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    renderer.Color = color;
                }
            }
        }

        public InputHandler InputHandler { get; set; }

        protected override void OnEnable()
        {
            // Note that PlayerIndex is always One when OnAwake is called. It needs to be updated whenever we update the index.
            base.OnEnable();

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
            timerBar.DepletionSpeed = 0.2f;
            timerBar.IsActive = false;

            var controlComp = Owner.GetOrAddComponent<PlayerControlComponent>();
            controlComp.InputHandler = InputHandler;

            var playerStates = Owner.GetOrAddComponent<PlayerStates>();
            playerStates.InputHandler = InputHandler;

            // animator need to be created after controlComp and input handler
            var playerAnimator = Owner.GetOrAddComponent<Animator>();
            playerAnimator.Depth = 0.1f;
            SetPlayerAnimationSprite();

            var cameraControlPoint = Owner.GetOrAddComponent<CameraControlPoint>();
            cameraControlPoint.Margin = new Border(5, 10, 5, 5);

            var itemsManager = Owner.GetOrAddComponent<ItemManager>();
            itemsManager.inputHandler = InputHandler;

            this.IsAlive = true;
        }

        private void SetPlayerAnimationSprite()
        {
            var playerAnimator = Owner.GetOrAddComponent<Animator>();

            string animationTexture = PlayerIndex switch
            {
                (PlayerIndex.One) => "Tiled/TiledsetTexture/PlayerAnimated",
                (PlayerIndex.Two) => "Tiled/TiledsetTexture/PlayerAnimated2",
                (PlayerIndex.Three) => "Tiled/TiledsetTexture/PlayerAnimated3",
                (PlayerIndex.Four) => "Tiled/TiledsetTexture/PlayerAnimated4",
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

            if (newState == null) return;
            state.OnExit();
            this.state = newState;
            state.OnEnter();
        }

    }
}
