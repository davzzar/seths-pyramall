using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TiledCS;

namespace SandPerSand
{
    public class GraphicalUserInterface : Behaviour
    {
        public static GraphicalUserInterface Instance
        {
            get
            {
                return GameObject.FindComponent<GraphicalUserInterface>();
            }
        }


        private GameObject guiGo;
        private GuiTextRenderer midScreenTextComp;
        private Dictionary<PlayerIndex, RenderPlayer> renderPlayers;
        private Dictionary<PlayerIndex, Vector2> positions, positionsUnits;
        private Dictionary<string, Vector2> object_size, delta_object;
        private Dictionary<string, int> itemIDtoTiledID;
        private TiledTileset tiledS;
        private GameState oldGameState = GameState.RoundCheck;

        protected override void OnAwake()
        {
            positions = new Dictionary<PlayerIndex, Vector2>{
                { PlayerIndex.One, new Vector2(0f, 0f) },
                { PlayerIndex.Two, new Vector2(1f, 0f)},
                { PlayerIndex.Three, new Vector2(0f, 1f)},
                { PlayerIndex.Four, new Vector2(1f, 1f)}
            };

            positionsUnits = new Dictionary<PlayerIndex, Vector2>{
                { PlayerIndex.One, new Vector2(0f, 0f) },
                { PlayerIndex.Two, new Vector2(-2f, 0f)},
                { PlayerIndex.Three, new Vector2(0f, -1f)},
                { PlayerIndex.Four, new Vector2(-2f, -1f)}
            };

            object_size = new Dictionary<string, Vector2>{
                { "small", new Vector2(0.5f, 0.5f)},
                { "big" , new Vector2(1f, 1f)}
            };

            delta_object = new Dictionary<string, Vector2>{
                { "minor_item", new Vector2(1f, 0.5f)},
                { "major_item" , new Vector2(1.5f, 0.5f)},
                { "char", new Vector2(0f, 0f)},
                { "coins" , new Vector2(1.5f, 0f)}
            };

            renderPlayers = new Dictionary<PlayerIndex, RenderPlayer>();

            itemIDtoTiledID = new Dictionary<string, int>();
            tiledS = new TiledTileset($"Content/tiles/TilesetItems.tsx");

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


        public GraphicalUserInterface()
        {

            guiGo = new GameObject();
            guiGo.Transform.LocalPosition = new Vector2(0.0f, 0f);
        }

        protected override void Update()
        {
            base.Update();
            if (oldGameState != GameStateManager.Instance.CurrentState)
            {
                GameState newGameState = GameStateManager.Instance.CurrentState;

                if (newGameState == GameState.Prepare)
                {
                    if (midScreenTextComp == null)
                    {
                        renderMidScreenText("Press A to start the Game");
                    }
                    else
                    {
                        updateMidScreenText("Press A to start the Game");
                    }
                }
                if (newGameState == GameState.InRound && oldGameState == GameState.Prepare)
                {
                    destroyMidScreenText();
                }
                if (newGameState == GameState.CountDown && oldGameState == GameState.InRound)
                {
                    GraphicalUserInterface.Instance.renderMidScreenText("10.0 Seconds to Finish the Round");
                }
                if (newGameState == GameState.RoundCheck && oldGameState == GameState.CountDown)
                {
                    string ranks = "";
                    //display ranks on screen
                    foreach (var item in PlayersManager.Instance.Players)
                    {
                        ranks += item.Value.GetComponent<PlayerStates>().RoundRank + " - Player " + item.Key + "\n";
                    }
                    GraphicalUserInterface.Instance.updateMidScreenText(ranks);
                }

                oldGameState = newGameState;
            } else if (oldGameState == GameState.CountDown)
            {
                GraphicalUserInterface.Instance.updateMidScreenText(String.Format("{0:0.0}", 10f - GameStateManager.Instance.countDowncounter) + " Seconds to Finish the Round");
            }
        }

        public void renderMidScreenText(string midScreenText)
        {
            midScreenTextComp = guiGo.AddComponent<GuiTextRenderer>();
            midScreenTextComp.PositionMode = GuiTextRenderer.ScreenPositionMode.Relative;
            midScreenTextComp.Text = midScreenText;
            midScreenTextComp.FontSize = 30f;
            midScreenTextComp.IsActive = true;
            midScreenTextComp.ScreenPosition = new Vector2(0.35f, 0.5f);
            midScreenTextComp.Depth = 0;
        }

        public void updateMidScreenText(string midScreenText)
        {
            midScreenTextComp.Text = midScreenText;
        }

        public void destroyMidScreenText()
        {
            midScreenTextComp.Destroy();
        }

        public void renderPlayerInfo(PlayerIndex playerIndex)
        {
            renderPlayers[playerIndex] = new RenderPlayer();

            renderPlayers[playerIndex].character = guiGo.AddComponent<GuiSpriteRenderer>();
            renderPlayers[playerIndex].character.LoadFromContent("player" + playerIndex.ToString());
            renderPlayers[playerIndex].character.PositionMode = GuiSpriteRenderer.ScreenPositionMode.Relative;
            renderPlayers[playerIndex].character.screenPosition = positions[playerIndex];
            renderPlayers[playerIndex].character.screenPositionUnits = positionsUnits[playerIndex] + delta_object["char"];
            renderPlayers[playerIndex].character.sizeUnits = object_size["big"];
            renderPlayers[playerIndex].character.size = new Vector2(0f, 0f);

            renderPlayers[playerIndex].majorItem = guiGo.AddComponent<GuiSpriteRenderer>();
            renderPlayers[playerIndex].majorItem.LoadFromContent("GUI/Item_slot");
            renderPlayers[playerIndex].majorItem.PositionMode = GuiSpriteRenderer.ScreenPositionMode.Relative;
            renderPlayers[playerIndex].majorItem.screenPosition = positions[playerIndex];
            renderPlayers[playerIndex].majorItem.screenPositionUnits = positionsUnits[playerIndex] + delta_object["major_item"];
            renderPlayers[playerIndex].majorItem.sizeUnits = object_size["small"];
            renderPlayers[playerIndex].majorItem.size = new Vector2(0f, 0f);

            renderPlayers[playerIndex].minorItem = guiGo.AddComponent<GuiSpriteRenderer>();
            renderPlayers[playerIndex].minorItem.LoadFromContent("GUI/Item_slot");
            renderPlayers[playerIndex].minorItem.PositionMode = GuiSpriteRenderer.ScreenPositionMode.Relative;
            renderPlayers[playerIndex].minorItem.screenPosition = positions[playerIndex];
            renderPlayers[playerIndex].minorItem.screenPositionUnits = positionsUnits[playerIndex] + delta_object["minor_item"];
            renderPlayers[playerIndex].minorItem.sizeUnits = object_size["small"];
            renderPlayers[playerIndex].minorItem.size = new Vector2(0f, 0f);

            renderPlayers[playerIndex].coins = guiGo.AddComponent<GuiSpriteRenderer>();
            renderPlayers[playerIndex].coins.LoadFromContent("TilesetCoins");
            renderPlayers[playerIndex].coins.PositionMode = GuiSpriteRenderer.ScreenPositionMode.Relative;
            renderPlayers[playerIndex].coins.screenPosition = positions[playerIndex];
            renderPlayers[playerIndex].coins.screenPositionUnits = positionsUnits[playerIndex] + delta_object["coins"];
            renderPlayers[playerIndex].coins.sizeUnits = object_size["small"];
            renderPlayers[playerIndex].coins.size = new Vector2(0f, 0f);
            renderPlayers[playerIndex].coins.sourceRectangle = new Rectangle(0, 0, 32, 32);

            renderPlayers[playerIndex].numOfCoins = guiGo.AddComponent<GuiTextRenderer>();
            renderPlayers[playerIndex].numOfCoins.Text = "00x";
            renderPlayers[playerIndex].numOfCoins.PositionMode = GuiTextRenderer.ScreenPositionMode.Relative;
            renderPlayers[playerIndex].numOfCoins.ScreenPosition = positions[playerIndex] + new Vector2(0.059f, 0.01f);
        }

        public void destroyPlayerInfo(PlayerIndex playerIndex)
        {
            renderPlayers[playerIndex].character.Destroy();
            renderPlayers[playerIndex].majorItem.Destroy();
            renderPlayers[playerIndex].minorItem.Destroy();
            renderPlayers[playerIndex].coins.Destroy();
            renderPlayers[playerIndex].numOfCoins.Destroy();
        }

        public void renderItem(PlayerIndex playerIndex, string item, Boolean Major)
        {
            int id = itemIDtoTiledID[item];

            if (Major)
            {
                renderPlayers[playerIndex].majorItem.LoadFromContent(Path.GetFileNameWithoutExtension(tiledS.Image.source));
                renderPlayers[playerIndex].majorItem.SetSourceRectangle(id, 32, 32);
            }
            else
            {
                renderPlayers[playerIndex].minorItem.LoadFromContent(Path.GetFileNameWithoutExtension(tiledS.Image.source));
                renderPlayers[playerIndex].minorItem.SetSourceRectangle(id, 32, 32);
            }
        }

        public void removeItem(PlayerIndex playerIndex, Boolean Major)
        {
            if (Major)
            {
                renderPlayers[playerIndex].majorItem.LoadFromContent("GUI/Item_slot");
                renderPlayers[playerIndex].minorItem.sourceRectangle = null;
            }
            else
            {
                renderPlayers[playerIndex].majorItem.LoadFromContent("GUI/Item_slot");
                renderPlayers[playerIndex].minorItem.sourceRectangle = null;
            }
        }

        public void renderCoins(PlayerIndex playerIndex, int coins)
        {
            if (coins < 10)
            {
                renderPlayers[playerIndex].numOfCoins.Text = "0" + coins.ToString() + "x";
            }
            else if (coins < 100)
            {
                renderPlayers[playerIndex].numOfCoins.Text = coins.ToString() + "x";
            }
            else
            {
                renderPlayers[playerIndex].numOfCoins.Text = "99x";
            }
        }

    }

    class RenderPlayer
    {
        public GuiSpriteRenderer character { get; set; }
        public GuiSpriteRenderer majorItem { get; set; }
        public GuiSpriteRenderer minorItem { get; set; }
        public GuiSpriteRenderer coins { get; set; }
        public GuiTextRenderer numOfCoins { get; set; }
    }
}
