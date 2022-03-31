using System;
using Microsoft.Xna.Framework;
namespace Engine
{
    public class MapRenderer:Renderer
    {
        private TileMap tiledM;

        public void LoadFromContent()
        {
            this.tiledM = this.Owner.GetComponent<TileMap>();
            if (tiledM == null)
            {
                throw new ArgumentNullException();
            }
            
        }
        public override void Draw()
        {
            if (this.tiledM == null)
            {
                return;
            }

            tiledM.DrawMap(Color.White, this.Depth);
        }
    }
}
