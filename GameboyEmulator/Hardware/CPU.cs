using System;
using System.Collections.Generic;
using System.Diagnostics;
using GameboyEmulator.Hardware;
using static GameboyEmulator.Hardware.enums.AsmEnums;

namespace GameboyEmulator
{

    interface IGamePak
    {
        byte ReadLoROM(ushort addr);
        byte ReadHiROM(ushort addr);
        void WriteROM(ushort addr, byte value);
        byte ReadERAM(ushort addr);
        void WriteERAM(ushort addr, byte value);
        void Init(byte[] ROM);
    }

    internal class CPU
    {
        CpuRegister registers;

        private AddressBus16Bit bus16Bit;

        public CpuRegister CpuRegisters { get { return registers; } }

        public AddressBus16Bit AddressBus { get { return bus16Bit; } set { bus16Bit = value; } }

        private bool _ime = false;

        public bool IME { get { return _ime; } set { _ime = value; } }

        public bool IME_ENABLER { get; set; }

        public bool HALTED { get; set; }

        public bool STOPPED { get; set; }

        public bool HALT_BUG { get; set; }

        // Interupts Enabled
        public byte IE { get { return bus16Bit.Memory[0xFFFF]; } set { bus16Bit.Memory[0xFFFF] = value; } }

        // Interup flag register
        public byte IF { get { return bus16Bit.Memory[0xFF0F]; } set { bus16Bit.Memory[0xFF0F] = value; } }

        public CPU()
        {
            registers = new CpuRegister();
        }

        #region Assembly Instructions

        public void ADD(Parameters p2)
        {
            // Execute CPU instruction
            ADD(Parameters.A, registers.GetValue(p2));
        }

        public void ADD(Parameters p, byte p2)
        {
            // Calculate ADD result
            byte b1 = registers.A;
            int result = b1 + p2;

            // Load result into register
            LD(Parameters.A, (byte)result);

            // SET Z FLAG
            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // Set N Flag
            RES((byte)FLAGS.N, Parameters.F);

            // Set H Flag
            if (SetFlagH(b1, p2))
                SET((byte)FLAGS.H, Parameters.F);
            else
                RES((byte)FLAGS.H, Parameters.F);

            // Set C Flag
            if (SetFlagC(result))
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);
        }

        public void ADD(Parameters p, ushort p2) 
        {
            //
            ushort b1;
            // Get parameter values from cpu register (dont trust this)
            registers.GetValue(p, out b1);

            // Calculate Add Result
            int result = b1 + p2;

            // Write result value into cpu register
            LD(p, (ushort)result);

            // SET N FLAG
            RES((byte)FLAGS.N, Parameters.F);

            // Set C Flag
            if (result >> 16 != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            // Not Sure about H (flag)
            // Neither where the docs lol.
            if (SetFlagH(b1, p2))
                SET((byte)FLAGS.H, Parameters.F);
            else
                RES((byte)FLAGS.H, Parameters.F);

        }

        public void SUB(byte value) 
        {
            // Get values
            byte b1 = registers.A;
            byte b2 = value;

            // Calculate SUB result
            int result = b1 - b2;

            // Load result value into register
            LD(Parameters.A, (byte)result);

            // Set Z Flag
            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // Update flags
            SET((byte)FLAGS.N, Parameters.F);

            // Set C Flag
            if (SetFlagC(result))
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            // Set H Flag
            if (SetFlagHSub(b1,  b2))
                SET((byte)FLAGS.H, Parameters.F);
            else
                RES((byte)FLAGS.H, Parameters.F);
        }

        public void ADC(Parameters p2) 
        {
            ADC(registers.GetValue(p2));
        }

        public void ADC(byte p2) 
        {
            // Retrieve carry bit
            int bit = BitHelper.IsFlagSet(registers.F, (byte)FLAGS.C) ? 1 : 0;

            // Calculate result
            byte b1 = registers.A;
            int result = b1 + p2 + bit;

            // Load result value into CPU register
            LD(Parameters.A, (byte)result);

            // Set Z Flag
            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // Set N Flag
            RES((byte)FLAGS.N, Parameters.F);

            // Set H Flag
            if (bit == 1)
            {
                if (SetFlagHCarry(b1, p2))
                    SET((byte)FLAGS.H, Parameters.F);
                else
                    RES((byte)FLAGS.H, Parameters.F);
            }
            else
            {
                if (SetFlagH(b1, p2))
                    SET((byte)FLAGS.H, Parameters.F);
                else
                    RES((byte)FLAGS.H, Parameters.F);
            }

            // Set C Flag
            if (SetFlagC(result))
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);
        }

        public void SBC(Parameters p, byte p2)
        {
            // Retrieve Carry bit
            int bit = BitHelper.IsFlagSet(registers.F, (byte)FLAGS.C) ? 1 : 0;

            // Substract
            int result = registers.A - p2 - bit;
 
            // Set ZFlag
            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // Set N Flag
            SET((byte)FLAGS.N, Parameters.F);

            // Set H Flag
            if (SetFlagHSubCarry(registers.A, p2))
                SET((byte)FLAGS.H, Parameters.F);
            else
                RES((byte)FLAGS.H, Parameters.F);

            // Set C FLag
            if (SetFlagC(result))
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            // Load result into CPU register
            LD(Parameters.A, (byte)result);
        }

        public void AND(byte p)
        {
            // AND registers.A REGISTER WITH P AND LOAD RESULT INTO A REGISTSER
            LD(Parameters.A, (byte)(registers.A & p));
            // Compares a with value and sets z flag
            CP((byte)0);
            // Reset flags (might be lethal if you don't)
            RES((byte)FLAGS.N, Parameters.F);
            SET((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.C, Parameters.F);
        }

        public void OR(byte n)
        {
            // AND A REGISTER WITH P AND LOAD RESULT INTO A REGISTSER
            LD(Parameters.A, (byte)(registers.A| n));
            // Compares a with value and sets z flag
            CP((byte)0);
            // Reset flags (might be lethal if you don't)
            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.C, Parameters.F);
        }

