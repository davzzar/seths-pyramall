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
        Boolean destroy = false;

        protected override void Update()
        {
            base.Update();
            if (destroy)
            {
                this.Owner.Destroy();
                Debug.Print("destroyed");
            }
        }

        public void init(CollectableType type, string collectableID, Vector2[] outline)
        {
            this.collectableType = type;
            this.CollectableID = collectableID;

            // Add collider and other compounents for the Tile GameObject
            var collCollider = this.Owner.AddComponent<PolygonCollider>();
            collCollider.Outline = outline;
            collCollider.IsTrigger = true;
            if(collectableType == CollectableType.item)
            {
                collCollider.CollisionEnter += ItemCollision;
            }
            else if(collectableType == CollectableType.coin)
            {
                collCollider.CollisionEnter += CoinCollision;
            }
        }

        private void ItemCollision(object sender, Collider e)
        {
            Debug.Print(sender.ToString());

            //playerGo playerGo = e.Owner;

            foreach (var item in PlayersManager.Instance.Players)
            {
                var playerIndex = item.Key;
                var player = item.Value;
                var collider = player.GetComponentInChildren<Collider>();

                if(collider == e)
                {
                    Debug.Print(playerIndex.ToString());
                    this.destroy = PlayersManager.Instance.addItemToInventory(playerIndex, CollectableID, false);
                }
            }
        }

        private void CoinCollision(object sender, Collider e)
        {
            Debug.Print(sender.ToString());

            //playerGo playerGo = e.Owner;

            foreach (var item in PlayersManager.Instance.Players)
            {
                var playerIndex = item.Key;
                var player = item.Value;
                var collider = player.GetComponentInChildren<Collider>();

                if (collider == e)
                {
                    PlayersManager.Instance.addCoins(playerIndex, 1);
                    this.destroy = true;
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
