using Engine;

namespace SandPerSand
{
    public class EntryScript : Behaviour
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            PlayersManager.Instance.InitialPositions.Add(this.Owner.Transform.Position);
            this.Owner.Destroy();
        }
    }
}