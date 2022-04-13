using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    public class PlayersManager : Component
    {
        public static PlayersManager Instance
        {
            get
            {
                return GameObject.FindComponent<PlayersManager>();
            }
        }

        private Dictionary<PlayerIndex, GameObject> multiplayersGo;

        protected override void OnAwake()
        {
            base.OnAwake();
            multiplayersGo = new Dictionary<PlayerIndex, GameObject>();
            foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
            {
                multiplayersGo.Add(playerIndex, null);
            }

        }

        public GameObject GetPlayer(PlayerIndex index) {
            return multiplayersGo[index];
        }

        public bool DestroyPlayer(PlayerIndex index)
        {
            if (multiplayersGo[index] != null)
            {
                multiplayersGo[index].Destroy();
                multiplayersGo[index] = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CreatePlayer(PlayerIndex playerIndex, Vector2 position)
        {
            if (multiplayersGo[playerIndex] != null)
            {
                multiplayersGo[playerIndex].Destroy();
            }
            var playerGo = new GameObject();
            playerGo.Transform.Position = position;

            var playerRenderer = playerGo.AddComponent<SpriteRenderer>();
            playerRenderer.LoadFromContent("Smiley");

            var playerCollider = playerGo.AddComponent<PolygonCollider>();
            playerCollider.Outline = new[]
            {
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, 0.5f)
            };
            playerCollider.Friction = 0.0f;
            var playerRB = playerGo.AddComponent<RigidBody>();
            playerRB.IsKinematic = false;
            playerRB.FreezeRotation = true;

            var playerCon = playerGo.AddComponent<PlayerControlComponent>();
            playerCon.PlayerIndex = playerIndex;

            multiplayersGo[playerIndex] = playerGo;


        }


    }
}
