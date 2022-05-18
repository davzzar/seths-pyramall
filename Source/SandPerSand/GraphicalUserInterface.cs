using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TiledCS;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;

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
        private GameState oldGameState = GameState.CountDown;
        private bool oldInMenu;
        private FontSystem _fontSystem;

        private Panel rootPanel;
        private Label MidscreenTextPanel;
        private Grid InventoryGrid;
        private Grid ScoreBoard;
        private int FontSize;
        private float InvSize;

        private Dictionary<PlayerIndex, Grid> InventoryRoot;
        private Dictionary<PlayerIndex, Label> JoinLabelRoot;
        private Dictionary<PlayerIndex, Label> CoinNumber;
        private Dictionary<PlayerIndex, Panel> Characters, MajorItems, MinorItems;
        private Dictionary<PlayerIndex, int> PlayerIndexToInt;
        private Dictionary<PlayerIndex, Color> PlayerIndexToColor;

        protected override void OnAwake()
        {
            InventoryRoot = new Dictionary<PlayerIndex, Grid>();
            Characters = new Dictionary<PlayerIndex, Panel>();
            MajorItems = new Dictionary<PlayerIndex, Panel>();
            MinorItems = new Dictionary<PlayerIndex, Panel>();
            CoinNumber = new Dictionary<PlayerIndex, Label>();
            JoinLabelRoot = new Dictionary<PlayerIndex, Label>();

            PlayerIndexToInt = new Dictionary<PlayerIndex, int>();
            PlayerIndexToColor = new Dictionary<PlayerIndex, Color>();

            int i = 0;
            int[] order = { 0, 3, 1, 2 };
            Color[] CharColors = {new Color(160, 132, 254), new Color(165, 255, 21), new Color(255,17,108), new Color(33,187,255)};
            foreach(PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
            {
                PlayerIndexToInt[index] = order[i];
                PlayerIndexToColor[index] = CharColors[i];
                i++;
            }


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

            _fontSystem = new FontSystem();
            _fontSystem.AddFont(File.ReadAllBytes(@"Content/Fonts/Retro_Gaming.ttf"));
            FontSize = (int) 64 * GameEngine.Instance.Resolution.X / 1920;
            InvSize = 1f / 8f;

            MidscreenTextPanel = new Label()
            {
                Text = "Press A to start the Game",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Font = _fontSystem.GetFont(FontSize),
            };

            InventoryGrid = new Grid()
            {
                ColumnSpacing = 4,
            };

            rootPanel = new Panel();

            rootPanel.AddChild(MidscreenTextPanel);
            rootPanel.AddChild(InventoryGrid);

            foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
            {
                JoinLabelRoot[index] = new Label()
                {
                    GridColumn = PlayerIndexToInt[index],
                    GridRow = 0,
                    Text = "Connect \n to Join \n the Game",
                    Font = _fontSystem.GetFont((int)(4 * InvSize * FontSize)),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment= HorizontalAlignment.Center,
                };
                InventoryGrid.AddChild(JoinLabelRoot[index]);
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

            if (GameStateManager.Instance.InMenu)
            {
                return;
            }

            if (oldGameState != GameStateManager.Instance.CurrentState || oldInMenu != GameStateManager.Instance.InMenu)
            {
                GameState newGameState = GameStateManager.Instance.CurrentState;
                bool newInMenu = GameStateManager.Instance.InMenu;

                if (newGameState == GameState.Prepare)
                {
                    if (UI.Root != rootPanel)
                    {
                        UI.Root = rootPanel;
                        MidscreenTextPanel.Text = "Press A to Start the Game";
                    }
                }else if (newGameState == GameState.RoundStartCountdown)
                {
                    MidscreenTextPanel.Text = "3";
                    MidscreenTextPanel.TextColor = Color.White;
                    foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
                    {
                        InventoryGrid.RemoveChild(JoinLabelRoot[index]);
                    }
                }
                else if (newGameState == GameState.InRound)
                {
                    UI.Root = InventoryGrid;

                }else if (newGameState == GameState.CountDown)
                {
                    UI.Root = rootPanel;
                    MidscreenTextPanel.Text = "10 Seconds to Finish the Round";
                }else if (newGameState == GameState.RoundCheck)
                {
                    MidscreenTextPanel.Text = "";
                    ScoreBoard = new Grid()
                    {
                        ColumnSpacing = 9,
                        RowSpacing = PlayersManager.Instance.Players.Count,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Background = new SolidBrush(new Color(38, 12, 26)),
                    };
                    //display ranks on screen

                    List<(int score, PlayerIndex index)> scores = new List<(int score, PlayerIndex index)> ();
                    foreach (var item in PlayersManager.Instance.Players)
                    {
                        scores.Add((item.Value.GetComponent<PlayerStates>().Score, item.Key));
                    }
                    scores.Sort((x, y) => y.Item1.CompareTo(x.Item1));

                    foreach (var score in scores)
                    {
                        Debug.Print(score.ToString());
                    }

                    for (int i = 0; i < PlayersManager.Instance.Players.Count; i++)
                    {
                                Label pos = new Label()
                                {
                                    Text = (i + 1).ToString(),
                                    GridColumn = 0,
                                    GridRow = i,
                                    Font = _fontSystem.GetFont(FontSize),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Padding = new Thickness(FontSize / 5),
                                    TextColor = PlayerIndexToColor[scores[i].index]
                                };

                                Panel image = new Panel()
                                {
                                    GridColumn = 1,
                                    GridRow = i,
                                    Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("GUI/player" + scores[i].index.ToString())),
                                    Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/4*" + InvSize.ToString() + ";this.h = W.w/4*" + InvSize.ToString() + ";"),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                };

                                Label name = new Label()
                                {
                                    Text = "Player" + scores[i].index.ToString(),
                                    GridColumn = 2,
                                    GridColumnSpan = 5,
                                    GridRow = i,
                                    Font = _fontSystem.GetFont(FontSize),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Padding = new Thickness(FontSize / 5),
                                    TextColor = PlayerIndexToColor[scores[i].index],
                                };

                                Label score = new Label()
                                {
                                    Text = scores[i].score.ToString(),
                                    GridColumn = 8,
                                    GridRow = i,
                                    Font = _fontSystem.GetFont(FontSize),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Padding = new Thickness(FontSize / 5),
                                    TextColor = PlayerIndexToColor[scores[i].index],
                                };

                                ScoreBoard.AddChild(pos);
                                ScoreBoard.AddChild(image);
                                ScoreBoard.AddChild(name);
                                ScoreBoard.AddChild(score);
                    }
                    rootPanel.AddChild(ScoreBoard);
                }else if(oldGameState == GameState.RoundCheck && newGameState != GameState.RoundCheck)
                {
                    rootPanel.RemoveChild(ScoreBoard);
                }
                if (newGameState == GameState.Shop)
                {
                    UI.Root = rootPanel;
                    MidscreenTextPanel.Text = "Welcome to the Shop";
                }

                

                oldGameState = newGameState;
                oldInMenu = newInMenu;
            } else if (GameStateManager.Instance.CurrentState == GameState.CountDown)
            {
                MidscreenTextPanel.Text = String.Format("{0:0.0}", 10f - GameStateManager.Instance.CountDowncounter) + " Seconds to Finish the Round";
            }
            else if (GameStateManager.Instance.CurrentState == GameState.RoundStartCountdown)
            {
                MidscreenTextPanel.Text = String.Format("{0:0}", Math.Ceiling(3f - GameStateManager.Instance.CountDowncounter));
            }else if (GameStateManager.Instance.CurrentState == GameState.Shop)
            {
                List<(int score, PlayerIndex index)> scores = new List<(int rank, PlayerIndex index)>();
                foreach (var item in PlayersManager.Instance.Players)
                {
                    scores.Add((item.Value.GetComponent<PlayerStates>().RoundRank, item.Key));
                }
                scores.Sort((x, y) => y.Item1.CompareTo(x.Item1));
                scores.Reverse();
                try
                {
                    MidscreenTextPanel.Text = "Player " + scores[PlayersManager.Instance.CurRank - 1].index + " can buy \n" + String.Format("{0:0}", Math.Ceiling(PlayersManager.Instance.ShopTimer.CountDown)) + " seconds left";
                    MidscreenTextPanel.TextColor = PlayerIndexToColor[scores[PlayersManager.Instance.CurRank - 1].index];
                }
                catch
                {
                    MidscreenTextPanel.TextColor = Color.White;
                }
            }
        }

        public void renderPlayerInfo(PlayerIndex playerIndex)
        {
            JoinLabelRoot[playerIndex].RemoveFromParent();

            InventoryRoot[playerIndex] = new Grid()
            {
                GridColumn = PlayerIndexToInt[playerIndex],
                ColumnSpacing = 4,
                RowSpacing = 3,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w*" + InvSize.ToString() + ";this.h = W.w*3/5*" + InvSize.ToString() + ";"),
            };

            Debug.Print(InvSize.ToString());

            Characters[playerIndex] = new Panel()
            {
                GridColumn = 0,
                GridRow = 0,
                GridColumnSpan = 3,
                GridRowSpan = 3,
                Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("GUI/player" + playerIndex.ToString())),
            };

            MajorItems[playerIndex] = new Panel()
            {
                GridColumn = 4,
                GridRow = 1,
                Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("GUI/Item_slot")),

            };

            var leftBumper = new Panel()
            {
                GridColumn = 3,
                GridRow = 0,
                Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("GUI/left_bumper")),
                BorderThickness = new Thickness((int)(20*InvSize)),
            };

            MinorItems[playerIndex] = new Panel()
            {
                GridColumn = 3,
                GridRow = 1,
                Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("GUI/Item_slot")),
            };

            var rightBumper = new Panel()
            {
                GridColumn = 4,
                GridRow = 0,
                Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("GUI/right_bumper")),
                BorderThickness = new Thickness((int)(20 * InvSize)),
            };

            CoinNumber[playerIndex] = new Label()
            {
                GridColumn = 3,
                GridRow = 2,
                Text = "00x",
                Font = _fontSystem.GetFont((int)(3.5f * InvSize * FontSize)),
                VerticalAlignment = VerticalAlignment.Center,
            };

            Panel coin = new Panel()
            {
                GridColumn = 4,
                GridRow = 2,
                Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("Tiled/TiledsetTexture/TilesetCoins"), new Rectangle(0, 0, 32, 32)),
            };



            InventoryRoot[playerIndex].AddChild(Characters[playerIndex]);
            InventoryRoot[playerIndex].AddChild(CoinNumber[playerIndex]);
            InventoryRoot[playerIndex].AddChild(MinorItems[playerIndex]);
            InventoryRoot[playerIndex].AddChild(leftBumper);
            InventoryRoot[playerIndex].AddChild(MajorItems[playerIndex]);
            InventoryRoot[playerIndex].AddChild(rightBumper);
            InventoryRoot[playerIndex].AddChild(coin);
            InventoryRoot[playerIndex].Background = new SolidBrush(new Color(96, 23, 33));

            InventoryRoot[playerIndex].BorderThickness = new Thickness((int)(InvSize * 50));
            InventoryRoot[playerIndex].Border = new SolidBrush(new Color(151, 59, 41));

            InventoryGrid.AddChild(InventoryRoot[playerIndex]);


        }

        public void destroyPlayerInfo(PlayerIndex playerIndex)
        {
            InventoryGrid.RemoveChild(InventoryRoot[playerIndex]);
            InventoryGrid.AddChild(JoinLabelRoot[playerIndex]);
        }

        public void renderItem(PlayerIndex playerIndex, string item, Boolean Major)
        {
            int id = itemIDtoTiledID[item];

            int x = (id % 8) * 32;
            int y = (id / 8) * 32;
            if (Major)
            {
                MajorItems[playerIndex].Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("Tiled/TiledsetTexture/TilesetItems"), new Rectangle(x, y, 32, 32));
            }
            else
            {
                MinorItems[playerIndex].Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("Tiled/TiledsetTexture/TilesetItems"), new Rectangle(x, y, 32, 32));
            }
        }

        public void removeItem(PlayerIndex playerIndex, Boolean Major)
        {
            if (Major)
            {
                MajorItems[playerIndex].Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("GUI/Item_slot"));
            }
            else
            {
                MinorItems[playerIndex].Background = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("GUI/Item_slot"));
            }
        }

        public void renderCoins(PlayerIndex playerIndex, int coins)
        {
            if (coins < 10)
            {
                CoinNumber[playerIndex].Text = "0" + coins.ToString() + "x";
            }
            else if (coins < 100)
            {
                CoinNumber[playerIndex].Text = coins.ToString() + "x";
            }
            else
            {
                CoinNumber[playerIndex].Text = "99x";
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
