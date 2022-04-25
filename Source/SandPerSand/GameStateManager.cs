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

        public bool exitTrigger;
        public bool TriggerExit()
        {
            Debug.Print("exit trigger script is run");
            Debug.Print(currentState.ToString());
            Debug.Print(exitTrigger.ToString());
            if(currentState== GameState.InRound||
                currentState== GameState.CountDown)
            {
                Debug.Print("reached if statement");
                if (exitTrigger == false)
                {
                    exitTrigger = true;
                    Debug.Print(this.exitTrigger.ToString());
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
            Debug.Print("gamestatemanager is created");
        }

        public static GameStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameStateManager();
                    currentState = GameState.Prepare;
                    GraphicalUserInterface.Instance.renderMidScreenText("To Start The Game Press A");
                }
                return instance;
            }
        }

        private static GameState currentState;

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
                        GraphicalUserInterface.Instance.destroyMidScreenText();
                        Debug.Print("GameState: Prepare-> InRound");
                    }
                    break;
                case GameState.InRound:
                    if (PlayersManager.Instance.CheckOneExit())
                    {
                        currentState = GameState.CountDown;
                        exitTrigger = false;
                        Debug.Print("GameState: InRound-> CountDown");
                        //
                        countDowncounter = 0f;
                        GraphicalUserInterface.Instance.renderMidScreenText( "10.0 Seconds to Finish the Round");
                    }
                    break;
                case GameState.CountDown:
                    countDowncounter += Time.DeltaTime;
                    GraphicalUserInterface.Instance.updateMidScreenText(String.Format("{0:0.0}", 10f - countDowncounter) + " Seconds to Finish the Round");
                    if (countDowncounter >= 10f || PlayersManager.Instance.CheckAllExit())
                    {
                        PlayersManager.Instance.finalizeRanks();
                        currentState = GameState.RoundCheck;
                        // Debug
                        Debug.Print("GameState: CountDown-> RoundCheck");
                        foreach(var item in PlayersManager.Instance.Players)
                        {
                            Debug.Print("Player "+ item.Key + " : Rank " +
                                item.Value.GetComponent<PlayerStates>().RoundRank);
                        }

                        string ranks = "";
                        //display ranks on screen
                        foreach (var item in PlayersManager.Instance.Players)
                        {
                            ranks += item.Value.GetComponent<PlayerStates>().RoundRank + " - Player " + item.Key + "\n";
                        }
                        GraphicalUserInterface.Instance.updateMidScreenText(ranks);

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
