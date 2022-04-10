using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using SandPerSand.SandSim;

namespace SandPerSand
{
    public sealed class SandInteractionTest : Behaviour
    {
        private SpriteRenderer renderer;

        private SandSimulation sandSimulation;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            if (this.renderer == null)
            {
                this.renderer = this.Owner.GetComponent<SpriteRenderer>();
            }

            if (this.sandSimulation == null)
            {
                this.sandSimulation = GameObject.FindComponent<SandSimulation>();
            }
        }

        /// <inheritdoc />
        protected override void Update()
        {
            if (this.renderer == null || this.sandSimulation == null) 
            {
                Debug.Print("The renderer or sand simulation was not found.");
                return;
            }

            var shape = new Circle(this.Transform.Position, this.Transform.Scale.X / 2f);
            var color = Color.White;

            if (this.sandSimulation.SandData.IsTouchingSand(in shape))
            {
                color = Color.Red;
            }

            this.renderer.Color = color;
        }
    }
}
