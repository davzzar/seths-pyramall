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
        internal static PlayersManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayersManager();
                    players = new Dictionary<PlayerIndex, GameObject>();
                    initialPositions = new List<Vector2>();
                }
                return instance;
            }
        }

        private static Dictionary<PlayerIndex, GameObject> players;

        internal void ResetAllPlayers()
        {
            foreach (var player in Players.Values)
            {
                player.GetComponent<PlayerStates>()!.Reset();
            }
        }

        public Dictionary<PlayerIndex, GameObject> Players
        {
            get => players;
        }

        private static List<Vector2> initialPositions;
        public List<Vector2> InitialPositions
        {
            get => initialPositions;
            private set
            {
                initialPositions = value;
            }
        }

        private static Vector2 shopEntryPosition;
        public Vector2 ShopEntryPosition {
            get => shopEntryPosition;
            set { shopEntryPosition = value; }
        }


        public PlayersManager()
        {
            Debug.Print("player manager created");
        }

        public GameState LastGameState { get; set; }
        public GameState CurrentGameState => GameStateManager.Instance.CurrentState;
        // TODO hard coded shopTime
        private float shopTime = 10f;
        private float shopTimeCounter = 0;
        private int curRank;
        private PlayerIndex[] rankList;

        protected override void Update()
        {
            if (CurrentGameState == GameState.InRound)
            {
                if (LastGameState == GameState.Shop)
                {
                    // this holds because we clear initial positions each time we exit a round
                    // initial positions are round specific
                    if (InitialPositions.ToArray().Length <= 0)
                    {
                        Debug.Print("Cannot respawn Players, because the map is " +
                            "not loaded or no initial positions on map.");
                        return;
                    }
                    LastGameState = GameState.InRound;
                    foreach (PlayerIndex playerIndex in Players.Keys)
                    {
                        RespawnPlayer(playerIndex, GetRandomInitialPos());
                    }
                }
            }
            else if (CurrentGameState == GameState.Shop)
            {
                bool atLeastOneAlive = false;
                foreach (var player in Players)
                {
                    if (player.Value.GetComponentInChildren<PlayerComponent>().IsAlive == true)
                    {
                        atLeastOneAlive = true;
                    }
                }
                if (!atLeastOneAlive)
                {
                    Debug.WriteLine("No players were alive. No shop.");
                    foreach (var player in Players)
                    {
                        player.Value.GetComponentInChildren<PlayerStates>().FnishedShop = true;
                    }
                    LastGameState = GameState.Shop;
                    return;
                }
                if (LastGameState != GameState.Shop)
                {
                    // FIXME wait for shop map
                    if (ShopEntryPosition.X < 4)
                    {
                        Debug.Print("Cannot respawn Player in the shop, " +
                            "because initial position is not yet loaded or invalid.");
                        return;
                    }
                    // clear initial positions when exit a round
                    // since they are round-specific FIXME better place to do this???
                    InitialPositions = new List<Vector2>();

                    // calculate rank list
                    rankList = new PlayerIndex[Players.Count];
                    foreach (var player in Players)
                    {
                        int rank = player.Value.GetComponent<PlayerStates>().RoundRank;
                        if (rank <= 0)
                        {
                            throw new Exception("Invalid rank at end");
                        }
                        rankList[rank - 1] = player.Key;
                    }

                    // respawn players -> queue by rank list
                    // disable all players controller
                    var entryX = ShopEntryPosition.X;
                    foreach (var playerIndex in rankList)
                    {
                        if (players[playerIndex].GetComponentInChildren<PlayerComponent>().IsAlive == true)
                        {
                            RespawnPlayer(playerIndex, new Vector2(entryX--, ShopEntryPosition.Y));
                            GetPlayer(playerIndex).GetComponent<PlayerControlComponent>().IsActive = false;
                        } else
                        {
                            players[playerIndex].GetComponentInChildren<PlayerStates>().FnishedShop = true;
                        }
                    }
                    // ShopEntryPsition can be reset right after use
                    // If not reset, players will be spawned before shop map is
                    // loaded next time ... then drop
                    ShopEntryPosition = Vector2.Zero;
                    LastGameState = GameState.Shop;
                    shopTimeCounter = shopTime;
                    curRank = 0;
                }
                shopTimeCounter += Time.DeltaTime;
                if (shopTimeCounter >= shopTime)
                {
                    // Reset the shop coutner
                    shopTimeCounter = 0;

                    // If this was not the first player, set the one before to be finished with the shop
                    if(curRank>0)
                    {
                        Players[rankList[curRank - 1]].GetComponent<PlayerStates>().FnishedShop = true;
                        //Players[rankList[curRank - 1]].GetComponent<PlayerControlComponent>().IsActive = false;
                    }

                    // Activate the controller of the next player
                    if(curRank < rankList.Length) Players[rankList[curRank]].GetComponent<PlayerControlComponent>().IsActive = true;
                    curRank++;
                }

            }
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
                players[playerIndex] = Template.MakePlayer(playerIndex, position);
            }
            else
            {
                players.Add(playerIndex, Template.MakePlayer(playerIndex, position));
            }
        }

        public void RespawnPlayer(PlayerIndex playerIndex, Vector2 position)
        {
            if (players.ContainsKey(playerIndex))
            {
                players[playerIndex].GetComponent<PlayerControlComponent>().IsActive = false;
                players[playerIndex].GetComponent<RigidBody>().LinearVelocity = Vector2.Zero;
                players[playerIndex].Transform.Position = position;
                players[playerIndex].GetComponent<Animator>().Entry();
                players[playerIndex].GetComponent<PlayerControlComponent>().IsActive = true;
                players[playerIndex].GetComponent<PlayerComponent>().IsAlive = true;
            }
            else
            {
                throw new InvalidOperationException("");
            }

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
            int tot_coins = state.addCoins(coins);
            GraphicalUserInterface.Instance.renderCoins(player, tot_coins);
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
                    "registered. Please add at least one 'Entry' Tile on map and make sure " +
                    "map is loaded before Players' creation");
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
                        if (InitialPositions.ToArray().Length > 0)
                        {
                            CreatePlayer(playerIndex, GetRandomInitialPos());
                        }
                        else
                        {
                            Debug.Print("Cannot create Player" + playerIndex
                                + ", because map is not loaded or no initial positions on map.");
                        }
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

        public Boolean CheckOneExit()
        {
            if (players.Count == 0)
            {
                return false;
            }
            foreach (var player in players.Values)
            {
                if (player.GetComponent<PlayerStates>().Exited)
                {
                    return true;
                }
            }
            return false;
        }

        public Boolean CheckAllDead()
        {
            if (players.Count == 0)
            {
                return true;
            }
            foreach (var player in players.Values)
            {
                if (player.GetComponent<PlayerComponent>().IsAlive)
                {
                    return false;
                } else {
                    player.GetComponent<PlayerStates>().RoundRank = players.Values.Count;
                }
            }
            return true;
        }

        public Boolean CheckAllExit()
        {
            if (players.Count == 0)
            {
                return false;
            }
            var allExitedFlag = true;
            foreach (var player in players.Values)
            {
                if (!player.GetComponent<PlayerStates>().Exited)
                {
                    allExitedFlag = false;
                }
            }
            return allExitedFlag;
        }

        public Boolean CheckAllFinishedShop()
        {
            if (players.Count == 0)
            {
                return true;
            }
            var allExitedFlag = true;
            foreach (var player in players.Values)
            {
                if (!player.GetComponent<PlayerStates>().FnishedShop)
                {
                    allExitedFlag = false;
                }
            }
            return allExitedFlag;
        }

        public void finalizeRanks()
        {
            int notExitedFrom = 1;
            foreach (var player in players.Values)
            {
                if (player.GetComponent<PlayerStates>().RoundRank != -1)
                {
                    notExitedFrom++;
                }
            }
            foreach (var player in players.Values)
            {
                if (player.GetComponent<PlayerStates>().RoundRank == -1)
                {
                    player.GetComponent<PlayerStates>().RoundRank = notExitedFrom++;
                }
            }
        }
    }

    public class PlayerStates : Behaviour
    {
        public Boolean Prepared;
        public static Boolean Paused;
        public string MinorItem;
        public string MajorItem;
        public int Coins;
        public bool Exited { get; set; }
        public bool FnishedShop { get; set; }
        public int RoundRank { get; set; }
        public int Score { get; set; }
        public InputHandler InputHandler { get; set; }
        private bool PrepareButtonPressed => InputHandler.getButtonState(Buttons.A) == ButtonState.Pressed;
        public GameState LastGameState{ get; set; }
        public GameState CurrentGameState => GameStateManager.Instance.CurrentState;

        public List<(string id, float timeleft)> activeItems;

        protected override void OnAwake()
        {
            Reset();
        }

        public void Reset()
        {
            Prepared = false;
            Paused = false;
            MinorItem = null;
            MajorItem = null;
            Coins = 0;
            Exited = false;
            FnishedShop = false;
            RoundRank = -1;
            activeItems = new List<(string id, float timeleft)>();
            Score = 0;
        }

        /// <summary>
        /// The update handles state transitions.
        /// </summary>
        protected override void Update()
        {
            //<<<<<<< Yuchen stuff
            if (PrepareButtonPressed && CurrentGameState == GameState.Prepare)
            {
                TogglePrepared();
            }
            if (LastGameState != CurrentGameState)
            {
                if(LastGameState == GameState.RoundCheck )
                {

                    FnishedShop = false;
                }else if(LastGameState == GameState.Shop)
                {
                    // reset round states
                    Exited = false;
                    RoundRank = -1;
                }
                LastGameState = CurrentGameState;
            }
            //======= End of Yuchen stuff

            //<<<<<<< Clemens stuff
            float timeDelta = Time.DeltaTime;

            List<int> remove = new List<int>();

            for (int i = 0; i < activeItems.Count; i++)
            {
                activeItems[i] = (activeItems[i].id, activeItems[i].timeleft - timeDelta);
                if (activeItems[i].timeleft < 0)
                {
                    remove.Add(i);
                }
            }

            for (int i = remove.Count - 1; i >= 0; i--)
            {
                activeItems.RemoveAt(remove[i]);
            }
            //======= End of Clemens stuff
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

        public Boolean addItemToInventory(string item, Boolean Major)
        {
            //returns true if item was added
            if (Major)
            {
                if(MajorItem == null)
                {
                    MajorItem = item;
                    return true;
                }
            }
            else
            {
                if (MinorItem == null)
                {
                    MinorItem = item;
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
                string tmp = MajorItem;
                MajorItem = null;
                return tmp;
            }
            else
            {
                string tmp = MinorItem;
                MinorItem=null;
                return tmp;
            }
        }

        public int addCoins(int amount)
        {
            Coins += amount;
            return Coins;
        }

        public (Boolean, int) spendCoins(int amount)
            //returns true when action was successfull
        {
            if(Coins >= amount)
            {
                Coins -= amount;
                return (true, Coins);
            }
            else
            {
                return (false, Coins);
            }
        }

        public float getJumpFactor()
        {
            float jumpfactor = 1;
            foreach(var item in activeItems)
            {
                if(item.id == "wings") 
                {
                    jumpfactor *= 1.5f;
                }
                else if(item.id == "ice_block")
                {
                    jumpfactor *= 0;
                }
            }
            return jumpfactor;
        }

        public float getAccellerationFactor()
        {
            float accelleration = 1;

            foreach (var item in activeItems)
            {
                if (item.id == "speedup")
                {
                    accelleration *= 1.5f;
                }
                else if (item.id == "ice_block")
                {
                    accelleration *= 0;
                }
            }
            return accelleration;
        }

        public float getInvertedMovement()
        {
            float invertedMovement = 1;

            foreach (var item in activeItems)
            {
                if (item.id == "dizzy_eyes")
                {
                    invertedMovement *= -1f;
                }
            }
            return invertedMovement;
        }
    }
}
