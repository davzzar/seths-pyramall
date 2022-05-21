using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace SandPerSand.Items
{
    public class Item
    {
        public ItemId Id { get; }
        public float TimeLeft { get; set; }
        public float TotTime { get; }
        public bool pursue { get; set; }
        public Vector2 Position { get; set; }
        public bool Delete = false;

        public Item(ItemId id, Vector2 Destination, float TimeLeft, float TotTime, bool pursue)
        {
            this.Id = id;
            this.Position = Destination;
            this.TimeLeft = TimeLeft;
            this.TotTime = TotTime;
            this.pursue = pursue;
        }
    }

    public class PositionSwapItem : Item
    {
        public Vector2 Source { get;}
        public Vector2 Velocity { get;}
        public float ExchangeTime { get; } = 1f;
        public float ExchangeTimePassed { get; set; } = 0f;

        public PositionSwapItem(ItemId id, Vector2 Destination, float TimeLeft, float TotTime, bool pursue, Vector2 Velocity, Vector2 Source) : base(id, Destination, TimeLeft, TotTime, pursue)
        {
            this.Position = Destination;
            this.Velocity = Velocity;
            this.Source = Source;
            this.pursue = true;
        }
    }
}

public enum ItemId
{
    lightning,
    magnet,
    ice_block,
    portable_sand_source,
    sunglasses,
    position_swap,
    wings,
    speedup,
    dizzy_eyes,
    shield,
}