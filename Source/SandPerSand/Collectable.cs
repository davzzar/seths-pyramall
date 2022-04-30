using Engine;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;


namespace SandPerSand
{
    public class Collectable : Behaviour
    {
        CollectableType collectableType;
        string CollectableID;

        public void init(CollectableType type, string collectableID, Vector2[] outline)
        {
            this.collectableType = type;
            this.CollectableID = collectableID;

            // Add collider and other compounents for the Tile GameObject
            var collCollider = this.Owner.AddComponent<PolygonCollider>();
            collCollider.Outline = outline;
            collCollider.IsTrigger = true;
            collCollider.CollisionEnter += OnCollisionEnter;
        }

        private void OnCollisionEnter(object sender, Collider e)
        {
            throw new NotImplementedException();
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
