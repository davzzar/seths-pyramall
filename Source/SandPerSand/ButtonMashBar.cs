using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    public class ButtonMashBar : TimerBar
    {
        // A Button Mash Bar has a capacity of 1 and deals with percentages.

        public InputHandler InputHandler { get; set; }
        private const Buttons ActionButton = Buttons.X;
        private bool ActionButtonPressed => InputHandler.getButtonState(ActionButton) == ButtonState.Pressed;

        public float FillSpeed { get; set; } = 0.1f;


        protected override void Update()
        {
            if (ActionButtonPressed) FillLevel += FillSpeed;

            base.Update();

            InputHandler.UpdateState();
        }
    }
}