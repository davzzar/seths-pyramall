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
            var (go, managerComponent, stateList) = MakeStateMachine<RealGameStateManager>(
                name, scene,
                typeof(PrepareState),
                typeof(PreRoundState),
                typeof(InRoundState),
                typeof(CountDownState),
                typeof(RoundCheckState),
                typeof(InShopState)
                );
            // TODO another place to define initial CurrentState
            managerComponent.CurrentState = stateList[0];
            return go;
        }
    }

    public class RealGameStateManager : StateManager<RealGameStateManager>
    {
        [Obsolete("CurrentGameState is deprecated; Please stop querying " +
"CurrentGameState all over the place, instead register your code to OnEnter," +
"OnExit events of corresponding State.")]
        public GameState CurrentGameState => CurrentState.GameState;
        protected override void Update()
        {
            base.Update();
        }
    }

    public class PrepareState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.Prepare;
        }
        public override void OnUpdate()
        {
            PlayersManager.Instance.CheckConnections();
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
            CountDowncounter = 0f;
            GameState = GameState.RoundStartCountdown;
        }
        public override void OnUpdate()
        {
            CountDowncounter += Time.DeltaTime;
            if (CountDowncounter >= 3f)
            {
                CountDowncounter = 0f;
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

        public override void OnUpdate()
        {
            if (PlayersManager.Instance.CheckOneExit())
            {
                ChangeState<CountDownState>();
            }
            if (PlayersManager.Instance.CheckAllDead())
            {
                ChangeState<CountDownState>();
                // TODO set different Timer
            }
        }
    }

    public class CountDownState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.CountDown;
            CountDowncounter = 0f;
        }

        public override void OnUpdate()
        {
            CountDowncounter += Time.DeltaTime;
            if (CountDowncounter >= 10f || PlayersManager.Instance.CheckAllDeadOrExit())
            {
                CountDowncounter = 0f;

                PlayersManager.Instance.FinalizeRanks();

                ChangeState<RoundCheckState>();

                // Debug
                Debug.Print("GameState: CountDown-> RoundCheck");
                foreach (var item in PlayersManager.Instance.Players)
                {
                    Debug.Print("Player " + item.Key + " : Rank " +
                        item.Value.GetComponent<PlayerStates>().RoundRank);
                }
            }
        }
    }

    public class RoundCheckState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.RoundCheck;
            CountDowncounter = 0f;
        }
        public override void OnUpdate()
        {
            CountDowncounter += Time.DeltaTime;
            if (CountDowncounter >= 2f)
            {
                CountDowncounter = 0f;
                if (PlayersManager.Instance.CheckAllDead())
                {
                    ChangeState<PreRoundState>();
                    RoundCheckToRoundStartCountDown();
                }
                else
                {
                    ChangeState<InShopState>();
                    RoundCheckToShop();
                }
            }
        }

        private void RoundCheckToShop()
        {
            // load new scene
            // FIXME correct shop scene number
            var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();
            // Load ShopScene current index = 3
            sceneManager.LoadAt(3);
        }

        private void RoundCheckToRoundStartCountDown()
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
    }

    public class InShopState : State<RealGameStateManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            GameState = GameState.Shop;
        }

        public override void OnUpdate()
        {
            if (PlayersManager.Instance.CheckAllFinishedShop())
            {
                ChangeState<PreRoundState>();
                ShopToRoundStartCountdown();
            }

        }

        private void ShopToRoundStartCountdown()
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




}
