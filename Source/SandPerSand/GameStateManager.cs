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

        private bool exitTrigger;
        public bool TriggerExit()
        {
            if(currentState== GameState.InRound||
                currentState== GameState.CountDown)
            {
                if (exitTrigger == false)
                {
                    exitTrigger = true;
                    return true;
                }
            }
            return false;
        }

        public GameStateManager()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Can't create more than one GameStateManager");
            }
            instance = this;
            currentState = GameState.Prepare;
        }

        private GameState currentState;

        public GameState CurrentState
        {
            get
            {
                return currentState;
            }
        }



        private float countDowncounter;
        protected override void Update()
        {
            switch (CurrentState)
            {
                case GameState.Prepare:
                    // at prepare state, PlayersManager keep checking for new gamepad
                    PlayersManager.Instance.CheckConnections();
                    if (PlayersManager.Instance.CheckAllPrepared())
                    {
                        currentState = GameState.InRound;
                        Debug.Print("GameState: Prepare-> InRound");
                    }
                    break;
                case GameState.InRound:
                    if (exitTrigger)
                    {
                        currentState = GameState.CountDown;
                        exitTrigger = false;
                        Debug.Print("GameState: InRound-> CountDown");
                        //
                        countDowncounter = 0f;
                    }
                    break;
                case GameState.CountDown:
                    countDowncounter += Time.DeltaTime;
                    if (countDowncounter >= 10f)
                    {
                        currentState = GameState.RoundCheck;
                        countDowncounter = 0f;
                        // Debug
                        Debug.Print("GameState: CountDown-> RoundCheck");
                        foreach(var item in PlayersManager.Instance.Players)
                        {
                            Debug.Print("Player "+ item.Key + " : Rank " +
                                item.Value.GetComponent<PlayerStates>().RoundRank);
                        }
                    }
                    break;
                case GameState.RoundCheck:
                    break;
            }
        }
    }

    public enum GameState
    {
        Prepare,
        InRound,
        CountDown,
        RoundCheck,
    }
}
