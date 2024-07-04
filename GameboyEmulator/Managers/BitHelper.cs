using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator
{
    internal class BitHelper
    {
        public static bool IsFlagSet(byte reg, int flag)
        {
            return (reg & flag) != 0;
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsBitSet(ushort b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }


        public static byte SetBit(byte a, int pos)
        {
            return (byte)(a | (1 << pos));
        }


        public static byte ResetBit(byte a, int pos)
        {
            // Clear 5th bit
            return  (byte)(a & ~(1 << pos));
        }
    }
}
