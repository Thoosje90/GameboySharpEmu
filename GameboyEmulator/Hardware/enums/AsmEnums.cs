using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware.enums
{
    public class AsmEnums
    {

        public enum Parameters
        {
            A = 43,
            B,
            C,
            D,
            E,
            F,
            H,
            L,
            HL,
            BC,
            DE,
            HLD,
            SP,
            PC,
            AF
        }

        public enum FLAGS
        {
            Z = 0x80,
            N = 0x40,
            H = 0x20,
            C = 0x10,
            NZ,
            NC
        }

        public enum Instructions
        {
            ADD = 1,
            SUB,
            POP,
            ADC,
            SBC,
            AND,
            OR,
            XOR,
            CP,
            INC,
            DEC,
            SWAP,
            LD,
            LDD,
            LDHL,
            DAA,
            CCF,
            SCF,
            NOP,
            HALT,
            STOP,
            DI,
            EI,
            RLCA,
            RLA,
            RRCA,
            RLC,
            RL,
            RRC,
            RR,
            RRA,
            SLA,
            SRA,
            SRL,
            BIT,
            SET,
            RES,
            JP,
            JR,
            CALL,
            PUSH,
            RST,
            RET,
            RETI,
            CPL
        }

        public enum Numbers
        {
            n1 = 58,
            n2,
            n3,
            n5,
            n6,
            n7,
            n8,
            n9
        }
    }
}
