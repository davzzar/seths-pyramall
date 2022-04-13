using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    public class GameStateManager : Behaviour
    {

        public static GameStateManager Instance
        {
            get
            {
                return GameObject.FindComponent<GameStateManager>();
            }
        }

        public GameStateManager()
        {

        }

        private static GameState currentState;

        public GameState CurrentState
        {
            get
            {
                return currentState;
            }
            set
            {
                currentState = value;
            }
        }

        public enum GameState
        {
            Prepare,
            InRound
        }

        // transfer variable used in Prepare state
        // at prepare state, keep checking for new gamepad and update the dictionary
        // once a gamepad is connected, add its PlayerIndex as key in the dict, with value false
        // once a gamepad is disconnected, delete its key in the dict
        // once all values in the dictionary are true, transfer to InRound state
        public Dictionary<PlayerIndex, Boolean> PlayerPrepared;

        protected override void OnEnable()
        {
            PlayerPrepared = new Dictionary<PlayerIndex, bool>();
            this.CurrentState = GameState.Prepare;
            Instance.CurrentState = GameState.Prepare;
        }



        protected override void Update()
        {
            if(CurrentState == GameState.Prepare) {
                var allPreparedFlag = true;
                foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
                {
                    GamePadCapabilities capabilities = GamePad.GetCapabilities(playerIndex);
                    if (capabilities.IsConnected)
                    {
                        if (!PlayerPrepared.ContainsKey(playerIndex))
                        {
                            PlayerPrepared.Add(playerIndex, false);
                            Console.WriteLine("New Connected controller:"+playerIndex);
                            //add player FIXME hard code
                            PlayersManager.Instance.CreatePlayer(playerIndex,new Vector2(5,5));


                        }
                        if (PlayerPrepared[playerIndex] == false)
                        {
                            allPreparedFlag = false;
                        }
                    }
                    else
                    {
                        if (PlayerPrepared.Remove(playerIndex))
                        {
                            Console.WriteLine("Disconnected:" + playerIndex);
                            //delete player
                            PlayersManager.Instance.DestroyPlayer(playerIndex);
                        }
                    }
                }

                if(allPreparedFlag && PlayerPrepared.Count != 0)
                {
                    //transfer
                    CurrentState = GameState.InRound;
                    Console.WriteLine("GameState: Prepare-> InRound");
                }
            }

        }
    }
}
