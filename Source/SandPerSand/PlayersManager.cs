using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Linq;

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
                }
                return instance;
            }
        }

        private static Dictionary<PlayerIndex, GameObject> players;
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
            instance = this;
            players = new Dictionary<PlayerIndex, GameObject>();
            initialPositions = new List<Vector2>();
            Debug.Print("player manager created");
        }

        public GameState? LastGameState { get; set; }
        public GameState CurrentGameState => GameStateManager.Instance.CurrentState;
        // TODO hard coded shopTime
        public float ShopTime { get; private set; } = 10f;
        public CancelableTimer ShopTimer { get; private set; } = null;
        public int CurRank { get; private set; }
        private PlayerIndex[] rankList;

        private SoundEffectPlayer itemPickupSoundEffect;
        private SoundEffectPlayer itemBuySoundEffect;
        protected override void OnEnable()
        {
            // add components to the manager owner
            itemPickupSoundEffect = Owner.AddComponent<SoundEffectPlayer>();
            itemPickupSoundEffect.LoadFromContent("Sounds/item_use01",
                "Sounds/item_use02",
                "Sounds/item_use03",
                "Sounds/item_use04");

            itemBuySoundEffect = Owner.AddComponent<SoundEffectPlayer>();
            itemBuySoundEffect.LoadFromContent("Sounds/shop_payment");
        }
        protected override void OnAwake()
        {
            base.OnAwake();
            LastGameState = null;
        }

        protected override void Update()
        {
            if (CurrentGameState == GameState.InRound)
            {
                if(LastGameState != GameState.InRound)
                {
                    //Enter
                    SetAllPlayerControls(true);
                    LastGameState = GameState.InRound;
                }
                else
                {
                    //During
                }
            }
            else if (CurrentGameState == GameState.Prepare)
            {
                if (LastGameState != GameState.Prepare)
                {
                    //Enter
                    SetAllPlayerControls(false);
                    LastGameState = GameState.Prepare;
                }
                else
                {
                    //During
                }
            }
            else if (CurrentGameState == GameState.RoundStartCountdown)
            {
                if(LastGameState != GameState.RoundStartCountdown)
                {
                    // Enter from shop
                    if (LastGameState == GameState.Shop)
                    {
                        if (EnterRoundStartCountdownFromShop())
                        {
                            LastGameState = GameState.RoundStartCountdown;
                        }
                    }
                    // Enter from other states (= Prepare)
                    else
                    {
                        SetAllPlayerControls(false);
                        LastGameState = GameState.RoundStartCountdown;
                    }
                }
                else
                {
                    //During
                }
            }
            else if (CurrentGameState == GameState.Shop)
            {
                // Enter shop
                if (LastGameState != GameState.Shop)
                {
                    if (EnterShop())
                    {
                        LastGameState = GameState.Shop;
                    }
                }
                else
                {
                    DuringShop();
                }
                
            }
        }

        private void DuringShop()
        {
            bool ItsCurrentPlayersTurn =
    CurRank == 0 || CurRank < rankList.Length &&
    Players[rankList[CurRank - 1]].GetComponent<PlayerStates>().FnishedShop;
            Debug.Print("CurRank:" + CurRank);
            if (ItsCurrentPlayersTurn)
            {
                var curPlayer = Players[rankList[CurRank]];
                var curPlayerState = curPlayer.GetComponent<PlayerStates>();
                Debug.Assert(curPlayerState.FnishedShop == false);
                PlayerUtils.ResumePlayerControl(curPlayer);
                // init new timer
                ShopTimer = Owner.AddComponent<CancelableTimer>();
                ShopTimer.Init(ShopTime,
                    () => {
                    curPlayerState.FnishedShop = true;
                    },
                    ()=> { return CheckAllFinishedShop(); }
                    );

                CurRank++;
            }
        }

        private bool EnterShop()
        {
            if (CheckAllDead())
            {
                Debug.WriteLine("No players were alive. No shop.");
                foreach (var player in Players)
                {
                    player.Value.GetComponentInChildren<PlayerStates>().FnishedShop = true;
                }
                return true;
            }
            // FIXME wait for shop map
            if (ShopEntryPosition.X < 4)
            {
                Debug.Print("Cannot respawn Player in the shop, " +
                    "because initial position is not yet loaded or invalid.");
                return false;
            }
            Debug.Print("PM: -> Shop");
            // clear initial positions when exit a round
            // since they are round-specific FIXME better place to do this???
            InitialPositions = new List<Vector2>();

            // calculate rank list
            rankList = new PlayerIndex[Players.Count];
            foreach (var player in Players)
            {
                var rank = player.Value.GetComponent<PlayerStates>().RoundRank;
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
                    PlayerUtils.ShieldPlayerControl(Players[playerIndex]);
                }
                else
                {
                    players[playerIndex].GetComponentInChildren<PlayerStates>().FnishedShop = true;
                }
            }
            // ShopEntryPsition can be reset right after use
            // If not reset, players will be spawned before shop map is
            // loaded next time ... then drop
            ShopEntryPosition = Vector2.Zero;
            CurRank = 0;
            return true;
        }

        private bool EnterRoundStartCountdownFromShop()
        {
            // this holds because we clear initial positions each time we exit a round
            // initial positions are round specific
            if (InitialPositions.ToArray().Length <= 0)
            {
                Debug.Print("Cannot respawn Players, because the map is " +
                    "not loaded or no initial positions on map.");
                return false;
            }
            foreach (var playerIndex in Players.Keys)
            {
                RespawnPlayer(playerIndex, GetRandomInitialPos());
            }
            SetAllPlayerControls(false);
            return true;
        }

        public GameObject GetPlayer(PlayerIndex index) {
            return players[index];
        }

        public bool DestroyPlayer(PlayerIndex index)
        {
            if (!players.ContainsKey(index))
            {
                return false;
            }
            
            GraphicalUserInterface.Instance.destroyPlayerInfo(index);
            players[index].Destroy();
            players.Remove(index);
            return true;

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

        public bool AddItemToInventory(PlayerIndex player, string item, bool major)
            //returns true if item was added. False if it is already full
        {
            var state = players[player].GetComponentInChildren<PlayerStates>();
            var success = state.AddItemToInventory(item, major);
            if (success)
            {
                GraphicalUserInterface.Instance.renderItem(player, item, major);
            }
            return success;
        }

        public string UseItem(PlayerIndex player, bool Major)
        {
            itemPickupSoundEffect?.Play();
            PlayerStates state = players[player].GetComponentInChildren<PlayerStates>();
            GraphicalUserInterface.Instance.removeItem(player, Major);
            return state.UseItem(Major);
        }

        public void AddCoins(PlayerIndex player, int coins)
        {
            var state = players[player].GetComponentInChildren<PlayerStates>();
            var totCoins = state.AddCoins(coins);
            GraphicalUserInterface.Instance.renderCoins(player, totCoins);
        }
        public bool SpendCoins(PlayerIndex player, int coins)
        {
            PlayerStates state = players[player].GetComponentInChildren<PlayerStates>();
            (Boolean, int) tmp = state.SpendCoins(coins);
            if (tmp.Item1)
            {
                // play sfx
                itemBuySoundEffect?.Play();
            }
            GraphicalUserInterface.Instance.renderCoins(player, tmp.Item2);
            return tmp.Item1;
        }
        private Vector2 GetRandomInitialPos()
        {
            var rd = new Random();
            var totalPosNum = InitialPositions.ToArray().Length;
            
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
                var capabilities = GamePad.GetCapabilities(playerIndex);
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

        public bool CheckAllPrepared()
        {
            if (players.Count == 0)
            {
                return false;
            }

            return Players.Values.All(player => player.GetComponent<PlayerStates>()!.Prepared);
        }

        public bool CheckOneExit()
        {
            return players.Values.Any(player => player.GetComponent<PlayerStates>()!.Exited);
        }

        public bool CheckAllDead()
        {
            if (players.Count == 0)
            {
                return true;
            }
            
            foreach (var player in players.Values)
            {
                if (player.GetComponent<PlayerComponent>()!.IsAlive)
                {
                    return false;
                }

                player.GetComponent<PlayerStates>()!.RoundRank = players.Values.Count;
            }
            return true;
        }

        public bool CheckAllDeadOrExit()
        {
            if (players.Count == 0)
            {
                return false;
            }

            return Players.Values.All(player => !player.GetComponent<PlayerComponent>().IsAlive || player.GetComponent<PlayerStates>().Exited);
        }

        public bool CheckAllFinishedShop()
        {
            return players.Values.All(player => player.GetComponent<PlayerStates>()!.FnishedShop);
        }

        public void FinalizeRanks()
        {
            var notExitedFrom = 1;
            foreach (var player in players.Values)
            {
                if (player.GetComponent<PlayerStates>().RoundRank != -1)
                {
                    notExitedFrom++;
                }
            }
            foreach (var player in players.Values)
            {
                if (player.GetComponent<PlayerStates>()!.RoundRank == -1)
                {
                    player.GetComponent<PlayerStates>()!.RoundRank = notExitedFrom++;
                }
            }
        }

        public List<GameObject> InGamePlayerGo()
        {
            return players.Values.Where(player => !player.GetComponent<PlayerStates>()!.Exited && player.GetComponent<PlayerComponent>()!.IsAlive).ToList();
        }

        public void SetAllPlayerControls(bool enabled)
        {
            foreach (var player in players.Values)
            {
                if (enabled)
                {
                    PlayerUtils.ResumePlayerControl(player);
                }
                else
                {
                    PlayerUtils.ShieldPlayerControl(player);
                }
            }
        }
    }

    public class PlayerStates : Behaviour
    {
        public bool Prepared;
        public static bool Paused;
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
        public Collider Collider { get; set; }

        public List<(string id, float timeleft, float tot_time, Vector2 pos)> ActiveItems { private set; get; }

        protected override void OnAwake()
        {
            Prepared = false;
            Paused = false;
            MinorItem = null;
            MajorItem = null;
            Coins = 0;
            Exited = false;
            FnishedShop = false;
            RoundRank = -1;
            ActiveItems = new List<(string id, float timeleft, float tot_time, Vector2 pos)>();
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
            var timeDelta = Time.DeltaTime;

            var remove = new List<int>();

            //bool lightning = false;

            for (var i = 0; i < ActiveItems.Count; i++)
            {
                var pos = ActiveItems[i].pos;
                var time = ActiveItems[i].timeleft;
                if (ActiveItems[i].id == "position_swap")
                {
                    Debug.Print((ActiveItems[i].pos - this.Transform.Position).LengthSquared().ToString());
                    if ((ActiveItems[i].pos - this.Transform.Position).LengthSquared() < 0.5f)
                    {
                        this.Transform.Position = ActiveItems[i].pos;
                        time = -1f;
                        Collider.IsActive = true;
                    }
                    else
                    {
                        var vel = (ActiveItems[i].pos - this.Transform.Position) / (ActiveItems[i].pos - this.Transform.Position).Length();
                        this.Transform.Position = (ActiveItems[i].pos * 0.1f + 0.9f * this.Transform.Position) + vel / 10;
                        var collider = this.Owner.GetComponent<Collider>();
                        Collider.IsActive = false;
                    }
                }
                else
                {
                    if (ActiveItems[i].pos.Y < 0)
                    {
                        pos = ActiveItems[i].pos;
                        time = ActiveItems[i].timeleft - timeDelta;
                    }
                    else if ((ActiveItems[i].pos - this.Transform.Position).LengthSquared() < 0.1f)
                    {
                        pos = -Vector2.One;
                        time = ActiveItems[i].timeleft - timeDelta;
                    }
                    else
                    {
                        var vel = (ActiveItems[i].pos - this.Transform.Position) / (ActiveItems[i].pos - this.Transform.Position).Length();
                        pos = ActiveItems[i].pos * 0.9f + 0.1f * this.Transform.Position - vel / 10;
                        time = ActiveItems[i].timeleft;
                    }
                }

                if (ActiveItems[i].timeleft < 0)
                {
                    remove.Add(i);
                }
                ActiveItems[i] = (ActiveItems[i].id, time, ActiveItems[i].tot_time, pos);

                //if(activeItems[i].id == "lightning")
                //{
                //    lightning = true;
                //}
            }

            //if (lightning)
            //{
            //    Collider.Owner.GetComponentInParents<PlayerComponent>().Transform.LossyScale = Vector2.One * 0.8f;
            //    Collider.IsActive = false;
            //    Collider.IsActive = true;
            //}

            for (var i = remove.Count - 1; i >= 0; i--)
            {
                if (ActiveItems[i].id == "lightning")
                {
                    Collider.Owner.GetComponentInParents<PlayerComponent>().Transform.LossyScale = Vector2.One;
                    Collider.Transform.LossyScale = Vector2.One * 0.8f;
                }
                ActiveItems.RemoveAt(remove[i]);
            }
            //======= End of Clemens stuff
        }

        public void TogglePrepared()
        {
            // Debug
            var playerIndex = InputHandler.PlayerIndex;
            if (Prepared)
            {
                //Prepared = false;
                //Debug.Print("Player" + playerIndex + "UnPrepared.");
            }
            else
            {
                Prepared = true;
                Debug.Print("Player" + playerIndex + "Prepared.");
            }
        }

        public static void TogglePaused()
        {
            Paused = !Paused;
        }

        public bool AddItemToInventory(string item, bool major)
        {
            //returns true if item was added
            if (major)
            {
                if (MajorItem == null)
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

        public string UseItem(bool major)
            //returns null if no item
        {
            if (major)
            {
                var tmp = MajorItem;
                MajorItem = null;
                return tmp;
            }
            else
            {
                var tmp = MinorItem;
                MinorItem=null;
                return tmp;
            }
        }

        public int AddCoins(int amount)
        {
            Coins += amount;
            return Coins;
        }

        public (bool, int) SpendCoins(int amount)
            //returns true when action was successfull
        {
            if (Coins < amount)
            {
                return (false, Coins);
            }
            
            Coins -= amount;
            return (true, Coins);

        }

        public float GetJumpFactor()
        {
            float jumpfactor = 1;
            foreach(var item in ActiveItems)
            {
                if(item.id == "wings") 
                {
                    jumpfactor *= 1.5f;
                }
                else if(item.id == "ice_block")
                {
                    jumpfactor *= 0;
                }
                else if ((item.id == "lightning"))
                {
                    jumpfactor *= 0.8f;
                }
            }
            return jumpfactor;
        }

        public float GetAccellerationFactor()
        {
            float accelleration = 1;

            foreach (var item in ActiveItems)
            {
                if (item.id == "speedup")
                {
                    accelleration *= 1.5f;
                }
                else if (item.id == "ice_block")
                {
                    accelleration *= 0f;
                }
                else if ((item.id == "lightning"))
                {
                    accelleration *= 0.8f;
                }
            }
            return accelleration;
        }

        public float GetInvertedMovement()
        {
            float invertedMovement = 1;

            foreach (var item in ActiveItems)
            {
                if (item.id == "dizzy_eyes")
                {
                    invertedMovement *= -1f;
                }
            }
            return invertedMovement;
        }

        public bool gravityOn()
        {
            foreach (var item in ActiveItems)
            {
                if (item.id == "position_swap")
                {
                    return false;
                }
            }
            return true;
        }

        public void AddActiveItem(string id, float timeleft, float totTime, Vector2 pos)
        {
            for (var i = 0; i < ActiveItems.Count; i++)
            { 
                if(id == ActiveItems[i].id)
                {
                    ActiveItems[i] = (id, timeleft, totTime, pos);
                    return;
                }
            }
            ActiveItems.Add((id, timeleft, totTime, pos));
        }
    }
}
