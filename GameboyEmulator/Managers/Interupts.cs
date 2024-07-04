using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameboyEmulator.Hardware;

namespace GameboyEmulator.Managers
{
    internal class Interupts
    {
        // Interupt enabled address
        private static ushort IE_ADDRESS = 0xFFFF;
        // Interupt register flag address
        private static ushort IF_ADDRESS = 0xFF0F;

        #region Interupts

        internal static void DoInterupts(CPU ZILOG64, AddressBus16Bit ADDRESSBUS)
        {
            // Wut this address loll
            byte IE = ADDRESSBUS.Read(IE_ADDRESS);
            // Get interupt request flag
            byte IF = ADDRESSBUS.Read(IF_ADDRESS);

            // Check each bit in requestFlag
            for (int bit = 0; bit < 5; bit++)
            {
                if ((((IE & IF) >> bit) & 0x1) == 1)
                {
                    // If bit is set then do service interupt
                    ServiceInterupt(ZILOG64, ADDRESSBUS, bit);
                }
            }

            // Update IME
            ZILOG64.UpdateIME();
        }

        internal static void ServiceInterupt(CPU ZILOG64, AddressBus16Bit ADDRESSBUS, int bit)
        {
            // Reset halt cpu DONT TRISY THIS
            if(ZILOG64.HALTED)
            {
                ZILOG64.CpuRegisters.PC++;
                ZILOG64.HALTED = false;
            }

            if (ZILOG64.STOPPED && bit == 4)
            {
                ZILOG64.CpuRegisters.PC++;
                ZILOG64.HALTED = false;
            }

            if (ZILOG64.IME)
            {
                // Push program counter on memory stack
                ZILOG64.PUSH(ZILOG64.CpuRegisters.PC);

                // Set new program counter
                switch (bit)
                {
                    case 0: ZILOG64.CpuRegisters.PC = 0x40; break;
                    case 1: ZILOG64.CpuRegisters.PC = 0x48; break;
                    case 2: ZILOG64.CpuRegisters.PC = 0x50; break;
                    case 4: ZILOG64.CpuRegisters.PC = 0x60; break;
                }

                ZILOG64.IME = false;

                // Reset bit back to zero (dont all trust this)
                ADDRESSBUS.Memory[IF_ADDRESS] = BitHelper.ResetBit(ADDRESSBUS.Memory[IF_ADDRESS], bit);
            }
        }

        internal static void RequestInterupt(AddressBus16Bit ADDRESSBUS, int bit)
        {
            // Read Interupt Request Flag from AddressBus
            byte reqeustFlag = ADDRESSBUS.Memory[IF_ADDRESS];
            // Set a requestFlag bit at Index
            reqeustFlag = BitHelper.SetBit(reqeustFlag, bit);
            // Update Request flag
            ADDRESSBUS.Memory[IF_ADDRESS] = reqeustFlag;
        }

        #endregion
    }
}
