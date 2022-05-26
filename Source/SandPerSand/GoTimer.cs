using Engine;
using System;
using System.Diagnostics;
namespace SandPerSand
{
    public delegate void GoDelegate(GameObject playerGo);
    public delegate void VoidDelegate();
    public delegate bool CancelIf();

    public class GoTimer : Behaviour
    {
        public float CountDown { get; protected set; }
        public GoDelegate GD { get; protected set; }
        public VoidDelegate VD { get; protected set; }

        protected override void Update()
        {
            if (CountDown <= 0)
            {
                Fire();
                if (IsAlive)
                {
                    Destroy();
                }
                return;
            }
            CountDown -= Time.DeltaTime;
        }

        public void Init()
        {
            if (GD != null || VD != null)
            {
                throw new InvalidOperationException();
            }
        }
        public void Init(float time, GoDelegate gd)
        {
            Init();
            CountDown = time;
            GD = gd;
        }

        public void Init(float time, VoidDelegate vd)
        {
            Init();
            CountDown = time;
            VD = vd;
        }

        public void Fire()
        {
            if (GD != null) GD(Owner);
            if (VD != null) VD();
        }
    }

    public class CancelableTimer : GoTimer
    {
        public CancelIf Cancel { get; protected set; }

        protected override void Update()
        {
            if (Cancel != null && Cancel())
            {
                Debug.Assert(IsAlive);
                Destroy();
            }
            base.Update();
        }

        public void Init(float time, VoidDelegate vd, CancelIf ci)
        {
            base.Init();
            CountDown = time;
            VD = vd;
            Cancel = ci;
        }

    }

    public class InStateTimer : GoTimer
    {
        public GameState LastGameState { get; set; }
        public GameState CurrentGameState => GameStateManager.Instance.CurrentState;

        protected override void Update()
        {
            if (LastGameState != CurrentGameState)
            {
                Debug.Assert(IsAlive);
                Destroy();
            }
            base.Update();
        }
    }
}
