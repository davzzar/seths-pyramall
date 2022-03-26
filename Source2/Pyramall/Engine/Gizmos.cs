using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class Gizmos
    {
        private const float GizmosAlpha = 0.5f;

        private static CommandEntry[] data;

        private static int numEntries;

        private static Texture2D pixel;

        internal static int CommandBufferCount => numEntries;

        static Gizmos()
        {
            numEntries = 0;
            data = new CommandEntry[32];
        }

        public static void DrawLine(in Vector2 point1, in Vector2 point2, Color color)
        {
            // calculate the distance between the two vectors
            float distance = Vector2.Distance(point1, point2);

            // calculate the rotation between the two vectors
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            DrawLine(in point1, distance, angle, color);
        }

        public static void DrawLine(in Vector2 from, in float distance, float rotation, Color color)
        {
            GrowBufferOnDemand();
            
            //color *= GizmosAlpha;
            data[numEntries] = new CommandEntry(CommandType.Line, in color, in from, in rotation, new Vector2(distance, 1f));
            numEntries++;
        }

        public static void DrawRect(in Vector2 center, in Vector2 size, float rotation, Color color)
        {
            throw new NotImplementedException();
        }

        public static void FillRect(in Vector2 center, in Vector2 size, float rotation, Color color)
        {
            GrowBufferOnDemand();

            color *= GizmosAlpha;
            var pos = center + new Vector2(-size.X, size.Y) / 2f;
            data[numEntries] = new CommandEntry(CommandType.Rect, in color, in pos, in rotation, in size);
            numEntries++;
        }

        internal static void OnRender()
        {
            InitRenderCache();

            var thicknessScale = Graphics.CurrentCamera.Height / Graphics.GraphicsDevice.Viewport.Height;

            for (var i = 0; i < numEntries; i++)
            {
                ref readonly var entry = ref data[i];
                Matrix3x3 matrix;
                Debug.Assert(entry.Type != CommandType.None);

                switch (entry.Type)
                {
                    case CommandType.Line:
                        var scale = entry.Scale;
                        scale.Y *= thicknessScale;
                        matrix = Matrix3x3.CreateTRS(in entry.Position, in entry.Rotation, in scale);
                        break;
                    case CommandType.Rect:
                        matrix = Matrix3x3.CreateTRS(in entry.Position, in entry.Rotation, in entry.Scale);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Graphics.Draw(pixel, entry.Color, ref matrix, 0f);
            }

            numEntries = 0;
        }

        private static void InitRenderCache()
        {
            if (pixel == null || pixel.IsDisposed)
            {
                pixel = new Texture2D(Graphics.GraphicsDevice, 1, 1);
                pixel.Name = "Pixel";
                pixel.SetData(new[] {Color.White});
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GrowBufferOnDemand()
        {
            if (data.Length <= numEntries)
            {
                var newData = new CommandEntry[data.Length * 2];
                data.CopyTo(newData, 0);
                data = newData;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct CommandEntry
        {
            public readonly CommandType Type;

            public readonly Color Color;

            public readonly Vector2 Position;

            public readonly float Rotation;

            public readonly Vector2 Scale;

            public CommandEntry(CommandType type, in Color color, in Vector2 position, in float rotation, in Vector2 scale)
            {
                this.Type = type;
                this.Color = color;
                this.Position = position;
                this.Rotation = rotation;
                this.Scale = scale;
            }
        }

        private enum CommandType
        {
            None = 0,
            Line = 1,
            Rect = 2
        }
    }
}
