using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Engine;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace SandPerSand
{
    public class MainMenu : Behaviour
    {
        // STYLESHEET

        private MenuState menuState = MenuState.Main;

        private FontSystem _fontSystem;

        private TextureRegion BackgroundTexture;

        private IBrush BackgroundBrush;

        private int currentItemIndex;

        private VerticalStackPanel rootStack;
        private Grid rootGrid;
        private int selectedItem = 0;
        InputHandler inputHandler;
        bool release = true;
        bool releaseX = true;
        bool aRelease = true;

        (int x, int y, string text)[] Resolutions = new (int x, int y, string text)[] {
                (1280, 720, "1280x720 (16:9)"),
                (1920, 1080, "1920x1080 (16:9)"),
                (2560, 1440, "2560x1440 (16:9)"),
                (3840, 2160, "3840x2160 (16:9)"),
            };
        int resSelect = 0;

        protected override void Update()
        {
            base.Update();

            if(UI.Root.GetType() == typeof(Panel))
            {
                if (menuState == MenuState.Main || menuState == MenuState.Play)
                {
                    var widgets = rootStack.Widgets;

                    // Check the device for Player One
                    GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);

                    // If there a controller attached, handle it
                    if (capabilities.IsConnected)
                    {
                        // Get the current state of Controller1
                        GamePadState state = GamePad.GetState(PlayerIndex.One);

                        if (capabilities.HasLeftYThumbStick)
                        {
                            if ((state.DPad.Down == Microsoft.Xna.Framework.Input.ButtonState.Pressed || state.ThumbSticks.Left.Y < -.5f) && release)
                            {
                                selectedItem++;
                                release = false;
                            }
                            else if ((state.DPad.Up == Microsoft.Xna.Framework.Input.ButtonState.Pressed || state.ThumbSticks.Left.Y > .5f) && release)
                            {
                                selectedItem--;
                                release = false;
                            }
                            else if (state.DPad.Up == Microsoft.Xna.Framework.Input.ButtonState.Released && state.DPad.Down == Microsoft.Xna.Framework.Input.ButtonState.Released && Math.Abs(state.ThumbSticks.Left.Y) < .5f)
                            {
                                release = true;
                            }
                        }

                        if (capabilities.HasAButton)
                        {
                            if (state.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed && aRelease)
                            {
                                ((TextButton)widgets[selectedItem + 1]).DoClick();
                                selectedItem = 0;
                                aRelease = false;
                            }
                            else if (state.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Released)
                            {
                                aRelease = true;
                            }
                        }
                    }

                    if (selectedItem > widgets.Count - 2)
                    {
                        selectedItem = 0;
                    }
                    else if (selectedItem < 0)
                    {
                        selectedItem = widgets.Count - 2;
                    }

                    for (int i = 1; i < widgets.Count; i++)
                    {
                        if (i - 1 == selectedItem)
                        {
                            ((TextButton)widgets[i]).IsPressed = true;
                        }
                        else
                        {
                            ((TextButton)widgets[i]).IsPressed = false;
                        }
                    }
                } else if (menuState == MenuState.Settings)
                {
                    var widgets = rootGrid.Widgets;

                    // Check the device for Player One
                    GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);

                    // If there a controller attached, handle it
                    if (capabilities.IsConnected)
                    {
                        // Get the current state of Controller1
                        GamePadState state = GamePad.GetState(PlayerIndex.One);

                        if (capabilities.HasLeftYThumbStick)
                        {
                            if ((state.DPad.Down == Microsoft.Xna.Framework.Input.ButtonState.Pressed || state.ThumbSticks.Left.Y < -.5f) && release)
                            {
                                selectedItem++;
                                release = false;
                            }
                            else if ((state.DPad.Up == Microsoft.Xna.Framework.Input.ButtonState.Pressed || state.ThumbSticks.Left.Y > .5f) && release)
                            {
                                selectedItem--;
                                release = false;
                            }
                            else if (state.DPad.Up == Microsoft.Xna.Framework.Input.ButtonState.Released && state.DPad.Down == Microsoft.Xna.Framework.Input.ButtonState.Released && Math.Abs(state.ThumbSticks.Left.Y) < .5f)
                            {
                                release = true;
                            }
                        }
                        if (capabilities.HasLeftXThumbStick)
                        {
                            if ((state.DPad.Left == Microsoft.Xna.Framework.Input.ButtonState.Pressed || state.ThumbSticks.Left.X < -.5f) && releaseX)
                            {
                                int index = selectedItem * 2 + 1;
                                if (index > widgets.Count){}
                                else if (widgets[selectedItem * 2 + 1].GetType() == typeof(HorizontalSlider))
                                {
                                    if (((HorizontalSlider)widgets[index]).Value <= 10)
                                    {
                                        ((HorizontalSlider)widgets[index]).Value = 0;
                                    }
                                    else
                                    {
                                        ((HorizontalSlider)widgets[index]).Value -= 10f;
                                    } 
                                }
                                else if (widgets[selectedItem * 2 + 1].GetType() == typeof(ComboBox))
                                {
                                    if (((ComboBox)widgets[index]).SelectedIndex > 0)
                                    {
                                        ((ComboBox)widgets[index]).SelectedIndex -= 1;
                                    }
                                }
                                releaseX = false;
                            }
                            else if ((state.DPad.Right == Microsoft.Xna.Framework.Input.ButtonState.Pressed || state.ThumbSticks.Left.X > .5f) && releaseX)
                            {
                                int index = selectedItem * 2 + 1;
                                if (index > widgets.Count){}
                                else if (widgets[selectedItem * 2 + 1].GetType() == typeof(HorizontalSlider))
                                {
                                    if (((HorizontalSlider)widgets[index]).Value >= 90)
                                    {
                                        ((HorizontalSlider)widgets[index]).Value = 100;
                                    }
                                    else
                                    {
                                        ((HorizontalSlider)widgets[index]).Value += 10f;
                                    }
                                }
                                else if (widgets[selectedItem * 2 + 1].GetType() == typeof(ComboBox))
                                {
                                    if (((ComboBox)widgets[index]).Items.Count - ((ComboBox)widgets[index]).SelectedIndex - 1 > 0)
                                    {
                                        ((ComboBox)widgets[index]).SelectedIndex += 1;
                                    }
                                }
                                releaseX = false;
                            }
                            else if (state.DPad.Left == Microsoft.Xna.Framework.Input.ButtonState.Released && state.DPad.Right == Microsoft.Xna.Framework.Input.ButtonState.Released && Math.Abs(state.ThumbSticks.Left.X) < .5f)
                            {
                                releaseX = true;
                            }
                        }

                        if (capabilities.HasAButton)
                        {
                            if (state.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed && aRelease)
                            {
                                if (widgets.Count / 2 == selectedItem)
                                {
                                    ((TextButton)rootStack.Widgets[2]).DoClick();
                                    selectedItem = 0;
                                }
                                else if (widgets[selectedItem * 2 + 1].GetType() == typeof(CheckBox))
                                {
                                    ((CheckBox)widgets[selectedItem * 2 + 1]).IsChecked = !((CheckBox)widgets[selectedItem * 2 + 1]).IsChecked;
                                }
                                aRelease = false;
                            }
                            else if (state.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Released)
                            {
                                aRelease = true;
                            }
                        }
                    }

                    if (selectedItem > widgets.Count / 2 + 1)
                    {
                        selectedItem = 0;
                    }
                    else if (selectedItem < 0)
                    {
                        selectedItem = (widgets.Count / 2);
                    }

                    for (int i = 0; i < widgets.Count / 2 + 1; i++)
                    {
                        int index = 2 * i + 1;
                        if (i == selectedItem)
                        {
                            if(i == 4)
                            {
                                ((TextButton)rootStack.Widgets[2]).IsPressed = true;
                            }
                            else if(widgets[index].GetType() == typeof(HorizontalSlider))
                            {
                                ((HorizontalSlider)widgets[index]).ImageButton.IsPressed = true;
                            }
                            else if (widgets[index].GetType() == typeof(CheckBox))
                            {
                                ((CheckBox)widgets[index]).Border = new SolidBrush(Color.CornflowerBlue);
                            }
                            else if (widgets[index].GetType() == typeof(ComboBox))
                            {
                                ((ComboBox)widgets[index]).Border = new SolidBrush(Color.CornflowerBlue);
                            }
                        }
                        else
                        {
                            if (i == 4)
                            {
                                ((TextButton)rootStack.Widgets[2]).IsPressed = false;
                            }
                            else if (widgets[index].GetType() == typeof(HorizontalSlider))
                            {
                                ((HorizontalSlider)widgets[index]).ImageButton.IsPressed = false;
                            }
                            else if (widgets[index].GetType() == typeof(CheckBox))
                            {
                                ((CheckBox)widgets[index]).Border = new SolidBrush(Color.Transparent);

                            }
                            else if (widgets[index].GetType() == typeof(ComboBox))
                            {
                                ((ComboBox)widgets[index]).Border = new SolidBrush(Color.Transparent);
                            }
                        }
                    }
                }
                else if (menuState == MenuState.ItemWiki)
                {

                }
            }
        }


        private TextButton createTextButton(string Text)
        {
            var button = new TextButton()
            {
                Text = Text,
                Padding = new Thickness(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ContentHorizontalAlignment = HorizontalAlignment.Center,
                Font = _fontSystem.GetFont(32),
                Background = new SolidBrush(new Color(155,34,38)),
                PressedTextColor = Color.White,
                Id = Text,
            };
            return button;
        }

        private Label createTitleLabel(string Text)
        {
            return new Label()
            {
                Text = Text,
                Font = _fontSystem.GetFont(64),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlign = TextAlign.Center,
            };
        }

        private Label createTextLabel(string Text)
        {
            return new Label()
            {
                Text = Text,
                Font = _fontSystem.GetFont(32),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }

        private void ShowMainMenu()
        {
            menuState = MenuState.Main;
            var rootPanel = new Panel
            {
                Background = this.BackgroundBrush
            };

            var panel = new VerticalStackPanel()
            {
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/3;"),
            };

            var GameTitle = createTitleLabel("Seth's Pyramall");

            // Create a start button with green text
            var startButton = createTextButton("Play");
            // When the start button is clicked, remove the GUI by setting UI.Root to null
            startButton.Click += (sender, e) =>
            {
                this.ShowStartGame();
            };

            // Add a settings button
            var settingsButton = createTextButton("Settings");
            settingsButton.Click += (sender, e) =>
            {
                this.ShowSettings();
            };

            // Add a settings button
            var itemWikiButton = createTextButton("Item Wiki");
            itemWikiButton.Click += (sender, e) =>
            {
                this.ShowItemWiki();
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
            rootStack = panel;

            rootPanel.AddChild(panel);

            UI.Root = rootPanel;
        }

        private void ShowStartGame()
        {
            menuState = MenuState.Play;
            var rootPanel = new Panel
            {
                Background = this.BackgroundBrush
            };

            var levelSelectorPanel = new VerticalStackPanel()
            {
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/3;"),
            };

            var titleLevelSelection = createTitleLabel("Play");

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
            var Mode2Button = createTextButton("Infinity Mode");

            // When the start button is clicked, remove the GUI by setting UI.Root to null
            Mode2Button.Click += (sender, e) =>
            {
                var loadManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                loadManager.LoadAt(1);
                UI.Root = GraphicalUserInterface.Instance.rootPanel;
            };

            // Create Exit to Menu
            var ExitToMenu = createTextButton("Back");

            ExitToMenu.Click += (sender, e) =>
            {
                this.ShowMainMenu();
            };
            rootStack = levelSelectorPanel;
            levelSelectorPanel.AddChild(titleLevelSelection);
            //levelSelectorPanel.AddChild(Mode1Button);
            levelSelectorPanel.AddChild(Mode2Button);
            levelSelectorPanel.AddChild(ExitToMenu);

            rootPanel.AddChild(levelSelectorPanel);

            UI.Root = rootPanel;
        }

        private void ShowSettings()
        {
            var rootPanel = new Panel
            {
                Background = this.BackgroundBrush
            };

            
            var titleSettings = createTitleLabel("Settings");

            var SettingsPanel = new VerticalStackPanel()
            {
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                GridColumn = 0,
                GridRow = 0,
                Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/2;"),
            };

            var VolumeSliderText = createTextLabel("Music");
            VolumeSliderText.GridRow = 0;
            VolumeSliderText.GridColumn = 0;
            VolumeSliderText.HorizontalAlignment = HorizontalAlignment.Left;

            var VolumeSlider = new HorizontalSlider()
            {
                Value = 50,
                Minimum = 0,
                Maximum = 100,
                GridRow = 0,
                GridColumn = 1,
            };

            VolumeSlider.ValueChanged += (sender, e) =>
            {
                // not implemented yet
            };

            var SoundSliderText = createTextLabel("Sounds");
            SoundSliderText.GridRow = 1;
            SoundSliderText.GridColumn = 0;
            SoundSliderText.HorizontalAlignment = HorizontalAlignment.Left;

            var SoundsSlider = new HorizontalSlider()
            {
                Value = 50,
                Minimum = 0,
                Maximum = 100,
                GridRow = 1,
                GridColumn = 1,
            };

            SoundsSlider.ValueChanged += (sender, e) =>
            {
                // not implemented yet
            };

            var ResolutionSelectorText = createTextLabel("Resolution");
            ResolutionSelectorText.GridRow = 2;
            ResolutionSelectorText.GridColumn = 0;
            ResolutionSelectorText.HorizontalAlignment = HorizontalAlignment.Left;

            //var ResolutionSelector = new Grid()
            //{
            //    GridRow = 2,
            //    GridColumn = 1,
            //    RowSpacing = 1,
            //    ColumnSpacing = 3,
            //};

            //var buttonLeft = createTextButton("<");
            //buttonLeft.GridColumn = 0;
            //buttonLeft.GridRow = 0;
            //var buttonRight = createTextButton(">");
            //buttonRight.GridColumn = 3;
            //buttonRight.GridRow = 0;
            //var buttonMid = createTextButton(Resolutions[resSelect].text);
            //buttonMid.GridColumn = 2;
            //buttonMid.GridRow = 0;


            //buttonLeft.Click += (sender, e) =>
            //{
            //    if (resSelect < 0)
            //    {
            //        resSelect--;
            //        buttonMid.Text = Resolutions[resSelect].text;
            //    }
            //};

            //buttonLeft.Click += (sender, e) =>
            //{
            //    if (Resolutions[resSelect + 1].x <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width || Resolutions[resSelect + 1].x <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
            //    {
            //        resSelect--;
            //        buttonMid.Text = Resolutions[resSelect].text;
            //    }
            //};
            //buttonMid.Click += (sender, e) =>
            //{
            //    var index = resSelect;
            //    this.BackgroundBrush = new TextureRegion(this.BackgroundTexture, new Rectangle(0, 0, Resolutions[index].x, Resolutions[index].y));
            //    var res = new Engine.Int2(Resolutions[index].x, Resolutions[index].y);
            //    GameEngine.Instance.Resolution = res;
            //};



            var ResolutionSelector = new ComboBox()
            {
                GridRow = 2,
                GridColumn = 1,
                BorderThickness = new Thickness(5),
        };

            foreach (var res in Resolutions)
            {
                if (res.x > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width || res.y > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    continue;
                }
                var item = new ListItem() { Text = res.text };
                ResolutionSelector.Items.Add(item);
            }

            ResolutionSelector.SelectedIndex = 0;
            ResolutionSelector.HorizontalAlignment = HorizontalAlignment.Right;

            ResolutionSelector.SelectedIndexChanged += (sender, e) =>
            {
                var index = ResolutionSelector.SelectedIndex.Value;
                this.BackgroundBrush = new TextureRegion(this.BackgroundTexture, new Rectangle(0, 0, Resolutions[index].x, Resolutions[index].y));
                var res = new Engine.Int2(Resolutions[index].x, Resolutions[index].y);
                GameEngine.Instance.Resolution = res;
            };

            var ToggleFullscreenText = createTextLabel("Fullscreen");
            ToggleFullscreenText.GridRow = 3;
            ToggleFullscreenText.GridColumn = 0;
            ToggleFullscreenText.HorizontalAlignment = HorizontalAlignment.Left;

            var ToggleFullscreen = new CheckBox()
            {
                IsChecked = false,
                TextPosition = ImageTextButton.TextPositionEnum.Left,
                Font = _fontSystem.GetFont(32),
                GridRow = 3,
                GridColumn = 1,
                HorizontalAlignment = HorizontalAlignment.Right,
                BorderThickness = new Thickness(5),
            };

            ToggleFullscreen.PressedChanged += (sender, e) =>
            {
                GameEngine.Instance.Fullscreen = ToggleFullscreen.IsChecked;
            };

            // Create Exit to Menu
            var ExitToMenuSettings = createTextButton("Back");
            ExitToMenuSettings.Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/3;");
            ExitToMenuSettings.HorizontalAlignment = HorizontalAlignment.Center;

            ExitToMenuSettings.Click += (sender, e) =>
            {
                this.ShowMainMenu();
            };

            var SettingsGrid = new Grid()
            {
                ColumnSpacing = 2,
                RowSpacing = 4,
            };

            SettingsGrid.AddChild(VolumeSliderText);
            SettingsGrid.AddChild(VolumeSlider);
            SettingsGrid.AddChild(SoundSliderText);
            SettingsGrid.AddChild(SoundsSlider);
            SettingsGrid.AddChild(ResolutionSelectorText);
            SettingsGrid.AddChild(ResolutionSelector);
            SettingsGrid.AddChild(ToggleFullscreenText);
            SettingsGrid.AddChild(ToggleFullscreen);

            SettingsPanel.AddChild(titleSettings);
            SettingsPanel.AddChild(SettingsGrid);
            SettingsPanel.AddChild(ExitToMenuSettings);

            rootPanel.AddChild(SettingsPanel);

            rootGrid = SettingsGrid;
            rootStack = SettingsPanel;
            menuState = MenuState.Settings;

            UI.Root = rootPanel;

        }

        private void ShowItemWiki()
        {
            menuState = MenuState.ItemWiki;
            var rootPanel = new Panel
            {
                Background = this.BackgroundBrush
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

            this.currentItemIndex = 0;
            this.ShowItem(this.currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);

            Action showNextItem = () =>
            {
                this.currentItemIndex++;
                this.ShowItem(this.currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);
            };

            Action showPrevItem = () =>
            {
                this.currentItemIndex--;
                this.ShowItem(this.currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);
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
                this.currentItemIndex--;

                if (this.currentItemIndex < 0)
                {
                    this.currentItemIndex = ItemWiki.ItemNametoItem.Count - 1;
                }

                this.ShowItem(this.currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);
            };

            rightButton.Click += (sender, e) =>
            {
                this.currentItemIndex = (this.currentItemIndex + 1) % ItemWiki.ItemNametoItem.Count;
                this.ShowItem(this.currentItemIndex, itemWiki, itemIcon, itemTitle, itemDescription);
            };

            // Create Exit to Menu
            var ExitToMenu = createTextButton("Back");
            ExitToMenu.GridColumn = 1;
            ExitToMenu.GridRow = 3;

            ExitToMenu.Click += (sender, e) =>
            {
                itemWikiGo.Destroy();
                
                this.ShowMainMenu();
            };

            itemNavGrid.AddChild(leftButton);
            itemNavGrid.AddChild(itemGrid);
            itemNavGrid.AddChild(rightButton);

            grid.AddChild(itemNavGrid);
            grid.AddChild(ExitToMenu);

            rootPanel.AddChild(grid);

            UI.Root = rootPanel;
        }

        private void ShowItem(int index, ItemWiki itemWiki, Image image, Label title, Label description)
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

        protected override void OnAwake()
        {

            inputHandler = new InputHandler(PlayerIndex.One);
            _fontSystem = new FontSystem();
            _fontSystem.AddFont(File.ReadAllBytes(@"Content/Fonts/Retro_Gaming.ttf"));

            GameEngine.Instance.Resolution = new Engine.Int2(Resolutions[0].x, Resolutions[0].y);

            this.BackgroundTexture = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("background"));
            this.BackgroundBrush = new SolidBrush(new Color(38,12,26));

            this.ShowMainMenu();
            
            UI.IsMouseVisible = true;
        }




    }
}
public enum MenuState
{
    Main,
    Play,
    Settings,
    ItemWiki,
}
