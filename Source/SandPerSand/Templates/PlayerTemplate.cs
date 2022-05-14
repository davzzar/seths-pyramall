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
            jumpSoundComp.Trigger = () =>
            {
                var pcc = playerGo.GetComponent<PlayerControlComponent>();

                if (pcc.WillJump)
                {
                    return true;
                }

                return false;
            };

            return playerGo;
        }
    }
}
