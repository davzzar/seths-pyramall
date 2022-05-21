using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;
using TiledCS;

namespace SandPerSand
{
    internal class ItemManager : Behaviour
    {
        public InputHandler inputHandler { get; set; }
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

            string itemId = "";

            var playerIndex = inputHandler.PlayerIndex;

            if ((inputHandler.getButtonState(Buttons.RightShoulder) == ButtonState.Pressed || inputHandler.getButtonState(Buttons.RightTrigger) == ButtonState.Pressed) && (GameStateManager.Instance.CurrentState == GameState.InRound || GameStateManager.Instance.CurrentState == GameState.CountDown))
            {
                itemId = PlayersManager.Instance.UseItem(playerIndex, true);
            }

            if ((inputHandler.getButtonState(Buttons.LeftShoulder) == ButtonState.Pressed || inputHandler.getButtonState(Buttons.LeftTrigger) == ButtonState.Pressed) && (GameStateManager.Instance.CurrentState == GameState.InRound || GameStateManager.Instance.CurrentState == GameState.CountDown))
            {
                itemId = PlayersManager.Instance.UseItem(playerIndex, false);
            }

            var players = PlayersManager.Instance.InGamePlayerGo();

            switch (itemId)
            {
                case "portable_sand_source":
                    SandSimulation sandSim = GameObject.FindComponent<SandSimulation>();
                    Vector2 pos = this.Transform.Position;
                    sandSim.AddSandSource(new Aabb(pos.X, pos.Y, 32 / 64f, 32 / 64f));
                    break;
                case "position_swap":
                    float y = 0;
                    int i = 0;
                    GameObject firstPlayer = null;
                    foreach (GameObject p in players)
                    {
                        if (p.Transform.Position.Y > y && p != this.Owner)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer = p;
                        }
                        i++;
                    }
                    if(firstPlayer!= null)
                    {
                        firstPlayer.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.PositionSwapItem(ItemId.position_swap, this.Transform.Position, 0f, 0f, true, Vector2.Zero, firstPlayer.Transform.Position));
                        this.Owner.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.PositionSwapItem(ItemId.position_swap, firstPlayer.Transform.Position, 0f, 0f, true, Vector2.Zero, this.Transform.Position));
                    }
                    break;
                case "lightning":
                    foreach (var p in players)
                    {
                        if (p != Owner && p.Transform.Position.Y > Transform.Position.Y)
                        {
                            var dist = p.Transform.Position.Y - this.Transform.Position.Y;
                            var duration = Math.Min(10, Math.Max(3, (int)(dist / 3)));
                            p.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.lightning, this.Transform.Position, duration, duration, true));
                        }
                    }
                    break;
                case "magnet":
                    break;
                case "ice_block":
                    float y2 = 0;
                    int i2 = 0;
                    GameObject firstPlayer2 = null;
                    foreach (GameObject p in players)
                    {
                        if (p.Transform.Position.Y > y2 && p != this.Owner)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer2 = p;
                        }
                        i2++;
                    }
                    if(firstPlayer2 != null)
                    {
                        firstPlayer2.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.ice_block, this.Transform.Position, 3f, 3f, true));
                    }
                    break;
                case "sunglasses":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.sunglasses, Vector2.Zero, float.NaN, float.NaN, false));
                    break;
                case "wings":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.wings, Vector2.Zero, 5f, 5f, false));
                    break;
                case "speedup":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.speedup, Vector2.Zero, 10f, 10f, false));
                    break;
                case "dizzy_eyes":
                    foreach (GameObject p in players)
                    {
                        if (p != this.Owner)
                        {
                            p.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.dizzy_eyes, this.Transform.Position, 5f, 5f, true));
                        }
                    }
                    break;
                case "shield":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.shield, Vector2.Zero, 10f, 10f, false));
                    break;
            }

            var activeItems = PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().ActiveItems;
            var pursueItems = PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().PursueItems;

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
                if (pursueItems[i].Id == ItemId.position_swap)
                {
                    PursueItemRendererList[i].SetSourceRectangle(63, 32, 32);
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

            var nobar = new string[] { "sunglasses"};
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
