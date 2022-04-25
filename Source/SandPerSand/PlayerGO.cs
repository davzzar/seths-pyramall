using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using System.Diagnostics;

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

            string spritePath = "";
            switch (playerIndex)
            {
                case (PlayerIndex.One):
                    spritePath = "sprite_player_one";
                    break;
                case (PlayerIndex.Two):
                    spritePath = "sprite_player_two";
                    break;
                case (PlayerIndex.Three):
                    spritePath = "sprite_player_three";
                    break;
                case (PlayerIndex.Four):
                    spritePath = "sprite_player_four";
                    break;
            }

            playerRenderer.LoadFromContent(spritePath);
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
            Debug.Print("playerwith player index created");
            Debug.Print(playerIndex.ToString());

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
            var playerIndex = playerGo.GetComponent<PlayerControlComponent>().InputHandler.PlayerIndex;

            string animationPath = "";
            switch (playerIndex)
            {
                case (PlayerIndex.One):
                    animationPath = "PlayerAnimated";
                    break;
                case (PlayerIndex.Two):
                    animationPath = "PlayerAnimated";
                    break;
                case (PlayerIndex.Three):
                    animationPath = "PlayerAnimated";
                    break;
                case (PlayerIndex.Four):
                    animationPath = "PlayerAnimated";
                    break;
            }
            playerAnimator.LoadFromContent(animationPath);
            var playerAnimatorController = playerGo.AddComponent<MyAnimatorController>();

            return playerGo;
        }
    }
}
