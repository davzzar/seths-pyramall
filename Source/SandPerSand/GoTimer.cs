using Engine;

namespace SandPerSand
{
    public delegate void GoDelegate(GameObject playerGo);

    public class GoTimer : Behaviour
    {
        public float Timer { get; private set; }
        public GoDelegate Delegate;

        protected override void Update()
        {
            Timer -= Time.DeltaTime;
            if (Timer <= 0)
            {
                Fire();
                Destroy();
            }
        }

        public void Init(float timer, GoDelegate gd)
        {
            Timer = timer;
            Delegate = gd;
        }

        public void Fire()
        {
            Delegate(Owner);
        }
    }
}
