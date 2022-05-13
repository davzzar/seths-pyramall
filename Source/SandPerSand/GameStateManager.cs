using System;
using System.Diagnostics;
using Engine;

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

            instance = this;
            CurrentState = GameState.Prepare;
            InMenu = true;
            Rounds = 3;
            CurrentRound = 1;

            Debug.Print("GameStateManager is created");
        }

        public static GameStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameStateManager
                    {
                        CurrentState = GameState.Prepare,
                        InMenu = true,
                        Rounds = 3,
                        CurrentRound = 1
                    };
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
                        CurrentState = GameState.InRound;
                        Debug.Print("GameState: Prepare -> InRound");
                        exitTrigger = false;
                    }

                    break;
                case GameState.InRound:
                    if (PlayersManager.Instance.CheckOneExit())
                    {
                        CurrentState = GameState.CountDown;
                        Debug.Print("GameState: InRound -> CountDown");
                        CountDownCounter = 0f;
                    }

                    if (PlayersManager.Instance.CheckAllDead())
                    {
                        CurrentState = GameState.CountDown;
                        Debug.Print("GameState: InRound -> CountDown");
                        CountDownCounter = 10.0f;
                    }

                    break;
                case GameState.CountDown:
                    CountDownCounter += Time.DeltaTime;
                    Debug.Print(CountDownCounter.ToString());
                    if (CountDownCounter >= 10f || PlayersManager.Instance.CheckAllExit())
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
            CurrentRound = 1;
            CurrentState = GameState.GameOver;

            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            sceneManager.LoadAt(0, () =>
            {
                var mainMenu = GameObject.FindComponent<MainMenu>();
                mainMenu.ShowWinScreen();
            });
        }
    }

    public enum GameState
    {
        Prepare,
        InRound,
        CountDown,
        RoundCheck,
        Shop,
        GameOver
    }
}