using Engine;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace SandPerSand
{
    public class ShopEntryScript : Behaviour
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            var playersManager = PlayersManager.Instance;
            PlayersManager.Instance.ShopEntryPosition = this.Owner.Transform.Position;
            this.Owner.Destroy();
        }
    }

    public class ShopExitScript : Behaviour
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void Update()
        {
            base.Update();
            var currentGameState = GameStateManager.Instance.CurrentState;
            if (currentGameState != GameState.Shop)
            {
                return;
            }

            foreach (var item in PlayersManager.Instance.Players)
            {
                var playerIndex = item.Key;
                var player = item.Value;
                var playerState = player.GetComponent<PlayerStates>();
                if (!playerState.FnishedShop)
                {
                    Vector2 distance = this.Transform.Position - player.Transform.Position;
                    if (distance.Length() <= 1f)
                    {
                        // a player reached the exit
                        //TODO record round ranking and other information somewhere
                        Debug.Print("Player " + playerIndex + " triggered Exit.");
                        playerState.FnishedShop = true;
                    }
                }
            }
        }
    }
}
