using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    public class GameStateManager : Behaviour
    {
        private static GameStateManager instance;
        internal static GameStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException(
                        "No GameStateManager component in the game. Please create one.");
                }
                return instance;
            }
        }

        public GameStateManager()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Can't create more than one GameStateManager");
            }
            instance = this;
            this.CurrentState = GameState.Prepare;
        }

        private static GameState currentState;

        public GameState CurrentState
        {
            get
            {
                return currentState;
            }
            set
            {
                currentState = value;
            }
        }

        public enum GameState
        {
            Prepare,
            InRound
        }

        protected override void Update()
        {
            switch (CurrentState)
            {
                case GameState.Prepare:
                    // at prepare state, PlayersManager keep checking for new gamepad
                    PlayersManager.Instance.CheckConnections();
                    if (PlayersManager.Instance.CheckAllPrepared())
                    {
                        CurrentState = GameState.InRound;
                        Debug.Print("GameState: Prepare-> InRound");
                    }
                    break;
            }
        }
    }
}
