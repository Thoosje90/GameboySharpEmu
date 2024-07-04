using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameboyEmulator.Hardware;
using static GameboyEmulator.Hardware.enums.AsmEnums;

namespace GameboyEmulator
{
    internal class ASMCompiler
    {

        public List<string> CompileASM(string[] sourceCode)
        {
            // Store compiled binary code on a stack
            List<string> binaryStack = new List<string>();

            foreach(string line in sourceCode)
            {
                // Seperate commands and parameters
                string[] blocks = line.TrimStart().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                // Store final binary block
                string blockBinary = "";

                // Iterate through commands and parameters
                for(int i = 0;i < blocks.Length;i++)
                {
                    // Loop through each cpu instructions
                    foreach(string instruction in Enum.GetNames(typeof(Instructions)))
                    {
                        if (blocks[i] == instruction)
                        {
                            // Get instruction
                            Instructions instruct = (Instructions)Enum.Parse(typeof(Instructions), blocks[i]);

                            // Convert instruction to binary and add to block
                            string binary = Convert.ToString(((byte)instruct), 2).PadLeft(8, '0');
                            blockBinary += binary + ((i < blocks.Length - 1) ? " " : "");
                            break;
                        }
                    }

                    // Loop through each cpu register
                    foreach (string registers in Enum.GetNames(typeof(Parameters)))
                    {
                        if (blocks[i] == registers)
                        {
                            // Get instruction
                            Parameters reg = (Parameters)Enum.Parse(typeof(Parameters), blocks[i]);

                            // Convert parameter to binary and add to block
                            string binary = Convert.ToString(((byte)reg), 2).PadLeft(8, '0');
                            blockBinary += binary + ((i < blocks.Length - 1) ? " " : "");
                            break;
                        }
                    }
                }

                //
                binaryStack.Add(blockBinary);
            }

            return binaryStack;
        }

        public List<string> CompileASM2(string[] sourceCode)
        {
            // Store compiled binary code on a stack
            List<string> binaryStack = new List<string>();

            foreach (string line in sourceCode)
            {
                // Seperate commands and parameters
                string[] blocks = line.TrimStart().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                // Store final binary block
                string blockBinary = "";

                // Iterate through commands and parameters
                for (int i = 0; i < blocks.Length; i++)
                {
                    // Loop through each cpu instructions
                    foreach (string instruction in Enum.GetNames(typeof(Instructions)))
                    {
                        if (blocks[i] == instruction)
                        {
                            // Get instruction
                            Instructions instruct = (Instructions)Enum.Parse(typeof(Instructions), blocks[i]);

                            // Convert instruction to binary and add to block
                            string binary = Convert.ToString(((byte)instruct), 2).PadLeft(8, '0');
                            blockBinary += binary + ((i < blocks.Length - 1) ? " " : "");
                            break;
                        }
                    }

                    // Loop through each cpu register
                    foreach (string registers in Enum.GetNames(typeof(Parameters)))
                    {
                        if (blocks[i] == registers)
                        {
                            // Get instruction
                            Parameters reg = (Parameters)Enum.Parse(typeof(Parameters), blocks[i]);

                            // Convert parameter to binary and add to block
                            string binary = Convert.ToString(((byte)reg), 2).PadLeft(8, '0');
                            blockBinary += binary + ((i < blocks.Length - 1) ? " " : "");
                            break;
                        }
                    }
                }

                //
                binaryStack.Add(blockBinary);
            }

            return binaryStack;
        }
    }

    class OpCodes
    {
        Dictionary<int, int> lol = new Dictionary<int, int>();

