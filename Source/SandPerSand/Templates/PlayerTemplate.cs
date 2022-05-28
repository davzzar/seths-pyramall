using System;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
            var playerGo = new GameObject($"Player {playerIndex}");
            var playerComponent = playerGo.AddComponent<PlayerComponent>();
            playerComponent.PlayerIndex = playerIndex;

            playerGo.Transform.LocalPosition = position;

            var landSoundComponent = playerGo.AddComponent<SoundEffectPlayer>();
            landSoundComponent.LoadFromContent("Sounds/player_land");
            landSoundComponent.Volume = 0.6f;
            landSoundComponent.Trigger = () =>
            {
                var pcc = playerGo.GetComponent<PlayerControlComponent>();
                return pcc.HasLanded;
            };

            var sandLandSoundComponent = playerGo.AddComponent<SoundEffectPlayer>();
            sandLandSoundComponent.LoadFromContent("Sounds/player_land_sand");
            sandLandSoundComponent.Volume = 0.6f;
            sandLandSoundComponent.Trigger = () =>
            {
                var pcc = playerGo.GetComponent<PlayerControlComponent>();
                return pcc.HasLandedInSand;
            };

            var diggingSoundComp = playerGo.AddComponent<SoundEffectPlayer>();
            diggingSoundComp.LoadFromContent("Sounds/player_sand_dig");
            diggingSoundComp.Volume = 0.2f;
            diggingSoundComp.Trigger = () =>
            {
                var pcc = playerGo.GetComponent<PlayerControlComponent>();
                return pcc.HasSandReached && pcc.InputHandler.getButtonState(Buttons.A) == ButtonState.Pressed;
            };

            var stepSoundComp = playerGo.AddComponent<SoundEffectPlayer>();
            stepSoundComp.LoadFromContent("Sounds/player_step01", 
                "Sounds/player_step02", 
                "Sounds/player_step03",
                "Sounds/player_step04", 
                "Sounds/player_step05");
            stepSoundComp.ShouldLock = true;
            stepSoundComp.Volume = 0.2f;
            stepSoundComp.Trigger = () =>
            {
                var pcc = playerGo.GetComponent<PlayerControlComponent>();
                var rb = playerGo.GetComponent<RigidBody>();
                return pcc.IsGrounded && MathF.Abs(rb.LinearVelocity.X) > 0.3f && pcc.HorizontalSpeed != 0f;
            };

            return playerGo;
        }
    }
}
