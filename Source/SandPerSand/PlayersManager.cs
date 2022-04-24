using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace SandPerSand
{
    public class PlayersManager : Behaviour
    {
        private static PlayersManager instance;
        private Dictionary<PlayerIndex, GameObject> players;
        public Dictionary<PlayerIndex, GameObject> Players
        {
            get => this.players;
        }

        private List<Vector2> initialPositions;
        public List<Vector2> InitialPositions
        {
            get => this.initialPositions;
        }

        internal static PlayersManager Instance
        {
            get
            {
                if(instance == null)
                {
                    throw new InvalidOperationException(
                        "No PlayersManager in the game. Please create one.");
                }
                return instance;
            }
        }


        public PlayersManager()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Can't create more than one PlayersManager");
            }
            instance = this;
            this.players = new Dictionary<PlayerIndex, GameObject>();
            this.initialPositions = new List<Vector2>();
        }

        public GameObject GetPlayer(PlayerIndex index) {
            return players[index];
        }

        public bool DestroyPlayer(PlayerIndex index)
        {
            if (players.ContainsKey(index))
            {
                players[index].Destroy();
                players.Remove(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CreatePlayer(PlayerIndex playerIndex, Vector2 position)
        {
            if (players.ContainsKey(playerIndex))
            {
                if (players[playerIndex] != null)
                {
                    players[playerIndex].Destroy();
                }
                players[playerIndex] = PlayerGo.Create(playerIndex, position);
            }
            else
            {
                players.Add(playerIndex, PlayerGo.Create(playerIndex, position));
            }
        }

        private Vector2 GetRandomInitialPos()
        {
            Random rd = new Random();
            int totalPosNum = InitialPositions.ToArray().Length;
            if (totalPosNum <= 0)
            {
                throw new InvalidOperationException("No initial position " +
                    "registered. Please add at least one 'Entry' Tile on map.");
            }
            return InitialPositions[rd.Next(0, totalPosNum)];
        }

        public void CheckConnections()
        {
            // check for new connection / disconnection
            foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
            {
                GamePadCapabilities capabilities = GamePad.GetCapabilities(playerIndex);
                if (capabilities.IsConnected)
                {
                    if (!players.ContainsKey(playerIndex))
                    {
                        Debug.Print("New Connected controller:" + playerIndex);
                        CreatePlayer(playerIndex, GetRandomInitialPos());
                    }
                }
                else
                {
                    if (DestroyPlayer(playerIndex))
                    {
                        //delete player
                        Debug.Print("Disconnected:" + playerIndex);
                    }
                }
            }

        }

        public Boolean CheckAllPrepared()
        {
            if (players.Count == 0)
            {
                return false;
            }
            var allPreparedFlag = true;
            foreach (var player in players.Values)
            {
                if (!player.GetComponent<PlayerStates>().Prepared)
                {
                    allPreparedFlag = false;
                }
            }
            return allPreparedFlag;
        }


    }

    public class PlayerStates : Behaviour
    {
        public Boolean Prepared;
        public static Boolean Paused;
        public bool Exited { get; set; }
        public int RoundRank { get; set; }
        public InputHandler InputHandler { get; set; }
        private bool PrepareButtonPressed => InputHandler.getButtonState(Buttons.A) == ButtonState.Pressed;

        protected override void OnAwake()
        {
            Prepared = false;
            Paused = false;
            Exited = false;
            RoundRank = -1;
        }

        /// <summary>
        /// The update handles state transitions.
        /// </summary>
        protected override void Update()
        {
            if (PrepareButtonPressed && GameStateManager.Instance.CurrentState == GameState.Prepare)
            {
                TogglePrepared();
            }
        }

        public void TogglePrepared()
        {
            // Debug
            var playerIndex = InputHandler.PlayerIndex;
            if (Prepared)
            {
                Prepared = false;
                Debug.Print("Player" + playerIndex + "UnPrepared.");
            }
            else
            {
                Prepared = true;
                Debug.Print("Player" + playerIndex + "Prepared.");
            }
        }

        public static void TogglePaused()
        {
            if (Paused)
            {
                Paused = false;
            }
            else
            {
                Paused = true;
            }
        }
    }
}
