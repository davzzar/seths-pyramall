using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SandPerSand
{
    public static partial class Template
    {
        public static (GameObject, Manager, List<State<Manager>>) MakeStateMachine<Manager>(string name = null, Scene scene = null, params Type[] states) where Manager: StateManager<Manager>, new()
        {
            var (managerGameObject, managerComponent) = Make<Manager>(name, scene);

            var stateList = new List<State<Manager>>();

            foreach (var state in states)
            {
                //// FIXME Runtime.Type causing exception , remove check for now
                //if (!typeof(State<Manager>).IsAssignableFrom(state.GetType()))
                //{
                //    throw new ArgumentException($"{state.GetType()} is not a valid state for state manager {typeof(Manager)}");
                //}

                // FIXME is scene needed here for creating stateGameObject?
                if (scene == null)
                {
                    scene = SceneManager.ScopedScene;
                }
                var stateGameObject = new GameObject(state.Name, scene);
                stateGameObject.Transform.Parent = managerGameObject.Transform;

                stateList.Add(stateGameObject.AddComponent(state) as State<Manager>);
            }

            return (managerGameObject, managerComponent, stateList);
        }
    }



    public class StateManager<T>: Behaviour where T: StateManager<T>
    {
        public State<T> CurrentState { get; internal set; }

        protected override void Update()
        {
            base.Update();
        }
    }

    public class State<T> : Behaviour where T : StateManager<T>
    {
        [Obsolete("countDowncounter is deprecated")]
        public float CountDowncounter;

        [Obsolete("GameState is deprecated")]
        public GameState GameState;

        protected T stateManager;

        public event EventHandler<State<T>> OnEnter;
        public event Action OnExit;
        public event Action OnExitDynamic;
        public event Action OnUpdate;

        protected override void OnAwake()
        {
            stateManager = Owner.Transform.Parent.Owner.GetComponent<T>();
            if (stateManager == null)
            {
                throw new InvalidOperationException($"Statemachine requires a manager component of type {typeof(T)}");
            }
            if (stateManager.CurrentState != this)
            {
                IsActive = false;
            }
        }

        protected override void Update()
        {
            base.Update();
            OnUpdate?.Invoke();
        }

        public void ChangeState<State>() where State : State<T>
        {
            var nextState = Owner.Transform.Parent.Owner
                .GetComponentInChildren<State>() ?? throw
                new ArgumentException($"{typeof(StateManager<T>)} " +
                $"has no state of type {typeof(State)}");

            var lastState = stateManager.CurrentState;

            if (nextState == lastState)
            {
                return;
            }
            
            stateManager.CurrentState.Exit();
            stateManager.CurrentState = nextState;
            stateManager.CurrentState.EnterFrom(lastState);
        }


        public void EnterFrom(State<T> lastState)
        {
            IsActive = true;
            OnEnter?.Invoke(this,lastState);
        }

        public void Exit()
        {
            IsActive = false;
            OnExit?.Invoke();
            OnExitDynamic?.Invoke();
            if (OnExitDynamic != null)
            {
                foreach (Action action in OnExitDynamic.GetInvocationList())
                    OnExitDynamic -= action;
            }
                
        }
    }
}
