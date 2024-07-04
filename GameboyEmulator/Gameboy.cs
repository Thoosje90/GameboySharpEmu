using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using GameboyEmulator.Events;
using GameboyEmulator.Hardware;
using GameboyEmulator.Managers;

namespace GameboyEmulator
{
   
    internal class GameBoy
    {

        public event EventHandler<EventArgs> GameBoyLogEvent;


        int dev = 0;

        // Gameboy refresh rates
        const int DMG_4Mhz = 4194304;
        const float REFRESH_RATE = 59.7275f;
        const int CYCLES_PER_UPDATE = (int)(DMG_4Mhz / REFRESH_RATE);

        // Debug 
        public bool _debugMode = false;
        public bool _printLogs = false;
        public bool _executeNextOpCode = true;
        public bool _runAllOpCode = false;

        // Gameboy State
        public bool _powerOn = false;
        public bool _programLoop = false;

        //mbc5
        private int ROM_BANK_LOWBITS = 1;
        private int ROM_BANK_HIGHBITS;

        // Gameboy Screen State
        public int sizeFactor = 1;
        private bool _widescreenMode = false;

        // Gameboy CPU
        public CPU ZILOG64 { get; set; }

        // Gameboy Addressbus
        public AddressBus16Bit ADDRESSBUS { get; set; }

        // Gameboy Pixel Proccesing Unit
        public PPU @PPU;

        // Gameboy Joypad & Keyboard controller
        public JoyPad JOYPAD;

        // External Gameboy catridge
        Catridge CARTRIDGE;

        // CPU Timers
        Timers TIMERS;

        // Stop watch
        Stopwatch SDL;

        // Gameboy interface Form
        MainForm f;

        // Form Refresh Timer
        System.Windows.Forms.Timer timer;

        // MISCELANIOUS GARBAGE FOR DECODING OPCODES
        OpCodesDecoder DECODER;

        public GameBoy(MainForm form, bool debugMode=false)
        {

            // Set PPU Garbage
            timer = new System.Windows.Forms.Timer();
            timer.Interval = (int)(1000 / 60);
            timer.Tick += Timer_Tick;
            timer.Enabled = true;

            // Initialize Hardware
            ZILOG64 = new CPU();
            ADDRESSBUS = new AddressBus16Bit();
            ADDRESSBUS.cpu = ZILOG64;
            CARTRIDGE = new Catridge();
            DECODER = new OpCodesDecoder();
            // Hook Memory Address Bus to the CPU
            ZILOG64.AddressBus = ADDRESSBUS;
            // Hook Cpu took PPU
            PPU = new PPU(ZILOG64, form);   
            PPU.Reset();
            // Initialize joypad
            JOYPAD = new JoyPad(ADDRESSBUS);
            TIMERS = new Timers(ADDRESSBUS);

            // Micellasnious (DEBUGING)
            //ADDRESSBUS.ppu = PPU;
            SDL = new Stopwatch();
            _debugMode = debugMode;

            //
            f = form;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            f.pictureBox1.Invalidate();
        }

        internal void PowerOn()
        {

            // Start Gameboy
            Thread t = new Thread(() => Start());
            t.Start();
            //Task t = Task.Factory.StartNew(Start, TaskCreationOptions.LongRunning);
        }

        internal int SwitchPallete()
        {
            // Store Current Pallete
            int index = PPU.ColorPallateIndex;

            // Switch Pallete
            PPU.SwitchPallate();

            // Return current pallete index
            return index;
        }

        internal int EnlargeView()
        {
            // Store Current Pallete
            sizeFactor *= 2;

            // Reset size factor at max 8
            if (sizeFactor > 8)
                sizeFactor = 1;

            // Resize PPU (Hack)
            PPU.Resize();
            
            // Return update size factor
            return sizeFactor;
        }

        public int WideScreenView()
        {
            // Toggle widescreen mode
            _widescreenMode = !_widescreenMode;
            return _widescreenMode ? 1 : 0;
        }

