using GameboyEmulator.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware
{
    [Serializable]
    internal class JoyPad : IHardware
    {
        
        //
        private const byte PAD_MASK = 0x10;
        private const byte BUTTON_MASK = 0x20;

        private Keyboard _keyboard;
        private AddressBus16Bit _addressBus16Bit;

        public JoyPad(AddressBus16Bit addressBus16Bit)
        {
            _addressBus16Bit = addressBus16Bit;
            _keyboard = new Keyboard();
        }

        #region JoyPad

        // JoyPad & Button States
        private static byte m_JoypadState = 0xF; 
        private static byte m_ButtonState = 0xF; 

        public ushort BusAddress { get { return 0xFF00; } }

        // end fake heeader

        internal void handleKeyDown(KeyEventArgs e)
        {
            byte b = (byte)_keyboard.GetKeyMask(e);

            if ((b & PAD_MASK) == PAD_MASK)
            {
                m_JoypadState = (byte)(m_JoypadState & ~(b & 0xF));
            }
            else if ((b & BUTTON_MASK) == BUTTON_MASK)
            {
                m_ButtonState = (byte)(m_ButtonState & ~(b & 0xF));
            }
        }

        internal void handleKeyUp(KeyEventArgs e)
        {
            //byte b = GetKeyBit(e);
            byte b = (byte)_keyboard.GetKeyMask(e);

            if ((b & PAD_MASK) == PAD_MASK)
            {
                m_JoypadState = (byte)(m_JoypadState | (b & 0xF));
            }
            else if ((b & BUTTON_MASK) == BUTTON_MASK)
            {
                m_ButtonState = (byte)(m_ButtonState | (b & 0xF));
            }
        }

        internal void HandleInput()
        {
            GetJoypadState();
        }

        private void GetJoypadState()
        {
            // this function CANNOT call ReadMemory(0xFF00) it must access it directly from m_Rom[0xFF00]
            // because ReadMemory traps this address

            byte JOYPAD_STATE = _addressBus16Bit.Memory[0xFF00];
            bool joypadSelected = !BitHelper.IsBitSet(JOYPAD_STATE, 4); // 0 = selected
            bool buttonsSelected = !BitHelper.IsBitSet(JOYPAD_STATE, 5); // 0 = selected

            if (joypadSelected)
            {
                // Write new joypad state
                byte joypad = (byte)((JOYPAD_STATE & 0xF0) | m_JoypadState);
                Write(0, joypad);

                // Request joypad interupt
                if (m_JoypadState != 0xf)
                    Interupts.RequestInterupt(_addressBus16Bit, 4);
            }
            
            if (buttonsSelected)
            {
                // Write new joypad state
                byte buttons = (byte)((JOYPAD_STATE & 0xF0) | m_ButtonState);
                Write(0, buttons);

                // Request joypad interupt
                if (m_ButtonState != 0xf)
                    Interupts.RequestInterupt(_addressBus16Bit, 4);
            }

            // Reset if joypad and buttons are not pressed
            if (!joypadSelected && !buttonsSelected)
                Write(0, 0xFF);
        }

        public byte Read(ushort address)
        {
            return _addressBus16Bit.Memory[0xFF00];
        }

        public void Write(ushort address, byte value)
        {
            _addressBus16Bit.Memory[0xFF00] = value;
        }

        public bool IsWithinRange(ushort address)
        {
            return (address == BusAddress);
        }

        #endregion
    }
}
