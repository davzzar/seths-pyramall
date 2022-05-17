using System;
using System.Diagnostics;
using System.IO;
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
    public class MainMenu : Component
    {
        // STYLESHEET

        private FontSystem _fontSystem;

        private IBrush Background;

        (int x, int y, string text)[] Resolutions = new (int x, int y, string text)[] {
                (1280, 720, "1280x720 (16:9)"),
                (1920, 1080, "1920x1080 (16:9)"),
                (2560, 1440, "2560x1440 (16:9)"),
                (3840, 2160, "3840x2160 (16:9)"),
            };

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

        protected override void OnAwake()
        {

            _fontSystem = new FontSystem();
            _fontSystem.AddFont(File.ReadAllBytes(@"Content/Fonts/Retro_Gaming.ttf"));

            GameEngine.Instance.Resolution = new Engine.Int2(Resolutions[0].x, Resolutions[0].y);

            // MAIN MENU

            // The vertical stack panel widget places the children in a vertical line with some spacing

            var rootPanel = new Panel();

            var startGamePanel = new Panel();

            var settingsPanel = new Panel();

            var panel = new VerticalStackPanel()
            {
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/3;"),
            };


            var brush = new TextureRegion(GameEngine.Instance.Content.Load<Texture2D>("background"));

            Background = new SolidBrush(new Color(38,12,26));

            rootPanel.Background = Background;
            startGamePanel.Background = Background;
            settingsPanel.Background = Background;

            var GameTitle = createTitleLabel("Seth's Pyramall");

            // Create a start button with green text
            var startButton = createTextButton("Play");
            // When the start button is clicked, remove the GUI by setting UI.Root to null
            startButton.Click += (sender, e) =>
            {
                UI.Root = startGamePanel;
            };

            // Add a settings button
            var settingsButton = createTextButton("Settings");
            settingsButton.Click += (sender, e) =>
            {
                UI.Root = settingsPanel;
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
            panel.AddChild(exitButton);

            rootPanel.AddChild(panel);

            // LEVEL / MODE SELECTION

            var levelSelectorPanel = new VerticalStackPanel()
            {
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Layout2d = new Myra.Graphics2D.UI.Properties.Layout2D("this.w = W.w/3;"),
            };

            var titleLevelSelection = createTitleLabel("Play");

            // Create Mode 1 Button
            var Mode1Button = createTextButton("Shop (Debug)");

            // When the start button is clicked, remove the GUI by setting UI.Root to null
            Mode1Button.Click += (sender, e) =>
            {
                var loadManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                GameStateManager.Instance.InMenu = false;
                loadManager.LoadAt(3);
                UI.Root = null;
            };

            // Create Mode 2 Button
            var Mode2Button = createTextButton("Round Game");

            // When the start button is clicked, remove the GUI by setting UI.Root to null
            Mode2Button.Click += (sender, e) =>
            {
                var loadManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                GameStateManager.Instance.InMenu = false;
                loadManager.LoadAt(1);
                UI.Root = null;
            };

            // Create Exit to Menu
            var ExitToMenu = createTextButton("Back");

            ExitToMenu.Click += (sender, e) =>
            {
                UI.Root = rootPanel;
            };

            levelSelectorPanel.AddChild(titleLevelSelection);
            levelSelectorPanel.AddChild(Mode1Button);
            levelSelectorPanel.AddChild(Mode2Button);
            levelSelectorPanel.AddChild(ExitToMenu);
            startGamePanel.AddChild(levelSelectorPanel);


            // SETTINGS

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

            var ResolutionSelector = new ComboBox()
            {
                GridRow = 2,
                GridColumn = 1,
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
                Background = new TextureRegion(brush, new Rectangle(0, 0, Resolutions[index].x, Resolutions[index].y));
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
                UI.Root = rootPanel;
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
            settingsPanel.AddChild(SettingsPanel);



            UI.Root = rootPanel;
            UI.IsMouseVisible = true;
        }
    }
}
