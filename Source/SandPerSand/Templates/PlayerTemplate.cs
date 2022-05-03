using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    public static partial class Template
    {
        /// <summary>
        /// Create and instantiate a player game object from template.
        /// </summary>
        /// <param name="playerIndex">Index of the layer to be created</param>
        /// <param name="position">Vector position for the player to be created</param>
        /// <returns></returns>
        public static GameObject MakePlayer(PlayerIndex playerIndex, Vector2 position)
        {
            var inputHandler = new InputHandler(playerIndex);
            Debug.Print("player with player index created");
            Debug.Print(playerIndex.ToString());

            var playerGo = new GameObject($"Player {playerIndex}");
            playerGo.Transform.LocalPosition = position;

            var playerCollider = playerGo.AddComponent<CircleCollider>();
            playerCollider.Radius = 1f;
            playerCollider.Friction = 0.0f;

            var playerRB = playerGo.AddComponent<RigidBody>();
            playerRB.IsKinematic = false;
            playerRB.FreezeRotation = true;
            playerRB.IgnoreGravity = true;

            playerGo.AddComponent<GroundCheckComponent>();

            var controlComp = playerGo.AddComponent<PlayerControlComponent>();
            controlComp.InputHandler = inputHandler;

            var playerStates = playerGo.AddComponent<PlayerStates>();
            playerStates.InputHandler = inputHandler;

            // animator need to be created after controlComp and input handler
            var playerAnimator = playerGo.AddComponent<Animator>();
             
            string animationTexture = playerIndex switch
            {
                (PlayerIndex.One) => "PlayerAnimated",
                (PlayerIndex.Two) => "PlayerAnimated2",
                (PlayerIndex.Three) => "PlayerAnimated3",
                (PlayerIndex.Four) => "PlayerAnimated4",
                _=> ""
            };
            
            playerAnimator.LoadFromContent("PlayerAnimated", animationTexture);
            playerGo.AddComponent<MyAnimatorController>();

            //FOR DEBUG (updated in the PlayerControlComponent)
            var textRenderer = playerGo.AddComponent<GuiTextRenderer>();
            textRenderer.ScreenPosition = Vector2.UnitY * 100f;
            var tracer = playerGo.AddComponent<TracerRendererComponent>();
            tracer.TraceLength = 60;

            var cameraControlPoint = playerGo.AddComponent<CameraControlPoint>();
            cameraControlPoint.Margin = new Border(5, 10, 5, 5);

            return playerGo;
        }
    }
}
