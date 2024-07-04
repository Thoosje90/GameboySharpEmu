using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware
{
    public interface IHardware
    {
        ushort BusAddress { get; }

        //ushort Size { get; }

        byte Read(ushort address);

        void Write(ushort address, byte value);

        bool IsWithinRange(ushort address);
    }

    public class AddressBus16Bit
    {
        // Gameboy hardware
        private List<IHardware> _hardware;
        // 16 bit address bus
        byte[] _memoryBus = new byte[65536];


        //Timer IO Regs
        public byte DIV { get { return Memory[0xFF04]; } set { Memory[0xFF04] = value; } } //FF04 - DIV - Divider Register (R/W)
        public byte TIMA { get { return Memory[0xFF05]; } set { Memory[0xFF05] = value; } } //FF05 - TIMA - Timer counter (R/W)
        public byte TMA { get { return Memory[0xFF06]; } set { Memory[0xFF06] = value; } } //FF06 - TMA - Timer Modulo (R/W)
        public byte TAC { get { return Memory[0xFF07]; } set { Memory[0xFF07] = value; } } //FF07 - TAC - Timer Control (R/W)
        public bool TAC_ENABLED { get { return (Memory[0xFF07] & 0x4) != 0; } } // Check if byte 2 is 1
        public byte TAC_FREQ { get { return (byte)(Memory[0xFF07] & 0x3); } } // returns byte 0 and 1

        //

        public byte LCDC { get { return Memory[0xFF40]; } }//FF40 - LCDC - LCD Control (R/W)
        public byte STAT { get { return Memory[0xFF41]; } set { Memory[0xFF41] = value; } }//FF41 - STAT - LCDC Status (R/W)

        public byte SCY { get { return Memory[0xFF42]; } }//FF42 - SCY - Scroll Y (R/W)
        public byte SCX { get { return Memory[0xFF43]; } }//FF43 - SCX - Scroll X (R/W)
        public byte LY { get { return Memory[0xFF44]; } set { Memory[0x44] = value; } }//FF44 - LY - LCDC Y-Coordinate (R) bypasses on write always 0
        public byte LYC { get { return Memory[0xFF45]; } }//FF45 - LYC - LY Compare(R/W)
        public byte WY { get { return Memory[0xFF4A]; } }//FF4A - WY - Window Y Position (R/W)
        public byte WX { get { return Memory[0xFF4B]; } }//FF4B - WX - Window X Position minus 7 (R/W)

        public byte BGP { get { return Memory[0xFF47]; } }//FF47 - BGP - BG Palette Data(R/W) - Non CGB Mode Only
        public byte OBP0 { get { return Memory[0xFF48]; } }//FF48 - OBP0 - Object Palette 0 Data (R/W) - Non CGB Mode Only
        public byte OBP1 { get { return Memory[0xFF49]; } }//FF49 - OBP1 - Object Palette 1 Data (R/W) - Non CGB Mode Only


        //
        // GAMEBOY COLOR
        //


        public byte BCPS { get { return Memory[0xFF68]; } }//FF47 - BGP - BG Palette Data(R/W) - Non CGB Mode Only
        public byte BCPD { get { return Memory[0xFF69]; } }//FF48 - OBP0 - Object Palette 0 Data (R/W) - Non CGB Mode Only
        public byte OCPS { get { return Memory[0xFF6A]; } }//FF49 - OBP1 - Object Palette 1 Data (R/W) - Non CGB Mode Only
        public byte OCPD { get { return Memory[0xFF6B]; } }//FF49 - OBP1 - Object Palette 1 Data (R/W) - Non CGB Mode Only

        //
        //
        public byte VRAM_BANK { get { return Memory[0xFF4F]; } }

        //
        internal CPU cpu { get; set; }

        public List<IHardware> @ASSIGN 
        { 
            set { _hardware = value; } 
            get { return _hardware; } 
        }

        public byte[] Memory 
        { 
            get { return _memoryBus; } 
        }

        public AddressBus16Bit()
        {
            _hardware = new List<IHardware>();
        }

        public ushort Read16bit(ushort address)
        {
            byte lsb = Read(address);
            byte msb = Read((ushort)(address+1));
            return (ushort)((msb << 8)| lsb);
        }

        public byte Read(ushort address)
        {
            //// Read from other bus addresses
            int len = _hardware.Count;
            int i = 0;
            //////foreach (IHardware hardware in _hardware)
            //for (i = 0; i < len; i++)
            //{
            //    // Read from Hardware assigned to this address
            //    if (_hardware[i].IsWithinRange(address))
            //    {
            //        return _hardware[i].Read(address);
            //    }
            //}

            if (address <= 0x7FFF)
            {
                return _hardware[3].Read(address);
                // return;
            }
            else if (address <= 0x9FFF)
            {
                return _hardware[1].Read(address);
                //   return;
            }
            else if (address <= 0xBFFF)
            {
                return _hardware[3].Read(address);
                //  return;
            }
            else if (address <= 0xFDFF)
            {
                return _hardware[0].Read(address);
                //  return;
            }
            else if (address == 0xFF00)
            {
                return _hardware[2].Read(address);
                // return;
            }
            //else if (address <= 0xFFFF)
            //{
            //    return _memoryBus[address];
            //    // return;
            //}

            // Prevent writing to unused regions
            // (this line shouldn't be here if you're emulator didnt' suck)
            if (unusedRegion(address))
            {
                // Return no data
                return 0;
            }

            // Return data from memory bus that shouldn't hold any data 
            // We'll fix that XD
            return _memoryBus[address]; ;

        }


        /// <summary>
        /// Write byte value into 16bit address bus
        /// this is not being used which means you fucked yourself
        /// </summary>
        /// <param name="sp">StackPointer</param>
        /// <param name="value">Byte Value</param>
        /// 
        public void WriteWord(ushort addr, ushort w)
        {
            Write((ushort)(addr + 1), (byte)(w >> 8));
            Write(addr, (byte)w);
        }

        int writeCount = 0;

        public void Write(ushort address, byte value)
        {
            // DMA TRANSFER (EDIT FOR GBC)
            if (address == 0xFF46)
            {
                if(_memoryBus[0xFF44] >= 135)
                    DoDMATransfer(value);

                return;
            }
            // reset the divider register (dont trust this)
            else if (address == 0xFF04)
            {
                _memoryBus[0xFF04] = 0;
                return;
            }
            // reset scanline (dont trust this)
            else if (address == 0xFF44)
            {
                _memoryBus[0xFF44] = 0;
                return;
            }
            //
            // reset IF (Interupt Flag) (dont trust this)
            else if (address == 0xFF0F)
            {
                _memoryBus[0xFF0F] = (value |= 0xE0);
                return;
            }

            // NANI XD
            if (address <= 0x7FFF)
            {
                _hardware[3].Write(address, value);
                return;
            }
            else if (address <= 0x9FFF)
            {
                _hardware[1].Write(address, value);
                return;
            }
            else if (address <= 0xBFFF)
            {
                _hardware[3].Write(address, value);
                return;
            }
            else if (address <= 0xFDFF)
            {
                _hardware[0].Write(address, value);
                return;
            }
            else if (address == 0xFF00)
            {
                _hardware[2].Write(address, value);
                return;
            }

            // Prevent writing to unused regions
            // (this line shouldn't be here if you're emulator didnt' suck)
            if (!unusedRegion(address))
            {
                // Write to memory bus that shouldn't have any values lol
                _memoryBus[address] = value;
            }
        }

        void DoDMATransfer(byte data)
        {
            // Get new address.
            // Source address is data * 100
            ushort newAddress = (ushort)(data << 8); 

            for (int i = 0; i < 0xA0; i++)
            {
                // Get data from new address
                byte newValue = Read((ushort)(newAddress + i));
                //Debug.WriteLine("DMA#" + dmaCount + " DMA TRANSFER INdEX: " + i + " Value: " +  + newValue);
                // Write data to Sprite Attribute Table (OAM)
                Write((ushort)(0xFE00 + i), newValue);
            }

            Memory[0xFF46] = data;
        }

        private bool unusedRegion(uint address)
        {
            return ((address >= 0xFEA0 && address <= 0xFEFF));
        }
    }
}