        internal void Load(string filepath)
        {
            // Throw exception when file not found
            if (!File.Exists(filepath))
                throw new FileNotFoundException("Rom file not found!");

            // Parse filepaths
            string filename = Path.GetFileNameWithoutExtension(filepath);
            string folderpath = Path.GetDirectoryName(filepath);
            string saveFile = folderpath + "\\" + filename;

            // Extract ROM Save states
            byte[] savedData = new byte[0];
            if (File.Exists(saveFile + ".sav"))
                savedData = File.ReadAllBytes(saveFile + ".sav");

            // Open romfile from diskdrive and copy it to the ROMSTACK
            using (Stream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                // Buffer (unnessary but wanted it myself)
                // 16kb is size of 1 rom bank
                byte[] buffer = new byte[16000];
                
                using (MemoryStream tmpStream = new MemoryStream())
                {
                    // Bytes read offset
                    int bytesRead;
                    // Read bytes into to buffer
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Write buffer to memory stream
                        tmpStream.Write(buffer, 0, bytesRead);
                    }

                    // use rom
                    byte[] rom = tmpStream.ToArray();
                    // Load romfile intro catridge
                    CARTRIDGE.LoadCatridge(rom);

                    if (savedData.Length > 0)
                    {
                        // Get ram banks
                        int ramBanks = RamBanks.TotalBanks(rom[0x0149]);
                        int ramBankOffset = 0;

                        while (ramBankOffset < ramBanks)
                        {
                            //
                            byte[] bank = new byte[8192];
                            for (int offset = 0; offset < 8192; offset++)
                            {
                                bank[offset] = savedData[offset + (ramBankOffset * 8192)];
                            }

                            // Write whole bank from file into SRAM bank
                            CARTRIDGE._ramBanks.WriteBank((byte)ramBankOffset, bank);
                            // Increment rambank offset
                            ramBankOffset++;
                        }
                    }


                }
            }
        }

        internal void Save(string filepath)
        {
            // Create save state file path
            string filename = Path.GetFileNameWithoutExtension(filepath);
            string folderpath = Path.GetDirectoryName(filepath);
            string saveFile = folderpath + "\\" + filename + ".sav";

            // Get Total 
            int totalRamBanks = RamBanks.TotalBanks(CARTRIDGE.MBC.RamSize);

            // Create buffer the size of all ram banks
            byte[] cram = new byte[totalRamBanks * 8912];
            // Current ram bank saved
            int ramCount = 0;

            // Write each ram bank to buffer
            while (ramCount < totalRamBanks)
            {
                //byte[] bank = new byte[8192];
                for (int offset = 0; offset < 8192; offset++)
                {
                    cram[offset + (ramCount * 8192)] = CARTRIDGE._ramBanks.ReadBankValue((byte)ramCount, (ushort)offset);
                }

                ramCount++;
            }

            // Write buffer containing rom save state to file
            File.WriteAllBytes(saveFile, cram);
        }


        internal void Start()
        {
            // Start SDL Timer
            SDL.Start();

            // Fps & Intervals
            float fps = 75.73f;
            float interval = 1000 / fps;

            // Remove hardware from addressbus already assigned 
            ADDRESSBUS.ASSIGN.Clear();

            // Assign hardware to AddressBus
            ZILOG64.AddressBus.ASSIGN = new List<IHardware>()
            {
               new RAM(),
               new VRAM(),
               JOYPAD,
               CARTRIDGE.MBC
            };

            // Reset Default hardware values
            ZILOG64.ResetCPU();

            // Get ellapsed milliesecond since initialization
            long time = 0;// SDL.ElapsedMilliseconds;

            // Fix screen tearing with this later
            //long targetMillis = 1000 / 60,
            //lastTime = (int)SDL.ElapsedMilliseconds,
            //targetTime = lastTime + targetMillis;

            // Set program loop
            _programLoop = true;
            _powerOn = true;

            // Enable form refreshtimer
            timer.Enabled = true;

            // Program loop
            while (_programLoop)
            {
                // Current ellapsed time
                long current = SDL.ElapsedMilliseconds;

                //Update program
                if ((time + interval) < current)
                {
                    // Update gameboy loop
                    Update();
                    // reset time
                    time = current;
                }
            }

            // Turn off gameboy state
            _powerOn = false;
          
            // Unload graphics
            PPU.DestroyGraphics();
        }

        internal void Stop()
        {
            _programLoop = false;
        }

        public void Update()
        {
            // Store cpu update cycles
            int m_CyclesThisUpdate = 0;
            const int m_TargetCycles = 70221;

            // Program LOOP (probably needs the other one)
            while (m_CyclesThisUpdate < CYCLES_PER_UPDATE) // (m_CyclesThisUpdate < m_TargetCycles) 
            {
                // Kill (this works)
                if (!_programLoop)
                    break;


                int newCycles = 4;

                // If CPU not halted
                if (!ZILOG64.HALTED)
                {
                    // Get opcode from memory
                    byte opcode = ZILOG64.AddressBus.Read(ZILOG64.CpuRegisters.PC++);

                    // Read and Execute Unprefixed Opcode 
                    newCycles = ZILOG64.ExecuteOpCode(opcode);
                }
                
                // Upate current update cycle
                m_CyclesThisUpdate += newCycles;
                dev += newCycles;

                // Run timers
                TIMERS.Tick(newCycles);

                // Display graphics
                PPU.DisplayGraphics(newCycles);

                // Handle keyboard to gameboy input
                JOYPAD.HandleInput();

                // Do Interupts
                Interupts.DoInterupts(ZILOG64, ADDRESSBUS);
            }
        }
    }   
}
