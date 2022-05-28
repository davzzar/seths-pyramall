using System;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    // TODO need a paused game state?
    // FIXME very coupled to pause menu gui (see how I set IsPaused)
    /// <summary>
    /// Controller for pausing and unpausing the game and showing the pause menu
    /// </summary>
    class PauseMenuController : Behaviour
    {
        private readonly InputHandler inputHandler = new InputHandler(PlayerIndex.One);
        private bool IsStartPressed =>  inputHandler.getButtonState(Buttons.Start) == ButtonState.Pressed;
        public static bool IsPaused { get; private set; }

        protected override void Update()
        {
            inputHandler.UpdateState();

            if (!GamePad.GetCapabilities(PlayerIndex.One).IsConnected) return;

            if (!IsStartPressed) return;
            if (!IsPaused)
            {
                Template.ShowPauseMenu();
                PauseGame();
            }
            else
            {
                Template.RemovePauseMenu();
            }
        }

        public static void UnpauseGame()
        {
            IsPaused = false;
            Time.TimeScale = 1f;
        }

        public static void PauseGame()
        {
            IsPaused = true;
            Time.TimeScale = 0f;
        }
    }
}