        public void XOR(byte n)
        {
            //
            LD(Parameters.A, (byte)(registers.GetValue(Parameters.A) ^ n));
            // Compares a with value and sets z flag
            CP((byte)0);
            // Reset flags (might be lethal if you don't)
            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.C, Parameters.F);
        }

        public void CP(byte p)
        {
            int result = (registers.A- p);

            // Set ZFLAG
            if ((byte)result == 0)
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // Set N Flag
            SET((byte)FLAGS.N, Parameters.F);

            // SET H FLag (if no borrow from fourth bit)
            if (SetFlagHSub(registers.A, p))
                SET((byte)FLAGS.H, Parameters.F);
            else
                RES((byte)FLAGS.H, Parameters.F);

            // Set C FLag
            if (registers.A < p)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);
        }

        public void INC(Parameters p)
        {
            switch(p)
            {
                case Parameters.SP:
                    registers.SP +=1;
                    return;
                case Parameters.BC:
                    registers.BC += 1;
                    return;
                case  Parameters.DE:
                    registers.DE += 1;
                    return;
                case  Parameters.AF:
                    registers.AF += 1;
                    return;
                case Parameters.HL:
                    registers.HL += 1;
                    return;
            }
                
            byte b1 = registers.GetValue(p);

            // byte b1, b1Copy;
            // Get parameter values from cpu register
            int result = b1 + 1;

            // Write to memory
            LD(p, (byte)result);

            // Set Z FLAG
            if ((byte)result == 0)
            {
                SET((byte)FLAGS.Z, Parameters.F);
            }
            else
            {
                RES((byte)FLAGS.Z, Parameters.F);
            }

            // SET H FLAG
            if (SetFlagH(b1, 1))
            {
                SET((byte)FLAGS.H, Parameters.F);
            }
            else
            {
                RES((byte)FLAGS.H, Parameters.F);
            }

            // Reset flags
            RES((byte)FLAGS.N, Parameters.F);

        }

        public void INC(ushort p)
        {
            // Massive sin
            ushort address = p;
            byte b1 = bus16Bit.Read(address);
            int result = b1 + 1;
            // Write to memory
            bus16Bit.Write(address, (byte)result);

            // Set Z FLAG
            if ((byte)result == 0)
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // SET H FLAG
            if (SetFlagH(b1, 1))
                SET((byte)FLAGS.H, Parameters.F);
            else
                RES((byte)FLAGS.H, Parameters.F);

            // Reset flags
            RES((byte)FLAGS.N, Parameters.F);
        }

        public void DEC(Parameters p)
        {
            byte b1;

            switch (p)
            {
                case Parameters.SP:
                    registers.SP -= 1;
                    return;
                case Parameters.BC:
                    registers.BC -= 1;
                    return;
                case Parameters.DE:
                    registers.DE -= 1;
                    return;
                case Parameters.AF:
                    registers.AF -= 1;
                    return;
                case Parameters.HL:
                    registers.HL -= 1;
                    return;
            }

            // Get parameter values from cpu register
            b1 = registers.GetValue(p);

            int result = b1 - 1;

            if (SetFlagHSub(b1, 1))
                SET((byte)FLAGS.H, Parameters.F);
            else
                RES((byte)FLAGS.H, Parameters.F);

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            SET((byte)FLAGS.N, Parameters.F);

            LD(p, (byte)result);

        }

        public void DEC(ushort p)
        {

            ushort address = p;

    
            byte b1 = bus16Bit.Read(address);
            int result = b1 - 1;
            // Write to memory
            bus16Bit.Write(address, (byte)result);

            // Set Z FLAG
            if ((byte)result == 0)
            {
                SET((byte)FLAGS.Z, Parameters.F);
            }
            else
            {
                RES((byte)FLAGS.Z, Parameters.F);
            }

            // SET H FLAG 
            if (SetFlagHSub(b1, 1))
            {
                SET((byte)FLAGS.H, Parameters.F);
            }
            else
            {
                RES((byte)FLAGS.H, Parameters.F);
            }

            // Reset flags
            SET((byte)FLAGS.N, Parameters.F);

        }

        public void SWAP(Parameters p)
        {
            byte val = registers.GetValue(p);
            byte high = (byte)(val >> 4);
            byte low = (byte)(val & 0x0f);
            byte swap = (byte)((low << 4) | high);
            // Update flags (dun work)
            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.C, Parameters.F);

            // Bitwise Complement all bytes  in p
            LD(p, swap);

            //// Bitwise Complement all bytes  in p
            //LD(p, (BitReverseTable[registers.GetValue(p)]));

            if (swap == 0)
            {
                SET((byte)FLAGS.Z, Parameters.F);
            }
            else
            {
                RES((byte)FLAGS.Z, Parameters.F);
            }
        }

        public void SWAP(ushort p)
        {
            byte val = bus16Bit.Read(p);
            byte high = (byte)(val >> 4);
            byte low = (byte)(val & 0x0f);
            byte swap = (byte)((low << 4) | high);


            // Update flags (dun work)
            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.C, Parameters.F);

            //// Bitwise Complement all bytes  in p (sinn sinnn sinnn)
            //LD(p, (BitReverseTable[bus16Bit.Read(p)]));

            // Bitwise Complement all bytes  in p (sinn sinnn sinnn)
            LD(p, swap);

            if (swap == 0)
            {
                SET((byte)FLAGS.Z, Parameters.F);
            }
            else
            {
                RES((byte)FLAGS.Z, Parameters.F);
            }
        }

        public void LD(Parameters p, byte n)
        {
            registers.SetValue(p, n);
        }

        public void LD(Parameters p, ushort nn)
        {
            registers.SetValue(p, nn);
        }

        public void LD(ushort nn, Parameters p)
        {
            bus16Bit.Write(nn, registers.GetValue(p));
        }

        public void LD(ushort nn, byte p)
        {
            bus16Bit.Write(nn, p);
        }

        public void LDI()
        {
            LD(Parameters.A, bus16Bit.Read(registers.HL));
            INC(Parameters.HL);
        }

        public void LDD()
        {
            LD(registers.HL, Parameters.A);
            DEC(Parameters.HL);
        }

        public void DAA() 
        {
            bool flagN = BitHelper.IsFlagSet(registers.F, (byte)FLAGS.N);
            bool FlagC = BitHelper.IsFlagSet(registers.F, (byte)FLAGS.C);
            bool flagH = BitHelper.IsFlagSet(registers.F, (byte)FLAGS.H);

            // note: assumes a is a uint8_t and wraps from 0xff to 0
            if (!flagN)
            {   
                // After addition, adjust if (half-)carry occurred or if result is out of bounds
                if (FlagC || registers.A> 0x99) // 0x9F
                {
                    registers.A+= 0x60;
                    // SET C FLAG
                    SET((byte)FLAGS.C, Parameters.F);
                }

                // Set H Flag
                if (flagH || (registers.A& 0x0f) > 0x09)
                {
                    registers.A += 0x6;
                }
            }
            else
            {  // After subtraction, only adjust if (half-)carry occurred
                if (FlagC)
                    registers.A -= 0x60;
                if (flagH)
                    registers.A -= 0x6;
            }

            // Set Z Flag
            if (registers.A== 0)
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // Reset flags
            RES((byte)FLAGS.H, Parameters.F);
        }

        public void SCF()
        {
            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            SET((byte)FLAGS.C, Parameters.F);
        }

        public void CPL()
        {
            // Update flags
            SET((byte)FLAGS.N, Parameters.F);
            SET((byte)FLAGS.H, Parameters.F);
            //A ^= 0xFF;
            registers.A= (byte)(~registers.A);
        }

        public void CCF()
        {
            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            if (BitHelper.IsFlagSet(registers.F, (byte)FLAGS.C))
                RES((byte)FLAGS.C, Parameters.F);
            else
                SET((byte)FLAGS.C, Parameters.F);
        }

        public void NOP()
        {
            // No Operation
        }

        public void DI()
        {
            // DISABLES INTERUPS NANI
            _ime = false;
        }

        public void EI()
        {
            // ENABLE INTERUPTS
            IME_ENABLER = true;
        }

        #region Rotate & shifting

        public void RLCA()
        {
            //RLC(Parameters.A);

            byte chunk = registers.GetValue(Parameters.A);
            byte result = (byte)((chunk << 1) | (chunk >> 7));

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x80) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            LD(Parameters.A, result);
        }

        public void RRCA() // 
        {
            //RRC(Parameters.A);

            byte chunk = registers.GetValue(Parameters.A);
            byte result = (byte)((chunk >> 1) | (chunk << 7));

            // Rotate register A right
            LD(Parameters.A, result);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.Z, Parameters.F);
        }

        public void RLA()
        {
            //RL(Parameters.A);

            byte chunk = registers.GetValue(Parameters.A);
            bool isCarrySet = BitHelper.IsFlagSet(registers.GetValue(Parameters.F), (byte)FLAGS.C);
            byte result = (byte)((chunk << 1) | (isCarrySet ? 1 : 0));

            if ((chunk & 0x80) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.Z, Parameters.F);

            LD(Parameters.A, result);
        }

        public void RRA()
        {
            // Rotate register A right ( SHOULD NOT HAVE Z FLAG)
            //RR(Parameters.A);

            byte chunk = registers.GetValue(Parameters.A);
            bool isCarrySet = BitHelper.IsFlagSet(registers.GetValue(Parameters.F), (byte)FLAGS.C);
            byte result = (byte)((chunk >> 1) | (isCarrySet ? 0x80 : 0));

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);
            RES((byte)FLAGS.Z, Parameters.F);

            LD(Parameters.A, result);
        }

        public void RLC(Parameters p)
        {
            //RLC(Parameters.A);
            byte chunk = registers.GetValue(p);
            byte result = (byte)((chunk << 1) | (chunk >> 7));

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            if ((chunk & 0x80) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            LD(p, result);
        }

        public void RLC(ushort p)
        {
            byte chunk = bus16Bit.Read(p);
            byte result = (byte)((chunk << 1) | (chunk >> 7));

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            if ((chunk & 0x80) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            LD(p, result);
        }

        public void RL(Parameters p)
        {
            //
            byte chunk = registers.GetValue(p);
            
            //
            bool isCarrySet = BitHelper.IsFlagSet(registers.GetValue(Parameters.F), (byte)FLAGS.C);
            byte result = (byte)((chunk << 1) | (isCarrySet ? 1 : 0));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x80) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);

        }

        public void RL(ushort p)
        {
            byte chunk = bus16Bit.Read(p);

            bool isCarrySet = BitHelper.IsFlagSet(registers.GetValue(Parameters.F), (byte)FLAGS.C);

            byte result = (byte)((chunk << 1) | (isCarrySet ? 1 : 0));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x80) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);

        }

        public void RRC(Parameters p)
        {
            byte chunk = registers.GetValue(p);
            byte result = (byte)((chunk >> 1) | (chunk << 7));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);
        }

        public void RRC(ushort p)
        {
            byte chunk = bus16Bit.Read(p);
            byte result = (byte)((chunk >> 1) | (chunk << 7));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);
        }

        public void RR(Parameters p)
        {
            byte chunk = registers.GetValue(p);
            bool isCarrySet = BitHelper.IsFlagSet(registers.GetValue(Parameters.F), (byte)FLAGS.C);

            byte result = (byte)((chunk >> 1) | (isCarrySet ? 0x80 : 0));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);
        }

        public void RR(ushort p) // YEA DO A SAFE VESION FIST LOL
        {
            byte chunk = bus16Bit.Read(p);

            bool isCarrySet = BitHelper.IsFlagSet(registers.GetValue(Parameters.F), (byte)FLAGS.C);

            byte result = (byte)((chunk >> 1) | (isCarrySet ? 0x80 : 0));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);
        }

        public void SLA(Parameters p)
        {
            byte chunk = registers.GetValue(p);
            byte result = (byte)((chunk << 1));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x80) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);

        }

        public void SLA(ushort p)
        {
            byte chunk = bus16Bit.Read(p);
            byte result = (byte)((chunk << 1));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x80) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);

        }

        public void SRA(Parameters p) // UNSURE UNSURE UNSURE
        {
            byte chunk = registers.GetValue(p);
            byte result = (byte)((chunk >> 1) | (chunk & 0x80));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);

        }

        public void SRA(ushort p) // GARBAGE FOR NOW
        {
            byte chunk = bus16Bit.Read(p);
            byte result = (byte)((chunk >> 1) | (chunk & 0x80));

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            LD(p, result);
        }

        public void SRL(Parameters p) // NOT GOOD
        {
            byte chunk = registers.GetValue(p);
            byte result = (byte)(chunk >> 1);

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            LD(p, result);
        }

        public void SRL(ushort p) // NOT GOOD
        {
            byte chunk = bus16Bit.Read(p);
            byte result = (byte)(chunk >> 1);

            if (SetFlagZ(result))
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            RES((byte)FLAGS.N, Parameters.F);
            RES((byte)FLAGS.H, Parameters.F);

            if ((chunk & 0x1) != 0)
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            LD(p, result);

        }

        #endregion

        public void BIT(byte b, Parameters p2)
        {
            // Set Z Flag
            if ((registers.GetValue(p2) & b) == 0)
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // Update flags
            RES((byte)FLAGS.N, Parameters.F);
            SET((byte)FLAGS.H, Parameters.F);
        }

        public void BIT(byte b, ushort address)
        {
            // Set Z Flag
            if ((address & b) == 0)
                SET((byte)FLAGS.Z, Parameters.F);
            else
                RES((byte)FLAGS.Z, Parameters.F);

            // Update flags
            RES((byte)FLAGS.N, Parameters.F);
            SET((byte)FLAGS.H, Parameters.F);
        }

        public void SET(byte b, Parameters p2)
        {
            LD(p2, (byte)(registers.GetValue(p2) | b));
        }

        public void SET(byte b, ushort address)
        {
            LD(address, (byte)(bus16Bit.Read(address) | b));
        }

        public byte SETr(byte b, ushort address)
        {
            return (byte)(bus16Bit.Read(address) | b);
        }

        public void RES(byte b, Parameters p2) 
        {
            LD(p2, (byte)(registers.GetValue(p2) & ~b));
        }

        public byte RESr(int b, ushort address) 
        {
            return (byte)(bus16Bit.Read(address) & ~b);
            //LD(address, (byte)((bus16Bit.Read(address) & ~(1 << b)) | (((0 & 1) << b) & (1 << b))));
        }

        public void JP(ushort nn)
        {
            LD(Parameters.PC, nn);
        }

        public bool JP(FLAGS f, ushort nn) // unifinished
        {
            if (f == FLAGS.NZ && !BitHelper.IsFlagSet(registers.F, (int)FLAGS.Z))
            {
                // Create address
                JP(nn);
                return true;
            }

            if (f == FLAGS.Z && BitHelper.IsFlagSet(registers.F, (int)FLAGS.Z))
            {
                // Create address
                JP(nn);
                return true;
            }

            if (f == FLAGS.NC && !BitHelper.IsFlagSet(registers.F, (int)FLAGS.C))
            {
                // Create address
                JP(nn);
                return true;
            }

            if (f == FLAGS.C && BitHelper.IsFlagSet(registers.F, (int)FLAGS.C))
            {
                // Create address
                JP(nn);
                return true;
            }

            return false;
        }

        public void JR(byte n)
        {
            unsafe
            {
                sbyte wtf = (sbyte)n;
                JP((ushort)(registers.PC + wtf));
            }
        }

        public bool JR(FLAGS f, byte n)
        {

            sbyte wtf = (sbyte)n;

            if (f == FLAGS.NZ && !BitHelper.IsFlagSet(registers.F, (int)FLAGS.Z))
            {
                // Create address
                JP((ushort)(registers.PC + wtf));
                registers.PC++;
                return true;
            }

            if (f == FLAGS.Z && BitHelper.IsFlagSet(registers.F, (int)FLAGS.Z))
            {
                // Create address
                JP((ushort)(registers.PC + wtf));
                registers.PC++;
                return true;
            }

            if (f == FLAGS.NC && !BitHelper.IsFlagSet(registers.F, (int)FLAGS.C))
            {
                // Create address
                JP((ushort)(registers.PC + wtf));
                registers.PC++;
                return true;
            }

            if (f == FLAGS.C && BitHelper.IsFlagSet(registers.F, (int)FLAGS.C))
            {
                // Create address
                JP((ushort)(registers.PC + wtf));
                registers.PC++;
                return true;
            }

            registers.PC++;
            return false;
        }

        public bool CALL(FLAGS f, ushort nn)
        {

            if (f == FLAGS.NZ && !BitHelper.IsFlagSet(registers.F , (int)FLAGS.Z))
            {
                // Create address
                PUSH(registers.PC);
                JP(nn);
                return true;
            }

            if (f == FLAGS.Z && BitHelper.IsFlagSet(registers.F , (int)FLAGS.Z))
            {
                // Create address
                PUSH(registers.PC);
                JP(nn);
                return true;
            }

            if (f == FLAGS.NC && !BitHelper.IsFlagSet(registers.F, (int)FLAGS.C))
            {
                // Create address
                PUSH(registers.PC);
                JP(nn);
                return true;
            }

            if (f == FLAGS.C && BitHelper.IsFlagSet(registers.F, (int)FLAGS.C))
            {
                // Create address
                PUSH(registers.PC);
                JP(nn);
                return true;
            }

            return false;

        }

        public void CALL(ushort nn)
        {
            PUSH(registers.PC);
            JP(nn);
        }

        public void PUSH(ushort nn)
        {
            // Get lsb and msb from SP register
            byte lsb = (byte)(nn & 0xFFu);
            byte msb = (byte)((nn >> 8) & 0xFFu);

            // Swap to little endian
            DEC(Parameters.SP);
            LD((ushort)(registers.SP), msb);
            DEC(Parameters.SP);
            LD((ushort)(registers.SP), lsb);
        }

        public void debug(int cycles, byte opcode)
        {

            //if (dev >= 494176)
            //{

                //if (dev >= 23440108 /*&& PC == 0x35A*/)
                Debug.WriteLine("Cycle " + cycles + "\n PC " + (registers.PC - 1).ToString("x4") + "\n Stack: " + registers.SP.ToString("x4") + "\n AF: " + registers.A.ToString("x2") + "" + registers.F.ToString("x2")
                    + "\n BC: " + registers.B.ToString("x2") + "" + registers.C.ToString("x2") + "\n DE: " + registers.D.ToString("x2") + "" + registers.E.ToString("x2") + "\n HL: " + registers.H.ToString("x2") + "" + registers.L.ToString("x2")
                    + "\n opcode " + opcode.ToString("x2") + "\n D16 " + bus16Bit.Read16bit(registers.PC).ToString("x4"));

                Debug.WriteLine("Opcode Executed");
            // }

            // Debug.WriteLine("Dev Cycles Executed=" + dev);
        }

        public void POP(Parameters p)
        {
            ushort popValue = bus16Bit.Read16bit(registers.SP);
            // Load two bytes from stack into register pair wut
            LD(p, popValue);
            // Increment stack
            INC(Parameters.SP);
            INC(Parameters.SP);
        }

        public void RST(byte nn) // Dont trust this
        {
            PUSH(registers.PC);
            JP(nn);
        }

        public void RET()
        {
            POP(Parameters.PC);
        }

        public bool RET(FLAGS p)
        {
            bool flagSet = false;

            // Update flags
            if (p == FLAGS.C && BitHelper.IsFlagSet(registers.F, (byte)FLAGS.C) == true)
                flagSet = true;
            if (p == FLAGS.NC && BitHelper.IsFlagSet(registers.F, (byte)FLAGS.C) == false)
                flagSet = true;
            if (p == FLAGS.Z && BitHelper.IsFlagSet(registers.F, (byte)FLAGS.Z) == true)
                flagSet = true;
            if (p == FLAGS.NZ && BitHelper.IsFlagSet(registers.F, (byte)FLAGS.Z) == false)
                flagSet = true;

            if (flagSet)
            {
                RET();
                return true;
            }

            return false;
        }

        public void RETI()
        {
            RET();
            _ime = true;
        }

        #endregion

        #region Cpu Control

        public void ResetCPU()
        {
            // Load registers with default value
            registers.AF = 0x01B0;
            registers.BC = 0x0013;
            registers.DE = 0x00D8;
            registers.HL = 0x014D;
            registers.SP = 0xFFFE;
            registers.PC = 0x100;

            // Set default values (wutt)
            bus16Bit.Memory[0xFF00] = 0xF;
            bus16Bit.Write(0xFF05, 0x00);
            bus16Bit.Write(0xFF06, 0x00);
            bus16Bit.Write(0xFF07, 0x00);
            bus16Bit.Write(0xFF10, 0x80);
            bus16Bit.Write(0xFF11, 0xBF);
            bus16Bit.Write(0xFF12, 0xF3);
            bus16Bit.Write(0xFF14, 0xBF);
            bus16Bit.Write(0xFF16, 0x3F);
            bus16Bit.Write(0xFF17, 0x00);
            bus16Bit.Write(0xFF19, 0xBF);
            bus16Bit.Write(0xFF1A, 0x7F);
            bus16Bit.Write(0xFF1B, 0xFF);
            bus16Bit.Write(0xFF1C, 0x9F);
            bus16Bit.Write(0xFF1E, 0xBF);
            bus16Bit.Write(0xFF20, 0xFF);
            bus16Bit.Write(0xFF21, 0x00);
            bus16Bit.Write(0xFF22, 0x00);
            bus16Bit.Write(0xFF23, 0xBF);
            bus16Bit.Write(0xFF24, 0x77);
            bus16Bit.Write(0xFF25, 0xF3);
            bus16Bit.Write(0xFF26, 0xF1);
            bus16Bit.Write(0xFF40, 0x91);
            bus16Bit.Write(0xFF42, 0x00);
            bus16Bit.Write(0xFF43, 0x00);
            bus16Bit.Write(0xFF45, 0x00);
            bus16Bit.Write(0xFF47, 0xFC);
            bus16Bit.Write(0xFF48, 0xFF);
            bus16Bit.Write(0xFF49, 0xFF);
            bus16Bit.Write(0xFF4A, 0x00);
            bus16Bit.Write(0xFF4B, 0x00);
            bus16Bit.Write(0xFF4D, 0xFF);
            bus16Bit.Write(0xFFFF, 0x00);
        }

        public void STOP()
        {
            DI();

            if (!_ime)
            {
                if ((IE & IF & 0x1F) == 0)
                {
                    STOPPED = true;
                    registers.PC--;
                }
                else
                {
                    HALT_BUG = true;
                }
            }
        }

        private void HALT()
        {
            if(!_ime)
            {
                if((IE & IF & 0x1F) == 0)
                {
                    HALTED = true;
                    registers.PC--;
                }
                else
                {
                    HALT_BUG = true;
                }
            }
        }

        public void UpdateIME()
        {
            _ime |= IME_ENABLER;
            IME_ENABLER = false;
        }

        #endregion

        #region FlagCheckers

        private bool SetFlagZ(int b)
        {
            return (byte)b == 0;
        }

        private bool SetFlagC(int i)
        {
            return (i >> 8) != 0;
        }

        private bool SetFlagH(byte b1, byte b2)
        {
            return ((b1 & 0xF) + (b2 & 0xF)) > 0xF;
        }

        private bool SetFlagH(ushort w1, ushort w2)
        {
            return ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF;
        }

        private bool SetFlagHCarry(byte b1, byte b2)
        {
            //int bit = BitHelper.IsFlagSet(F, (byte)FLAGS.C) ? 1 : 0;
            return ((b1 & 0xF) + (b2 & 0xF))  >= 0xF;
        }

        private bool SetFlagHSub(byte b1, byte b2)
        {
            return (b1 & 0xF) < (b2 & 0xF);
        }

        private bool SetFlagHSubCarry(byte b1, byte b2)
        {
            int carry = BitHelper.IsFlagSet(registers.F, (byte)FLAGS.C) ? 1 : 0;
            return (b1 & 0xF) < ((b2 & 0xF) + carry);
        }

        #endregion

        private ushort GetWord()
        {
            // PC++;
            ushort word3 = AddressBus.Read((ushort)(registers.PC + 1));
            word3 = (ushort)(word3 << 8);
            word3 |= AddressBus.Read(registers.PC);
            registers.PC += 2;
            return word3;
        }

        private ushort DADr8(ushort w)
        {
            
            //00HC | warning r8 is signed!

            byte b = bus16Bit.Read(registers.PC);
            RES((byte)FLAGS.Z, Parameters.F);
            RES((byte)FLAGS.N, Parameters.F);

            if (SetFlagH((byte)w, b))
                SET((byte)FLAGS.H, Parameters.F);
            else
                RES((byte)FLAGS.H, Parameters.F);

            if (SetFlagC((byte)w + b))
                SET((byte)FLAGS.C, Parameters.F);
            else
                RES((byte)FLAGS.C, Parameters.F);

            // Increment PC
            registers.PC++;

            return (ushort)(w + (sbyte)b);
        }


        #region OpCode Methods

        public int totalCycles = 0;

        internal int ExecuteOpCode(byte opcode)
        {
            // Stolen
            if (HALT_BUG)
            {
                registers.PC--;
                HALT_BUG = false;
            }

            int cpuCycles = 0;

            switch (opcode)
            {
                case 0xcb:
                    byte prefixOpcode = AddressBus.Read((ushort)(registers.PC++));
                    cpuCycles += ExecutePrefixOpCode(prefixOpcode);
                    break;
                case 0x0:
                    NOP();
                    cpuCycles += 4; break;
                case 0x1:
                    ushort aa6 = GetWord();
                    LD(Parameters.BC, aa6);
                    cpuCycles += 12; break;
                case 0x2:
                    LD(registers.BC, Parameters.A);
                    cpuCycles += 8; break;
                case 0x3:
                    INC(Parameters.BC);
                    cpuCycles += 8; break;
                case 0x4:
                    INC(Parameters.B);
                    cpuCycles += 4; break;
                case 0x5:
                    DEC(Parameters.B);
                    cpuCycles += 4; break;
                case 0x6:
                    LD(Parameters.B, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0x7:
                    RLCA();
                    cpuCycles += 4; break;
                case 0x8:
                    ushort address = GetWord();
                    bus16Bit.WriteWord(address, registers.SP);
                    cpuCycles += 20; break;
                case 0x9:
                    ADD(Parameters.HL, registers.BC);
                    cpuCycles += 8; break;
                case 0xa:
                    LD(Parameters.A, bus16Bit.Read(registers.BC));
                    cpuCycles += 8; break;
                case 0xb:
                    DEC(Parameters.BC);
                    cpuCycles += 8; break;
                case 0xc:
                    INC(Parameters.C);
                    cpuCycles += 4; break;
                case 0xd:
                    DEC(Parameters.C);
                    cpuCycles += 4; break;
                case 0xe:
                    LD(Parameters.C, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xf:
                    RRCA();
                    cpuCycles += 4; break;
                case 0x10:
                    STOP();
                    cpuCycles += 4; break;
                case 0x11:
                    ushort myWord = GetWord();
                    LD(Parameters.DE, myWord);
                    cpuCycles += 12; break;
                case 0x12:
                    LD(registers.DE, Parameters.A);
                    cpuCycles += 8; break;
                case 0x13:
                    INC(Parameters.DE);
                    cpuCycles += 8; break;
                case 0x14:
                    INC(Parameters.D);
                    cpuCycles += 4; break;
                case 0x15:
                    DEC(Parameters.D);
                    cpuCycles += 4; break;
                case 0x16:
                    LD(Parameters.D, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0x17:
                    RLA();
                    cpuCycles += 4; break;
                case 0x18:
                    JR(AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 12; break;
                case 0x19:
                    ADD(Parameters.HL, registers.DE);
                    cpuCycles += 8; break;
                case 0x1a:
                    LD(Parameters.A, bus16Bit.Read(registers.DE));
                    cpuCycles += 8; break;
                case 0x1b:
                    DEC(Parameters.DE);
                    cpuCycles += 8; break;
                case 0x1c:
                    INC(Parameters.E);
                    cpuCycles += 4; break;
                case 0x1d:
                    DEC(Parameters.E);
                    cpuCycles += 4; break;
                case 0x1e:
                    LD(Parameters.E, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0x1f:
                    RRA();
                    cpuCycles += 4; break;
                case 0x20:
                    if (JR(FLAGS.NZ, AddressBus.Read((registers.PC))))
                    {
                        cpuCycles += 12;
                    }
                    else
                    {
                        cpuCycles += 8;
                    }
                    break;
                case 0x21:
                    ushort fixlol = GetWord();
                    LD(Parameters.HL, fixlol);
                    cpuCycles += 12;
                    break;
                case 0x22:
                    LD(registers.HL, registers.A);
                    INC(Parameters.HL);
                    cpuCycles += 8; break;
                case 0x23:
                    INC(Parameters.HL);
                    cpuCycles += 8; break;
                case 0x24:
                    INC(Parameters.H);
                    cpuCycles += 4; break;
                case 0x25:
                    DEC(Parameters.H);
                    cpuCycles += 4; break;
                case 0x26:
                    LD(Parameters.H, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0x27:
                    DAA();
                    cpuCycles += 4; break;
                case 0x28:
                    if (JR(FLAGS.Z, AddressBus.Read((registers.PC))))
                    {
                        cpuCycles += 12;
                    }
                    else
                    {
                        cpuCycles += 8;
                    }
                    break;
                case 0x29:
                    ADD(Parameters.HL, registers.HL);
                    cpuCycles += 8; break;
                case 0x2a:
                    LD(Parameters.A, bus16Bit.Read(registers.HL));
                    INC(Parameters.HL);
                    cpuCycles += 8; break;
                case 0x2b:
                    DEC(Parameters.HL);
                    cpuCycles += 8; break;
                case 0x2c:
                    INC(Parameters.L);
                    cpuCycles += 4; break;
                case 0x2d:
                    DEC(Parameters.L);
                    cpuCycles += 4; break;
                case 0x2e:
                    LD(Parameters.L, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0x2f:
                    CPL();
                    cpuCycles += 4; break;

                case 0x30:
                    if (JR(FLAGS.NC, AddressBus.Read((registers.PC))))
                    {
                        cpuCycles += 12;
                    }
                    else
                    {
                        cpuCycles += 8;
                    }
                    break;
                case 0x31:
                    ushort aa5 = GetWord();
                    // 16bit loader
                    LD(Parameters.SP, aa5);
                    cpuCycles += 12; break;
                case 0x32:
                    LD(registers.HL, Parameters.A);
                    //HL--;
                    DEC(Parameters.HL);
                    cpuCycles += 8; break;
                case 0x33:
                    INC(Parameters.SP);
                    cpuCycles += 8; break;
                case 0x34:
                    INC(registers.HL);
                    cpuCycles += 12; break;
                case 0x35:
                    DEC(registers.HL);
                    cpuCycles += 12; break;
                case 0x36:
                    LD(registers.HL, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 12; break;
                case 0x37:
                    SCF();
                    cpuCycles += 4; break;
                case 0x38:
                    if (JR(FLAGS.C, AddressBus.Read((registers.PC))))
                    {
                        cpuCycles += 12;
                    }
                    else
                    {
                        cpuCycles += 8;
                    }
                    break;
                case 0x39:
                    ADD(Parameters.HL, registers.SP);
                    cpuCycles += 8; break;
                case 0x3a:
                    LD(Parameters.A, bus16Bit.Read(registers.HL));
                    DEC(Parameters.HL);
                    cpuCycles += 8; break;
                case 0x3b:
                    DEC(Parameters.SP);
                    cpuCycles += 8; break;
                case 0x3c:
                    INC(Parameters.A);
                    cpuCycles += 4; break;
                case 0x3d:
                    DEC(Parameters.A);
                    cpuCycles += 4; break;
                case 0x3e:
                    LD(Parameters.A, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0x3f:
                    CCF();
                    cpuCycles += 4; break;

                case 0x40:
                    //LD(Parameters.B, Parameters.B);
                    cpuCycles += 4; break;
                case 0x41:
                    registers.B = registers.C;
                    cpuCycles += 4; break;
                case 0x42:
                    registers.B = registers.D;
                    cpuCycles += 4; break;
                case 0x43:
                    registers.B = registers.E;
                    cpuCycles += 4; break;
                case 0x44:
                    registers.B = registers.H;
                    cpuCycles += 4; break;
                case 0x45:
                    registers.B = registers.L;
                    cpuCycles += 4; break;
                case 0x46:
                    LD(Parameters.B, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x47:
                    registers.B = registers.A;
                    cpuCycles += 4; break;
                case 0x48:
                    registers.C = registers.B;
                    cpuCycles += 4; break;
                case 0x49:
                    //LD(Parameters.C, Parameters.C);
                    cpuCycles += 4; break;
                case 0x4A:
                    registers.C = registers.D;
                    cpuCycles += 4; break;
                case 0x4B:
                    registers.C = registers.E;
                    cpuCycles += 4; break;
                case 0x4C:
                    registers.C = registers.H;
                    cpuCycles += 4; break;
                case 0x4D:
                    registers.C = registers.L;
                    cpuCycles += 4; break;
                case 0x4E:
                    LD(Parameters.C, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x4F:
                    registers.C = registers.A;
                    cpuCycles += 4; break;
                case 0x50:
                    registers.D = registers.B;
                    cpuCycles += 4; break;
                case 0x51:
                    registers.D = registers.C;
                    cpuCycles += 4; break;
                case 0x52:
                    cpuCycles += 4; break;
                case 0x53:
                    registers.D = registers.E;
                    cpuCycles += 4; break;
                case 0x54:
                    registers.D = registers.H;
                    cpuCycles += 4; break;
                case 0x55:
                    registers.D = registers.L;
                    cpuCycles += 4; break;
                case 0x56:
                    LD(Parameters.D, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x57:
                    registers.D = registers.A;
                    cpuCycles += 4; break;
                case 0x58:
                    registers.E = registers.B;
                    cpuCycles += 4; break;
                case 0x59:
                    registers.E = registers.C;
                    cpuCycles += 4; break;
                case 0x5A:
                    registers.E = registers.D;
                    cpuCycles += 4; break;
                case 0x5B:
                    cpuCycles += 4; break;
                case 0x5C:
                    registers.E = registers.H;
                    cpuCycles += 4; break;
                case 0x5D:
                    registers.E = registers.L;
                    cpuCycles += 4; break;
                case 0x5E:
                    LD(Parameters.E, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x5F:
                    registers.E = registers.A;
                    cpuCycles += 4; break;
                case 0x60:
                    registers.H = registers.B;
                    cpuCycles += 4; break;
                case 0x61:
                    registers.H = registers.C;
                    cpuCycles += 4; break;
                case 0x62:
                    registers.H = registers.D;
                    cpuCycles += 4; break;
                case 0x63:
                    registers.H = registers.E;
                    cpuCycles += 4; break;
                case 0x64:
                    cpuCycles += 4; break;
                case 0x65:
                    registers.H = registers.L;
                    cpuCycles += 4; break;
                case 0x66:
                    LD(Parameters.H, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x67:
                    registers.H = registers.A;
                    cpuCycles += 4; break;
                case 0x68:
                    registers.L = registers.B;
                    cpuCycles += 4; break;
                case 0x69:
                    registers.L = registers.C;
                    cpuCycles += 4; break;
                case 0x6A:
                    registers.L = registers.D;
                    cpuCycles += 4; break;
                case 0x6B:
                    registers.L = registers.E;
                    cpuCycles += 4; break;
                case 0x6C:
                    registers.L = registers.H;
                    cpuCycles += 4; break;
                case 0x6D:
                    //LD(Parameters.L, Parameters.L);
                    cpuCycles += 4; break;
                case 0x6E:
                    LD(Parameters.L, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x6F:
                    registers.L = registers.A;
                    cpuCycles += 4; break;

                case 0x70:
                    LD(registers.HL, Parameters.B);
                    cpuCycles += 8; break;
                case 0x71:
                    LD(registers.HL, Parameters.C);
                    cpuCycles += 8; break;
                case 0x72:
                    LD(registers.HL, Parameters.D);
                    cpuCycles += 8; break;
                case 0x73:
                    LD(registers.HL, Parameters.E);
                    cpuCycles += 8; break;
                case 0x74:
                    LD(registers.HL, Parameters.H);
                    cpuCycles += 8; break;
                case 0x75:
                    LD(registers.HL, Parameters.L);
                    cpuCycles += 8; break;
                case 0x76:
                    // HALT CPU
                    HALT();
                    cpuCycles += 4; break;
                case 0x77:
                    LD(registers.HL, Parameters.A);
                    cpuCycles += 8; break;
                case 0x78:
                    registers.A = registers.B;
                    cpuCycles += 4; break;
                case 0x79:
                    registers.A = registers.C;
                    cpuCycles += 4; break;
                case 0x7A:
                    registers.A = registers.D;
                    cpuCycles += 4; break;
                case 0x7B:
                    registers.A = registers.E;
                    cpuCycles += 4; break;
                case 0x7C:
                    registers.A = registers.H;
                    cpuCycles += 4; break;
                case 0x7D:
                    registers.A = registers.L;
                    cpuCycles += 4; break;
                case 0x7E:
                    LD(Parameters.A, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x7F:
                    //LD(Parameters.A, Parameters.A);
                    cpuCycles += 4; break;

                case 0x80:
                    ADD(Parameters.B);
                    cpuCycles += 4; break;
                case 0x81:
                    ADD(Parameters.C);
                    cpuCycles += 4; break;
                case 0x82:
                    ADD(Parameters.D);
                    cpuCycles += 4; break;
                case 0x83:
                    ADD(Parameters.E);
                    cpuCycles += 4; break;
                case 0x84:
                    ADD(Parameters.H);
                    cpuCycles += 4; break;
                case 0x85:
                    ADD(Parameters.L);
                    cpuCycles += 4; break;
                case 0x86:
                    ADD(Parameters.A, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x87:
                    ADD(Parameters.A);
                    cpuCycles += 4; break;

                case 0x88:
                    ADC(Parameters.B);
                    cpuCycles += 4; break;
                case 0x89:
                    ADC(Parameters.C);
                    cpuCycles += 4; break;
                case 0x8A:
                    ADC(Parameters.D);
                    cpuCycles += 4; break;
                case 0x8B:
                    ADC(Parameters.E);
                    cpuCycles += 4; break;
                case 0x8C:
                    ADC(Parameters.H);
                    cpuCycles += 4; break;
                case 0x8D:
                    ADC(Parameters.L);
                    cpuCycles += 4; break;
                case 0x8E:
                    ADC(bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x8F:
                    ADC(Parameters.A);
                    cpuCycles += 4; break;

                case 0x90:
                    SUB(registers.B);
                    cpuCycles += 4; break;
                case 0x91:
                    SUB(registers.C);
                    cpuCycles += 4; break;
                case 0x92:
                    SUB(registers.D);
                    cpuCycles += 4; break;
                case 0x93:
                    SUB(registers.E);
                    cpuCycles += 4; break;
                case 0x94:
                    SUB(registers.H);
                    cpuCycles += 4; break;
                case 0x95:
                    SUB(registers.L);
                    cpuCycles += 4; break;
                case 0x96:
                    SUB(bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x97:
                    SUB(registers.A);
                    cpuCycles += 4; break;

                case 0x98:
                    SBC(Parameters.A, registers.B);
                    cpuCycles += 4; break;
                case 0x99:
                    SBC(Parameters.A, registers.C);
                    cpuCycles += 4; break;
                case 0x9A:
                    SBC(Parameters.A, registers.D);
                    cpuCycles += 4; break;
                case 0x9B:
                    SBC(Parameters.A, registers.E);
                    cpuCycles += 4; break;
                case 0x9C:
                    SBC(Parameters.A, registers.H);
                    cpuCycles += 4; break;
                case 0x9D:
                    SBC(Parameters.A, registers.L);
                    cpuCycles += 4; break;
                case 0x9E:
                    SBC(Parameters.A, bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0x9F:
                    SBC(Parameters.A, registers.A);
                    cpuCycles += 4; break;

                case 0xa0:
                    AND(registers.B);
                    cpuCycles += 4; break;
                case 0xa1:
                    AND(registers.C);
                    cpuCycles += 4; break;
                case 0xa2:
                    AND(registers.D);
                    cpuCycles += 4; break;
                case 0xa3:
                    AND(registers.E);
                    cpuCycles += 4; break;
                case 0xa4:
                    AND(registers.H);
                    cpuCycles += 4; break;
                case 0xa5:
                    AND(registers.L);
                    cpuCycles += 4; break;
                case 0xa6:
                    AND(bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0xa7:
                    AND(registers.A);
                    cpuCycles += 4; break;

                case 0xa8:
                    XOR(registers.B);
                    cpuCycles += 4; break;
                case 0xa9:
                    XOR(registers.C);
                    cpuCycles += 4; break;
                case 0xaA:
                    XOR(registers.D);
                    cpuCycles += 4; break;
                case 0xaB:
                    XOR(registers.E);
                    cpuCycles += 4; break;
                case 0xaC:
                    XOR(registers.H);
                    cpuCycles += 4; break;
                case 0xaD:
                    XOR(registers.L);
                    cpuCycles += 4; break;
                case 0xaE:
                    XOR(bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0xaF:
                    XOR(registers.A);
                    cpuCycles += 4; break;

                case 0xb0:
                    OR(registers.B);
                    cpuCycles += 4; break;
                case 0xb1:
                    OR(registers.C);
                    cpuCycles += 4; break;
                case 0xb2:
                    OR(registers.D);
                    cpuCycles += 4; break;
                case 0xb3:
                    OR(registers.E);
                    cpuCycles += 4; break;
                case 0xb4:
                    OR(registers.H);
                    cpuCycles += 4; break;
                case 0xb5:
                    OR(registers.L);
                    cpuCycles += 4; break;
                case 0xb6:
                    OR(bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0xb7:
                    OR(registers.A);
                    cpuCycles += 4; break;
                case 0xb8:
                    CP(registers.B);
                    cpuCycles += 4; break;
                case 0xb9:
                    CP(registers.C);
                    cpuCycles += 4; break;
                case 0xbA:
                    CP(registers.D);
                    cpuCycles += 4; break;
                case 0xbB:
                    CP(registers.E);
                    cpuCycles += 4; break;
                case 0xbC:
                    CP(registers.H);
                    cpuCycles += 4; break;
                case 0xbD:
                    CP(registers.L);
                    cpuCycles += 4; break;
                case 0xbE:
                    CP(bus16Bit.Read(registers.HL));
                    cpuCycles += 8; break;
                case 0xbF:
                    CP(registers.A);
                    cpuCycles += 4; break;

                case 0xc0:
                    if (RET(FLAGS.NZ))
                    {
                        cpuCycles += 20;
                    }
                    else
                    {
                        cpuCycles += 8;
                    }
                    break;
                case 0xc1:
                    POP(Parameters.BC);
                    cpuCycles += 12; break;
                case 0xc2:
                    ushort aa0 = GetWord();
                    if (JP(FLAGS.NZ, aa0)) // fails
                    {
                        cpuCycles += 16;
                    }
                    else
                    {
                        cpuCycles += 12;
                    }
                    break;
                case 0xc3:
                    ushort aa1 = GetWord();
                    JP(aa1);
                    cpuCycles += 16; break;
                case 0xc4:

                    ushort aa2 = GetWord();
                    if (CALL(FLAGS.NZ, aa2))
                    {
                        cpuCycles += 24;
                    }
                    else
                    {
                        cpuCycles += 12;
                    }
                    break;

                case 0xc5:
                    PUSH(registers.BC);
                    cpuCycles += 16; break;
                case 0xc6:
                    ADD(Parameters.A, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xc7:
                    RST((byte)0x0);
                    cpuCycles += 16; break;
                case 0xc8:
                    if (RET(FLAGS.Z))
                    {
                        cpuCycles += 20;
                    }
                    else
                    {
                        cpuCycles += 8;
                    }
                    break;
                case 0xc9:
                    RET();
                    cpuCycles += 16; break;
                case 0xcA:
                    ushort a7 = GetWord();
                    if (JP(FLAGS.Z, a7))
                    {
                        cpuCycles += 16;
                    }
                    else
                    {
                        cpuCycles += 12;
                    }
                    break;
                case 0xcC:
                    ushort a = GetWord();
                    if (CALL(FLAGS.Z, a))
                    {
                        cpuCycles += 24;
                    }
                    else
                    {
                        cpuCycles += 12;
                    }
                    break;
                case 0xcD:
                    ushort a2 = GetWord();
                    CALL(a2);
                    cpuCycles += 24;
                    break;
                case 0xcE:
                    ADC(AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xcF:
                    RST(0x08);
                    cpuCycles += 16; break;

                case 0xd0:
                    if (RET(FLAGS.NC))
                    {
                        cpuCycles += 20;
                    }
                    else
                    {
                        cpuCycles += 8;
                    }
                    break;
                case 0xd1:
                    POP(Parameters.DE);
                    cpuCycles += 12; break;
                case 0xd2:
                    ushort a4 = GetWord();
                    if (JP(FLAGS.NC, a4))
                    {
                        cpuCycles += 16;
                    }
                    else
                    {
                        cpuCycles += 12;
                    }// change opcode to address bus
                    break;
                case 0xd3:
                    ushort a8 = GetWord();
                    JP(a8);
                    cpuCycles += 12; break;
                case 0xd4:
                    ushort a3 = GetWord();
                    if (CALL(FLAGS.NC, a3)) // change opcode to address bus
                    {
                        cpuCycles += 24;
                    }
                    else
                    {
                        cpuCycles += 12;
                    }
                    break;
                case 0xd5:
                    PUSH(registers.DE);
                    cpuCycles += 16; break;
                case 0xd6:
                    SUB(AddressBus.Read((registers.PC))); // change opcode to address bus
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xd7:
                    RST(0x10);
                    cpuCycles += 16; break;
                case 0xd8:
                    if (RET(FLAGS.C))
                    {
                        cpuCycles += 20;
                    }
                    else
                    {
                        cpuCycles += 8;
                    }
                    break;
                case 0xd9:
                    RETI();
                    cpuCycles += 16; break;
                case 0xdA:
                    ushort a1 = GetWord();
                    if (JP(FLAGS.C, a1)) // change opcode to address bus
                    {
                        cpuCycles += 16;
                    }
                    else
                    {
                        cpuCycles += 12;
                    }
                    break;
                case 0xdB:
                    // Unused
                    cpuCycles += 4; break;
                case 0xdC:
                    ushort a9 = GetWord();
                    if (CALL(FLAGS.C, a9))
                    {
                        cpuCycles += 24;
                    }
                    else
                    {
                        cpuCycles += 12;
                    };
                    break;
                case 0xdD:
                    // Unused
                    cpuCycles += 4; break;
                case 0xdE:
                    SBC(Parameters.A, AddressBus.Read((registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xdF:
                    RST(0x18);
                    cpuCycles += 16; break;

                case 0xe0:
                    byte nn = AddressBus.Read(registers.PC);
                    LD((ushort)(0xFF00 + nn), Parameters.A);
                    registers.PC++;
                    cpuCycles += 12; break;
                case 0xe1:
                    POP(Parameters.HL);
                    cpuCycles += 12; break;
                case 0xe2:
                    // Somethin different bout these
                    LD((ushort)(0xFF00 + registers.C), registers.A);
                    cpuCycles += 8; break;
                case 0xe3:
                    //  UNSUSED
                    cpuCycles += 4; break;
                case 0xe4:
                    // UNSUSED
                    cpuCycles += 4; break;
                case 0xe5:
                    PUSH(registers.HL);
                    cpuCycles += 16; break;
                case 0xe6:
                    AND(AddressBus.Read(registers.PC));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xe7:
                    RST(0x20);
                    cpuCycles += 16; break;
                case 0xe8:
                    registers.SP = DADr8(registers.SP);
                    cpuCycles += 16; break;
                case 0xe9:
                    JP(registers.HL);
                    cpuCycles += 4;
                    break;
                case 0xeA:
                    ushort word = GetWord();
                    LD(word, registers.A);
                    cpuCycles += 16; break;
                case 0xeB:
                    // Unused
                    cpuCycles += 4; break;
                case 0xeC:
                    // unused
                    cpuCycles += 4; break;
                case 0xeD:
                    // Unused
                    cpuCycles += 4; break;
                case 0xeE:
                    XOR(AddressBus.Read(registers.PC));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xeF:
                    RST(0x28);
                    cpuCycles += 16; break;

                case 0xf0:
                    byte n = AddressBus.Read(registers.PC);
                    LD(Parameters.A, AddressBus.Read((ushort)(0xFF00 + n)));
                    registers.PC++;
                    // CPU CYCLES
                    cpuCycles += 12;
                    break;
                case 0xf1:
                    POP(Parameters.AF);
                    cpuCycles += 12; break;
                case 0xf2:
                    LD(Parameters.A, AddressBus.Read((ushort)(0xFF00 + registers.C)));
                    cpuCycles += 8; break;
                case 0xf3:
                    DI();
                    cpuCycles += 4; break;
                case 0xf4:
                    // UNSUSED
                    cpuCycles += 4; break;
                case 0xf5:
                    PUSH(registers.AF);
                    cpuCycles += 16; break;
                case 0xf6:
                    OR(AddressBus.Read((ushort)(registers.PC)));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xf7:
                    RST(0x30);
                    cpuCycles += 16; break;
                case 0xf8:
                    registers.HL = DADr8(registers.SP);
                    cpuCycles += 12; break;
                case 0xf9:
                    LD(Parameters.SP, registers.HL);
                    cpuCycles += 8;
                    // cpuCycles += 12;
                    break;
                case 0xfA:
                    ushort word1 = GetWord();
                    byte result = AddressBus.Read(word1);
                    registers.A = result;
                    cpuCycles += 16; break;
                case 0xfB:
                    EI();
                    cpuCycles += 4; break;
                case 0xfC:
                    // unused
                    cpuCycles += 4; break;
                case 0xfD:
                    // Unused
                    cpuCycles += 4; break;
                case 0xfE:
                    CP(AddressBus.Read(registers.PC));
                    registers.PC++;
                    cpuCycles += 8; break;
                case 0xfF:
                    RST(0x38);
                    cpuCycles += 16; break;
            }

            totalCycles += cpuCycles;
            // return cpu cycles
            return cpuCycles;
        }

        internal int ExecutePrefixOpCode(byte opcode)
        {
            int cpuCycles = 0;

            // Execute OpCode with Prefixes
            switch (opcode)
            {
                case 0x0:
                    RLC(Parameters.B);
                    cpuCycles += 8;
                    break;
                case 0x1:
                    RLC(Parameters.C);
                    cpuCycles += 8;
                    break;
                case 0x2:
                    RLC(Parameters.D);
                    cpuCycles += 8;
                    break;
                case 0x3:
                    RLC(Parameters.E);
                    cpuCycles += 8;
                    break;
                case 0x4:
                    RLC(Parameters.H);
                    cpuCycles += 8;
                    break;
                case 0x5:
                    RLC(Parameters.L);
                    cpuCycles += 8;
                    break;
                case 0x6:
                    RLC(registers.HL);
                    cpuCycles += 16;
                    break;
                case 0x7:
                    RLC(Parameters.A);
                    cpuCycles += 8;
                    break;
                case 0x8:
                    RRC(Parameters.B);
                    cpuCycles += 8;
                    break;
                case 0x9:
                    RRC(Parameters.C);
                    cpuCycles += 8;
                    break;
                case 0xa:
                    RRC(Parameters.D);
                    cpuCycles += 8;
                    break;
                case 0xb:
                    RRC(Parameters.E);
                    cpuCycles += 8;
                    break;
                case 0xc:
                    RRC(Parameters.H);
                    cpuCycles += 8;
                    break;
                case 0xd:
                    RRC(Parameters.L);
                    cpuCycles += 8;
                    break;
                case 0xe:
                    RRC(registers.HL);
                    cpuCycles += 16;
                    break;
                case 0xf:
                    RRC(Parameters.A);
                    cpuCycles += 8;
                    break;


                case 0x10:
                    RL(Parameters.B);
                    cpuCycles += 8;
                    break;
                case 0x11:
                    RL(Parameters.C);
                    cpuCycles += 8;
                    break;
                case 0x12:
                    RL(Parameters.D);
                    cpuCycles += 8;
                    break;
                case 0x13:
                    RL(Parameters.E);
                    cpuCycles += 8;
                    break;
                case 0x14:
                    RL(Parameters.H);
                    cpuCycles += 8;
                    break;
                case 0x15:
                    RL(Parameters.L);
                    cpuCycles += 8;
                    break;
                case 0x16:
                    RL(registers.HL);
                    cpuCycles += 16;
                    break;
                case 0x17:
                    RL(Parameters.A);
                    cpuCycles += 8;
                    break;
                case 0x18:
                    RR(Parameters.B);
                    cpuCycles += 8;
                    break;
                case 0x19:
                    RR(Parameters.C);
                    cpuCycles += 8;
                    break;
                case 0x1a:
                    RR(Parameters.D);
                    cpuCycles += 8;
                    break;
                case 0x1b:
                    RR(Parameters.E);
                    cpuCycles += 8;
                    break;
                case 0x1c:
                    RR(Parameters.H);
                    cpuCycles += 8;
                    break;
                case 0x1d:
                    RR(Parameters.L);
                    cpuCycles += 8;
                    break;
                case 0x1e:
                    RR(registers.HL);
                    cpuCycles += 16;
                    break;
                case 0x1f:
                    RR(Parameters.A);
                    cpuCycles += 8;
                    break;


                case 0x20:
                    SLA(Parameters.B);
                    cpuCycles += 8;
                    break;
                case 0x21:
                    SLA(Parameters.C);
                    cpuCycles += 8;
                    break;
                case 0x22:
                    SLA(Parameters.D);
                    cpuCycles += 8;
                    break;
                case 0x23:
                    SLA(Parameters.E);
                    cpuCycles += 8;
                    break;
                case 0x24:
                    SLA(Parameters.H);
                    cpuCycles += 8;
                    break;
                case 0x25:
                    SLA(Parameters.L);
                    cpuCycles += 8;
                    break;
                case 0x26:
                    SLA(registers.HL);
                    cpuCycles += 16;
                    break;
                case 0x27:
                    SLA(Parameters.A);
                    cpuCycles += 8;
                    break;
                case 0x28:
                    SRA(Parameters.B);
                    cpuCycles += 8;
                    break;
                case 0x29:
                    SRA(Parameters.C);
                    cpuCycles += 8;
                    break;
                case 0x2a:
                    SRA(Parameters.D);
                    cpuCycles += 8;
                    break;
                case 0x2b:
                    SRA(Parameters.E);
                    cpuCycles += 8;
                    break;
                case 0x2c:
                    SRA(Parameters.H);
                    cpuCycles += 8;
                    break;
                case 0x2d:
                    SRA(Parameters.L);
                    cpuCycles += 8;
                    break;
                case 0x2e:
                    SRA(registers.HL);
                    cpuCycles += 16;
                    break;
                case 0x2f:
                    SRA(Parameters.A);
                    cpuCycles += 8;
                    break;


                case 0x30:
                    SWAP(Parameters.B); // FAIL SWAP HIGH AND LOW NIBBLES
                    cpuCycles += 8;
                    break;
                case 0x31:
                    SWAP(Parameters.C);
                    cpuCycles += 8;
                    break;
                case 0x32:
                    SWAP(Parameters.D);
                    cpuCycles += 8;
                    break;
                case 0x33:
                    SWAP(Parameters.E);
                    cpuCycles += 8;
                    break;
                case 0x34:
                    SWAP(Parameters.H);
                    cpuCycles += 8;
                    break;
                case 0x35:
                    SWAP(Parameters.L);
                    cpuCycles += 8;
                    break;
                case 0x36:
                    SWAP(registers.HL);
                    cpuCycles += 16; break;
                case 0x37:
                    SWAP(Parameters.A);
                    cpuCycles += 8; break;
                case 0x38:
                    SRL(Parameters.B);
                    cpuCycles += 8; break;
                case 0x39:
                    SRL(Parameters.C);
                    cpuCycles += 8; break;
                case 0x3a:
                    SRL(Parameters.D);
                    cpuCycles += 8; break;
                case 0x3b:
                    SRL(Parameters.E);
                    cpuCycles += 8; break;
                case 0x3c:
                    SRL(Parameters.H);
                    cpuCycles += 8; break;
                case 0x3d:
                    SRL(Parameters.L);
                    cpuCycles += 8; break;
                case 0x3e:
                    SRL(registers.HL);
                    cpuCycles += 16; break;
                case 0x3f:
                    SRL(Parameters.A);
                    cpuCycles += 8; break;


                case 0x40:
                    BIT(0x1, Parameters.B);
                    cpuCycles += 8; break;
                case 0x41:
                    BIT(0x1, Parameters.C);
                    cpuCycles += 8; break;
                case 0x42:
                    BIT(0x1, Parameters.D);
                    cpuCycles += 8; break;
                case 0x43:
                    BIT(0x1, Parameters.E);
                    cpuCycles += 8; break;
                case 0x44:
                    BIT(0x1, Parameters.H);
                    cpuCycles += 8; break;
                case 0x45:
                    BIT(0x1, Parameters.L);
                    cpuCycles += 8; break;
                case 0x46:
                    BIT(0x1, bus16Bit.Read(registers.HL));
                    cpuCycles += 12; break;
                case 0x47:
                    BIT(0x1, Parameters.A);
                    cpuCycles += 8; break;

                case 0x48:
                    BIT(0x2, Parameters.B);
                    cpuCycles += 8; break;
                case 0x49:
                    BIT(0x2, Parameters.C);
                    cpuCycles += 8; break;
                case 0x4a:
                    BIT(0x2, Parameters.D);
                    cpuCycles += 8; break;
                case 0x4b:
                    BIT(0x2, Parameters.E);
                    cpuCycles += 8; break;
                case 0x4c:
                    BIT(0x2, Parameters.H);
                    cpuCycles += 8; break;
                case 0x4d:
                    BIT(0x2, Parameters.L);
                    cpuCycles += 8; break;
                case 0x4e:
                    BIT(0x2, bus16Bit.Read(registers.HL));
                    cpuCycles += 12; break;
                case 0x4f:
                    BIT(0x2, Parameters.A);
                    cpuCycles += 8; break;


                case 0x50:
                    BIT(0x4, Parameters.B);
                    cpuCycles += 8; break;
                case 0x51:
                    BIT(0x4, Parameters.C);
                    cpuCycles += 8; break;
                case 0x52:
                    BIT(0x4, Parameters.D);
                    cpuCycles += 8; break;
                case 0x53:
                    BIT(0x4, Parameters.E);
                    cpuCycles += 8; break;
                case 0x54:
                    BIT(0x4, Parameters.H);
                    cpuCycles += 8; break;
                case 0x55:
                    BIT(0x4, Parameters.L);
                    cpuCycles += 8; break;
                case 0x56:
                    BIT(0x4, bus16Bit.Read(registers.HL));
                    cpuCycles += 12; break;
                case 0x57:
                    BIT(0x4, Parameters.A);
                    cpuCycles += 8; break;

                case 0x58:
                    BIT(0x8, Parameters.B);
                    cpuCycles += 8; break;
                case 0x59:
                    BIT(0x8, Parameters.C);
                    cpuCycles += 8; break;
                case 0x5a:
                    BIT(0x8, Parameters.D);
                    cpuCycles += 8; break;
                case 0x5b:
                    BIT(0x8, Parameters.E);
                    cpuCycles += 8; break;
                case 0x5c:
                    BIT(0x8, Parameters.H);
                    cpuCycles += 8; break;
                case 0x5d:
                    BIT(0x8, Parameters.L);
                    cpuCycles += 8; break;
                case 0x5e:
                    BIT(0x8, bus16Bit.Read(registers.HL));
                    cpuCycles += 12; break;
                case 0x5f:
                    BIT(0x8, Parameters.A);
                    cpuCycles += 8; break;


                case 0x60:
                    BIT(0x10, Parameters.B);
                    cpuCycles += 8; break;
                case 0x61:
                    BIT(0x10, Parameters.C);
                    cpuCycles += 8; break;
                case 0x62:
                    BIT(0x10, Parameters.D);
                    cpuCycles += 8; break;
                case 0x63:
                    BIT(0x10, Parameters.E);
                    cpuCycles += 8; break;
                case 0x64:
                    BIT(0x10, Parameters.H);
                    cpuCycles += 8; break;
                case 0x65:
                    BIT(0x10, Parameters.L);
                    cpuCycles += 8; break;
                case 0x66:
                    BIT(0x10, bus16Bit.Read(registers.HL));
                    cpuCycles += 12; break;
                case 0x67:
                    BIT(0x10, Parameters.A);
                    cpuCycles += 8; break;

                case 0x68:
                    BIT(0x20, Parameters.B);
                    cpuCycles += 8; break;
                case 0x69:
                    BIT(0x20, Parameters.C);
                    cpuCycles += 8; break;
                case 0x6a:
                    BIT(0x20, Parameters.D);
                    cpuCycles += 8; break;
                case 0x6b:
                    BIT(0x20, Parameters.E);
                    cpuCycles += 8; break;
                case 0x6c:
                    BIT(0x20, Parameters.H);
                    cpuCycles += 8; break;
                case 0x6d:
                    BIT(0x20, Parameters.L);
                    cpuCycles += 8; break;
                case 0x6e:
                    BIT(0x20, bus16Bit.Read(registers.HL));
                    cpuCycles += 12; break;
                case 0x6f:
                    BIT(0x20, Parameters.A);
                    cpuCycles += 8; break;


                case 0x70:
                    BIT(0x40, Parameters.B);
                    cpuCycles += 8; break;
                case 0x71:
                    BIT(0x40, Parameters.C);
                    cpuCycles += 8; break;
                case 0x72:
                    BIT(0x40, Parameters.D);
                    cpuCycles += 8; break;
                case 0x73:
                    BIT(0x40, Parameters.E);
                    cpuCycles += 8; break;
                case 0x74:
                    BIT(0x40, Parameters.H);
                    cpuCycles += 8; break;
                case 0x75:
                    BIT(0x40, Parameters.L);
                    cpuCycles += 8; break;
                case 0x76:
                    BIT(0x40, bus16Bit.Read(registers.HL));
                    cpuCycles += 12; break;
                case 0x77:
                    BIT(0x40, Parameters.A);
                    cpuCycles += 8; break;

                case 0x78:
                    BIT(0x80, Parameters.B);
                    cpuCycles += 8; break;
                case 0x79:
                    BIT(0x80, Parameters.C);
                    cpuCycles += 8; break;
                case 0x7a:
                    BIT(0x80, Parameters.D);
                    cpuCycles += 8; break;
                case 0x7b:
                    BIT(0x80, Parameters.E);
                    cpuCycles += 8; break;
                case 0x7c:
                    BIT(0x80, Parameters.H);
                    cpuCycles += 8; break;
                case 0x7d:
                    BIT(0x80, Parameters.L);
                    cpuCycles += 8; break;
                case 0x7e:
                    BIT(0x80, bus16Bit.Read(registers.HL));
                    cpuCycles += 12; break; // 16  should be correct
                case 0x7f:
                    BIT(0x80, Parameters.A);
                    cpuCycles += 8; break;

                /////////////////////////

                case 0x80:
                    RES(0x1, Parameters.B);
                    cpuCycles += 8; break;
                case 0x81:
                    RES(0x1, Parameters.C);
                    cpuCycles += 8; break;
                case 0x82:
                    RES(0x1, Parameters.D);
                    cpuCycles += 8; break;
                case 0x83:
                    RES(0x1, Parameters.E);
                    cpuCycles += 8; break;
                case 0x84:
                    RES(0x1, Parameters.H);
                    cpuCycles += 8; break;
                case 0x85:
                    RES(0x1, Parameters.L);
                    cpuCycles += 8; break;
                case 0x86:
                    bus16Bit.Write(registers.HL, RESr(0x1, registers.HL));
                    cpuCycles += 16; break;
                case 0x87:
                    RES(0x1, Parameters.A);
                    cpuCycles += 8; break;

                case 0x88:
                    RES(0x2, Parameters.B);
                    cpuCycles += 8; break;
                case 0x89:
                    RES(0x2, Parameters.C);
                    cpuCycles += 8; break;
                case 0x8a:
                    RES(0x2, Parameters.D);
                    cpuCycles += 8; break;
                case 0x8b:
                    RES(0x2, Parameters.E);
                    cpuCycles += 8; break;
                case 0x8c:
                    RES(0x2, Parameters.H);
                    cpuCycles += 8; break;
                case 0x8d:
                    RES(0x2, Parameters.L);
                    cpuCycles += 8; break;
                case 0x8e:
                    bus16Bit.Write(registers.HL, RESr(0x2, registers.HL));
                    cpuCycles += 16; break;
                case 0x8f:
                    RES(0x2, Parameters.A);
                    cpuCycles += 8; break;


                case 0x90:
                    RES(0x4, Parameters.B);
                    cpuCycles += 8; break;
                case 0x91:
                    RES(0x4, Parameters.C);
                    cpuCycles += 8; break;
                case 0x92:
                    RES(0x4, Parameters.D);
                    cpuCycles += 8; break;
                case 0x93:
                    RES(0x4, Parameters.E);
                    cpuCycles += 8; break;
                case 0x94:
                    RES(0x4, Parameters.H);
                    cpuCycles += 8; break;
                case 0x95:
                    RES(0x4, Parameters.L);
                    cpuCycles += 8; break;
                case 0x96:
                    bus16Bit.Write(registers.HL, RESr(0x4, registers.HL));
                    cpuCycles += 16; break;
                case 0x97:
                    RES(0x4, Parameters.A);
                    cpuCycles += 8; break;

                case 0x98:
                    RES(0x8, Parameters.B);
                    cpuCycles += 8; break;
                case 0x99:
                    RES(0x8, Parameters.C);
                    cpuCycles += 8; break;
                case 0x9a:
                    RES(0x8, Parameters.D);
                    cpuCycles += 8; break;
                case 0x9b:
                    RES(0x8, Parameters.E);
                    cpuCycles += 8; break;
                case 0x9c:
                    RES(0x8, Parameters.H);
                    cpuCycles += 16; break;
                case 0x9d:
                    RES(0x8, Parameters.L);
                    cpuCycles += 8; break;
                case 0x9e:
                    bus16Bit.Write(registers.HL, RESr(0x8, registers.HL));
                    cpuCycles += 8; break;
                case 0x9f:
                    RES(0x8, Parameters.A);
                    cpuCycles += 8; break;

                case 0xa0:
                    RES(0x10, Parameters.B);
                    cpuCycles += 8; break;
                case 0xa1:
                    RES(0x10, Parameters.C);
                    cpuCycles += 8; break;
                case 0xa2:
                    RES(0x10, Parameters.D);
                    cpuCycles += 8; break;
                case 0xa3:
                    RES(0x10, Parameters.E);
                    cpuCycles += 8; break;
                case 0xa4:
                    RES(0x10, Parameters.H);
                    cpuCycles += 8; break;
                case 0xa5:
                    RES(0x10, Parameters.L);
                    cpuCycles += 8; break;
                case 0xa6:
                    bus16Bit.Write(registers.HL, RESr(0x10, registers.HL));
                    cpuCycles += 16; break;
                case 0xa7:
                    RES(0x10, Parameters.A);
                    cpuCycles += 8; break;

                case 0xa8:
                    RES(0x20, Parameters.B);
                    cpuCycles += 8; break;
                case 0xa9:
                    RES(0x20, Parameters.C);
                    cpuCycles += 8; break;
                case 0xaa:
                    RES(0x20, Parameters.D);
                    cpuCycles += 8; break;
                case 0xab:
                    RES(0x20, Parameters.E);
                    cpuCycles += 8; break;
                case 0xac:
                    RES(0x20, Parameters.H);
                    cpuCycles += 8; break;
                case 0xad:
                    RES(0x20, Parameters.L);
                    cpuCycles += 8; break;
                case 0xae:
                    bus16Bit.Write(registers.HL, RESr(0x20, registers.HL));
                    cpuCycles += 16; break;
                case 0xaf:
                    RES(0x20, Parameters.A);
                    cpuCycles += 8; break;


                case 0xb0:
                    RES(0x40, Parameters.B);
                    cpuCycles += 8; break;
                case 0xb1:
                    RES(0x40, Parameters.C);
                    cpuCycles += 8; break;
                case 0xb2:
                    RES(0x40, Parameters.D);
                    cpuCycles += 8; break;
                case 0xb3:
                    RES(0x40, Parameters.E);
                    cpuCycles += 8; break;
                case 0xb4:
                    RES(0x40, Parameters.H);
                    cpuCycles += 8; break;
                case 0xb5:
                    RES(0x40, Parameters.L);
                    cpuCycles += 8; break;
                case 0xb6:
                    //RES(6, HL);
                    bus16Bit.Write(registers.HL, RESr(0x40, registers.HL));
                    cpuCycles += 16; break;
                case 0xb7:
                    RES(0x40, Parameters.A);
                    cpuCycles += 8; break;

                case 0xb8:
                    RES(0x80, Parameters.B);
                    cpuCycles += 8; break;
                case 0xb9:
                    RES(0x80, Parameters.C);
                    cpuCycles += 8; break;
                case 0xba:
                    RES(0x80, Parameters.D);
                    cpuCycles += 8; break;
                case 0xbb:
                    RES(0x80, Parameters.E);
                    cpuCycles += 8; break;
                case 0xbc:
                    RES(0x80, Parameters.H);
                    cpuCycles += 8; break;
                case 0xbd:
                    RES(0x80, Parameters.L);
                    cpuCycles += 8; break;
                case 0xbe:
                    bus16Bit.Write(registers.HL, RESr(0x80, registers.HL));
                    cpuCycles += 16; break;
                case 0xbf:
                    RES(0x80, Parameters.A);
                    cpuCycles += 8; break;

                case 0xc0:
                    SET(0x1, Parameters.B);
                    cpuCycles += 8; break;
                case 0xc1:
                    SET(0x1, Parameters.C);
                    cpuCycles += 8; break;
                case 0xc2:
                    SET(0x1, Parameters.D);
                    cpuCycles += 8; break;
                case 0xc3:
                    SET(0x1, Parameters.E);
                    cpuCycles += 8; break;
                case 0xc4:
                    SET(0x1, Parameters.H);
                    cpuCycles += 8; break;
                case 0xc5:
                    SET(0x1, Parameters.L);
                    cpuCycles += 8; break;
                case 0xc6:
                    bus16Bit.Write(registers.HL, SETr(0x1, registers.HL));
                    cpuCycles += 16; break;
                case 0xc7:
                    SET(0x1, Parameters.A);
                    cpuCycles += 8; break;

                case 0xc8:
                    SET(0x2, Parameters.B);
                    cpuCycles += 8; break;
                case 0xc9:
                    SET(0x2, Parameters.C);
                    cpuCycles += 8; break;
                case 0xca:
                    SET(0x2, Parameters.D);
                    cpuCycles += 8; break;
                case 0xcb:
                    SET(0x2, Parameters.E);
                    cpuCycles += 8; break;
                case 0xcc:
                    SET(0x2, Parameters.H);
                    cpuCycles += 8; break;
                case 0xcd:
                    SET(0x2, Parameters.L);
                    cpuCycles += 8; break;
                case 0xce:
                    bus16Bit.Write(registers.HL, SETr(0x2, registers.HL));
                    cpuCycles += 16; break;
                case 0xcf:
                    SET(0x2, Parameters.A);
                    cpuCycles += 8; break;


                case 0xd0:
                    SET(0x4, Parameters.B);
                    cpuCycles += 8; break;
                case 0xd1:
                    SET(0x4, Parameters.C);
                    cpuCycles += 8; break;
                case 0xd2:
                    SET(0x4, Parameters.D);
                    cpuCycles += 8; break;
                case 0xd3:
                    SET(0x4, Parameters.E);
                    cpuCycles += 8; break;
                case 0xd4:
                    SET(0x4, Parameters.H);
                    cpuCycles += 8; break;
                case 0xd5:
                    SET(0x4, Parameters.L);
                    cpuCycles += 8; break;
                case 0xd6:
                    bus16Bit.Write(registers.HL, SETr(0x4, registers.HL));
                    cpuCycles += 16; break;
                case 0xd7:
                    SET(0x4, Parameters.A);
                    cpuCycles += 8; break;

                case 0xd8:
                    SET(0x8, Parameters.B);
                    cpuCycles += 8; break;
                case 0xd9:
                    SET(0x8, Parameters.C);
                    cpuCycles += 8; break;
                case 0xda:
                    SET(0x8, Parameters.D);
                    cpuCycles += 8; break;
                case 0xdb:
                    SET(0x8, Parameters.E);
                    cpuCycles += 8; break;
                case 0xdc:
                    SET(0x8, Parameters.H);
                    cpuCycles += 8; break;
                case 0xdd:
                    SET(0x8, Parameters.L);
                    cpuCycles += 8; break;
                case 0xde:
                    bus16Bit.Write(registers.HL, SETr(0x8, registers.HL));
                    cpuCycles += 16; break;
                case 0xdf:
                    SET(0x8, Parameters.A);
                    cpuCycles += 8; break;


                case 0xe0:
                    SET(0x10, Parameters.B);
                    cpuCycles += 8; break;
                case 0xe1:
                    SET(0x10, Parameters.C);
                    cpuCycles += 8; break;
                case 0xe2:
                    SET(0x10, Parameters.D);
                    cpuCycles += 8; break;
                case 0xe3:
                    SET(0x10, Parameters.E);
                    cpuCycles += 8; break;
                case 0xe4:
                    SET(0x10, Parameters.H);
                    cpuCycles += 8; break;
                case 0xe5:
                    SET(0x10, Parameters.L);
                    cpuCycles += 8; break;
                case 0xe6:
                    bus16Bit.Write(registers.HL, SETr(0x10, registers.HL));
                    cpuCycles += 16; break;
                case 0xe7:
                    SET(0x10, Parameters.A);
                    cpuCycles += 8; break;

                case 0xe8:
                    SET(0x20, Parameters.B);
                    cpuCycles += 8; break;
                case 0xe9:
                    SET(0x20, Parameters.C);
                    cpuCycles += 8; break;
                case 0xea:
                    SET(0x20, Parameters.D);
                    cpuCycles += 8; break;
                case 0xeb:
                    SET(0x20, Parameters.E);
                    cpuCycles += 8; break;
                case 0xec:
                    SET(0x20, Parameters.H);
                    cpuCycles += 8; break;
                case 0xed:
                    SET(0x20, Parameters.L);
                    cpuCycles += 8; break;
                case 0xee:
                    bus16Bit.Write(registers.HL, SETr(0x20, registers.HL));
                    cpuCycles += 16; break;
                case 0xef:
                    SET(0x20, Parameters.A);
                    cpuCycles += 8; break;

                case 0xf0:
                    SET(0x40, Parameters.B);
                    cpuCycles += 8; break;
                case 0xf1:
                    SET(0x40, Parameters.C);
                    cpuCycles += 8; break;
                case 0xf2:
                    SET(0x40, Parameters.D);
                    cpuCycles += 8; break;
                case 0xf3:
                    SET(0x40, Parameters.E);
                    cpuCycles += 8; break;
                case 0xf4:
                    SET(0x40, Parameters.H);
                    cpuCycles += 8; break;
                case 0xf5:
                    SET(0x40, Parameters.L);
                    cpuCycles += 8; break;
                case 0xf6:
                    bus16Bit.Write(registers.HL, SETr(0x40, registers.HL));
                    cpuCycles += 16; break;
                case 0xf7:
                    SET(0x40, Parameters.A);
                    cpuCycles += 8; break;

                case 0xf8:
                    SET(0x80, Parameters.B);
                    cpuCycles += 8; break;
                case 0xf9:
                    SET(0x80, Parameters.C);
                    cpuCycles += 8; break;
                case 0xfa:
                    SET(0x80, Parameters.D);
                    cpuCycles += 8; break;
                case 0xfb:
                    SET(0x80, Parameters.E);
                    cpuCycles += 8; break;
                case 0xfc:
                    SET(0x80, Parameters.H);
                    cpuCycles += 8; break;
                case 0xfd:
                    SET(0x80, Parameters.L);
                    cpuCycles += 8; break;
                case 0xfe:
                    bus16Bit.Write(registers.HL, SETr(0x80, registers.HL));
                    cpuCycles += 16; break;
                case 0xff:
                    SET(0x80, Parameters.A);
                    cpuCycles += 8; break;
            }

            // return cpu cycles
            return cpuCycles;
        }

        #endregion


    }
}
