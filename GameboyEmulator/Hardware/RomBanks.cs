using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator
{
    [Serializable]
    internal class RomBanks
    {
        byte[][] romBanks;

        public RomBanks()
        {
            // Initialize default
            romBanks = new byte[0][];
        }

        public byte[] GetBank(byte bankIndex)
        {
            // Get entire rom bank
            return romBanks[bankIndex];
        }

        public byte ReadBankValue(int bankIndex, ushort memoryAddress)
        {
            // Read value from proper bank and memory address
            return romBanks[bankIndex][memoryAddress];
        }

        public void WriteBankValue(int bankIndex, ushort memoryAddress, byte value)
        {
            // Write value into proper bank and memory address
            romBanks[bankIndex][memoryAddress] = value;
        }

        public void WriteBank(int bankIndex, byte[] data)
        {
            // Write value into proper bank and memory address
            romBanks[bankIndex] = data;
        }

        public void InitializeBankSize(byte hexCode)
        {
            // Initialize total rombanks
            romBanks = new byte [TotalBanks(hexCode)][];

            // Assign 16kb for each rom bank
            for(int i = 0; i < romBanks.Length; i++)
                romBanks[i] = new byte[16384];
        }

        public static int TotalBanks(byte hexCode)
        {

            switch ((byte)hexCode)
            {
                case 0x0:
                    return 2;
                case 0x1:
                    return 4;
                case 0x2:
                    return 8;
                case 0x3:
                    return 16;
                case 0x4:
                    return 32;
                case 0x5:
                    return 64;
                case 0x6:
                    return 128;
                case 0x52:
                    return 72;
                case 0x53:
                    return 80;
                case 0x54:
                    return 96;
            }

            return 2;
        }
    }


    [Serializable]
    internal class RamBanks
    {
        byte[][] ramBanks;

        public RamBanks()
        {
            // Initialize default
            ramBanks = new byte[0][];
        }

        public byte[] GetBank(byte bankIndex)
        {
            // Get entire rom bank
            return ramBanks[bankIndex];
        }

        public byte ReadBankValue(byte bankIndex, ushort memoryAddress)
        {
            if (ramBanks.Length == 0)
                return (byte)0;

            // Read value from proper bank and memory address
            return ramBanks[bankIndex][memoryAddress];
        }

        public void WriteBankValue(byte bankIndex, ushort memoryAddress, byte value)
        {
            // Write value into proper bank and memory address
            ramBanks[bankIndex][memoryAddress] = value;
        }

        public void WriteBank(byte bankIndex, byte[] data)
        {
            // Write value into proper bank and memory address
            ramBanks[bankIndex] = data;
        }

        public void InitializeBankSize(byte hexCode)
        {
            // Dont initialize
            if (hexCode == 0x0)
                return;

            // Initialize total rombanks
            ramBanks = new byte[TotalBanks(hexCode)][];

            // Assign 16kb for each dam bank
            for (int i = 0; i < ramBanks.Length; i++)
              ramBanks[i] = new byte[0x2000];

            //ramBanks[i] = new byte[
            //    hexCode == 0x0 ? 
            //    0x2000 : 
            //    0x4000];
        }

        public static int TotalBanks(byte hexCode)
        {
            switch ((byte)hexCode)
            {
                case 0x0:
                    return 0; // 2kb
                case 0x1:
                    return 1; // 8kb
                case 0x2:
                    return 2; // 16kb
                case 0x3:
                    return 4; // 32kb
                case 0x4:
                    return 16; // 128kb
            }

            return 0;
        }
    }
}
