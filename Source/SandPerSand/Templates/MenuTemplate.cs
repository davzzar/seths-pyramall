using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Net;
using Engine;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace SandPerSand
{
    public static partial class Template
    {
        // Sounds 
        // TODO find a better way than making these static
        private static SoundEffectPlayer uiSelectSfx;
        private static SoundEffectPlayer uiBackSfx;
        private static SoundEffectPlayer uiClickSfx;
        private static SoundEffectPlayer uiPauseSfx;
        private static SoundEffectPlayer uiUnpauseSfx;
        private static SoundEffectPlayer uiStartGameSfx;
        private static SoundEffectPlayer uiTickRightSfx;
        private static SoundEffectPlayer uiTickLeftSfx;
        private static SoundEffectPlayer uiToggleOnSfx;
        private static SoundEffectPlayer uiToggleOffSfx;

        // STYLESHEET

        private static FontSystem _fontSystem;

        private static TextureRegion BackgroundTexture;

        private static IBrush BackgroundBrush;

        private static int currentItemIndex;

        private static GameObject menuSfxGO;

        static readonly (int x, int y, string text)[] resolutions = new (int x, int y, string text)[] {
                (1280, 720, "1280x720 (16:9)"),
                (1920, 1080, "1920x1080 (16:9)"),
                (2560, 1440, "2560x1440 (16:9)"),
                (3840, 2160, "3840x2160 (16:9)"),
            };

        private static bool initializedMenu;

        private static TextButton createTextButton(string Text)
        {
            var button = new TextButton()
            {
                Text = Text,
                Padding = new Thickness(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ContentHorizontalAlignment = HorizontalAlignment.Center,
                Font = _fontSystem.GetFont(32),
                Background = new SolidBrush(new Color(155, 34, 38)),
                PressedTextColor = Color.White,
            };

            // add selection sound effect.
            // FIXME Will play on menu creation as well,
            // since FocusControl will set the top control's IsPressed = true
            button.PressedChanged += (sender, e) =>
            {
                if (button.IsPressed)
                {
                    uiClickSfx?.Play();
                }
            };


            return button;
        }

        private static Label createTitleLabel(string Text)
        {
            return new Label()
            {
                Text = Text,
                Font = _fontSystem.GetFont(64),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlign = TextAlign.Center,
            };
        }

        private static Label createTextLabel(string Text)
        {
            return new Label()
            {
                Text = Text,
                Font = _fontSystem.GetFont(32),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }

        private static VerticalStackPanel createVerticalStackPanel()
        {
            var panel = new VerticalStackPanel()
            {
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/3;"),
            };

            return panel;
        }

        private static HorizontalSlider createSlider()
        {
            var slider = new HorizontalSlider()
            {
                Value = 50,
                Minimum = 0,
                Maximum = 100
            };

            slider.ValueChanged += (sender, e) =>
            {
                if (e.NewValue > e.OldValue)
                {
                    uiTickRightSfx?.Play();
                }
                else
                {
                    uiTickLeftSfx?.Play();
                }
            };

            return slider;
        }

        public static void ShowMainMenu()
        {
            var rootPanel = new Panel
            {
                Background = BackgroundBrush
            };

            var panel = createVerticalStackPanel();

            var GameTitle = createTitleLabel("Seth's Pyramall");

            // Create a start button with green text
            var startButton = createTextButton("Play");
            // When the start button is clicked, remove the GUI by setting UI.Root to null
            startButton.Click += (sender, e) =>
            {
                ShowPlayModeMenu();
                uiSelectSfx?.Play();
            };

            // Add a settings button
            var settingsButton = createTextButton("Settings");
            settingsButton.Click += (sender, e) =>
            {
                ShowSettings();
                uiSelectSfx?.Play();
            };

            // Add a settings button
            var itemWikiButton = createTextButton("Item Wiki");
            itemWikiButton.Click += (sender, e) =>
            {
                ShowItemWiki();
                uiSelectSfx?.Play();
            };

            // Lastly an exit button that kills the app dead
            var exitButton = createTextButton("Exit");
            exitButton.Click += (sender, e) =>
            {
                GameEngine.Instance.Exit();
            };

            // Add the buttons in the correct order
            panel.AddChild(GameTitle);
            panel.AddChild(startButton);
            panel.AddChild(settingsButton);
            panel.AddChild(itemWikiButton);
            panel.AddChild(exitButton);

            // Add applicable controls to controls manager
            MenuControlsManager.Instance.SetControls(startButton, settingsButton, itemWikiButton, exitButton);

            rootPanel.AddChild(panel);

            UI.Root = rootPanel;
        }

        public static void RemovePauseMenu()
        {
            // unpause game
            var pauseController = GameObject.FindComponent<PauseMenuController>();
            PauseMenuController.UnpauseGame();
            // Reset the GUI
            UI.Root = GraphicalUserInterface.Instance.rootPanel;
            MenuControlsManager.Instance.ClearControls();
        }

        public static void ShowPauseMenu()
        {
            var pausedLabel = createTitleLabel("Game Paused");

            var resumeButton = createTextButton("Resume Game");
            resumeButton.Click += (sender, e) =>
            {
                RemovePauseMenu();
                uiUnpauseSfx?.Play();
            };

            var itemWikiButton = createTextButton("Item Wiki");
            itemWikiButton.Click += (sender, e) =>
            {
                ShowItemWiki();
                uiSelectSfx?.Play();
            };

            var settingsButton = createTextButton("Settings");
            settingsButton.Click += (sender, e) =>
            {
                ShowSettings();
                uiSelectSfx?.Play();
            };

            // BUG: Restart button causes player to disappear, fix this!
            /*var restartGameButton = createTextButton("Restart Game");
            restartGameButton.Click += (sender, e) =>
            {
                // FIXME Does not actually exit the game, just loads menu

                var playersManager = GameObject.FindComponent<PlayersManager>();
                playersManager.Reset();
                
                // exit game, unload scene, show menu
                var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                //show menu
                sceneManager.LoadAt(1);

                RemovePauseMenu();
                
                uiBackSfx?.Play();
            };*/

            var exitToMenuButton = createTextButton("Exit to Main Menu");
            exitToMenuButton.Id = "_ExitToMenu";
            exitToMenuButton.Click += (sender, e) =>
            {
                // FIXME Does not actually exit the game, just loads menu

                // exit game, unload scene, show menu
                var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                //show menu
                sceneManager.LoadAt(0);

                var playersManager = GameObject.FindComponent<PlayersManager>();
                playersManager.Reset();

                RemovePauseMenu();
                
                uiBackSfx?.Play();
            };

            var pauseMenuPanel = createVerticalStackPanel();

            pauseMenuPanel.AddChild(pausedLabel);
            pauseMenuPanel.AddChild(resumeButton);
            pauseMenuPanel.AddChild(itemWikiButton);
            pauseMenuPanel.AddChild(settingsButton);
            //pauseMenuPanel.AddChild(restartGameButton);
            pauseMenuPanel.AddChild(exitToMenuButton);

            //MenuControlsManager.Instance.SetControls(resumeButton, itemWikiButton, settingsButton, restartGameButton, exitToMenuButton);
            MenuControlsManager.Instance.SetControls(resumeButton, itemWikiButton, settingsButton, exitToMenuButton);

            var rootPanel = new Panel();
            rootPanel.Background = new SolidBrush("#000000AA");
            rootPanel.AddChild(pauseMenuPanel);

            UI.Root = rootPanel;
        }

        public static void ShowPlayModeMenu()
        {
            var rootPanel = new Panel
            {
                Background = BackgroundBrush
            };

            var levelSelectorPanel = createVerticalStackPanel();

            var titleModeSelection = createTitleLabel("Select Play Mode");

            // Create Mode 1 Button
            //var Mode1Button = createTextButton("Shop (Debug)");

            // When the start button is clicked, remove the GUI by setting UI.Root to null
            /*Mode1Button.Click += (sender, e) =>
            {
                var loadManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                GameStateManager.Instance.InMenu = false;
                loadManager.LoadAt(3);
                UI.Root = GraphicalUserInterface.Instance.rootPanel;
            };*/

            // Create Mode 2 Button
            var infinityModeButton = createTextButton("Infinity Mode");

            // When the start button is clicked, remove the GUI by setting UI.Root to null
            infinityModeButton.Click += (sender, e) =>
            {
                var gsm = GameObject.FindComponent<RealGameStateManager>();
                gsm.GetState<InMenuState>().ChangeState<PrepareState>();
                gsm.Rounds = RoundsGenerator.InfiniteRounds();
                gsm.Rounds.MoveNext();
                
                var loadManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                loadManager.LoadLevelScene();
                
                UI.Root = GraphicalUserInterface.Instance.rootPanel;
                // disable menu controls
                // TODO May be move this to a game state OnEnter?
                MenuControlsManager.Instance.ClearControls();

                // start game sfx
                uiStartGameSfx?.Play();
            };

            // Create Mode 3 Button
            var roundModeButton = createTextButton("Rounds Game");

            AddRoundsGameClickListener(roundModeButton);

            // Create Exit to Menu
            var exitToMenuButton = createTextButton("Back");

            exitToMenuButton.Click += (sender, e) =>
            {
                ShowPreviousMenu();
                uiBackSfx?.Play();
            };

            levelSelectorPanel.AddChild(titleModeSelection);
            //levelSelectorPanel.AddChild(Mode1Button);
            levelSelectorPanel.AddChild(infinityModeButton);
            levelSelectorPanel.AddChild(roundModeButton);
            levelSelectorPanel.AddChild(exitToMenuButton);

            MenuControlsManager.Instance.SetControls(infinityModeButton, roundModeButton, exitToMenuButton);

            rootPanel.AddChild(levelSelectorPanel);

            UI.Root = rootPanel;
        }

        public static void ShowWinScreen(List<(int score, PlayerIndex index)> scores)
        {
            var rootPanel = new Panel
            {
                Background = BackgroundBrush
            };

            var winScreenPanel = createVerticalStackPanel();

            var winner = $"Player {scores[0].index}";

            var titleModeSelection = createTitleLabel($"{winner} wins!");

            var replayButton = createTextButton("Replay");

            AddRoundsGameClickListener(replayButton);

            var exitToMenuButton = createTextButton("Back To Menu");

            exitToMenuButton.Click += (sender, e) =>
            {
                ShowPlayModeMenu();
                uiBackSfx?.Play();
            };

            winScreenPanel.AddChild(titleModeSelection);
            winScreenPanel.AddChild(replayButton);
            winScreenPanel.AddChild(exitToMenuButton);

            MenuControlsManager.Instance.SetControls(replayButton, exitToMenuButton);

            rootPanel.AddChild(winScreenPanel);

            UI.Root = rootPanel;
        }

        private static void AddRoundsGameClickListener(TextButton button)
        {
            button.Click += (sender, e) =>
            {
                var gsm = GameObject.FindComponent<RealGameStateManager>();
                gsm.GetState<InMenuState>().ChangeState<PrepareState>();
                gsm.Rounds = RoundsGenerator.Rounds(3);
                gsm.Rounds.MoveNext();

                var loadManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                loadManager.LoadLevelScene();

                UI.Root = GraphicalUserInterface.Instance.rootPanel;
                // disable menu controls
                // TODO May be move this to a game state OnEnter?
                MenuControlsManager.Instance.ClearControls();

                // start game sfx
                uiStartGameSfx?.Play();
            };
        }

        public static void ShowSettings()
        {
            var rootPanel = new Panel
            {
                Background = BackgroundBrush
            };


            var titleSettings = createTitleLabel("Settings");

            var settingsPanel = createVerticalStackPanel();
            settingsPanel.GridColumn = 0;
            settingsPanel.GridRow = 0;
            settingsPanel.Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/2;");
            

            var volumeSliderText = createTextLabel("Music");
            volumeSliderText.GridRow = 0;
            volumeSliderText.GridColumn = 0;
            volumeSliderText.HorizontalAlignment = HorizontalAlignment.Left;

            var volumeSlider = createSlider();
            volumeSlider.GridRow = 0;
            volumeSlider.GridColumn = 1;

            var soundSliderText = createTextLabel("Sounds");
            soundSliderText.GridRow = 1;
            soundSliderText.GridColumn = 0;
            soundSliderText.HorizontalAlignment = HorizontalAlignment.Left;

            var soundsSlider = createSlider();
            soundsSlider.GridRow = 1;
            soundsSlider.GridColumn = 1;

            var resolutionSelectorText = createTextLabel("Resolution");
            resolutionSelectorText.GridRow = 2;
            resolutionSelectorText.GridColumn = 0;
            resolutionSelectorText.HorizontalAlignment = HorizontalAlignment.Left;

            var resolutionSelector = new ComboBox()
            {
                GridRow = 2,
                GridColumn = 1,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(3),
                SelectedIndex = 0
            };

            foreach (var res in resolutions)
            {
                if (res.x > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width || res.y > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    continue;
                }
                var item = new ListItem() { Text = res.text };
                resolutionSelector.Items.Add(item);
            }

            resolutionSelector.SelectedIndex = 0;
            resolutionSelector.HorizontalAlignment = HorizontalAlignment.Right;

            resolutionSelector.SelectedIndexChanged += (sender, e) =>
            {
                uiTickLeftSfx?.Play();

                var index = resolutionSelector.SelectedIndex.Value;
                BackgroundBrush = new TextureRegion(BackgroundTexture, new Rectangle(0, 0, resolutions[index].x, resolutions[index].y));
                var res = new Engine.Int2(resolutions[index].x, resolutions[index].y);
                GameEngine.Instance.Resolution = res;
            };

            // TODO add focus sound effect. No idea how to find the trigger...

            var toggleFullscreenText = createTextLabel("Fullscreen");
            toggleFullscreenText.GridRow = 3;
            toggleFullscreenText.GridColumn = 0;
            toggleFullscreenText.HorizontalAlignment = HorizontalAlignment.Left;

            var ToggleFullscreen = new CheckBox()
            {
                IsChecked = false,
                TextPosition = ImageTextButton.TextPositionEnum.Left,
                Font = _fontSystem.GetFont(32),
                GridRow = 3,
                GridColumn = 1,
                HorizontalAlignment = HorizontalAlignment.Right,
                BorderThickness = new Thickness(1),
                OverBorder = new SolidBrush(Color.Cyan),
                Padding = new Thickness(10, 0)
            };

            ToggleFullscreen.PressedChanged += (sender, e) =>
            {
                // sound effects
                if (ToggleFullscreen.IsChecked)
                {
                    uiToggleOnSfx?.Play();
                }
                else
                {
                    uiToggleOffSfx?.Play();
                }

                GameEngine.Instance.Fullscreen = ToggleFullscreen.IsChecked;
            };

            ToggleFullscreen.MouseEntered += (sender, e) =>
            {
                uiClickSfx?.Play();
            };


            // Create Exit to Menu
            var exitToMenuButton = createTextButton("Back");
            exitToMenuButton.Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/3;");
            exitToMenuButton.HorizontalAlignment = HorizontalAlignment.Center;

            exitToMenuButton.Click += (sender, e) =>
            {
                ShowPreviousMenu();
                uiBackSfx?.Play();
            };

            var grid = new Grid()
            {
                ColumnSpacing = 2,
                RowSpacing = 4,
            };

            grid.AddChild(volumeSliderText);
            grid.AddChild(volumeSlider);
            grid.AddChild(soundSliderText);
            grid.AddChild(soundsSlider);
            grid.AddChild(resolutionSelectorText);
            grid.AddChild(resolutionSelector);
            grid.AddChild(toggleFullscreenText);
            grid.AddChild(ToggleFullscreen);

            settingsPanel.AddChild(titleSettings);
            settingsPanel.AddChild(grid);
            settingsPanel.AddChild(exitToMenuButton);

            MenuControlsManager.Instance.SetControls(volumeSlider, soundsSlider, resolutionSelector, ToggleFullscreen, exitToMenuButton);

            rootPanel.AddChild(settingsPanel);

            UI.Root = rootPanel;
        }

        public static void ShowItemWiki()
        {
            var rootPanel = new Panel
            {
                Background = BackgroundBrush
            };

            var grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                //Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/3;"),
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1f));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1.5f));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1f));

            grid.RowsProportions.Add(new Proportion(ProportionType.Part, 1f));
            grid.RowsProportions.Add(new Proportion(ProportionType.Part, 1.2f));
            grid.RowsProportions.Add(new Proportion(ProportionType.Part, 0.2f));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Part, 1f));

            var itemWikiGo = new GameObject("Item Wiki");
            var itemWiki = itemWikiGo.AddComponent<ItemWiki>();
            itemWiki.LoadFromContent("TilesetItems");

            var itemNavGrid = new Grid()
            {
                Background = new SolidBrush(new Color(Color.White * 0.3f, 0.3f)),
                GridColumn = 1,
                GridRow = 1
            };

            itemNavGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 0.1f));
            itemNavGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            itemNavGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 0.1f));

            var itemGrid = new Grid()
            {
                GridColumn = 1,
                GridRow = 0
            };

            itemGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 0.2f));
            itemGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));

            itemGrid.RowsProportions.Add(Proportion.Auto);
            itemGrid.RowsProportions.Add(Proportion.Fill);

            var itemIcon = new Image()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                GridColumn = 0,
                GridRow = 0,
                ResizeMode = ImageResizeMode.KeepAspectRatio,
                Scale = new Vector2(1.5f, 1.5f)
            };

            var itemTitle = createTextLabel("ITEM TITLE");
            itemTitle.Padding = new Thickness(8, 8);
            itemTitle.HorizontalAlignment = HorizontalAlignment.Left;
            itemTitle.VerticalAlignment = VerticalAlignment.Center;
            itemTitle.GridColumn = 1;
            itemTitle.GridRow = 0;
            itemTitle.Scale = new Vector2(1.3f, 1.3f);
            itemTitle.TextColor = Color.Yellow;

            var itemDescription = createTextLabel("ITEM DESCRIPTION");
            itemDescription.Padding = new Thickness(8, 8);
            itemDescription.HorizontalAlignment = HorizontalAlignment.Left;
            itemDescription.VerticalAlignment = VerticalAlignment.Top;
            itemDescription.GridColumnSpan = 2;
            itemDescription.GridRow = 1;
            itemDescription.Wrap = true;

            itemGrid.AddChild(itemIcon);
            itemGrid.AddChild(itemTitle);
            itemGrid.AddChild(itemDescription);

            currentItemIndex = 0;
            ShowItem(currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);

            Action showNextItem = () =>
            {
                currentItemIndex++;
                ShowItem(currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);
            };

            Action showPrevItem = () =>
            {
                currentItemIndex--;
                ShowItem(currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);
            };

            var leftButton = new TextButton()
            {
                Text = "<",
                ContentVerticalAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                GridColumn = 0,
                GridRow = 0,
            };

            var rightButton = new TextButton()
            {
                Text = ">",
                ContentVerticalAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                GridColumn = 2,
                GridRow = 0
            };

            leftButton.Click += (sender, e) =>
            {
                currentItemIndex--;

                if (currentItemIndex < 0)
                {
                    currentItemIndex = ItemWiki.ItemNametoItem.Count - 1;
                }

                ShowItem(currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);
            };

            rightButton.Click += (sender, e) =>
            {
                currentItemIndex = (currentItemIndex + 1) % ItemWiki.ItemNametoItem.Count;
                ShowItem(currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);
            };

            // Create Exit to Menu
            var ExitToMenu = createTextButton("Back");
            ExitToMenu.GridColumn = 1;
            ExitToMenu.GridRow = 3;

            ExitToMenu.Click += (sender, e) =>
            {
                itemWikiGo.Destroy();
                ShowPreviousMenu();
                uiBackSfx?.Play();
            };

            itemNavGrid.AddChild(leftButton);
            itemNavGrid.AddChild(itemGrid);
            itemNavGrid.AddChild(rightButton);

            grid.AddChild(itemNavGrid);
            grid.AddChild(ExitToMenu);

            rootPanel.AddChild(grid);

            MenuControlsManager.Instance.SetControls(leftButton, rightButton, ExitToMenu);

            UI.Root = rootPanel;
        }

        private static void ShowPreviousMenu()
        {
            var loadManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            var sceneIdx = loadManager.LoadedSceneIndex;
            if (sceneIdx == 0) // in menu
            {
                ShowMainMenu();
            }
            else
            {
                ShowPauseMenu();
            }
        }

        private static void ShowItem(int index, ItemWiki itemWiki, Image image, Label title, Label description)
        {
            var items = ItemWiki.ItemNametoItem.Values.ToArray();
            var item = items[index];
            title.Text = item.DisplayName;
            description.Text = item.Description;

            var texture = GameEngine.Instance.Content.Load<Texture2D>("Tiled/TiledsetTexture/TilesetItems");

            var sourceRect = new Rectangle(
                item.TileID * 32 % texture.Width,
                item.TileID * 32 / texture.Width * 32,
                32,
                32);

            var brush = new TextureRegion(texture, sourceRect);
            image.Renderable = brush;
        }

        public static void InitializeMenu()
        {
            if (!initializedMenu)
            {
                _fontSystem = new FontSystem();
                _fontSystem.AddFont(File.ReadAllBytes(@"Content/Fonts/Retro_Gaming.ttf"));

                GameEngine.Instance.Resolution = new Engine.Int2(resolutions[0].x, resolutions[0].y);

                BackgroundTexture = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("background"));
                BackgroundBrush = new SolidBrush(new Color(38, 12, 26));

                // add menu sound effects.
                menuSfxGO = new GameObject("Menu sound effects");

                const string soundPathPrefix = "Sounds/InterfaceSounds/";
                
                uiSelectSfx = menuSfxGO.AddComponent<SoundEffectPlayer>();
                uiSelectSfx.LoadFromContent(soundPathPrefix + "select_01", 
                    soundPathPrefix + "select_02", 
                    soundPathPrefix + "select_03");

                uiBackSfx = menuSfxGO.AddComponent<SoundEffectPlayer>();
                uiBackSfx.LoadFromContent(soundPathPrefix + "back_01",
                    soundPathPrefix + "back_02",
                    soundPathPrefix + "back_03",
                    soundPathPrefix + "back_04");

                uiClickSfx = menuSfxGO.AddComponent<SoundEffectPlayer>();
                uiClickSfx.LoadFromContent(soundPathPrefix + "click_01",
                    soundPathPrefix + "click_02",
                    soundPathPrefix + "click_03",
                    soundPathPrefix + "click_04",
                    soundPathPrefix + "click_05");

                uiStartGameSfx = menuSfxGO.AddComponent<SoundEffectPlayer>();
                uiStartGameSfx.LoadFromContent(soundPathPrefix + "startgame_01",
                    soundPathPrefix + "startgame_02",
                    soundPathPrefix + "startgame_03",
                    soundPathPrefix + "startgame_04");

                uiTickRightSfx = menuSfxGO.AddComponent<SoundEffectPlayer>();
                uiTickRightSfx.LoadFromContent(soundPathPrefix + "tick_right");

                uiTickLeftSfx = menuSfxGO.AddComponent<SoundEffectPlayer>();
                uiTickLeftSfx.LoadFromContent(soundPathPrefix + "tick_left");

                uiToggleOnSfx = menuSfxGO.AddComponent<SoundEffectPlayer>();
                uiToggleOnSfx.LoadFromContent(soundPathPrefix + "toggle_on");

                uiToggleOffSfx = menuSfxGO.AddComponent<SoundEffectPlayer>();
                uiToggleOffSfx.LoadFromContent(soundPathPrefix + "toggle_off");

                initializedMenu = true;
            }
        }
    }
}
