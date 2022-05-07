using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    public class GameStateManager : Behaviour
    {
        private static GameStateManager instance;

        public bool exitTrigger;
        public bool TriggerExit()
        {
            Debug.Print("exit trigger script is run");
            if(currentState== GameState.InRound||
                currentState== GameState.CountDown)
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
                    currentState = GameState.Prepare;
                }
                return instance;
            }
        }

        private static GameState currentState;

        public GameState CurrentState
        {
            get
            {
                return currentState;
            }
        }



        public float countDowncounter { private set; get; }
        protected override void Update()
        {
            switch (CurrentState)
            {
                case GameState.Prepare:
                    // at prepare state, PlayersManager keep checking for new gamepad
                    PlayersManager.Instance.CheckConnections();
                    if (PlayersManager.Instance.CheckAllPrepared())
                    {
                        currentState = GameState.InRound;
                        Debug.Print("GameState: Prepare-> InRound");
                    }
                    break;
                case GameState.InRound:
                    if (PlayersManager.Instance.CheckOneExit())
                    {
                        currentState = GameState.CountDown;
                        exitTrigger = false;
                        Debug.Print("GameState: InRound-> CountDown");
                        //
                        countDowncounter = 0f;
                    }
                    break;
                case GameState.CountDown:
                    countDowncounter += Time.DeltaTime;
                    if (countDowncounter >= 10f || PlayersManager.Instance.CheckAllExit())
                    {
                        countDowncounter = 0;
                        PlayersManager.Instance.finalizeRanks();
                        currentState = GameState.RoundCheck;
                        // Debug
                        Debug.Print("GameState: CountDown-> RoundCheck");
                        foreach(var item in PlayersManager.Instance.Players)
                        {
                            Debug.Print("Player "+ item.Key + " : Rank " +
                                item.Value.GetComponent<PlayerStates>().RoundRank);
                        }
                    }
                    break;
                case GameState.RoundCheck:
                    countDowncounter += Time.DeltaTime;
                    if (countDowncounter >= 2f)
                    {
                        countDowncounter = 0;
                        currentState = GameState.Shop;
                        Debug.Print("GameState: RoundCheck-> Shop");
                        // accept rank as initial parameters
                        // load new scene
                        // create players in queue
                        var sceneManager = GameObject.FindComponent<Program.SceneManagerComponent>();

                        // get rank list
                        var rankList = new PlayerIndex[PlayersManager.Instance.Players.Count]; 
                        foreach (var item in PlayersManager.Instance.Players)
                        {
                            int rank = item.Value.GetComponent<PlayerStates>().RoundRank;
                            if(rank>0)rankList[rank-1] = item.Key;
                        }
                        // respawn players -> queue by rank list
                        // disable all players controller
                        // FIXME get correct shop entrys
                        var entryX = 10;
                        var entryY = 3;
                        foreach (var playerIndex in rankList)
                        {
                            var player = PlayersManager.Instance.GetPlayer(playerIndex);
                            PlayersManager.Instance.RespawnPlayer(playerIndex, new Vector2(entryX--, entryY));
                            player.GetComponent<PlayerControlComponent>().IsActive = false;
                        }
                        // FIXME correct shop scene number
                        sceneManager.LoadAt(1);

                    }
                    break;
                case GameState.Shop:
                    // disable the current player's controller after they finished shopping or after the time limit
                    // move the finished player to the exit if they is not there
                    // enable the next first player's controller
                    // ...
                    // after all player is moved to exit, proceed to the next round
                    countDowncounter += Time.DeltaTime;
                    if (countDowncounter >= 10f)
                    {
                        countDowncounter = 0;
                    }
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
        Shop,
    }
}
