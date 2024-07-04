using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware
{
    [Serializable]
    internal class RAM : IHardware
    {
        private ushort _busAddress = 0xC000;
        private ushort _size = 8192;
        private const int _switchRamSize = 4096;

        // Gameboy ram
        byte[] _ram = new byte[8192];
        // Gameboy color ram
        byte[][] _switchram = new byte[8][];

        public ushort BusAddress { get { return _busAddress; } }

        public int Index { get; set; }

        public ushort Size { get { return _size; } }

        public bool ColorMode { get; set; }

        public RAM()
        {
            // Gameboy color ram
            _switchram[0] = new byte[_switchRamSize];
            _switchram[1] = new byte[_switchRamSize];
            _switchram[2] = new byte[_switchRamSize];
            _switchram[3] = new byte[_switchRamSize];
            _switchram[4] = new byte[_switchRamSize];
            _switchram[5] = new byte[_switchRamSize];
            _switchram[6] = new byte[_switchRamSize];
            _switchram[7] = new byte[_switchRamSize];
        }

        public byte Read(ushort address)
        {
            // Echo of internal 8kb ram 
            if (address >= 0xE000 && address <= 0xFDFF)
            {
                address = (ushort)(address - 0x2000);
            }

            if(ColorMode)
            {
                // Read switchable WRAM (GBC Only)
                if (address >= 0xD000 && address >= 0xDFFF)
                {
                    // Convert Bank Index to array index
                    int safeIndex = Index > 0 ? Index - 1 : 0;
                    // Read value from wram bank
                    return _switchram[safeIndex][address - _busAddress];
                }
            }

            // Read value from wram
            return _ram[address - _busAddress];
        }

        public void Write(ushort address, byte value)
        {
            try
            {
                // Echo of internal 8kb ram 
                if (address >= 0xE000 && address <= 0xFDFF)
                {
                    address = (ushort)(address - 0x2000);
                }

                if(ColorMode)
                {
                    // Write switchable WRAM (GBC Only)
                    if (address >= 0xD000 && address >= 0xDFFF)
                    {
                        // Convert Bank Index to array index
                        int safeIndex = Index > 0 ? Index - 1 : 0;
                        // Write value to wram
                        _switchram[safeIndex][address - 0xD000] = value;
                        // break
                        return;
                    }
                }

                // Write value to wram
                _ram[address - BusAddress] = value;
            }
            catch (Exception) { }
        }

        public bool IsWithinRange(ushort address)
        {
            return (address >= BusAddress && address <= 0xDFFF || address >= 0xE000 && address <= 0xFDFF);
        }
    }
}
