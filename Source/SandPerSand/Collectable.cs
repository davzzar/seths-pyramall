using Engine;
using Microsoft.Xna.Framework;
using System.Diagnostics;


namespace SandPerSand
{
    public class Collectable : Behaviour
    {
        CollectableType collectableType;
        string CollectableID;

        public void init(CollectableType type, string collectableID)
        {
            this.collectableType = type;
            this.CollectableID = collectableID;
        }


        protected override void Update()
        {
            foreach (var item in PlayersManager.Instance.Players)
            {
                var playerIndex = item.Key;
                var player = item.Value;
                var playerState = player.GetComponent<PlayerStates>();
                if (!playerState.Exited)
                {
                    Vector2 distance = this.Transform.Position - player.Transform.Position;
                    if (distance.Length() <= 1f)
                    {
                        Debug.Print("Player " + playerIndex + " collected." + CollectableID);
                        // a player reached the exit
                        //TODO record round ranking and other information somewhere
                    }
                }
            }
        }
    }

    public enum CollectableType
    {
        coin,
        item
    }

}
