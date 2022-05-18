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
        private static GameState currentState;
        public static bool inMenu;
        public static float countDowncounter;

        public GameStateManager()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Can't create more than one GameStateManager");
            }
            instance = this;
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
                    if (PlayersManager.Instance.CheckAllPrepared())
                    {
                        currentState = GameState.RoundStartCountdown;
                        Debug.Print("GameState: Prepare-> RoundStartCountDown");
                        countDowncounter = 0;
                    }
                    break;

                case GameState.RoundStartCountdown:
                    {
                        countDowncounter += Time.DeltaTime;
                        if(countDowncounter >= 3f)
                        {
                            currentState = GameState.InRound;
                        }
                        break;
                    }
                case GameState.InRound:
                    if (PlayersManager.Instance.CheckOneExit() ||
                        PlayersManager.Instance.CheckAllDead())
                    {
                        currentState = GameState.CountDown;
                        Debug.Print("GameState: InRound-> CountDown");
                        countDowncounter = 0f;
                    }
                    break;
                case GameState.CountDown:
                    countDowncounter += Time.DeltaTime;
                    if (countDowncounter >= 10f || PlayersManager.Instance.CheckAllDeadOrExit())
                    {
                        countDowncounter = 0f;
                        currentState = GameState.RoundCheck;
                    }
                    break;
                case GameState.RoundCheck:
                    countDowncounter += Time.DeltaTime;
                    if (countDowncounter >= 2f)
                    {
                        countDowncounter = 0f;
                        if (PlayersManager.Instance.CheckAllDead())
                        {
                            //RoundCheckToRoundStartCountDown();
                            Debug.WriteLine("No players were alive. No shop.");
                            currentState = GameState.RoundStartCountdown;
                            Debug.Print("GameState: RoundCheck-> RoundStartCountDown");
                        }
                        else
                        {
                            //RoundCheckToShop();
                            currentState = GameState.Shop;
                            Debug.Print("GameState: RoundCheck-> Shop");
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
                        currentState = GameState.RoundStartCountdown;
                        Debug.Print("GameState: Shop-> RoundStartCountdown");
                        //ShopToRoundStartCountdown();
                    }
                    break;
            }
        }

        public void RoundCheckToShop()
        {
            // load new scene
            // FIXME correct shop scene number
            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            // Load ShopScene current index = 3
            sceneManager.LoadAt(3);
        }
        public void RoundCheckToRoundStartCountDown()
        {
            foreach (var player in PlayersManager.Instance.Players.Values)
            {
                PlayerUtils.UnhidePlayer(player);
                player.GetComponent<PlayerComponent>()!.IsAlive = true;
            }
            // TODO load correct scene
            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            // Load RoundScene current index = 1
            sceneManager.Reload();

        }

        public void ShopToRoundStartCountdown()
        {
            foreach (var player in PlayersManager.Instance.Players.Values)
            {
                PlayerUtils.UnhidePlayer(player);
                player.GetComponent<PlayerComponent>()!.IsAlive = true;
            }
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
