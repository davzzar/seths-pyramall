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
        private static bool exitTrigger = false;
        private static GameState currentState;
        public static bool inMenu;
        public static float countDowncounter;

        public bool TriggerExit()
        {
            Debug.Print("exit trigger script is run");
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
                    inMenu = true;
                }
                return instance;
            }
        }

        public GameState CurrentState
        {
            get
            {
                return currentState;
            }
        }
        
        public bool InMenu { get
            {
                return inMenu;
            }
            set { inMenu = value; }
        }

        public float CountDowncounter { 
            get
            {
                return countDowncounter; 
            } 
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
                        currentState = GameState.InRound;
                        Debug.Print("GameState: Prepare-> InRound");
                        exitTrigger = false;
                    }
                    break;
                case GameState.InRound:
                    if (PlayersManager.Instance.CheckOneExit())
                    {
                        currentState = GameState.CountDown;
                        Debug.Print("GameState: InRound-> CountDown");
                        countDowncounter = 0f;
                    }
                    if (PlayersManager.Instance.CheckAllDead())
                    {
                        currentState = GameState.CountDown;
                        Debug.Print("GameState: InRound-> CountDown");
                        countDowncounter = 10.0f;
                    }
                    break;
                case GameState.CountDown:
                    countDowncounter += Time.DeltaTime;
                    Debug.Print(countDowncounter.ToString());
                    if (countDowncounter >= 10f || PlayersManager.Instance.CheckAllExit())
                    {
                        countDowncounter = 0f;
                        exitTrigger = false;
                        PlayersManager.Instance.finalizeRanks();
                        currentState = GameState.RoundCheck;
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
                    countDowncounter += Time.DeltaTime;
                    if (countDowncounter >= 2f)
                    {
                        countDowncounter = 0f;
                        RoundCheckToShop();
                    }
                    break;
                case GameState.Shop:
                    // disable the current player's controller after they finished shopping or after the time limit
                    // move the finished player to the exit if they is not there
                    // enable the next first player's controller
                    // ...
                    // after all player is moved to exit, proceed to the next round
                    if (PlayersManager.Instance.CheckAllFinishedShop())
                    {
                        currentState = GameState.Prepare;
                        Debug.Print("GameState: Shop-> Prepare");
                        // TODO load correct scene
                        var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                        // Load RoundScene current index = 1
                        sceneManager.LoadAt(1);
                    }
                    break;
            }
        }

        private void RoundCheckToShop()
        {
            currentState = GameState.Shop;
            Debug.Print("GameState: RoundCheck-> Shop");
            // load new scene
            // FIXME correct shop scene number
            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            // Load ShopScene current index = 3
            sceneManager.LoadAt(3);

        }
    }

    public enum GameState
    {
        Prepare,
        InRound,
        CountDown,
        RoundCheck,
        Shop,
    }
}
