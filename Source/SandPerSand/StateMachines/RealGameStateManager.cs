using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Dynamic;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{

    public static partial class Template
    {
        public static GameObject MakeGameStateManager(string name = null, Scene scene = null)
        {
            var (go, managerComponent, stateList) = MakeStateMachine<RealGameStateManager>(
                name, scene,
                typeof(PrepareState),
                typeof(PreRoundState),
                typeof(InRoundState),
                typeof(CountDownState),
                typeof(RoundCheckState),
                typeof(InShopState),
                typeof(InMenuState)
                );
            // TODO another place to set up initial CurrentState
            managerComponent.CurrentState = stateList[0];
            managerComponent.CurrentState.EnterFrom(null);
            return go;
        }
    }

    public class RealGameStateManager : StateManager<RealGameStateManager>
    {
        public IEnumerator<int> Rounds { get; set; }

        [Obsolete("CurrentGameState is deprecated; Please stop querying " +
                  "CurrentGameState all over the place, instead register your code to OnEnter," +
                  "OnExit events of corresponding State.")]
        public GameState CurrentGameState => CurrentState.GameState;

        protected override void Update()
        {
            base.Update();
        }

        public State GetState<State>() where State : State<RealGameStateManager>
        {
            return Owner.GetComponentInChildren<State>();
        }

        public void Reset()
        {
            if (this.CurrentState is PrepareState)
            {
                return;
            }

            var nextState = this.GetState<PrepareState>();
            this.CurrentState.ChangeState<PrepareState>();
        }
    }

    public class PrepareState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.Prepare;
        }
        protected override void Update()
        {
            base.Update();
            if (PlayersManager.Instance.CheckAllPrepared())
            {
                ChangeState<PreRoundState>();
            }
        }
    }

    public class PreRoundState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.RoundStartCountdown;
            OnEnter += (sender, lastState) => {
                CountDowncounter = 0f;
            };
        }
        protected override void Update()
        {
            base.Update();
            CountDowncounter += Time.DeltaTime;
            if (CountDowncounter >= 3f)
            {
                ChangeState<InRoundState>();
            }
        }
    }

    public class InRoundState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.InRound;
        }

        protected override void Update()
        {
            base.Update();
            if (PlayersManager.Instance.CheckOneExit()||
                PlayersManager.Instance.CheckAllDead())
            {
                ChangeState<CountDownState>();
            }
        }
    }

    public class CountDownState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.CountDown;
            OnEnter += (sender, lastState) => {
                CountDowncounter = 0f;
            };
        }

        protected override void Update()
        {
            base.Update();
            CountDowncounter += Time.DeltaTime;
            if (CountDowncounter >= 10f ||
                PlayersManager.Instance.CheckAllDeadOrExit())
            {
                ChangeState<RoundCheckState>();
            }
        }
    }

    public class RoundCheckState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.RoundCheck;
            OnEnter += (sender, lastState) => {
                CountDowncounter = 0f;
            };
        }

        protected override void Update()
        {
            base.Update();
            CountDowncounter += Time.DeltaTime;
            if (!(CountDowncounter >= 2f))
            {
                return;
            }

            if (!stateManager.Rounds.MoveNext())
            {
                ChangeState<InMenuState>();
                return;
            }

            if (PlayersManager.Instance.CheckAllDead())
            {
                ChangeState<PreRoundState>();
            }
            else
            {
                ChangeState<InShopState>();
            }
        }
    }

    public class InShopState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.Shop;
        }

        protected override void Update()
        {
            base.Update();
            if (PlayersManager.Instance.AllFinishedShop)
            {
                ChangeState<PreRoundState>();
            }
        }
    }

    public class InMenuState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.Menu;
        }
    }
}
