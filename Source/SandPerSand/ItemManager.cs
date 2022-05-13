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
        public List<SpriteRenderer> ItemRendererList;
        public GameObject RenderGameObject;
        private Dictionary<string, int> itemIDtoTiledID;
        private TiledTileset tiledS;

        protected override void OnAwake()
        {
            base.OnAwake();
            RenderGameObject = new GameObject();
            ItemRendererList = new List<SpriteRenderer>();

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

        }

        protected override void Update()
        {
            base.Update();

            string itemId = "";

            var playerIndex = inputHandler.PlayerIndex;

            if((inputHandler.getButtonState(Buttons.RightShoulder) == ButtonState.Pressed || inputHandler.getButtonState(Buttons.RightTrigger) == ButtonState.Pressed) && GameStateManager.Instance.CurrentState == GameState.InRound)
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
                        if(p.Transform.Position.Y > y)
                        {
                            y = p.Transform.Position.Y;
                            
                            firstPlayer = p.PlayerIndex;
                        }
                        i++;
                    }
                    //Vector2 postmp = players[firstPlayer].Transform.Position;
                    //players[firstPlayer].Transform.Position = this.Transform.Position;
                    //this.Transform.Position = postmp;
                    PlayersManager.Instance.Players[firstPlayer].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 5, this.Transform.Position);
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, 5, PlayersManager.Instance.Players[firstPlayer].Transform.Position);
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
                                    duration = (int) (dist / 3);
                                }
                                PlayersManager.Instance.Players[p.PlayerIndex].GetComponentInChildren<PlayerStates>().addActiveItem(itemId, duration, this.Transform.Position);
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
                        if (p.Transform.Position.Y > y2  && p.PlayerIndex != playerIndex)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer2 = p.PlayerIndex;
                        }
                        i2++;
                    }
                    PlayersManager.Instance.Players[firstPlayer2].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 3, this.Transform.Position));
                    break;
                case "sunglasses":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 100, this.Transform.Position));
                    break;
                case "wings":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 10, this.Transform.Position));
                    break;
                case "speedup":
                    //PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 10));
                    break;
                case "dizzy_eyes":
                    PlayerComponent[] players3 = GameObject.FindComponents<PlayerComponent>();
                    foreach (PlayerComponent p in players3)
                    {
                        if(p.PlayerIndex != playerIndex)
                        {
                            PlayersManager.Instance.Players[p.PlayerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 5, this.Transform.Position));
                        }
                    }
                    break;
                case "shield":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 10, this.Transform.Position));
                    break;
            }

            var activeItems = PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems;

            if (activeItems.Count != ItemRendererList.Count)
            {
                for(int i = 0; i < ItemRendererList.Count; i++)
                {
                    ItemRendererList[i].Destroy();
                }
                ItemRendererList = new List<SpriteRenderer>();
                for(int i = 0; i < activeItems.Count; i++)
                {
                    ItemRendererList.Add(RenderGameObject.AddComponent<SpriteRenderer>());
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
            }

        }
    }
}
