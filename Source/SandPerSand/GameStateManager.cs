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

        private static bool finishShopTrigger = false;
        public bool TriggerFinishShop()
        {
            if (currentState == GameState.Shop&&!finishShopTrigger)
            {
                finishShopTrigger = true;
                return true;
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



        public float countDowncounter { private set; get; }
        
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
                    break;
                case GameState.CountDown:
                    countDowncounter += Time.DeltaTime;
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
                        finishShopTrigger = false;
                        ShopToInRound();
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
            sceneManager.LoadAt(1);

        }

        private void ShopToInRound()
        {
            currentState = GameState.InRound;
            Debug.Print("GameState: Shop-> InRound");
            // TODO load correct scene
            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            sceneManager.LoadAt(0);

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
