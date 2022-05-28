using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Engine;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace SandPerSand
{
    public class MainMenu : Component
    {
        protected override void OnAwake()
        {
            Template.InitializeMenu();
            Template.ShowMainMenu();
            
            UI.IsMouseVisible = true;
        }
    }
}
