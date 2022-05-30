using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mime;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace SandPerSand
{
    public class MenuControlsManager : Behaviour
    {
        private static MenuControlsManager instance;

        public static MenuControlsManager Instance
        {
            get
            {
                if (instance != null) return instance;

                var go = new GameObject("Menu Controls Manager", SceneManager.ActiveScene);
                instance = go.AddComponent<MenuControlsManager>();

                return instance;
            }
        }

        // Input handling
        private InputHandler inputHandler = new InputHandler(PlayerIndex.One);
        private bool IsConnected => GamePad.GetState(PlayerIndex.One).IsConnected;
        private bool DownPressed => inputHandler.getButtonState(Buttons.DPadDown) == ButtonState.Pressed;
        private bool UpPressed => inputHandler.getButtonState(Buttons.DPadUp) == ButtonState.Pressed;
        private bool LeftPressed => inputHandler.getButtonState(Buttons.DPadLeft) == ButtonState.Pressed;
        private bool RightPressed => inputHandler.getButtonState(Buttons.DPadRight) == ButtonState.Pressed;
        private bool SelectPressed => inputHandler.getButtonState(Buttons.A) == ButtonState.Pressed;



        private List<Widget> controlsList = new List<Widget>();
        private int currentControlsIndex;

        private Widget CurrentControl => controlsList[currentControlsIndex];

        private readonly Dictionary<Type, (Action<Widget, bool> setFocus, Action<Widget> onSelect, Action<Widget> onLeft, Action<Widget> onRight)>
            controlFactory =
                new Dictionary<Type, (Action<Widget, bool>, Action<Widget>, Action<Widget>, Action<Widget>)>
                {
                    {typeof(TextButton), (
                        (w, b) => ((TextButton)w).IsPressed = b, 
                        w => ((TextButton)w).DoClick(),
                        null, 
                        null)
                    },
                    {typeof(HorizontalSlider), (
                        (w, b) => ((Slider) w).ImageButton.IsPressed = b, 
                        null, 
                        w => ((Slider) w).Value -= 5f, 
                        w => ((Slider) w).Value += 5f)
                    },
                    {typeof(CheckBox), (
                            (w, b) => ((CheckBox) w).IsMouseInside = b,
                            w =>
                            {
                                var box = (CheckBox) w;
                                box.IsChecked = !box.IsChecked;
                            },
                            null,
                            null
                        )

                    },
                    {typeof(ComboBox), (
                            (w, b) =>
                            {
                                var combox = (ComboBox) w;
                                combox.Border =
                                    b ? new SolidBrush(Color.Aqua) : new SolidBrush(Color.Transparent);
                            },
                            null,
                            w => 
                            {   var cbox = (ComboBox) w;
                                cbox.SelectedIndex = MathHelper.Clamp(cbox.SelectedIndex!.Value-1, 0, cbox.Items.Count-1);
                            },
                            w => 
                            {
                                var cbox = (ComboBox) w;
                                cbox.SelectedIndex = MathHelper.Clamp(cbox.SelectedIndex!.Value+1, 0, cbox.Items.Count-1);
                            }
                        )
                    }
                };

        public void SetControls(params Widget[] controls)
        {
            if (controls == null)
            {
                throw new ArgumentNullException(nameof(controls));
            }

            controlsList.Clear();
            controlsList.AddRange(controls);
            currentControlsIndex = 0;
            FocusControl();
        }

        public void ClearControls()
        {
            SetControls();
        }

        protected override void OnAwake()
        {
            if (instance != null && instance != this)
            {
                throw new InvalidOperationException("Attempted to create multiple singleton instances.");
            }

            instance = this;
        }

        protected override void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void Update()
        {
            if (controlsList.Count == 0)
            {
                return;
            }

            if (!IsConnected)
            {
                return;
            }

            inputHandler.UpdateState();

            // Handle Input and State
            if (DownPressed)
            {
                currentControlsIndex++;
                
                if (currentControlsIndex >= controlsList.Count)
                {
                    currentControlsIndex = 0;
                }

                FocusControl();
            } 
            else if (UpPressed)
            {
                currentControlsIndex--;
                
                if (currentControlsIndex < 0)
                {
                    currentControlsIndex = controlsList.Count - 1;
                }

                FocusControl();
            }

            // Handle control interaction
            var controlType = CurrentControl.GetType();
            if (!controlFactory.TryGetValue(controlType, out var factory))
            {
                return;
            }
            
            if (SelectPressed && factory.onSelect != null)
            {
                factory.onSelect(CurrentControl);
            }
            else if (LeftPressed && factory.onLeft != null)
            {
                factory.onLeft(CurrentControl);
            }
            else if (RightPressed && factory.onRight != null)
            {
                factory.onRight(CurrentControl);
            }
        }

        private void FocusControl()
        {
            if (controlsList.Count == 0)
            {
                return;
            }

            foreach (var ctrl in controlsList)
            {
                var controlType = ctrl.GetType();
                if (!controlFactory.TryGetValue(controlType, out var factory))
                {
                    continue;
                }

                factory.setFocus(ctrl, ctrl == this.CurrentControl);
            }
        }

    }
}