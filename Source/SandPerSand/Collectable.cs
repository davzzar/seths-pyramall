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
            Debug.Print("enter");
        }

    }

    public enum CollectableType
    {
        coin,
        item
    }

}
