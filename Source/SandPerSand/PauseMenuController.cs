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
        private static SoundEffectPlayer uiPauseSfx, uiUnpauseSfx;
        public static bool IsPaused { get; private set; }


        protected override void OnAwake()
        {
            // add sound effects
            const string soundPathPrefix = "Sounds/InterfaceSounds/";
            
            uiPauseSfx = Owner.AddComponent<SoundEffectPlayer>();
            uiPauseSfx.LoadFromContent(soundPathPrefix + "pause_01",
                soundPathPrefix + "pause_02",
                soundPathPrefix + "pause_03",
                soundPathPrefix + "pause_04",
                soundPathPrefix + "pause_05");

            uiUnpauseSfx = Owner.AddComponent<SoundEffectPlayer>();
            uiUnpauseSfx.LoadFromContent(soundPathPrefix + "unpause_01",
                soundPathPrefix + "unpause_02",
                soundPathPrefix + "unpause_03",
                soundPathPrefix + "unpause_04",
                soundPathPrefix + "unpause_05");
        }

        protected override void Update()
        {
            inputHandler.UpdateState();

            if (!GamePad.GetState(PlayerIndex.One).IsConnected) return;

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
            uiUnpauseSfx?.Play();
            Time.TimeScale = 1f;
        }

        public static void PauseGame()
        {
            IsPaused = true;
            uiPauseSfx?.Play();
            Time.TimeScale = 0f;
        }
    }
}