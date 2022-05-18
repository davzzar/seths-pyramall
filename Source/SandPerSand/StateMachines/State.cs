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
                if (!typeof(State<Manager>).IsAssignableFrom(state.GetType()))
                {
                    throw new ArgumentException($"{state.GetType()} is not a valid state for state manager {typeof(Manager)}");
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
    }

    public class State<T>: Behaviour where T: StateManager<T>
    {
        protected T stateManager;

        public event Action OnEnter;

        public event Action OnExit;

        protected override void OnAwake()
        {
            base.OnAwake();

            stateManager = Owner.Transform.Parent.Owner.GetComponent<T>();

            if (stateManager == null)
            {
                throw new InvalidOperationException($"Statemachine requires a manager component of type {typeof(T)}");
            }
        }

        protected void ChangeState<State>() where State : State<T>
        {
            var nextState = Owner.Transform.Parent.Owner.GetComponentInChildren<State>() ?? throw new ArgumentException($"{typeof(StateManager)} has no state of type {typeof(State)}");

            stateManager.CurrentState.Exit();
            stateManager.CurrentState = nextState;
            stateManager.CurrentState.Enter();
        }

        public void Enter()
        {
            Owner.IsEnabled = true;
            OnEnter?.Invoke();
        }

        public void Exit()
        {
            Owner.IsEnabled = false;
            OnExit?.Invoke();
        }
    }
}
