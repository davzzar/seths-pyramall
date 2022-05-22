using Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using TiledCS;

namespace SandPerSand.Items
{
    internal class ItemRenderer : Behaviour
    {
        public List<GameObject> PursueItemRendererGo;
        public List<SpriteRenderer> PursueItemRendererList;
        public List<GameObject> ActiveItemRendererGo;
        public List<SpriteRenderer> ActiveItemRendererList;
        public Dictionary<ItemId, TimerBar> ItemTimerBar;
        private Dictionary<string, int> itemIDtoTiledID;
        private TiledTileset tiledS;
        private Dictionary<ItemId, Color> ItemBarColor;
        private bool wasfacingright = true;
        protected override void OnAwake()
        {
            base.OnAwake();
            ActiveItemRendererGo = new List<GameObject>();
            ActiveItemRendererList = new List<SpriteRenderer>();
            PursueItemRendererGo = new List<GameObject>();
            PursueItemRendererList = new List<SpriteRenderer>();
            ItemTimerBar = new Dictionary<ItemId, TimerBar>();

            itemIDtoTiledID = new Dictionary<string, int>();
            tiledS = new TiledTileset($"Content/Tiled/Tiledset/TilesetItems.tsx");

            if (tiledS == null)
            {
                throw new NullReferenceException("Load tiledS Failed");
            }

            foreach (TiledTile tile in tiledS.Tiles)
            {
                foreach (TiledProperty property in tile.properties)
                {
                    if (property.name == "item_id")
                    {
                        itemIDtoTiledID[property.value] = tile.id;
                    }
                }
            }

            ItemBarColor = new Dictionary<ItemId, Color>();

            ItemBarColor[ItemId.shield] = Color.Blue;
            ItemBarColor[ItemId.wings] = Color.Gray;
            ItemBarColor[ItemId.lightning] = Color.Yellow;
            ItemBarColor[ItemId.dizzy_eyes] = Color.Black;
            ItemBarColor[ItemId.ice_block] = Color.Blue;
        }

