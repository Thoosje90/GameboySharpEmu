using static GameboyEmulator.Hardware.enums.AsmEnums;

namespace GameboyEmulator.Hardware
{
    // Emulates 8bit, 16bit CPU Registers
    // By Thoosje 2022

    internal class CpuRegister
    {

        public CpuRegister()
        {

        }

        // CPU Registers 8bit
        public byte A, B, C, D, E, F, H, L;

        // CPU Registers 16bit
        public ushort AF { get { return (ushort)(A << 8 | F); } set { A = (byte)(value >> 8); F = (byte)(value & 0xF0); } }
        public ushort BC { get { return (ushort)(B << 8 | C); } set { B = (byte)(value >> 8); C = (byte)value; } }
        public ushort DE { get { return (ushort)(D << 8 | E); } set { D = (byte)(value >> 8); E = (byte)value; } }
        public ushort HL { get { return (ushort)(H << 8 | L); } set { H = (byte)(value >> 8); L = (byte)value; } }

        public ushort PC;

        public ushort SP;

        /// <summary>
        /// Get Byte Value From CPU Register
        /// </summary>
        public byte GetValue(Parameters p)
        {
            switch (p)
            {
                case Parameters.A:
                    return A;

                case Parameters.B:
                    return B;

                case Parameters.C:
                    return C;

                case Parameters.D:
                    return D;

                case Parameters.E:
                    return E;

                case Parameters.F:
                    return F;

                case Parameters.H:
                    return H;

                case Parameters.L:
                    return L;

                default:
                    return 0;

            }
        }

        /// <summary>
        /// Get UShort Value From CPU Register
        /// </summary>
        public void GetValue(Parameters p, out ushort value)
        {
            switch (p)
            {
                case Parameters.AF:
                    value = AF;
                    break;
                case Parameters.BC:
                    value = BC;
                    break;
                case Parameters.DE:
                    value = DE;
                    break;
                case Parameters.HL:
                    value = HL;
                    break;
                case Parameters.SP:
                    value = SP;
                    break;
                case Parameters.PC:
                    value = PC;
                    break;
                default:
                    value = 0;
                    break;
            }
        }

        /// <summary>
        /// Set Byte Value in CPU Register
        /// </summary>
        public void SetValue(Parameters p, byte value)
        {
            switch (p)
            {
                case Parameters.A:
                    A = value;
                    break;
                case Parameters.B:
                    B = value;
                    break;
                case Parameters.C:
                    C = value;
                    break;
                case Parameters.D:
                    D = value;
                    break;
                case Parameters.E:
                    E = value;
                    break;
                case Parameters.F:
                    F = value;
                    break;
                case Parameters.H:
                    H = value;
                    break;
                case Parameters.L:
                    L = value;
                    break;
                case Parameters.HL:
                    H = 0;
                    HL = (ushort)(value + (H << 8));
                    break;
            }
        }

        /// <summary>
        /// Set UShort Value in CPU Register
        /// </summary>
        public void SetValue(Parameters p, ushort value)
        {
            switch (p)
            {
                case Parameters.A:
                    A = (byte)(value >> 8);
                    break;
                case Parameters.B:
                    B = (byte)(value >> 8);
                    break;
                case Parameters.C:
                    C = (byte)(value >> 8);
                    break;
                case Parameters.D:
                    D = (byte)(value >> 8);
                    break;
                case Parameters.E:
                    E = (byte)(value >> 8);
                    break;
                case Parameters.F:
                    F = (byte)(value >> 8);
                    break;
                case Parameters.H:
                    H = (byte)(value >> 8);
                    break;
                case Parameters.L:
                    L = (byte)(value >> 8);
                    break;
                case Parameters.AF:
                    AF = value;
                    break;
                case Parameters.BC:
                    BC = value;
                    break;
                case Parameters.DE:
                    DE = value;
                    break;
                case Parameters.HL:
                    HL = value;
                    break;
                case Parameters.SP:
                    SP = value;
                    break;
                case Parameters.PC:
                    PC = value;
                    break;
            }
        }

    }
}
