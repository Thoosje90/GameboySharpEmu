using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware
{
    [Serializable]
    internal class VRAM : IHardware
    {
        private const ushort _busAddress = 0x8000;
        private const ushort _size = 8192;

        byte[] _vram = new byte[8192];

        public ushort BusAddress { get { return _busAddress; } }


        public byte Read(ushort address)
        {
            return _vram[address - BusAddress];
        }

        public void Write(ushort address, byte value)
        {
            _vram[address - BusAddress] = value;
        }

        public bool IsWithinRange(ushort address)
        {
            return (address >= BusAddress && address <= 0x9FFF);
        }

    }
}
