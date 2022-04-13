using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SandPerSand
{
    public class TracerRendererComponent : Behaviour
    {
        public float TraceThinckness { get; set; } = 3f;
        public Color TraceColor { get; set; } = Color.White;
        public int TraceLength { get; set; } = 25;
        private Queue<Vector2> traceQueue;

        public TracerRendererComponent()
        {
            traceQueue = new Queue<Vector2>(TraceLength);

        }
        protected override void Update()
        {
            traceQueue.Enqueue(this.Transform.Position);

            // dequeue if capacity is reached...
            if (traceQueue.Count > TraceLength) traceQueue.Dequeue();

            DrawTrace();
        }

        private void DrawTrace()
        {
            bool hasFrom = false;
            Vector2 from = Vector2.Zero; // hack for the intellisense
            foreach (Vector2 point in traceQueue)
            {
                if (hasFrom)
                {
                    Gizmos.DrawLine(point, from, TraceColor, TraceThinckness);
                }
                from = point;
                hasFrom = true;
            }
        }
    }
}
