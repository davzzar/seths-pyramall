using System;
using System.ComponentModel;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    public class PlayerComponent : Behaviour
    {
        private PlayerIndex playerIndex;

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

        public InputHandler InputHandler { get; set; }

        protected override void OnAwake()
        {
            // Note that PlayerIndex is always One when OnAwake is called. It needs to be updated whenever we update the index.
            base.OnAwake();

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
            var playerCollider = colliderGo.AddComponent<CircleCollider>();
            playerCollider.Radius = 0.325f;
            playerCollider.Friction = 0.0f;


            var playerRB = Owner.AddComponent<RigidBody>();
            playerRB.IsKinematic = false;
            playerRB.FreezeRotation = true;
            playerRB.IgnoreGravity = true;

            Owner.AddComponent<GroundCheckComponent>();

            var controlComp = Owner.AddComponent<PlayerControlComponent>();
            controlComp.InputHandler = InputHandler;

            var playerStates = Owner.AddComponent<PlayerStates>();
            playerStates.InputHandler = InputHandler;

            var buttonMasher = Owner.AddComponent<ButtonMashBar>();
            buttonMasher.InputHandler = InputHandler;

            // animator need to be created after controlComp and input handler
            var playerAnimator = Owner.AddComponent<Animator>();
            SetPlayerAnimationSprite();

            var cameraControlPoint = Owner.AddComponent<CameraControlPoint>();
            cameraControlPoint.Margin = new Border(5, 10, 5, 5);
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
            Owner.AddComponent<MyAnimatorController>();
        }
    }


}
