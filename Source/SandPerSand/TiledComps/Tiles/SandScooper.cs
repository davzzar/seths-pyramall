using Engine;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;
using System.ComponentModel;
namespace SandPerSand
{
    public class SandScooper : Behaviour
    {
        public float Radius { get; set; } = 1.5f;
        public float Interval { get; set; } = 3f;
        private float couter = 0f;
        // Sand Interaction
        private SandSimulation sandSimulation;
        public SandSimulation SandSimulation
        {
            get
            {
                if (this.sandSimulation == null || !this.sandSimulation.IsAlive)
                {
                    this.sandSimulation = GameObject.FindComponent<SandSimulation>();

                }
                return sandSimulation;
            }
        }

        protected override void Update()
        {
            couter += Time.DeltaTime;
            if (couter > Interval)
            {
                couter = 0f;
                Circle circle = new Circle(Transform.Position, Radius);
                SandSimulation.RemoveSand(circle);
            }
        }

    }
}
