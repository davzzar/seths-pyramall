using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    public static partial class Template
    {
        public static GameObject MakeGameStateManager(string name = null, Scene scene = null)
        {
            var (go, _, _) = MakeStateMachine<GameStateManager>(name, scene, typeof(StartMenuState), typeof(PreparingRoundState), typeof(InRoundState), typeof(CountDownState), typeof(InShopState), typeof(WinScreenState), typeof(PausedState));
            return go;
        }
    }

    public class GameStateManager : StateManager<GameStateManager>
    {
        private static GameStateManager instance;

        public bool exitTrigger;
        public bool TriggerExit()
        {
            Debug.Print("exit trigger script is run");
            if(CurrentState == GameState.InRound ||
                CurrentState == GameState.CountDown)
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
                    instance.CurrentState = GameState.Prepare;
                    GraphicalUserInterface.Instance.renderMidScreenText("To Start The Game Press A");
                }
                return instance;
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
                        CurrentState = GameState.InRound;
                        GraphicalUserInterface.Instance.destroyMidScreenText();
                        Debug.Print("GameState: Prepare-> InRound");
                    }
                    break;
                case GameState.InRound:
                    if (PlayersManager.Instance.CheckOneExit())
                    {
                        CurrentState = GameState.CountDown;
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
                        CurrentState = GameState.RoundCheck;
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

    public class StartMenuState : State<GameStateManager>
    {
        protected override void Update()
        {

        }
    }

    public class PreparingRoundState : State<GameStateManager>
    {

        protected override void Update()
        {
            PlayersManager.Instance.CheckConnections();
            if (PlayersManager.Instance.CheckAllPrepared())
            {
                ChangeState<InRoundState>();
                Debug.Print("GameState: Prepare-> InRound");
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            OnExit += GraphicalUserInterface.Instance.destroyMidScreenText;
        }
    }

    public class InRoundState : State<GameStateManager>
    {
        protected override void Update()
        {
            if (PlayersManager.Instance.CheckOneExit())
            {
                ChangeState<CountDownState>();
                exitTrigger = false;
                Debug.Print("GameState: InRound-> CountDown");
            }
        }
    }

    public class CountDownState : State<GameStateManager>
    {
        private float counter;

        protected override void Update()
        {
            counter += Time.DeltaTime;
            GraphicalUserInterface.Instance.updateMidScreenText(String.Format("{0:0.0}", 10f - counter) + " Seconds to Finish the Round");

            if (counter >= 10f || PlayersManager.Instance.CheckAllExit())
            {
                PlayersManager.Instance.finalizeRanks();
                ChangeState<InShopState>();                
                Debug.Print("GameState: CountDown-> RoundCheck");
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            OnEnter += () =>
            {
                counter = 0f;
                GraphicalUserInterface.Instance.renderMidScreenText("10.0 Seconds to Finish the Round");
            };
        }
    }

    public class InShopState : State<GameStateManager>
    {
        protected override void Update()
        {

        }
    }

    public class WinScreenState : State<GameStateManager>
    {
        protected override void Update()
        {

        }

        protected override void OnAwake()
        {
            base.OnAwake();

            OnEnter += () =>
            {
                foreach (var item in PlayersManager.Instance.Players)
                {
                    Debug.Print("Player " + item.Key + " : Rank " +
                        item.Value.GetComponent<PlayerStates>().RoundRank);
                }

                string ranks = "";
                //display ranks on screen
                foreach (var item in PlayersManager.Instance.Players)
                {
                    ranks += item.Value.GetComponent<PlayerStates>().RoundRank + " - Player " + item.Key + "\n";
                }
                GraphicalUserInterface.Instance.updateMidScreenText(ranks);
            };
        }
    }

    public class PausedState : State<GameStateManager>
    {
        protected override void Update()
        {

        }
    }
}
