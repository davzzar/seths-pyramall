using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;

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
                        currentState = GameState.RoundStartCountdown;
                        Debug.Print("GameState: Prepare-> RoundStartCountDown");
                        countDowncounter = 0;
                        exitTrigger = false;
                        //// unhide all players
                        //foreach (var player in PlayersManager.Instance.Players.Values)
                        //{
                        //    PlayerUtils.UnhidePlayer(player);
                        //    player.GetComponent<PlayerComponent>()!.IsAlive = true;
                        //}
                    }
                    break;
                case GameState.RoundStartCountdown:
                    {
                        foreach (var player in PlayersManager.Instance.Players.Values)
                        {
                            PlayerUtils.UnhidePlayer(player);
                            player.GetComponent<PlayerComponent>()!.IsAlive = true;                            
                        }
                        countDowncounter += Time.DeltaTime;
                        if(countDowncounter >= 3f)
                        {
                            currentState = GameState.InRound;
                        }
                        break;
                    }
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
                    if (countDowncounter >= 10f || PlayersManager.Instance.CheckAllDeadOrExit())
                    {
                        countDowncounter = 0f;
                        exitTrigger = false;
                        PlayersManager.Instance.FinalizeRanks();
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
                        if (PlayersManager.Instance.CheckAllDead())
                        {
                            Debug.WriteLine("No players were alive. No shop.");
                            RoundCheckToRoundStartCountDown();
                        }
                        else
                        {
                            RoundCheckToShop();
                        }
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
                        ShopToRoundStartCountdown();
                        // rik's code
                        foreach (var player in PlayersManager.Instance.Players.Values)
                        {
                            PlayerUtils.UnhidePlayer(player);
                            player.GetComponent<PlayerComponent>()!.IsAlive = true;
                        }
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
        private void RoundCheckToRoundStartCountDown()
        {
            currentState = GameState.RoundStartCountdown;
            Debug.Print("GameState: RoundCheck-> RoundStartCountDown");
            // TODO load correct scene
            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            // Load RoundScene current index = 1
            sceneManager.Reload();

        }

        private void ShopToRoundStartCountdown()
        {
            currentState = GameState.RoundStartCountdown;
            Debug.Print("GameState: Shop-> RoundStartCountdown");
            // TODO load correct scene
            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            // Load RoundScene current index = 1
            sceneManager.LoadAt(1);
        }
    }

    public enum GameState
    {
        Prepare,
        RoundStartCountdown,
        InRound,
        CountDown,
        RoundCheck,
        Shop,
    }
}
