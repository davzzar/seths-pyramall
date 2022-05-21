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
        private RealGameStateManager GSM;
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

        public float ShopTime { get; private set; } = Conf.Time.ShopTime;
        public GoTimer ShopTimer { get; private set; } = null;
        public int CurRank { get; private set; }

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

        private void DefPrepare()
        {
            GSM.GetState<PrepareState>().OnEnter += (sender, lastState) =>
            {
                SetAllPlayerControls(false);
            };
            GSM.GetState<PrepareState>().OnUpdate += () =>
            {
                CheckConnections();
            };

        }
        private void DefPreRound()
        {
            bool isEntered = false;
            GSM.GetState<PreRoundState>().OnEnter += (sender, lastState) =>
            {
                foreach (var player in Players.Values)
                {
                    PlayerUtils.UnhidePlayer(player);
                    player.GetComponent<PlayerComponent>()!.IsAlive = true;
                }
                SetAllPlayerControls(false);
                if (lastState.GetType() == typeof(PrepareState))
                {
                    isEntered = true;
                }
                else
                {
                    isEntered = false;
                }
                    
            };
            GSM.GetState<PreRoundState>().OnUpdate += () =>
            {
                if (!isEntered)
                {
                    isEntered = EnterRoundStartCountdownFromShopOrRoundCheck();
                }
                else
                {
                    //Do real update stuff
                }
            };
        }

        private void DefInRound()
        {
            GSM.GetState<InRoundState>().OnEnter += (sender, lastState) =>
            {
                SetAllPlayerControls(true);
            };

        }
        private void DefRoundCheck()
        {
            GSM.GetState<RoundCheckState>().OnEnter += (sender, lastState) => {
                FinalizeRanks();
                // hide all players
                foreach (var item in Players)
                {
                    PlayerUtils.HidePlayer(item.Value);
                    Debug.Print("Player " + item.Key + " : Rank " +
                        item.Value.GetComponent<PlayerStates>().RoundRank);
                }
            };
            GSM.GetState<RoundCheckState>().OnExit += ( ) => {
                // clear initial positions when exit a round
                InitialPositions = new List<Vector2>();
            };
            
        }

        public List<PlayerIndex> ShopQueue;
        public bool AllFinishedShop => ShopQueue.All(playerIndex => Players[playerIndex].GetComponent<PlayerStates>()!.FinishedShop);
        private void DefInShop()
        {
            ShopQueue = new List<PlayerIndex>();
            bool isEntered = false;

            InShopState inShop = GSM.GetState<InShopState>();
            inShop.OnEnter += (sender,preState) => {
                isEntered = false;
            };
            inShop.OnUpdate += () => {
                if (!isEntered)
                {
                    isEntered = EnterShop();
                }
                else
                {
                    DuringShop();
                }
            };
            inShop.OnExit += () => {
                GoTimer[] timerList = Owner.GetComponents<GoTimer>();
                foreach (var timer in timerList)
                {
                    timer.Destroy();
                }
                ShopQueue = new List<PlayerIndex>();
                // ShopEntryPsition must be reset after use
                ShopEntryPosition = Vector2.Zero;
            };
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            GSM = GameObject.FindComponent<RealGameStateManager>();
            DefPrepare();
            DefPreRound();
            DefInRound();
            DefRoundCheck();
            DefInShop();
        }

        protected override void Update()
        {
        }

        private void DuringShop()
        {
            bool ItsCurrentPlayersTurn =
    CurRank == 0 || CurRank < ShopQueue.Count &&
    Players[ShopQueue[CurRank - 1]].GetComponent<PlayerStates>().FinishedShop;
            if (ItsCurrentPlayersTurn)
            {
                var curPlayer = Players[ShopQueue[CurRank]];
                var curPlayerState = curPlayer.GetComponent<PlayerStates>();
                //Debug.Assert(curPlayerState.FinishedShop == false);
                PlayerUtils.ResumePlayerControl(curPlayer);
                // init new timer
                ShopTimer = Owner.AddComponent<GoTimer>();
                // bug only canceled the last timer
                ShopTimer.Init(ShopTime,
                    () => {
                        curPlayerState.FinishedShop = true;
                        PlayerUtils.HidePlayer(curPlayer);
                    });

                CurRank++;
            }
        }

        private bool EnterShop()
        {
            // wait for shop map
            if (ShopEntryPosition.X < 4)
            {
                Debug.Print("Cannot respawn Player in the shop, " +
                    "because initial position is not yet loaded or invalid.");
                return false;
            }
            Debug.Print("PM: -> Shop");
            // respawn players -> queue by rank list
            var entryX = ShopEntryPosition.X;
            foreach (var playerIndex in ShopQueue)
            {
                RespawnPlayer(playerIndex, new Vector2(entryX--, ShopEntryPosition.Y));
                // disable all players controller
                PlayerUtils.ShieldPlayerControl(Players[playerIndex]);
            }
            CurRank = 0;
            return true;
        }

        private bool EnterRoundStartCountdownFromShopOrRoundCheck()
        {
            // make sure initial positions are loaded from the new level
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
                players[playerIndex].GetComponent<RigidBody>()!.IsKinematic = false;
                players[playerIndex].Transform.Position = position;
                players[playerIndex].GetComponent<SpriteRenderer>()!.IsActive = true;
                players[playerIndex].GetComponent<Animator>().Entry();
                players[playerIndex].GetComponent<PlayerControlComponent>().IsActive = true;
                players[playerIndex].GetComponent<PlayerComponent>().IsAlive = true;
                players[playerIndex].GetComponent<PlayerComponent>()!.AddCameraControlPoint();
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
        private bool finishedShop;
        public bool FinishedShop
        {
            get => finishedShop;
            set
            {
                finishedShop = value;
            }
        }
        public int RoundRank { get; set; }
        public int Score { get; set; }
        public InputHandler InputHandler { get; set; }
        private bool PrepareButtonPressed => InputHandler.getButtonState(Buttons.A) == ButtonState.Pressed;
        public GameState LastGameState{ get; set; }
        public Collider Collider { get; set; }

        public List<Items.Item> ActiveItems { private set; get; }
        public List<Items.Item> PursueItems { private set; get; }

        protected override void OnAwake()
        {
            Prepared = false;
            Paused = false;
            MinorItem = null;
            MajorItem = null;
            Coins = 0;
            Exited = false;
            FinishedShop = false;
            RoundRank = -1;
            ActiveItems = new List<Items.Item>();
            PursueItems = new List<Items.Item>();
            Score = 0;

            var realGSM = GameObject.FindComponent<RealGameStateManager>();
            realGSM.GetState<InShopState>().OnExit += () =>
            {
                FinishedShop = false;
            };
            realGSM.GetState<PreRoundState>().OnEnter += (sender,preState) =>
            {
                Exited = false;
                RoundRank = -1;
            };
            realGSM.GetState<PrepareState>().OnUpdate += () =>
            {
                if (PrepareButtonPressed)
                {
                    TogglePrepared();
                }
            };
        }

        /// <summary>
        /// The update handles state transitions.
        /// </summary>
        protected override void Update()
        {
            var timeDelta = Time.DeltaTime;

            var remove = new List<int>();

            bool lightning = false;

            for (var i = 0; i < PursueItems.Count; i++)
            {
                if (PursueItems[i].Id == ItemId.position_swap)
                {
                    if (((Items.PositionSwapItem)PursueItems[i]).ExchangeTimePassed < ((Items.PositionSwapItem)PursueItems[i]).ExchangeTime)
                    {
                        float fraction = ((Items.PositionSwapItem)PursueItems[i]).ExchangeTimePassed - ((Items.PositionSwapItem)PursueItems[i]).ExchangeTime;
                        fraction = fraction * fraction;
                        PursueItems[i].Position = PursueItems[i].Position * fraction + (1 - fraction) * this.Transform.Position;
                        ((Items.PositionSwapItem)PursueItems[i]).ExchangeTimePassed += timeDelta;
                        Collider.IsActive = false;
                    }
                    else
                    {
                        PursueItems[i].Delete = true;
                        Collider.IsActive = true;
                    }
                }
                else
                {
                    if ((PursueItems[i].Position - this.Transform.Position).LengthSquared() < 0.1f)
                    {
                        PursueItems[i].pursue = false;
                    }
                    else
                    {
                        var vel = (PursueItems[i].Position - this.Transform.Position) / (PursueItems[i].Position - this.Transform.Position).Length();
                        PursueItems[i].Position = PursueItems[i].Position * 0.9f + 0.1f * this.Transform.Position - vel / 10;
                    }
                }
             }

            for (var i = PursueItems.Count - 1; i >= 0; i--)
            {
                if (PursueItems[i].Delete)
                {
                    PursueItems.RemoveAt(i);
                }
                else if (!PursueItems[i].pursue)
                {
                    for (var j = 0; j < ActiveItems.Count; i++)
                    {
                        if (PursueItems[j].Id == ActiveItems[j].Id)
                        {
                            ActiveItems[j].TimeLeft = PursueItems[j].TotTime;
                            continue;
                        }
                    }
                    PursueItems.RemoveAt(i);
                }
            }

            for (var i = ActiveItems.Count - 1; i >= 0; i--)
            {
                ActiveItems[i].TimeLeft = ActiveItems[i].TimeLeft - timeDelta;

                if(ActiveItems[i].TimeLeft < 0)
                {
                    ActiveItems[i].Delete = true;
                }

                if (ActiveItems[i].Id == ItemId.lightning)
                {
                    lightning = true;
                }
                if (ActiveItems[i].Delete)
                {
                    ActiveItems.RemoveAt(i);
                }
            }

            //if (lightning)
            //{
            //    Collider.Owner.GetComponentInParents<PlayerComponent>().Transform.LossyScale = Vector2.One * 0.8f;
            //    Collider.IsActive = false;
            //    Collider.IsActive = true;
            //}
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
                MajorItem = item;
                return true;
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
                if(item.Id == ItemId.wings) 
                {
                    jumpfactor *= 2f;
                }
                else if(item.Id == ItemId.ice_block)
                {
                    jumpfactor *= 0;
                }
                else if ((item.Id == ItemId.lightning))
                {
                    jumpfactor *= 0.5f;
                }
            }
            return jumpfactor;
        }

        public float GetAccellerationFactor()
        {
            float accelleration = 1;

            foreach (var item in ActiveItems)
            {
                if (item.Id == ItemId.speedup)
                {
                    accelleration *= 1.5f;
                }
                else if (item.Id == ItemId.ice_block)
                {
                    accelleration *= 0f;
                }
                else if ((item.Id == ItemId.lightning))
                {
                    accelleration *= 0.5f;
                }
            }
            return accelleration;
        }

        public float GetInvertedMovement()
        {
            float invertedMovement = 1;

            foreach (var item in ActiveItems)
            {
                if (item.Id == ItemId.dizzy_eyes)
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
                if (item.Id == ItemId.position_swap)
                {
                    return false;
                }
            }
            return true;
        }

        public void AddActiveItem(Items.Item newItem)
        {
            if (newItem.pursue)
            {
                PursueItems.Add(newItem);
            }
            else
            {
                for (var i = 0; i < ActiveItems.Count; i++)
                {
                    if (newItem.Id == ActiveItems[i].Id)
                    {
                        ActiveItems[i].TimeLeft = newItem.TotTime;
                        return;
                    }
                }
                ActiveItems.Add(newItem);
            }
        }
    }
}
