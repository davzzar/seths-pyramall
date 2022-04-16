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

        public static GameStateManager Instance
        {
            get
            {
                return GameObject.FindComponent<GameStateManager>();
            }
        }

        public GameStateManager()
        {

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

        // transfer variable used in Prepare state
        // at prepare state, keep checking for new gamepad and update the dictionary
        // once a gamepad is connected, add its PlayerIndex as key in the dict, with value false
        // once a gamepad is disconnected, delete its key in the dict
        // once all values in the dictionary are true, transfer to InRound state
        

        protected override void OnEnable()
        {
            this.CurrentState = GameState.Prepare;
            Instance.CurrentState = GameState.Prepare;
        }



        protected override void Update()
        {
            switch (CurrentState)
            {
                case GameState.Prepare:
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
