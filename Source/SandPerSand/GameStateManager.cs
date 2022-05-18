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
        public static bool inMenu;

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
                    inMenu = true;
                }
                return instance;
            }
        }

        [Obsolete("GameStateManager.CurrentState is deprecated; Please stop querying " +
"CurrentGameState all over the place, instead register your code to OnEnter," +
"OnExit events of corresponding State.")]
        public GameState CurrentState
        {
            get
            {
                return GameObject.FindComponent<RealGameStateManager>().CurrentGameState;
            }
        }
        [Obsolete("GameStateManager will soon no longer exist.")]
        public bool InMenu { get
            {
                return inMenu;
            }
            set { inMenu = value; }
        }

        [Obsolete("GameStateManager will soon no longer exist.")]
        public float CountDowncounter { 
            get
            {
                return GameObject.FindComponent<RealGameStateManager>().CurrentState.CountDowncounter; 
            } 
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
