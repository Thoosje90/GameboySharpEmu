using GameboyEmulator.Events;
using GameboyEmulator.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware
{
    internal class Timers
    {
        // Request interypt event handler
        public event InteruptEventHandler? RequestInterupt;

        // Address buss
        private AddressBus16Bit addressBus16Bit;
        private const int DMG_DIV_FREQ = 256;

        public Timers(AddressBus16Bit bus16Bit)
        {
            addressBus16Bit = bus16Bit;
        }

        #region Timers

        // Fake timer variables
        int timerCounter = 0;
        int divCounter = 0;

        internal void Tick(int cycles)
        { 
            // Handle DIV counting register
            HandleDivider(cycles);
            // Handle timer interuptions
            HandleTimer(cycles);
        }

        private void HandleDivider(int cycles)
        {
            // Increment fake devider varibale
            divCounter += cycles;
            
            //
            if (divCounter >= DMG_DIV_FREQ)
            {
                // INCREMENT DIV REGISTER
                addressBus16Bit.Memory[0xFF04]++;
                // Reset counter
                divCounter -= DMG_DIV_FREQ;
            }
        }

        private void HandleTimer(int cycles)
        {
            byte timerAtts = addressBus16Bit.Read(0xFF07);

            if ((timerAtts & 0x4) != 0)
            {
                // Increment fake timer variable
                timerCounter += cycles;

                // Current clockspeed of the timer
                int clockSpeed = 0;
                // Get current clockspeed index
                byte timerVal = (byte)(timerAtts & 0x03);

                // Set clock speed
                switch (timerVal)
                {
                    case 0: clockSpeed = 1024; break;
                    case 1: clockSpeed = 16; break;
                    case 2: clockSpeed = 64; break;
                    case 3: clockSpeed = 256; break;
                    default: break;
                }

                // Increment the timer
                if (timerCounter >= clockSpeed)
                {
                    // INCREMENT TIMA
                    addressBus16Bit.Memory[0xFF05]++;
                    timerCounter -= clockSpeed;
                }

                // Request the interupt
                if (addressBus16Bit.Read(0xFF05) == 0xFF)
                {
                    // Requestion timer interupt
                    Interupts.RequestInterupt(addressBus16Bit, 2);
                    // DEVIDE TIMA
                    addressBus16Bit.Memory[0xFF05] = addressBus16Bit.Read(0xFF06);
                }
            }
        }

        #endregion

    }
}
