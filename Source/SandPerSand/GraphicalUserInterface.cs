using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    public class GraphicalUserInterface : Component
    {
        public static GraphicalUserInterface Instance
        {
            get
            {
                return GameObject.FindComponent<GraphicalUserInterface>();
            }
        }


        private GameObject midscreentext;
        private GuiTextRenderer midScreenTextComp;
        private Dictionary<PlayerIndex, GuiTextRenderer> players;

        private Dictionary<PlayerIndex, Vector2> positions;
        protected override void OnAwake()
        {

            positions = new Dictionary<PlayerIndex, Vector2>{
                { PlayerIndex.One, new Vector2(0.1f, 0.1f) },
                { PlayerIndex.Two, new Vector2(0.9f, 0.1f)},
                { PlayerIndex.Three, new Vector2(0.1f, 0.9f)},
                { PlayerIndex.Four, new Vector2(0.9f, 0.9f)}
            };

            players = new Dictionary<PlayerIndex, GuiTextRenderer>();
            //foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
            //{
            //    players.Add(playerIndex, null);
            //    renderPlayerInfo(playerIndex);
            //}
        }


    public GraphicalUserInterface()
        {

            midscreentext = new GameObject();
            midscreentext.Transform.LocalPosition = new Vector2(0.0f, 0f);
        }


        public void renderStartInfo()
        {
            midScreenTextComp = midscreentext.AddComponent<GuiTextRenderer>();
            midScreenTextComp.PositionMode = GuiTextRenderer.ScreenPositionMode.Relative;
            midScreenTextComp.Text = "To Start The Game press A";
            midScreenTextComp.FontSize = 30f;
            midScreenTextComp.IsActive = true;
            midScreenTextComp.ScreenPosition = new Vector2(0.25f, 0.5f);
            midScreenTextComp.Depth = 0;
        }

        public void destroyStartInfo()
        {
            midScreenTextComp.Destroy();
        }

        public void renderPlayerInfo(PlayerIndex playerIndex)
        {
            var midScreenTextComp = midscreentext.AddComponent<GuiTextRenderer>();
            players[playerIndex] = midscreentext.AddComponent<GuiTextRenderer>();
            players[playerIndex].Text = "Player" + playerIndex;
            players[playerIndex].PositionMode = GuiTextRenderer.ScreenPositionMode.Relative;
            players[playerIndex].ScreenPosition = positions[playerIndex];
            Debug.Print(playerIndex.ToString());
        }

        public void destroyPlayerInfo(PlayerIndex playerIndex)
        {
            players[playerIndex].Destroy();
        }

    }

    struct playerInfo
    {
        public string minor_item;
        public string major_item;
    }
}