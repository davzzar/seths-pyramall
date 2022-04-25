using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace Engine
{
    public readonly struct LayerMask
    {
        public static readonly LayerMask None = new LayerMask(0);

        public static readonly LayerMask All = new LayerMask(~0);

        public readonly int Value;

        public LayerMask(int mask)
        {
            this.Value = mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasLayer(int layer)
        {
            if (layer < 0 || layer > 31)
            {
                return false;
            }

            return (this.Value & (1 << layer)) != 0;
        }

        public static implicit operator int(LayerMask layerMask)
        {
            return layerMask.Value;
        }

        public static implicit operator LayerMask(int mask)
        {
            return new LayerMask(mask);
        }

        public static LayerMask operator | (LayerMask left, LayerMask right)
        {
            return new LayerMask(left.Value | right.Value);
        }

        public static LayerMask operator & (LayerMask left, LayerMask right)
        {
            return new LayerMask(left.Value & right.Value);
        }

        public static LayerMask FromLayers(int layer0)
        {
            if (layer0 < 0 || layer0 > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(layer0));
            }

            return new LayerMask(1 << layer0);
        }

        public static LayerMask FromLayers(int layer0, int layer1)
        {
            if (layer0 < 0 || layer0 > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(layer0));
            }

            if (layer1 < 0 || layer1 > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(layer1));
            }

            return new LayerMask((1 << layer0) | (1 << layer1));
        }

        public static LayerMask FromLayers(int layer0, int layer1, int layer2)
        {
            if (layer0 < 0 || layer0 > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(layer0));
            }

            if (layer1 < 0 || layer1 > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(layer1));
            }

            if (layer2 < 0 || layer2 > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(layer2));
            }

            return new LayerMask((1 << layer0) | (1 << layer1) | (1 << layer2));
        }

        public static LayerMask FromLayers(params int[] layers)
        {
            var value = 0;

            for (var i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];

                if (layer < 0 || layer > 31)
                {
                    throw new ArgumentOutOfRangeException(nameof(layers));
                }

                value |= 1 << layer;
            }

            return new LayerMask(value);
        }
    }
}
