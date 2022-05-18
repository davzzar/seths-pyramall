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
            var (go, _, _) = MakeStateMachine<GameStateManager2>(
                name, scene,
                typeof(PrepareState),
                typeof(PreRoundState),
                typeof(InRoundState),
                typeof(CountDownState),
                typeof(RoundCheckState),
                typeof(InShopState)
                );
            return go;
        }
    }

    public class GameStateManager2 : StateManager<GameStateManager2>
    {
        [Obsolete("CurrentGameState is deprecated; Please stop querying " +
"CurrentGameState all over the place, instead register your code to OnEnter," +
"OnExit events of corresponding State.")]
        public GameState CurrentGameState
        {
            get
            {
                GameState currentGameState = CurrentState switch
                {
                    (PrepareState) => GameState.Prepare,
                    (PreRoundState) => GameState.RoundStartCountdown,
                    (InRoundState) => GameState.InRound,
                    (CountDownState) => GameState.CountDown,
                    (RoundCheckState) => GameState.RoundCheck,
                    (InShopState) => GameState.Shop,
                    _ => GameState.Prepare
                };
                return currentGameState;
            }
        }

        protected override void Update()
        {
            base.Update();
            // check correctness
            Debug.Assert(CurrentGameState == GameStateManager.Instance.CurrentState);
        }
    }

    public class PrepareState : State<GameStateManager2>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
        }
        protected override void Update()
        {
            if (PlayersManager.Instance.CheckAllPrepared())
            {
                ChangeState<PreRoundState>();
            }
        }
    }

    public class PreRoundState : State<GameStateManager2>
    {
        float countDowncounter;
        protected override void OnAwake()
        {
            base.OnAwake();
            countDowncounter = 0f;
        }
        protected override void Update()
        {
            countDowncounter += Time.DeltaTime;
            if (countDowncounter >= 3f)
            {
                ChangeState<InRoundState>();
            }
        }
    }

    public class InRoundState : State<GameStateManager2>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void Update()
        {
            if (PlayersManager.Instance.CheckOneExit())
            {
                ChangeState<CountDownState>();
                //countDowncounter = 10f;
            }
            if (PlayersManager.Instance.CheckAllDead())
            {
                ChangeState<CountDownState>();
                //countDowncounter = 0f;
            }
        }
    }

    public class CountDownState : State<GameStateManager2>
    {
        private float countDowncounter;

        protected override void OnAwake()
        {
            base.OnAwake();
            countDowncounter = 0f;
        }

        protected override void Update()
        {
            countDowncounter += Time.DeltaTime;
            if (countDowncounter >= 10f || PlayersManager.Instance.CheckAllDeadOrExit())
            {
                countDowncounter = 0f;
                PlayersManager.Instance.FinalizeRanks();
                ChangeState<RoundCheckState>();
            }
        }
    }

    public class RoundCheckState : State<GameStateManager2>
    {
        private float countDowncounter;
        protected override void OnAwake()
        {
            base.OnAwake();
        }
        protected override void Update()
        {
            countDowncounter += Time.DeltaTime;
            if (countDowncounter >= 2f)
            {
                countDowncounter = 0f;
                if (PlayersManager.Instance.CheckAllDead())
                {
                    Debug.WriteLine("No players were alive. No shop.");
                    ChangeState<PreRoundState>();
                }
                else
                {
                    ChangeState<InShopState>();
                }
            }
        }
    }

    public class InShopState : State<GameStateManager2>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void Update()
        {
            if (PlayersManager.Instance.CheckAllFinishedShop())
            {
                ChangeState<PreRoundState>();
            }

        }
    }




}