        public OpCodes()
        {
            // FIX THIS TO PROPER HEX CALCULATIONS SOMEDAY LOL
            // FUTURE VERSION

            lol.Add(0x0, 0);
            lol.Add((int)Instructions.LD + (int)Parameters.BC + short.MaxValue, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.BC + (int)Parameters.A, 0x2);
            lol.Add((int)Instructions.INC + (int)Parameters.BC, 0x3);
            lol.Add((int)Instructions.INC + (int)Parameters.B, 0x4);
            lol.Add((int)Instructions.DEC + (int)Parameters.B, 0x5);
            lol.Add((int)Instructions.LD + (int)Parameters.B + byte.MaxValue, 0x6);
            lol.Add((int)Instructions.RLCA, 0x7);
            lol.Add((int)Parameters.BC + 255, 0x8);
            lol.Add((int)Instructions.ADD + (int)Parameters.HL + (int)Parameters.BC, 0x9);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.BC, 0xA);
            lol.Add((int)Instructions.DEC + (int)Parameters.BC, 0xB);
            lol.Add((int)Instructions.INC + (int)Parameters.C, 0xC);
            lol.Add((int)Instructions.DEC + (int)Parameters.C, 0xC);
            lol.Add((int)Instructions.LD + (int)Parameters.C + byte.MaxValue, 0xE);
            lol.Add((int)Instructions.RRCA, 0xF);

            lol.Add((int)Instructions.STOP, 0x10);
            lol.Add((int)Instructions.LD + (int)Parameters.DE + short.MaxValue, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.DE + (int)Parameters.A, 0x2);
            lol.Add((int)Instructions.INC + (int)Parameters.DE, 0x3);
            lol.Add((int)Instructions.INC + (int)Parameters.D, 0x4);
            lol.Add((int)Instructions.DEC + (int)Parameters.D, 0x5);
            lol.Add((int)Instructions.LD + (int)Parameters.D + byte.MaxValue, 0x6);
            lol.Add((int)Instructions.RLA, 0x7);
            lol.Add((int)Instructions.JR + 255, 0x8);
            lol.Add((int)Instructions.ADD + (int)Parameters.HL + (int)Parameters.DE, 0x9);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.DE, 0xA);
            lol.Add((int)Instructions.DEC + (int)Parameters.DE, 0xB);
            lol.Add((int)Instructions.INC + (int)Parameters.E, 0xC);
            lol.Add((int)Instructions.DEC + (int)Parameters.E, 0xC);
            lol.Add((int)Instructions.LD + (int)Parameters.E + byte.MaxValue, 0x1E);
            lol.Add((int)Instructions.RRA, 0xF);

