using Engine;
using System;

namespace SandPerSand
{
    public delegate void GoDelegate(GameObject playerGo);
    public delegate void VoidDelegate();

    public class GoTimer : Behaviour
    {
        public float Timer { get; private set; }
        public GoDelegate GD { get; private set; }
        public VoidDelegate VD { get; private set; }

        protected override void Update()
        {
            if (Timer <= 0)
            {
                Fire();
                Destroy();
            }
            Timer -= Time.DeltaTime;
        }

        public void Init()
        {
            if (GD != null|| VD != null)
            {
                throw new InvalidOperationException();
            }
        }
        public void Init(float timer, GoDelegate gd)
        {
            Init();
            Timer = timer;
            GD = gd;
        }

        public void Init(float timer, VoidDelegate vd)
        {
            Init();
            Timer = timer;
            VD = vd;
        }

        public void Fire()
        {
            if (GD != null) GD(Owner);
            if (VD != null) VD();
        }
    }

}
