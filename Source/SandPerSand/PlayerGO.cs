using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    // TODO Make this a subclass of GameObject
    public class PlayerGo
    {
        public static GameObject Create(PlayerIndex playerIndex, Vector2 position)
        {
            var playerGo = new GameObject
            {
                Transform =
                {
                    Position = position
                }
            };

            var playerRenderer = playerGo.AddComponent<SpriteRenderer>();
            playerRenderer.LoadFromContent("ProtoPlayer");
            playerRenderer.Depth = 0f;

            var playerCollider = playerGo.AddComponent<CircleCollider>();
            playerCollider.Radius = 1f;
            playerCollider.Friction = 0.0f;

            var playerRB = playerGo.AddComponent<RigidBody>();
            playerRB.IsKinematic = false;
            playerRB.FreezeRotation = true;
            playerRB.IgnoreGravity = true;

            playerGo.AddComponent<GroundCheckComponent>();

            var inputHandler = new InputHandler(playerIndex);
            var controlComp = playerGo.AddComponent<PlayerControlComponent>();
            controlComp.InputHandler = inputHandler;

            var playerStates = playerGo.AddComponent<PlayerStates>();
            playerStates.InputHandler = inputHandler;

            //FOR DEBUG (updated in the PlayerControlComponent)
            var textRenderer = playerGo.AddComponent<GuiTextRenderer>();
            textRenderer.ScreenPosition = Vector2.UnitY * 100f;
            var tracer = playerGo.AddComponent<TracerRendererComponent>();
            tracer.TraceLength = 60;

            return playerGo;
        }

        public static GameObject AddAnim(GameObject playerGo)
        {
            // delete static sprite
            playerGo.GetComponent<SpriteRenderer>().Destroy();

            // animator need to be created after controlComp and input handler
            var playerAnimator = playerGo.AddComponent<Animator>();
            playerAnimator.LoadFromContent("PlayerAnimated");
            var playerAnimatorController = playerGo.AddComponent<MyAnimatorController>();

            return playerGo;
        }
    }
}