            lol.Add((int)Instructions.JR + (int)FLAGS.NZ + byte.MaxValue, 0x10);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + short.MaxValue, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.A, 0x2);
            lol.Add((int)Instructions.INC + (int)Parameters.HL, 0x3);
            lol.Add((int)Instructions.INC + (int)Parameters.H, 0x4);
            lol.Add((int)Instructions.DEC + (int)Parameters.H, 0x5);
            lol.Add((int)Instructions.LD + (int)Parameters.H + byte.MaxValue, 0x6);
            lol.Add((int)Instructions.DAA, 0x7);
            lol.Add((int)Instructions.JR + (int)FLAGS.Z + 255, 0x8);
            lol.Add((int)Instructions.ADD + (int)Parameters.HL + (int)Parameters.HL, 0x9);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.HL, 0xA);
            lol.Add((int)Instructions.DEC + (int)Parameters.HL, 0xB);
            lol.Add((int)Instructions.INC + (int)Parameters.L, 0xC);
            lol.Add((int)Instructions.DEC + (int)Parameters.L, 0xC);
            lol.Add((int)Instructions.LD + (int)Parameters.L + byte.MaxValue, 0x1E);
            lol.Add((int)Instructions.CPL, 0xF);

            lol.Add((int)Instructions.JR + (int)FLAGS.NC + byte.MaxValue, 0x10);
            lol.Add((int)Instructions.LD + (int)Parameters.SP + short.MaxValue, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.A, 0x2);
            lol.Add((int)Instructions.INC + (int)Parameters.SP, 0x3);
            lol.Add((int)Instructions.INC + (int)Parameters.HL, 0x4);
            lol.Add((int)Instructions.DEC + (int)Parameters.HL, 0x5);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + byte.MaxValue, 0x6);
            lol.Add((int)Instructions.SCF, 0x7);
            lol.Add((int)Instructions.JR + 255, 0x8);
            lol.Add((int)Instructions.ADD + (int)Parameters.HL + (int)Parameters.DE, 0x9);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.DE, 0xA);
            lol.Add((int)Instructions.DEC + (int)Parameters.SP, 0xB);
            lol.Add((int)Instructions.INC + (int)Parameters.A, 0xC);
            lol.Add((int)Instructions.DEC + (int)Parameters.A, 0xC);
            lol.Add((int)Instructions.LD + (int)Parameters.A + byte.MaxValue, 0x1E);
            lol.Add((int)Instructions.CCF, 0xF);

            lol.Add((int)Instructions.LD + (int)Parameters.B + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.B + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.B + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.B + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.B + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.B + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.B + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.B + (int)Parameters.A, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.C + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.C + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.C + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.C + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.C + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.C + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.C + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.C + (int)Parameters.A, 0x1);

            lol.Add((int)Instructions.LD + (int)Parameters.D + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.D + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.D + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.D + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.D + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.D + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.D + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.D + (int)Parameters.A, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.E + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.E + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.E + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.E + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.E + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.E + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.E + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.E + (int)Parameters.A, 0x1);

            lol.Add((int)Instructions.LD + (int)Parameters.H + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.H + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.H + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.H + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.H + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.H + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.H + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.H + (int)Parameters.A, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.A, 0x1);

            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.LD + (int)Instructions.HALT, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.HL + (int)Parameters.A, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.LD + (int)Parameters.L + (int)Parameters.A, 0x1);

            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.ADD + (int)Parameters.A + (int)Parameters.A, 0x1);
            lol.Add((int)Instructions.ADC + (int)Parameters.A + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.ADC + (int)Parameters.A + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.ADC + (int)Parameters.A + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.ADC + (int)Parameters.A + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.ADC + (int)Parameters.A + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.ADC + (int)Parameters.A + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.ADC + (int)Parameters.A + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.ADC + (int)Parameters.A + (int)Parameters.A, 0x1);

            lol.Add((int)Instructions.SUB + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.SUB + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.SUB + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.SUB + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.SUB + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.SUB + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.SUB + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.SUB + (int)Parameters.A, 0x1);
            lol.Add((int)Instructions.SBC + (int)Parameters.A + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.SBC + (int)Parameters.A + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.SBC + (int)Parameters.A + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.SBC + (int)Parameters.A + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.SBC + (int)Parameters.A + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.SBC + (int)Parameters.A + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.SBC + (int)Parameters.A + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.SBC + (int)Parameters.A + (int)Parameters.A, 0x1);

            lol.Add((int)Instructions.AND + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.AND + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.AND + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.AND + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.AND + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.AND + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.AND + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.AND + (int)Parameters.A, 0x1);
            lol.Add((int)Instructions.XOR + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.XOR + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.XOR + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.XOR + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.XOR + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.XOR + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.XOR + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.XOR + (int)Parameters.A, 0x1);

            lol.Add((int)Instructions.OR + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.OR + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.OR + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.OR + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.OR + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.OR + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.OR + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.OR + (int)Parameters.A, 0x1);
            lol.Add((int)Instructions.CP + (int)Parameters.B, 0x1);
            lol.Add((int)Instructions.CP + (int)Parameters.C, 0x1);
            lol.Add((int)Instructions.CP + (int)Parameters.D, 0x1);
            lol.Add((int)Instructions.CP + (int)Parameters.E, 0x1);
            lol.Add((int)Instructions.CP + (int)Parameters.H, 0x1);
            lol.Add((int)Instructions.CP + (int)Parameters.L, 0x1);
            lol.Add((int)Instructions.CP + (int)Parameters.HL, 0x1);
            lol.Add((int)Instructions.CP + (int)Parameters.A, 0x1);
        }
    }
}
