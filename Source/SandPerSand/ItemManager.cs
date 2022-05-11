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
        public SpriteRenderer ItemRenderer { get; set; }
        private Dictionary<string, int> itemIDtoTiledID;
        private TiledTileset tiledS;

        protected override void OnAwake()
        {
            base.OnAwake();
            ItemRenderer = this.Owner.AddComponent<SpriteRenderer>();
            ItemRenderer.LoadFromContent("TilesetItems");

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

            if(inputHandler.getButtonState(Buttons.X) == ButtonState.Pressed && GameStateManager.Instance.CurrentState == GameState.InRound)
            {
                itemId = PlayersManager.Instance.useItem(playerIndex, false);
            }

            if (inputHandler.getButtonState(Buttons.Y) == ButtonState.Pressed && GameStateManager.Instance.CurrentState == GameState.InRound)
            {
                itemId = PlayersManager.Instance.useItem(playerIndex, true);
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
                    int firstPlayer = 0;
                    foreach (PlayerComponent p in players)
                    {
                        if(p.Transform.Position.Y > y)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer = i;
                        }
                        i++;
                    }
                    Vector2 postmp = players[firstPlayer].Transform.Position;
                    players[firstPlayer].Transform.Position = this.Transform.Position;
                    this.Transform.Position = postmp;
                    break;
                case "lightning":
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
                        if (p.Transform.Position.Y > y2)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer2 = p.PlayerIndex;
                        }
                        i2++;
                    }
                    PlayersManager.Instance.Players[firstPlayer2].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 3));
                    break;
                case "sunglasses":
                    break;
                case "wings":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 10));
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
                            PlayersManager.Instance.Players[p.PlayerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 5));
                        }
                    }
                    break;
                case "shield":
                    //PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 10));
                    break;
            }

            var activeItems = PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems;

            if (activeItems.Count > 0)
            {
                string id = activeItems[0].id;
                int tileID = itemIDtoTiledID[id];
                ItemRenderer.SetSourceRectangle(tileID, 32, 32);
            }
            else
            {
                ItemRenderer.SetSourceRectangle(63, 32, 32);

            }

        }
    }
}
