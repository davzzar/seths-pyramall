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
            var playerGo = new GameObject($"Player {playerIndex}");
            var playerComponent = playerGo.AddComponent<PlayerComponent>();
            playerComponent.PlayerIndex = playerIndex;

            playerGo.Transform.LocalPosition = position;

            var jumpSoundComp = playerGo.AddComponent<SoundEffectPlayer>();
            jumpSoundComp.LoadFromContent("Sounds/player_land");
            jumpSoundComp.Volume = 0.6f;
            jumpSoundComp.Trigger = () =>
            {
                var pcc = playerGo.GetComponent<PlayerControlComponent>();
                return pcc.WillJump;
            };

            var diggingSoundComp = playerGo.AddComponent<SoundEffectPlayer>();
            diggingSoundComp.LoadFromContent("Sounds/player_sand_dig");
            diggingSoundComp.Volume = 0.2f;
            diggingSoundComp.Trigger = () =>
            {
                var pcc = playerGo.GetComponent<PlayerControlComponent>();
                return pcc.HasSandReached;
            };

            return playerGo;
        }
    }
}
