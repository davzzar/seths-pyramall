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
        public List<GameObject> ItemRendererGo;
        public List<SpriteRenderer> ItemRendererList;
        public Dictionary<string, TimerBar> ItemTimerBar;
        private Dictionary<string, int> itemIDtoTiledID;
        private TiledTileset tiledS;
        private Dictionary<string, Color> ItemBarColor;

        protected override void OnAwake()
        {
            base.OnAwake();
            ItemRendererGo = new List<GameObject>();
            ItemRendererList = new List<SpriteRenderer>();
            ItemTimerBar = new Dictionary<string, TimerBar>();

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

            ItemBarColor = new Dictionary<string, Color>();

            ItemBarColor["shield"] = Color.Blue;
            ItemBarColor["wings"] = Color.Gray;
            ItemBarColor["lightning"] = Color.Yellow;
            ItemBarColor["dizzy_eyes"] = Color.Black;
            ItemBarColor["shield"] = Color.Blue;
            ItemBarColor["ice_block"] = Color.Blue;


        }

        protected override void Update()
        {
            base.Update();

            string itemId = "";

            var playerIndex = inputHandler.PlayerIndex;

            if ((inputHandler.getButtonState(Buttons.RightShoulder) == ButtonState.Pressed || inputHandler.getButtonState(Buttons.RightTrigger) == ButtonState.Pressed) && GameStateManager.Instance.CurrentState == GameState.InRound)
            {
                itemId = PlayersManager.Instance.useItem(playerIndex, true);
            }

            if ((inputHandler.getButtonState(Buttons.LeftShoulder) == ButtonState.Pressed || inputHandler.getButtonState(Buttons.LeftTrigger) == ButtonState.Pressed) && GameStateManager.Instance.CurrentState == GameState.InRound)
            {
                itemId = PlayersManager.Instance.useItem(playerIndex, false);
            }

            switch (itemId)
            {
                case "portable_sand_source":
                    SandSimulation sandSim = GameObject.FindComponent<SandSimulation>();
                    Vector2 pos = this.Transform.Position;
                    sandSim.AddSandSource(new Aabb(pos.X, pos.Y, 32 / 64f, 32 / 64f));
                    break;
                case "position_swap":
                    PlayerComponent[] players = GameObject.FindComponents<PlayerComponent>();
                    float y = 0;
                    int i = 0;
                    PlayerIndex firstPlayer = playerIndex;
                    foreach (PlayerComponent p in players)
                    {
                        if (p.Transform.Position.Y > y)
                        {
                            y = p.Transform.Position.Y;

                            firstPlayer = p.PlayerIndex;
                        }
                        i++;
                    }
                    //Vector2 postmp = players[firstPlayer].Transform.Position;
                    //players[firstPlayer].Transform.Position = this.Transform.Position;
                    //this.Transform.Position = postmp;
                    PlayersManager.Instance.Players[firstPlayer].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 5f, 5f, this.Transform.Position);
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 5f, 5f, PlayersManager.Instance.Players[firstPlayer].Transform.Position);
                    break;
                case "lightning":
                    PlayerComponent[] playerslit = GameObject.FindComponents<PlayerComponent>();
                    foreach (PlayerComponent p in playerslit)
                    {
                        if (p.Transform.Position.Y > this.Transform.Position.Y)
                        {
                            if (p.PlayerIndex != playerIndex && p.Transform.Position.Y > this.Transform.Position.Y)
                            {
                                int duration = 0;
                                float dist = p.Transform.Position.Y - this.Transform.Position.Y;
                                if (dist > 30)
                                {
                                    duration = 10;
                                }
                                else
                                {
                                    duration = (int)(dist / 3);
                                }
                                PlayersManager.Instance.Players[p.PlayerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, duration, duration, this.Transform.Position);
                            }
                        }
                    }
                    break;
                case "magnet":
                    break;
                case "ice_block":
                    PlayerComponent[] players2 = GameObject.FindComponents<PlayerComponent>();
                    float y2 = 0;
                    int i2 = 0;
                    PlayerIndex firstPlayer2 = 0;
                    foreach (PlayerComponent p in players2)
                    {
                        if (p.Transform.Position.Y > y2 && p.PlayerIndex != playerIndex)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer2 = p.PlayerIndex;
                        }
                        i2++;
                    }
                    PlayersManager.Instance.Players[firstPlayer2].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 3f, 3f, this.Transform.Position);
                    break;
                case "sunglasses":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, float.NaN, float.NaN, -Vector2.One);
                    break;
                case "wings":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 10f, 10f, -Vector2.One);
                    break;
                case "speedup":
                    //PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 10);
                    break;
                case "dizzy_eyes":
                    PlayerComponent[] players3 = GameObject.FindComponents<PlayerComponent>();
                    foreach (PlayerComponent p in players3)
                    {
                        if (p.PlayerIndex != playerIndex)
                        {
                            PlayersManager.Instance.Players[p.PlayerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 5f, 5f, this.Transform.Position);
                        }
                    }
                    break;
                case "shield":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 10f, 10f, this.Transform.Position);
                    break;
            }

            var activeItems = PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems;
            bool facingright = this.Owner.GetComponent<PlayerControlComponent>().rigidBody.LinearVelocity.X >= 0;

            if (activeItems.Count != ItemRendererList.Count)
            {
                for (int i = 0; i < ItemRendererList.Count; i++)
                {
                    ItemRendererList[i].Destroy();
                    ItemRendererGo[i].Destroy();
                }
                ItemRendererList = new List<SpriteRenderer>();
                ItemRendererGo = new List<GameObject>();
                for (int i = 0; i < activeItems.Count; i++)
                {
                    ItemRendererGo.Add(new GameObject());
                    ItemRendererList.Add(ItemRendererGo[i].AddComponent<SpriteRenderer>());
                    string id = activeItems[i].id;
                    int tileID = itemIDtoTiledID[id];
                    ItemRendererList[i].LoadFromContent($"Tiled/TiledsetTexture/TilesetItems");
                    ItemRendererList[i].SetSourceRectangle(tileID, 32, 32);
                }
            }

            for (int i = 0; i < activeItems.Count; i++)
            {
                if (activeItems[i].pos.Y < 0)
                {
                    ItemRendererList[i].Transform.Position = this.Transform.Position;

                }
                else
                {
                    ItemRendererList[i].Transform.Position = activeItems[i].pos;
                }
                if (facingright && (activeItems[i].id == "wings" || activeItems[i].id == "sunglasses"))
                {
                    ItemRendererList[i].Effect = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                }
                else
                {
                    ItemRendererList[i].Effect = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
                }
                if (activeItems[i].id == "wings")
                {
                    ItemRendererList[i].Depth = 0.2f;
                    ItemRendererList[i].Transform.LossyScale = 0.6f * Vector2.One;
                    ItemRendererList[i].Transform.Position = this.Transform.Position + (facingright ? -Vector2.UnitX * .2f : Vector2.UnitX * .2f);
                }
                else if (activeItems[i].id == "sunglasses")
                {
                    ItemRendererList[i].Depth = 0.05f;
                    ItemRendererList[i].Transform.LossyScale = 0.6f * Vector2.One;
                    ItemRendererList[i].Transform.Position = this.Transform.Position + this.Owner.GetComponent<PlayerControlComponent>().rigidBody.LinearVelocity / 50 - (Vector2.UnitY * 0.15f) + (facingright ? -Vector2.UnitX * .1f : Vector2.UnitX * .1f);
                }
                //else if (activeItems[i].id == "lightning")
                //{
                //    if (activeItems[i].pos.Y < 0)
                //    {
                //        ItemRendererList[i].SetSourceRectangle(63, 32, 32);
                //    }
                //    else
                //    {
                //        //ItemRendererList[i].Transform.LocalRotation = new Quaternion(i,)
                //    }
                //}
                else if (activeItems[i].id == "position_swap")
                {
                    ItemRendererList[i].SetSourceRectangle(63, 32, 32);
                }
                else
                {
                    ItemRendererList[i].Depth = 0.05f;
                }

                if (activeItems[i].id != "position_swap")
                {

                }
            }

            var nobar = new string[] { "sunglasses", "position_swap" };
            int barpos = 0;

            for (int i = 0; i < activeItems.Count; i++)
            {
                if (Array.IndexOf(nobar, activeItems[i].id) == -1)
                {
                    if ((!ItemTimerBar.ContainsKey(activeItems[i].id) || !ItemTimerBar[activeItems[i].id].IsAlive) && activeItems[i].timeleft > 1)
                    {
                        ItemTimerBar[activeItems[i].id] = this.Owner.AddComponent<TimerBar>();
                        ItemTimerBar[activeItems[i].id].ShouldSelfDestruct = true;
                        ItemTimerBar[activeItems[i].id].SetDurationAndRefill(activeItems[i].timeleft, activeItems[i].tot_time);
                        ItemTimerBar[activeItems[i].id].FillColor = ItemBarColor[activeItems[i].id];
                        ItemTimerBar[activeItems[i].id].OriginOffset = -Vector2.UnitY * (-1 - barpos * 0.2f);
                    }
                    else
                    {
                        if (activeItems[i].tot_time - activeItems[i].timeleft < 0.1f)
                        {
                            ItemTimerBar[activeItems[i].id].SetDurationAndRefill(activeItems[i].timeleft);
                            ItemTimerBar[activeItems[i].id].OriginOffset = Vector2.UnitY * (-1 - barpos * 0.2f);
                        }
                    }
                    barpos++;

                }
            }

        }
    }
}
