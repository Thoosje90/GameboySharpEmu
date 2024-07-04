using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator
{
    internal class OpCodesDecoder
    {
        public bool is8BitLoader(byte opcode)
        {
            return Table8BitLoaders().Contains(opcode);
        }

        public bool is16BitLoader(byte opcode)
        {
            return Table16BitLoaders().Contains(opcode);
        }

        public int[] Table8BitLoaders()
        {
            return new int[]
            {
                0x06,
                0x16,
                0x26,
                0x36,
                0xC6,
                0xD6,
                0xE6,
                0xF6,
                0x0E,
                0x1E,
                0x2E,
                0x3E,
                0xCE,
                0xDE,
                0xEE,
                0xFE,
                0xE8,
                0xE,
                0xF,
            };
        }

        public int[] Table16BitLoaders()
        {
            return new int[]
            {
                0x91,
                0x11,
                0x21,
                0x31,
                0xC2,
                0xD2,
                0xC3,
                0xD3,
                0xCA,
                0xDA,
                0xEA,
                0xFA,
                0xCC,
                0xDC,
                0xCD,
            };
        }
    }
}
