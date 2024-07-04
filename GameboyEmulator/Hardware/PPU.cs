using GameboyEmulator.Events;
using GameboyEmulator.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware
{
    // of 256x256 pixels or 32x32 tiles (8x8 pixels each).
    // Only 160x144 pixels can be displayed on the screen.


    internal class PPU
    {
        public struct Sprite
        {
            public byte Y;
            public byte X;
            public byte Index;
            public byte Flag;
        }

        public int LY { get { return cpu.AddressBus.Memory[SCANLINE]; } set { cpu.AddressBus.Memory[SCANLINE] = (byte)value; } }


        public event InteruptEventHandler RequestInterupt;

        CPU cpu;
        Graphics graphics;
        Bitmap display;
        public DirectBitmap bmp;


        //
        private const int SCREEN_VBLANK_HEIGHT = 153;
        private const int OAM_CYCLES = 80;
        private const int VRAM_CYCLES = 172;
        private const int HBLANK_CYCLES = 204;
        private const int SCANLINE_CYCLES = 456;

        // PPU Registers
        const ushort LCDC = 0xFF40;
        public const int LCD_STAT = 0xFF41;
        const int SCY = 0xFF42;
        const int SCX = 0xFF43;
        const int WY = 0xFF4A;
        const int WX = 0xFF4B;
        const int SCANLINE = 0xFF44;

        //
        private int scanlineCounter;
        // SIN
        private int vblankcount;
        public int hack = 0;

        // Sizes
        const int resolutionWidth = 160;
        const int resolutionHeight = 144;

        ushort[,,] m_CGBBackgroundPalettes = new ushort[8, 4, 2];
        ushort[,,] m_CGBSpritePalettes = new ushort[8, 4, 2];
        // BGB COLOR
        //private int[] color = new int[] { 0x00FFFFFF, 0x00808080, 0x00404040, 0 };

        private int[] colorObj0 = new int[] { 0x00FFFFFF, 0x00FF8584, 0x00833100, 0 };
        //OBJ0
        private int[] colorObj1 = new int[] { 0x00FFFFFF, 0x000000FE, 0x00008300, 0 };
        // OBJ1
        private int[] color = new int[] { 0x00f6f7e9, 0x0076c96b, 0x0061bbc2, 0 };

        //
        public MainForm window;

        public Bitmap Display { get { return display; } }

        public PPU(CPU z80, MainForm f)
        {
            window = f;
            cpu = z80;
        }

        public int ColorPallateIndex = 1;
        public void SwitchPallate()
        {
            GBCPalletes gBCPalletes = new GBCPalletes();
            gBCPalletes.GetPallete(ColorPallateIndex);

            //
            colorObj0 = gBCPalletes.colorObj0;
            colorObj1 = gBCPalletes.colorObj1;
            color = gBCPalletes.color;

            ColorPallateIndex++;
            if (ColorPallateIndex >= 14)
                ColorPallateIndex = 1;
        }

        public void Reset()
        {
            bmp = new DirectBitmap();
            window.pictureBox1.Image = bmp.Bitmap;
            LY = 0;
        }

        public void Resize()
        {
            bmp.Dispose();
            bmp = new DirectBitmap();
            window.pictureBox1.Image = bmp.Bitmap;
        }

        internal void DisplayGraphics(int cycles)
        {
            //
            scanlineCounter += cycles;
            SetLCDStatus(cpu.AddressBus);
        }

        private void drawScanLine(AddressBus16Bit bus16Bit)
        {
            // Read lcdc control
            byte LCDC_CONTROL = bus16Bit.Memory[LCDC];

            // Display Background Tiles
            if (BitHelper.IsBitSet(LCDC_CONTROL, 0)) // Check this function lol
                DisplayTiles(bus16Bit);

            // Display Sprites
            if (BitHelper.IsBitSet(LCDC_CONTROL, 1))
                DisplaySprites(bus16Bit);
        }

        public void RenderFrame()
        {
            // Refresh picture box (lame)
            window.pictureBox1.Invalidate();
        }

        public void DisplayTiles(AddressBus16Bit bus16Bit)
        {
            //
            int scanline = LY;

            // Background scroll coordinates
            byte scrolX, scrolY;
            scrolX = bus16Bit.Read(SCX);
            scrolY = bus16Bit.Read(SCY);

            // Window coordinates on screen
            byte windowX, windowY;
            windowX = (byte)(bus16Bit.Read(WX) - 7);
            windowY = bus16Bit.Memory[WY];

            // Get lcd control flag
            byte flag = bus16Bit.Memory[LCDC];

            // Check if window is being drawn
            bool isWindowed = BitHelper.IsBitSet(flag, 5) && windowY <= scanline;

            // Get scanline on the map and calculate the current row in the tilemap memory
            byte yPos = isWindowed ?
                (byte)(scanline - windowY) :
                (byte)(scanline + scrolY);

            //
            byte tileLine = (byte)((yPos & 7) * 2);
            // Tile map row
            ushort tileMapRow = (ushort)(yPos / 8 * 32);
            // Tile ram address
            ushort tileMap = getTileMapAddress(isWindowed, bus16Bit.Read(LCDC));

            //
            byte low = 0;
            byte hi = 0;

            bool test = false;

            int[] colors = new int[resolutionWidth];
            for (int pixel = 0; pixel < resolutionWidth; pixel++)
            {
                byte xPos = (byte)(pixel + scrolX);

                //
                if ((pixel & 0x7) == 0 || ((pixel + scrolX) & 0x7) == 0)
                {
                    // Get current scanline on screen (y coordinate)
                    // byte scanline = bus16Bit.Read(SCANLINE);
                    ushort tileMapCol = (ushort)(xPos / 8);
                    ushort tileMapAddress = (ushort)(tileMap + tileMapRow + tileMapCol);

                    ushort tileAddress = (ushort)(getTileDataAddress(bus16Bit.Read(LCDC)));

                    // Is Signed (not sure how this works yet(
                    if (BitHelper.IsBitSet(bus16Bit.Memory[LCDC], 4))
                        tileAddress += (ushort)(bus16Bit.Read(tileMapAddress) * 16);
                    else
                        tileAddress += (ushort)(((sbyte)bus16Bit.Read(tileMapAddress) + 128) * 16);

                    // Read tile color index from tileLocation
                    low = bus16Bit.Read((ushort)(tileAddress + tileLine));
                    hi = bus16Bit.Read((ushort)(tileAddress + tileLine + 1));
                }

                if(!colortable.ContainsKey(xPos))
                    colortable[xPos] = (7 - (xPos & 7));

                // Create color index from tile
                int colorBit = colortable[xPos];// 7 - (xPos & 7); //inversed
                int colorIndex = GetColorIndex(colorBit, low, hi);
                int colour = (bus16Bit.Read(0xFF47) >> (colorIndex * 2)) & 0x3;
                colors[pixel] = color[colour]; 
                bmp.SetPixel(pixel, scanline, color[colour]);

            }
        }

        Dictionary<int, int> colortable = new Dictionary<int, int>();

        public void DisplaySprites(AddressBus16Bit bus16Bit)
        {
            // bs ly (saves reading)
            int ly = LY;

            // Offset for sprite attributes in the memory //$FE00-FE9F
            int ramOffset = (0xFE00 + 0x9C);

            // Gameboy can display up to 40 sprites upon a tile background
            for (int spriteOffset = ramOffset; spriteOffset >= 0xFE00; spriteOffset -= 4)
            {
                // Store sprite attributes
                Sprite sprite = new Sprite();
                // Initialize sprite attributes
                sprite.Y = bus16Bit.Read((ushort)(spriteOffset));
                sprite.X = bus16Bit.Read((ushort)(spriteOffset + 1));
                sprite.Index = bus16Bit.Read((ushort)(spriteOffset + 2));
                sprite.Flag = bus16Bit.Read((ushort)(spriteOffset + 3));

                // Set sprite X & Y coordinates (dont trust this)
                sprite.X -= 8;
                sprite.Y -= 16;

                // Determine sprite size mode
                bool is8x16Mode = BitHelper.IsBitSet(bus16Bit.Read(LCDC), 2);
                int spriteSize = is8x16Mode ? 16 : 8;

                // Determine if scan line is within sprite
                if (ly >= sprite.Y && ly < sprite.Y + spriteSize)
                {
                    // Get color pallete
                    byte colorPalette = BitHelper.IsBitSet(sprite.Flag, 4) ?
                        bus16Bit.Memory[0xFF49] :
                        bus16Bit.Memory[0xFF48];

                    int[] hexColorPallete = BitHelper.IsBitSet(sprite.Flag, 4) ?
                        colorObj1 :
                        colorObj0;

                    // Get current scanline on sprite
                    short lineIndex = (short)(ly - sprite.Y);

                    // Fly Y
                    bool flipY = BitHelper.IsBitSet(sprite.Flag, 6);
                    // Flip sprite vertically
                    if (flipY)
                        lineIndex = (short)((spriteSize - 1 - (ly - sprite.Y)));

                    // get scanline address in vram 
                    // increment sprite index times 16 because each row of 1x8 pixels is 2 bytes
                    ushort tileAddress = (ushort)(0x8000 + (sprite.Index * 16) + (lineIndex * 2)); // this is switable!!!!
                    byte data1 = bus16Bit.Read(tileAddress);
                    byte data2 = bus16Bit.Read((ushort)(tileAddress + 1));

                    // Scan sprite row in reverse cuzz faster with bit shifting
                    for (int pi = 0; pi < 8; pi++)
                    {
                        // Determine if sprites are flipped
                        bool flipX = BitHelper.IsBitSet(sprite.Flag, 5);
                        // Swap color bits horizontally
                        int colorBit = flipX ? pi : (7 - pi);

                        // Color color pallete index
                        int colorIndex = GetColorIndex(colorBit, data1, data2);
                        int colorPaletteIndex = ColorPallete.getColorPalleteIndex(colorPalette, colorIndex);

                        // Reverse pixel index
                        int pixel = sprite.X + pi;

                        // Check if pixel within boundary of screen
                        if (pixel >= 0 && pixel < resolutionWidth)
                        {
                            // check if pixel is hidden behind background
                            if (!IsTransparant(colorIndex) && (isAboveBG(sprite.Flag) || 
                                isBackGroundWhite(bus16Bit.Read(0xFF47), pixel, ly)))
                            {
                                // Set bitmap pixel in medium fastest way possible (for now)
                                bmp.SetPixel(pixel, ly, hexColorPallete[colorPaletteIndex]);
                            }
                        }
                    }
                }
            }
        }

        private bool IsTransparant(int colorIndex)
        {
            return (colorIndex == 0);
        }

        private bool isBackGroundWhite(byte BGP, int x, int y)
        {
            int id = BGP & 0x3;
            return bmp.GetPixel(x, y) == color[id];
        }

        private bool isAboveBG(byte attr)
        {
            //Bit7 OBJ-to - BG Priority(0 = OBJ Above BG, 1 = OBJ Behind BG color 1 - 3)
            return attr >> 7 == 0;
        }

        public void DestroyGraphics()
        {
            //// Destroy graphics
            //graphics.Flush();
            //graphics.Dispose();
        }

        private int GetColorIndex(int colorBit, byte low, byte high)
        {
            int hi = (high >> colorBit) & 0x1;
            int lo = (low >> colorBit) & 0x1;
            return (hi << 1 | lo);
        }


        #region Helping Methods

        #region LCD STATUS & MODE

        internal void SetLCDStatus(AddressBus16Bit bus16Bit)
        {
            // Store copy of LCD status that can be edited
            byte lcdStatus = (byte)(bus16Bit.Read(PPU.LCD_STAT) & 0x3);

            // Is LCD NOT ENABLED
            if (!BitHelper.IsBitSet(bus16Bit.Memory[LCDC], 7))
            {
                // Reset scanline
                // (also writes to IO register in memory)
                LY = 0;
                scanlineCounter = 0;
                bus16Bit.Write(LCD_STAT, (byte)(bus16Bit.Read(PPU.LCD_STAT) & ~0x3));
                return;
            }

            // Get current LCD mode
            byte currentMode = (byte)(lcdStatus & 0x3);
            // New LCD mode
            byte newMode;
            // Request new interupt
            bool reqInterupt = false;

            switch (currentMode)
            {
                case 0: //HBLANK - Mode 0 (204 cycles) Total M2+M3+M0 = 456 Cycles
                        //(it fucked jumps to this one for no reason in some games)
                    if (scanlineCounter >= HBLANK_CYCLES)
                    {
                        // Increment scanline
                        // (also writes to IO register in memory)
                        LY++;
                        scanlineCounter -= HBLANK_CYCLES;

                        if (LY == resolutionHeight)
                        {
                            //
                            //check if we arrived Vblank
                            changeSTATMode(1, bus16Bit);
                            Interupts.RequestInterupt(bus16Bit, 0);
                            RenderFrame();

                        }
                        else
                        {
                            //not arrived yet so return to 2
                            changeSTATMode(2, bus16Bit);
                        }
                    }
                    break;
                case 1: //VBLANK - Mode 1 (4560 cycles - 10 lines)
                    if (scanlineCounter >= SCANLINE_CYCLES)
                    {
                        // Increment scanline
                        // (also writes to IO register in memory)
                        LY++;
                        scanlineCounter -= SCANLINE_CYCLES;

                        if (LY > SCREEN_VBLANK_HEIGHT)
                        { //check end of VBLANK
                            changeSTATMode(2, bus16Bit);
                            // Increment scanline
                            // (also writes to IO register in memory)
                            LY = 0;
                        }
                    }
                    break;
                case 2: //Accessing OAM - Mode 2 (80 cycles)
                    if (scanlineCounter >= OAM_CYCLES)
                    {
                        changeSTATMode(3, bus16Bit);
                        scanlineCounter -= OAM_CYCLES;
                    }
                    break;
                case 3: //Accessing VRAM - Mode 3 (172 cycles) Total M2+M3 = 252 Cycles
                    if (scanlineCounter >= VRAM_CYCLES)
                    {
                        if (LY <= resolutionHeight)
                        {
                            drawScanLine(bus16Bit);
                            changeSTATMode(0, bus16Bit);
                            //changeSTATMode(0, bus16Bit);
                            scanlineCounter -= VRAM_CYCLES;
                        }
                        else // very lame bug fix
                        {
                            LY++;
                            //if (scanlineCounter >= SCANLINE_CYCLES)
                            //{
                            //    // (also writes to IO register in memory)
                            //    LY = 0;
                            //    drawScanLine(bus16Bit);
                            //    changeSTATMode(0, bus16Bit);
                            //}
                        }
                    }
                    break;
            }

            // COMPARE LY WITH LYC REGISTER
            if (LY == bus16Bit.Read(0xFF45)) // this asshole jumps to 0
            {
                //handle coincidence Flag
                //mmu.STAT = bitSet(2, bus16Bit.Read(PPU.LCD_STAT));
                bus16Bit.Write(PPU.LCD_STAT, BitHelper.SetBit(bus16Bit.Read(PPU.LCD_STAT), 2));

                if (BitHelper.IsBitSet(bus16Bit.Read(PPU.LCD_STAT), 6))
                {
                    //mmu.requestInterrupt(LCD_INTERRUPT);
                    Interupts.RequestInterupt(bus16Bit, 1);
                }
            }
            else
            {
                // mmu.STAT = bitClear(2, mmu.STAT);
                bus16Bit.Write(PPU.LCD_STAT, BitHelper.ResetBit(bus16Bit.Read(PPU.LCD_STAT), 2));
            }
        }

        private void changeSTATMode(int mode, AddressBus16Bit bus16Bit)
        {
            byte STAT = (byte)(bus16Bit.Read(PPU.LCD_STAT) & ~0x3);
            bus16Bit.Write(PPU.LCD_STAT, (byte)(STAT | mode));

            //Accessing OAM - Mode 2 (80 cycles)
            if (mode == 2 && BitHelper.IsBitSet(STAT, 5))
            {
                // Bit 5 - Mode 2 OAM Interrupt         (1=Enable) (Read/Write)
                Interupts.RequestInterupt(bus16Bit, 1);
            }

            //case 3: //Accessing VRAM - Mode 3 (172 cycles) Total M2+M3 = 252 Cycles
            //HBLANK - Mode 0 (204 cycles) Total M2+M3+M0 = 456 Cycles
            else if (mode == 0 && BitHelper.IsBitSet(STAT, 3))
            {
                // Bit 3 - Mode 0 H-Blank Interrupt     (1=Enable) (Read/Write)
                Interupts.RequestInterupt(bus16Bit, 1);
            }

            //VBLANK - Mode 1 (4560 cycles - 10 lines)
            else if (mode == 1 && BitHelper.IsBitSet(STAT, 4))
            {
                // Bit 4 - Mode 1 V-Blank Interrupt     (1=Enable) (Read/Write)
                Interupts.RequestInterupt(bus16Bit, 1);
            }

        }
        #endregion

        private ushort getTileMapAddress(bool isWindowed, byte flag)
        {
            // Set background tile or window memory
            if (isWindowed)
                // Get window tilemap address
                return BitHelper.IsBitSet(flag, 6) ? (ushort)0x9C00 : (ushort)0x9800;
            else
                // Get BG Tile map address
                return BitHelper.IsBitSet(flag, 3) ? (ushort)0x9C00 : (ushort)0x9800;
        }

        private ushort getTileDataAddress(byte flag)
        {
            // Set proper bus address for tiles
            return BitHelper.IsBitSet(flag, 4) ? (ushort)0x8000 : (ushort)0x8800;

        }

        #endregion

    }

    class ColorPallete
    {
        public enum Colors
        {
            White = 0, // 0x0b11
            DarkGray = 1, //0x0b10,
            LightGray = 2, //0x0b01
            Black = 3 // 0x0b00
        }

        public static Color getColor(int colorPallete, int colorIndex)
        {
            int colour = (colorPallete >> colorIndex * 2) & 0x3;

            switch (colour)
            {
                case 0:
                    return Color.White; // Transparent
                case 1:
                    return Color.LightGray;
                case 2:
                    return Color.DarkGray;
                case 3:
                    return Color.Black;
                default:
                    return Color.Red;
            }
        }

        public static int getColorPalleteIndex(int colorPallete, int colorIndex)
        {
            int colour = (colorPallete >> colorIndex * 2) & 0x3;
            return colour;
        }
    }

    class GBCPalletes
    {
        //private int[] color = new int[] { 0x00FFFFFF, 0x00808080, 0x00404040, 0 };

        public int[] colorObj0 = new int[] { 0x00FFFFFF, 0x00FF8584, 0x00833100, 0 };
        //OBJ0
        public int[] colorObj1 = new int[] { 0x00FFFFFF, 0x000000FE, 0x00008300, 0 };
        // OBJ1
        public int[] color = new int[] { 0x00f6f7e9, 0x0076c96b, 0x0061bbc2, 0 };

        public void GetPallete(int hexCode)
        {
            switch(hexCode)
            {
                case 1:
                    colorObj0 = new int[] { 0x00ffffff, 0x00ffad63, 0x00833100, 0 };
                    colorObj1 = new int[] { 0x00ffffff, 0x00ffad63, 0x00833100, 0 };
                    color = new int[] { 0x00ffffff, 0x00ffad63, 0x00833100, 0 };
                    break;
                case 2:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00008300, 0x00833100, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x0065a49b, 0x002482d2, 0 };
                    color = new int[] { 0x00FFFFFF, 0x00ff8584, 0x00943a3a, 0 };
                    break;
                case 3:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00ffad63, 0x00833100, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x00ffad63, 0x00833100, 0 };
                    color = new int[] { 0x00FFFFFF, 0x00ce9c85, 0x00846b29, 0x005b3109 };
                    break;
                case 4:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00ff8584, 0x00833100, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x00ffad63, 0x000fa20f, 0 };
                    color = new int[] { 0x00FFFFFF, 0x0065a49b, 0x000072fe, 0 };
                    break;
                case 5:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00ff8584, 0x00943a3a, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x00ffad63, 0x00833100, 0 };
                    color = new int[] { 0x00FFFFFF, 0x008b8cde, 0x0053528c, 0 };
                    break;
                case 6:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00808080, 0x00404040, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x00808080, 0x00404040, 0 };
                    color = new int[] { 0x00FFFFFF, 0x00808080, 0x00404040, 0 };
                    break;
                case 7:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00fe9494, 0x009394fe, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x00fe9494, 0x009394fe, 0 };
                    color = new int[] { 0x00FFFFFF, 0x00fe9494, 0x009394fe, 0 };
                    break;
                case 8:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00ffea00, 0x00e31a1a, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x00ffea00, 0x00e31a1a, 0 };
                    color = new int[] { 0x00FFFFFF, 0x00ffea00, 0x00e31a1a, 0 };
                    break;
                case 9:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x0053f009, 0x00e34812, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x0053f009, 0x00e34812, 0 };
                    color = new int[] { 0x00FFFFFF, 0x0053f009, 0x00e34812, 0 };
                    break;
                case 10:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x0065a49b, 0x000054fe, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x007bff30, 0x00008300, 0 };
                    color = new int[] { 0x00FFFFFF, 0x00f4f400, 0x007d4900, 0 };
                    break;
                case 11:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00ff8584, 0x00943a3a, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x00ff8584, 0x00943a3a, 0 };
                    color = new int[] { 0x00FFFFFF, 0x007bff30, 0x000163c6, 0 };
                    break;
                case 12:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00008486, 0x00ffde00, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x00008486, 0x00ffde00, 0 };
                    color = new int[] { 0x00FFFFFF, 0x00008486, 0x00ffde00, 0 };
                    break;
                case 13:
                    colorObj0 = new int[] { 0x00FFFFFF, 0x00FF8584, 0x00833100, 0 };
                    colorObj1 = new int[] { 0x00FFFFFF, 0x000000FE, 0x00008300, 0 };
                    color = new int[] { 0x00f6f7e9, 0x0076c96b, 0x0061bbc2, 0 };
                    break;


    }
        }
    }
}

