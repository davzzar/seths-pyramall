using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Engine
{
    public static class MathUtils
    {
        /// <summary>
        /// Makes the component wise absolute of the given vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 AbsComponents(in this Vector2 vector)
        {
            return new Vector2(MathF.Abs(vector.X), MathF.Abs(vector.Y));
        }

        /// <summary>
        /// Given an axis aligned rectangle defined by <c>min</c> and <c>max</c>, evaluates whether the <c>point</c> is inside.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="min">The minimum of the axis aligned rectangle (Bottom left corner).</param>
        /// <param name="max">The maximum of the axis aligned rectangle (Top right corner).</param>
        /// <returns>Returns <c>true</c> if the point is inside the rect or on its boundary, <c>false</c> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InRectMm(in Vector2 point, in Vector2 min, in Vector2 max)
        {
            return min.X <= point.X && min.Y <= point.Y && point.X <= max.X && point.Y <= max.Y;
        }

        /// <summary>
        /// Given a ray <c>orig + t * dir</c> and an axis aligned rectangle defined by <c>min</c> and <c>max</c>, calculates if and where an intersection occurs.
        /// </summary>
        /// <param name="orig">The starting point of the ray.</param>
        /// <param name="dir">The direction of the ray.</param>
        /// <param name="min">The minimum of the axis aligned rectangle (Bottom left corner).</param>
        /// <param name="max">The maximum of the axis aligned rectangle (Top right corner).</param>
        /// <param name="t">Returns the closest <c>t</c> for which an intersection occurs.</param>
        /// <returns>Returns <c>true</c> if an intersection occurs, <c>false</c> otherwise.</returns>
        public static bool RayRectMmIntersection(in Vector2 orig, in Vector2 dir, in Vector2 min, in Vector2 max, out float t)
        {
            t = float.PositiveInfinity;

            // Left
            if (LineYAxisIntersection(new Vector2(orig.X - min.X, orig.Y), in dir, out var ct, out var a))
            {
                if (0f <= ct && min.Y <= a && a <= max.Y)
                {
                    t = ct;
                }
            }

            // Right
            if (LineYAxisIntersection(new Vector2(orig.X - max.X, orig.Y), in dir, out ct, out a))
            {
                if (0f <= ct && ct < t && min.Y <= a && a <= max.Y)
                {
                    t = ct;
                }
            }

            // Bottom
            if (LineXAxisIntersection(new Vector2(orig.X, orig.Y - min.Y), in dir, out ct, out a))
            {
                if (0f <= ct && ct < t && min.X <= a && a <= max.X)
                {
                    t = ct;
                }
            }

            // Top
            if (LineXAxisIntersection(new Vector2(orig.X, orig.Y - max.Y), in dir, out ct, out a))
            {
                if (0f <= ct && ct < t && min.X <= a && a <= max.X)
                {
                    t = ct;
                }
            }

            return !float.IsPositiveInfinity(t);
        }

        /// <summary>
        /// Given a line <b>orig + t * dir</b>, calculates at which <c>t</c> and <c>y</c> that line intersects with the y axis.
        /// </summary>
        /// <param name="orig">A point on the line.</param>
        /// <param name="dir">The direction of the line.</param>
        /// <param name="t">Returns the value of <c>t</c> at which the intersection occurs.</param>
        /// <param name="y">Returns the <c>y</c> value on the y axis at which the intersection occurs.</param>
        /// <returns>Returns <c>true</c> if the line intersects with the y axis, <c>false</c> otherwise.</returns>
        public static bool LineYAxisIntersection(in Vector2 orig, in Vector2 dir, out float t, out float y)
        {
            if (MathF.Abs(dir.X) <= 1e-5f)
            {
                t = float.PositiveInfinity;
                y = float.PositiveInfinity;
                return false;
            }

            t = -orig.X / dir.X;
            y = orig.Y + dir.Y * t;
            return true;
        }

        /// <summary>
        /// Given a line <b>orig + t * dir</b>, calculates at which <c>t</c> and <c>x</c> that line intersects with the x axis.
        /// </summary>
        /// <param name="orig">A point on the line.</param>
        /// <param name="dir">The direction of the line.</param>
        /// <param name="t">Returns the value of <c>t</c> at which the intersection occurs.</param>
        /// <param name="x">Returns the <c>x</c> value on the x axis at which the intersection occurs.</param>
        /// <returns>Returns <c>true</c> if the line intersects with the x axis, <c>false</c> otherwise.</returns>
        public static bool LineXAxisIntersection(in Vector2 orig, in Vector2 dir, out float t, out float x)
        {
            if (MathF.Abs(dir.Y) <= 1e-5f)
            {
                t = float.PositiveInfinity;
                x = float.PositiveInfinity;
                return false;
            }

            t = -orig.Y / dir.Y;
            x = orig.X + dir.X * t;
            return true;
        }

        /// <summary>
        /// FreyaHolmee/Mathfs implementation of MoveTowards utility function for floats.
        /// Source: https://github.com/FreyaHolmer/Mathfs/blob/master/Mathfs.cs#L783
        public static float MoveTowards(in float current, in float target, in float maxDistanceDelta)
        {
            if (MathF.Abs(target - current) <= maxDistanceDelta)
                return target;
            return current + MathF.Sign(target - current) * maxDistanceDelta;
        }

        /// <summary>
        /// Port of Unity's MoveTowards utility function for Vector2.
        /// Source: https://github.com/Unity-Technologies/UniteAustinTechnicalPresentation/blob/48cbbffc485b7b9fd5d48a861136d60a644c740f/StressTesting/Assets/Scripts/Systems/Jobs/MathUtils.cs#L40
        /// </summary>
        [Obsolete("MoveTowards for 2Vector is deprecated since it may not work in the inverse case.")]
        public static Vector2 MoveTowards(in Vector2 current, in Vector2 target, in float maxDistanceDelta)
        {
            Vector2 delta = target - current;

            float sqrDelta = delta.LengthSquared();
            float sqrMaxDistanceDelta = maxDistanceDelta * maxDistanceDelta;

            if (sqrDelta > sqrMaxDistanceDelta)
            {
                float deltaMagnitude = MathF.Sqrt(sqrDelta);
                if (deltaMagnitude > 0f)
                {
                    return current + delta / deltaMagnitude * maxDistanceDelta;
                }
                return current;
            }
            return target;
        }
    }
}
