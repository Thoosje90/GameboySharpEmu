using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware
{
    internal class Catridge
    {
        // Memory Bank Controller
        private MBC _mbc;

        // Gameboy Color Compatible Catridges
        public bool GameBoyColor { get; set; }

        // Rom Bank (stores data from the catridge)
        public RomBanks _romBanks { get; set; }

        // Ram Banks (Emulates ram chips on cartridges
        public RamBanks _ramBanks { get; set; }

        public MBC @MBC { get { return _mbc; } }

        public Catridge()
        {
            _romBanks = new RomBanks();
            _ramBanks = new RamBanks();
            _mbc = new MBC(_romBanks, _ramBanks);
        }

        public void LoadCatridge(byte[] bytes)
        {
            // Initialize Memory Bank Controller
            _mbc.MBCByte = bytes[0x0147]; // nooo
            _mbc.RomSize = bytes[0x0148]; // nooo
            _mbc.RamSize = bytes[0x0149]; // nooo
            // This should be checked every read and write
            _mbc.BankIndex = 1; // this is not true for all cases lol

            // Check if catridge compatible with gameboy color
            GameBoyColor = (bytes[0x0143] == 0x80);
            //_mbc.RamIndex = (byte)((bytes[0x6000]) & (byte)((1 << 2) - 1)); // get only first 2 bits (unless ram mode yolo)
            // Load memory bank controller
            _mbc.Initialize();

            // Get total rom banks count (this is retarted)
            int totalBanks = RomBanks.TotalBanks(bytes[0x0148]);
            // Bytes read from ROM
            int readOffset = 0;

            for (int i = 0; i < totalBanks; i++)
            {
                // Copy data from rom to temporary rom bank
                // 16kb "default" rombank size
                byte[] romBank = new byte[16384];

                // Copy data from catridge to rom bank buffer
                Buffer.BlockCopy(bytes, readOffset, romBank, 0, romBank.Length);

                // Write data to rombank
                _mbc.WriteRomBank(i, romBank);

                // Increment offset 16 kb
                readOffset += romBank.Length;
            }   
        }
    }
}
