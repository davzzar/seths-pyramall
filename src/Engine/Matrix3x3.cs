using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Engine
{
    public struct Matrix3x3
    {
        public static readonly Matrix3x3 Identity = new Matrix3x3(1, 0, 0, 0, 1, 0, 0, 0, 1);

        public static readonly Matrix3x3 Zero = new Matrix3x3(0);

        public static readonly Matrix3x3 One = new Matrix3x3(1);

        public Vector3 R1, R2, R3;

        public Vector3 C1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new Vector3(this.R1.X, this.R2.X, this.R3.X);
            set
            {
                this.R1.X = value.X;
                this.R2.X = value.Y;
                this.R3.X = value.Z;
            }
        }

        public Vector3 C2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new Vector3(this.R1.Y, this.R2.Y, this.R3.Y);
            set
            {
                this.R1.Y = value.X;
                this.R2.Y = value.Y;
                this.R3.Y = value.Z;
            }
        }

        public Vector3 C3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new Vector3(this.R1.Z, this.R2.Z, this.R3.Z);
            set
            {
                this.R1.Z = value.X;
                this.R2.Z = value.Y;
                this.R3.Z = value.Z;
            }
        }

        public float M11
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R1.X;
            set => this.R1.X = value;
        }

        public float M12
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R1.Y;
            set => this.R1.Y = value;
        }

        public float M13
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R1.Z;
            set => this.R1.Z = value;
        }

        public float M21
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R2.X;
            set => this.R2.X = value;
        }

        public float M22
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R2.Y;
            set => this.R2.Y = value;
        }

        public float M23
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R2.Z;
            set => this.R2.Z = value;
        }

        public float M31
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R3.X;
            set => this.R3.X = value;
        }

        public float M32
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R3.Y;
            set => this.R3.Y = value;
        }

        public float M33
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this.R3.Z;
            set => this.R3.Z = value;
        }

        public Matrix3x3(float value)
        {
            this.R1 = new Vector3(value);
            this.R2 = new Vector3(value);
            this.R3 = new Vector3(value);
        }

        public Matrix3x3(in Vector3 r1, in Vector3 r2, in Vector3 r3)
        {
            this.R1 = r1;
            this.R2 = r2;
            this.R3 = r3;
        }

        public Matrix3x3(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32,
            float m33)
        {
            this.R1 = new Vector3(m11, m12, m13);
            this.R2 = new Vector3(m21, m22, m23);
            this.R3 = new Vector3(m31, m32, m33);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix3x3 Adjoint()
        {
            return this.Cofactor().Transpose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix3x3 Cofactor()
        {
            return new Matrix3x3
            (
                +(this.M22 * this.M33 - this.M23 * this.M32),
                -(this.M21 * this.M33 - this.M23 * this.M31),
                -(this.M21 * this.M32 - this.M22 * this.M31),
                -(this.M12 * this.M33 - this.M13 * this.M32),
                +(this.M11 * this.M33 - this.M13 * this.M31),
                -(this.M11 * this.M32 - this.M12 * this.M31),
                +(this.M12 * this.M23 - this.M13 * this.M22),
                -(this.M11 * this.M23 - this.M13 * this.M21),
                +(this.M11 * this.M22 - this.M12 * this.M21));
        }

        public readonly void DecomposeTRS(out Vector2 position, out float radians, out Vector2 scale)
        {
            position = new Vector2(this.M13, this.M23);

            radians = (float)Math.Atan2(this.M21, this.M11);

            var scaleX = (float)Math.Sqrt(this.M11 * this.M11 + this.M21 * this.M21);
            var scaleY = (float)Math.Sqrt(this.M12 * this.M12 + this.M22 * this.M22);
            scale = new Vector2(scaleX, scaleY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Determinant()
        {
            return
                this.M11 * (this.M22 * this.M33 - this.M23 * this.M32) -
                this.M12 * (this.M21 * this.M33 - this.M23 * this.M31) +
                this.M13 * (this.M21 * this.M32 - this.M22 * this.M31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix3x3 Invert()
        {
            var det = this.Determinant();

            if (Math.Abs(det) < 1e-5f)
            {
                throw new InvalidOperationException("The matrix is not invertible.");
            }

            return this.Adjoint() / det;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 TransformPoint(in Vector2 p)
        {
            var p3 = new Vector3(p.X, p.Y, 1);
            var result = this * p3;
            return new Vector2(result.X, result.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 TransformDirection(in Vector2 p)
        {
            var p3 = new Vector3(p.X, p.Y, 0);
            var result = this * p3;
            return new Vector2(result.X, result.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix3x3 Transpose()
        {
            return new Matrix3x3(this.M11, this.M21, this.M31, this.M12, this.M22, this.M32, this.M13, this.M23,
                this.M33);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 CreateRotation(float radians)
        {
            var sin = (float) Math.Sin(radians);
            var cos = (float) Math.Cos(radians);
            return new Matrix3x3(
                cos, -sin, 0,
                sin, cos, 0,
                0, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 CreateScale(float scale)
        {
            return new Matrix3x3(
                scale, 0, 0,
                0, scale, 0,
                0, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 CreateScale(in Vector2 scale)
        {
            return new Matrix3x3(
                scale.X, 0, 0,
                0, scale.Y, 0,
                0, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 CreateTranslation(in Vector2 position)
        {
            return new Matrix3x3(
                1, 0, position.X,
                0, 1, position.Y,
                0, 0, 1);
        }

        public static Matrix3x3 CreateTRS(in Vector2 position, float radians, in Vector2 scale)
        {
            return CreateTranslation(position) * CreateRotation(radians) * CreateScale(scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator +(in Matrix3x3 left, in Matrix3x3 right)
        {
            return new Matrix3x3(left.R1 + right.R1, left.R2 + right.R2, left.R3 + right.R3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator -(in Matrix3x3 left, in Matrix3x3 right)
        {
            return new Matrix3x3(left.R1 - right.R1, left.R2 - right.R2, left.R3 - right.R3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator *(in Matrix3x3 left, in Matrix3x3 right)
        {
            return new Matrix3x3(
                Vector3.Dot(left.R1, right.C1),
                Vector3.Dot(left.R1, right.C2),
                Vector3.Dot(left.R1, right.C3),
                Vector3.Dot(left.R2, right.C1),
                Vector3.Dot(left.R2, right.C2),
                Vector3.Dot(left.R2, right.C3),
                Vector3.Dot(left.R3, right.C1),
                Vector3.Dot(left.R3, right.C2),
                Vector3.Dot(left.R3, right.C3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator *(in Matrix3x3 left, float right)
        {
            return new Matrix3x3(left.R1 * right, left.R2 * right, left.R3 * right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator *(float left, in Matrix3x3 right)
        {
            return new Matrix3x3(left * right.R1, left * right.R2, left * right.R3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Matrix3x3 left, in Vector3 right)
        {
            return new Vector3(
                Vector3.Dot(left.R1, right),
                Vector3.Dot(left.R2, right), 
                Vector3.Dot(left.R3, right));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 left, in Matrix3x3 right)
        {
            return new Vector3(
                Vector3.Dot(left, right.C1),
                Vector3.Dot(left, right.C2),
                Vector3.Dot(left, right.C3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator /(in Matrix3x3 left, float right)
        {
            return new Matrix3x3(left.R1 / right, left.R2 / right, left.R3 / right);
        }
    }
}