        protected override void Update()
        {
            base.Update();

            // update playerstate rendering

            var activeItems = this.Owner.GetComponentInChildren<PlayerStates>().ActiveItems;
            var pursueItems = this.Owner.GetComponentInChildren<PlayerStates>().PursueItems;

            double xVel = this.Owner.GetComponent<PlayerControlComponent>().rigidBody.LinearVelocity.X;

            bool facingright = xVel >= 0;

            if (Math.Abs(xVel) < 0.001)
            {
                facingright = wasfacingright;
            }
            wasfacingright = facingright;


            if (activeItems.Count != ActiveItemRendererList.Count)
            {
                for (int i = 0; i < ActiveItemRendererList.Count; i++)
                {
                    ActiveItemRendererList[i].Destroy();
                }
                ActiveItemRendererList = new List<SpriteRenderer>();

                for (int i = 0; i < activeItems.Count; i++)
                {
                    ActiveItemRendererGo.Add(new GameObject());
                    ActiveItemRendererGo[i].Transform.Parent = this.Owner.Transform;
                    ActiveItemRendererList.Add(ActiveItemRendererGo[i].AddComponent<SpriteRenderer>());
                    ItemId id = activeItems[i].Id;
                    int tileID = itemIDtoTiledID[id.ToString()];
                    ActiveItemRendererList[i].LoadFromContent($"Tiled/TiledsetTexture/TilesetItems");
                    ActiveItemRendererList[i].SetSourceRectangle(tileID, 32, 32);
                }
            }


            for (int i = 0; i < activeItems.Count; i++)
            {
                if (facingright && (activeItems[i].Id == ItemId.wings || activeItems[i].Id == ItemId.sunglasses))
                {
                    ActiveItemRendererList[i].Effect = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                }
                else if (activeItems[i].Id == ItemId.wings || activeItems[i].Id == ItemId.sunglasses)
                {
                    ActiveItemRendererList[i].Effect = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
                }

                if (activeItems[i].Id == ItemId.wings)
                {
                    ActiveItemRendererList[i].Depth = 0.2f;
                    ActiveItemRendererList[i].Transform.LossyScale = 0.6f * Vector2.One;
                    ActiveItemRendererList[i].Transform.LocalPosition = -(Vector2.UnitY * 0.1f) + (facingright ? -Vector2.UnitX * .4f : Vector2.UnitX * .4f);
                }
                else if (activeItems[i].Id == ItemId.sunglasses)
                {
                    ActiveItemRendererList[i].Depth = 0.05f;
                    ActiveItemRendererList[i].Transform.LossyScale = 0.5f * Vector2.One;
                    ActiveItemRendererList[i].Transform.LocalPosition = -(Vector2.UnitY * 0.125f) + (facingright ? -Vector2.UnitX * .1f : Vector2.UnitX * .1f);
                }
                else if (activeItems[i].Id == ItemId.lightning)
                {
                    ActiveItemRendererList[i].SetSourceRectangle(63, 32, 32);
                }
                else
                {
                    ActiveItemRendererList[i].Depth = 0.05f;
                }
            }


            if (pursueItems.Count != PursueItemRendererList.Count)
            {
                for (int i = 0; i < PursueItemRendererList.Count; i++)
                {
                    PursueItemRendererList[i].Destroy();
                    if (PursueItemRendererGo[i] != null)
                    {
                        PursueItemRendererGo[i].Destroy();
                    }
                }
                PursueItemRendererList = new List<SpriteRenderer>();
                PursueItemRendererGo = new List<GameObject>();


                for (int i = 0; i < pursueItems.Count; i++)
                {
                    PursueItemRendererGo.Add(new GameObject());
                    PursueItemRendererList.Add(PursueItemRendererGo[i].AddComponent<SpriteRenderer>());
                    ItemId id = pursueItems[i].Id;
                    int tileID = itemIDtoTiledID[id.ToString()];
                    PursueItemRendererList[i].LoadFromContent($"Tiled/TiledsetTexture/TilesetItems");
                    PursueItemRendererList[i].SetSourceRectangle(tileID, 32, 32);
                }
            }

            for (int i = 0; i < pursueItems.Count; i++)
            {
                PursueItemRendererList[i].Transform.Position = pursueItems[i].Position;
                if (pursueItems[i].Id == ItemId.position_swap)
                {
                    PursueItemRendererList[i].SetSourceRectangle(63, 32, 32);
                }

            }

            var nobar = new string[] { "sunglasses" };
            int barpos = 0;

            for (int i = 0; i < activeItems.Count; i++)
            {
                if (Array.IndexOf(nobar, activeItems[i].Id) == -1)
                {
                    if ((!ItemTimerBar.ContainsKey(activeItems[i].Id) || !ItemTimerBar[activeItems[i].Id].IsAlive) && activeItems[i].TimeLeft > 1)
                    {
                        ItemTimerBar[activeItems[i].Id] = this.Owner.AddComponent<TimerBar>();
                        ItemTimerBar[activeItems[i].Id].ShouldSelfDestruct = true;
                        ItemTimerBar[activeItems[i].Id].SetDurationAndRefill(activeItems[i].TimeLeft, activeItems[i].TotTime);
                        ItemTimerBar[activeItems[i].Id].FillColor = ItemBarColor[activeItems[i].Id];
                        ItemTimerBar[activeItems[i].Id].OriginOffset = -Vector2.UnitY * (-1 - barpos * 0.2f);
                    }
                    else
                    {
                        if (activeItems[i].TotTime - activeItems[i].TimeLeft < 0.1f)
                        {
                            ItemTimerBar[activeItems[i].Id].SetDurationAndRefill(activeItems[i].TimeLeft);
                            ItemTimerBar[activeItems[i].Id].OriginOffset = Vector2.UnitY * (-1 - barpos * 0.2f);
                        }
                    }
                    barpos++;

                }
            }
        }
    }
}
