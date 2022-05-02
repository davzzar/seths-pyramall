using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    public static partial class Template
    {
        public static GameObject MakePlayer(PlayerIndex playerIndex, Vector2 position)
        {
            var playerGo = new GameObject($"Player {playerIndex}");
            playerGo.Transform.LocalPosition = position;

            var playerRenderer = playerGo.AddComponent<SpriteRenderer>();

            string spritePath = playerIndex switch
            {
                (PlayerIndex.One) => "sprite_player_one",
                (PlayerIndex.Two) => "sprite_player_two",
                (PlayerIndex.Three) => "sprite_player_three",
                (PlayerIndex.Four) => "sprite_player_four",
                _ => ""
            };

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
            Debug.Print("player with player index created");
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

            var cameraControlPoint = playerGo.AddComponent<CameraControlPoint>();
            cameraControlPoint.Margin = new Border(5, 10, 5, 5);

            return playerGo;
        }
    }
}
