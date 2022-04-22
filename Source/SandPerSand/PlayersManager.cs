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
    public class PlayersManager : Component
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
                GraphicalUserInterface.Instance.destroyPlayerInfo(index);
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
            GraphicalUserInterface.Instance.renderPlayerInfo(playerIndex);
            if (players.ContainsKey(playerIndex))
            {
                if (players[playerIndex] != null)
                {
                    players[playerIndex].Destroy();
                }
                players[playerIndex] = new GameObject();
            }
            else
            {
                players.Add(playerIndex, new GameObject());
            }

            var playerGo = players[playerIndex];
            playerGo.Transform.Position = position;
            var playerRenderer = playerGo.AddComponent<SpriteRenderer>();
            playerRenderer.LoadFromContent("Smiley");

            var playerCollider = playerGo.AddComponent<PolygonCollider>();
            playerCollider.Outline = new[]
            {
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, 0.5f)
            };
            playerCollider.Friction = 0.0f;
            var playerRB = playerGo.AddComponent<RigidBody>();
            playerRB.IsKinematic = false;
            playerRB.FreezeRotation = true;
            var playerCon = playerGo.AddComponent<PlayerControlComponent>();
            playerCon.PlayerIndex = playerIndex;
            var playerStates = playerGo.AddComponent<PlayerStates>();


        }

        public Boolean addItemToInventory(PlayerIndex player, string item, Boolean Major)
            //returns true if item was added. False if it is already full
        {
            PlayerStates state = players[player].GetComponentInChildren<PlayerStates>();
            Boolean success = state.addItemToInventory(item, Major);
            if (success)
            {
                GraphicalUserInterface.Instance.renderItem(player, item, Major);
            }
            return success;
        }

        public string useItem(PlayerIndex player, Boolean Major)
        {
            PlayerStates state = players[player].GetComponentInChildren<PlayerStates>();
            GraphicalUserInterface.Instance.removeItem(player, Major);
            return state.useItem(Major);
        }

        public void addCoins(PlayerIndex player, int coins)
        {
            PlayerStates state = players[player].GetComponentInChildren<PlayerStates>();
            GraphicalUserInterface.Instance.renderCoins(player, coins);
            state.addCoins(coins);
        }
        public Boolean spendCoins(PlayerIndex player, int coins)
        {
            PlayerStates state = players[player].GetComponentInChildren<PlayerStates>();
            (Boolean, int) tmp = state.spendCoins(coins);
            GraphicalUserInterface.Instance.renderCoins(player, tmp.Item2);
            return tmp.Item1;
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

    public class PlayerStates : Component
    {
        public Boolean Prepared;
        public static Boolean Paused;
        public string minor_item;
        public string major_item;
        public int coins;
        public bool Exited { get; set; }
        public int RoundRank { get; set; }

        protected override void OnAwake()
        {
            Prepared = false;
            Paused = false;
            minor_item = null;
            major_item = null;
            coins = 0;
            Exited = false;
            RoundRank = -1;
        }

        public void TogglePrepared()
        {
            // Debug
            var playerIndex = this.Owner.GetComponent<PlayerControlComponent>().PlayerIndex;
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

        public Boolean addItemToInventory(string item, Boolean Major)
        {
            //returns true if item was added
            if (Major)
            {
                if(major_item == null)
                {
                    major_item = item;
                    return true;
                }
            }
            else
            {
                if (minor_item == null)
                {
                    minor_item = item;
                    return true;
                }
            }
            return false;
        }

        public string useItem(Boolean Major)
            //returns null if no item
        {
            if (Major)
            {
                string tmp = major_item;
                major_item = null;
                return tmp;
            }
            else
            {
                string tmp = minor_item;
                minor_item=null;
                return tmp;
            }
        }

        public int addCoins(int amount)
        {
            coins += amount;
            return coins;
        }

        public (Boolean, int) spendCoins(int amount)
            //returns true when action was successfull
        {
            if(coins > amount)
            {
                coins = amount = coins;
                return (true, coins);
            }
            else
            {
                return (false, coins);
            }
        }
    }
}
