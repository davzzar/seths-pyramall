using System;
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
        private bool exitTrigger;

        public GameStateManager()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Can't create more than one GameStateManager");
            }

            Reset();
            Rounds = 3;

            Debug.Print("GameStateManager is created");
        }

        public void Reset()
        {
            instance = this;
            CurrentState = GameState.Prepare;
            InMenu = true;
            CurrentRound = 1;
            
            PlayersManager.Instance.ResetAllPlayers();
        }

        public static GameStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameStateManager();
                }

                return instance;
            }
        }

        public GameState CurrentState { get; private set; }

        public bool InMenu { get; set; }

        public float CountDownCounter { get; private set; }

        public int Rounds { get; set; }

        public int CurrentRound { get; private set; }

        public bool TriggerExit()
        {
            Debug.Print("exit trigger script is run");
            if ((CurrentState != GameState.InRound && CurrentState != GameState.CountDown) || !exitTrigger)
            {
                return false;
            }

            exitTrigger = true;
            return true;
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
                        CurrentState = GameState.RoundStartCountdown;
                        Debug.Print("GameState: Prepare-> RoundStartCountDown");
                        CountDownCounter = 0;
                        exitTrigger = false;
                    }
                    break;
                case GameState.RoundStartCountdown:
                    {
                        CountDownCounter += Time.DeltaTime;
                        if(CountDownCounter >= 3f)
                        {
                            CurrentState = GameState.InRound;
                        }
                        break;
                    }
                case GameState.InRound:
                    if (PlayersManager.Instance.CheckOneExit())
                    {
                        CurrentState = GameState.CountDown;
                        Debug.Print("GameState: InRound -> CountDown");
                        CountDownCounter = 0f;
                    } else if (PlayersManager.Instance.CheckAllDead())
                    {
                        CurrentState = GameState.CountDown;
                        Debug.Print("GameState: InRound -> CountDown");
                        CountDownCounter = 10.0f;
                    }

                    break;
                case GameState.CountDown:
                    CountDownCounter += Time.DeltaTime;
                    Debug.Print(CountDownCounter.ToString());
                    if (CountDownCounter >= 10f || PlayersManager.Instance.CheckDeadOrAllExit())
                    {
                        CountDownCounter = 0f;
                        exitTrigger = false;
                        PlayersManager.Instance.finalizeRanks();
                        CurrentState = GameState.RoundCheck;
                        // Debug
                        Debug.Print("GameState: CountDown -> RoundCheck");
                        foreach (var item in PlayersManager.Instance.Players)
                        {
                            Debug.Print($"Player {item.Key} : Rank {item.Value.GetComponent<PlayerStates>()!.RoundRank}");
                        }
                    }

                    break;
                case GameState.RoundCheck:
                    CountDownCounter += Time.DeltaTime;
                    if (CountDownCounter >= 2f)
                    {
                        CountDownCounter = 0f;
                        CurrentRound++;
                        if (CurrentRound > Rounds)
                        {
                            RoundCheckToWinScreen();
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
                        CurrentState = GameState.Prepare;
                        Debug.Print("GameState: Shop -> Prepare");
                        // TODO load correct scene
                        var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
                        // Load RoundScene current index = 1
                        sceneManager.LoadAt(1);
                    }

                    break;
                case GameState.GameOver:
                    break;
            }
        }

        private void RoundCheckToShop()
        {
            CurrentState = GameState.Shop;
            Debug.Print("GameState: RoundCheck -> Shop");
            // load new scene
            // FIXME correct shop scene number
            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            // Load ShopScene current index = 3
            sceneManager.LoadAt(3);
        }

        private void RoundCheckToWinScreen()
        {
            CurrentState = GameState.GameOver;
            InMenu = true;
        }
        private void ShopToInRound()
        {
            CurrentState = GameState.RoundStartCountdown;
            Debug.Print("GameState: Shop-> InRound");
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
        GameOver
    }
}