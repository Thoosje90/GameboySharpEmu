using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Hardware
{
    [Serializable]
    internal class MBC : IHardware
    {
        //
        // TODO UPDATE CYCLES FOR RTC TIMER
        //

        // RTC Timer
        RtcTimer mRtcTimer;
        // Rombanks and rambanks
        RomBanks _romBanks;
        RamBanks _ramBanks;
        // MBC 5 HIGH AND LOW BIT
        byte MBC5_HIGHBIT;
        byte MBC5_LOWBIT;

        //
        byte _mbcByte = 0x0; // fuck other types for now
        // Rp, bank
        byte _romSize = 0x0;
        int _romBankIndex = 1;
        // Ram bank
        byte _ramSize = 0x0;
        byte _ramBankIndex = 0x0;

        //
        bool _memoryModel16_8 = false;
        bool _enableRamBank = false;

        MBCTypes _mbcType;

        public enum MBCTypes
        {
            None,
            MBC1,
            MBC2,
            MBC3,
            MBC4,
            MBC5
        }

        public byte BankIndex { set { _romBankIndex = value; } }

        public byte RomSize { set { _romSize = value; } }

        public byte MBCByte { set { _mbcByte = value; } }

        public MBCTypes MBCType { get { return _mbcType; } }

        public byte RamSize { set { _ramSize = value; } get { return _ramSize; } }

        public byte RamIndex { set { _ramBankIndex = value; } }

        public ushort BusAddress { get { return 0x0; } }

        public ushort Size { get { return 0x0; } }

        public MBC(RomBanks catridge, RamBanks rambanks)
        {

            _romBanks = catridge;
            _ramBanks = rambanks;
        }

        public void Initialize()
        {
            // Set memory bank controller type 1,3,5 are supported
            switch (_mbcByte)
            {
                case 0x0: _mbcType = MBCTypes.None; break; 
                case 0x1: _mbcType = MBCTypes.MBC1; break;
                case 0x2: _mbcType = MBCTypes.MBC1; break;
                case 0x3: _mbcType = MBCTypes.MBC1; break;
                case 0x5: _mbcType = MBCTypes.MBC2; break;
                case 0x6: _mbcType = MBCTypes.MBC2; break;
                case 0x8: _mbcType = MBCTypes.None; break; // SUCKS
                //case 0x9: _mbcType = MBCTypes.None; break;
                case 0x10: _mbcType = MBCTypes.MBC3; break;
                //case 0xB: _mbcType = MBCTypes.MBC1; break;
                //case 0xC: _mbcType = MBCTypes.MBC2; break;
                //case 0xD: _mbcType = MBCTypes.MBC2; break;
                case 0x12: _mbcType = MBCTypes.MBC3; break;
                case 0x13: _mbcType = MBCTypes.MBC3; break;
                case 0x19: _mbcType = MBCTypes.MBC5; break;
                case 0x1A: _mbcType = MBCTypes.MBC5; break;
                case 0x1B: _mbcType = MBCTypes.MBC5; break;
                case 0x1C: _mbcType = MBCTypes.MBC5; break;
                case 0x1D: _mbcType = MBCTypes.MBC5; break;
                case 0x1E: _mbcType = MBCTypes.MBC5; break;
                default: _mbcType = MBCTypes.None; break;
            }

            // Initialize rom banks
            _romBanks.InitializeBankSize(_romSize);
            _ramBanks.InitializeBankSize(_ramSize);

            //
            if(_mbcType == MBCTypes.MBC3)
            {
                mRtcTimer = new RtcTimer();
            }
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0 && address <= 0x7FFF)
            {
                // Read address from Ram bank
                return ReadRomBankValue(address);
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (_mbcType == MBCTypes.None || _mbcType == MBCTypes.MBC1 || _mbcType == MBCTypes.MBC5)
                {
                    if (_enableRamBank)
                        // Read address from Ram bank
                        return ReadRamBankValue((ushort)(address - 0xA000));
                    else
                        return 0xFF;
                }
                else if (_mbcType == MBCTypes.MBC3)
                {
                    if (_enableRamBank)
                    {
                        switch (_ramBankIndex)
                        {
                            case 0x0:
                            case 0x1:
                            case 0x2:
                            case 0x3:
                                // Read address from Ram bank
                                return ReadRamBankValue((ushort)(address - 0xA000));
                            case 0x8:
                                return mRtcTimer.RTC_S;
                            case 0x9:
                                return mRtcTimer.RTC_M;
                            case 0xA:
                                return mRtcTimer.RTC_H;
                            case 0xB:
                                return mRtcTimer.RTC_DL;
                            case 0xC:
                                return mRtcTimer.RTC_DH;
                            default:
                                return 0xFF;
                        }
                    }
                    else
                    {
                        return 0xFF;
                    }
                }
            }

            return 0;
        }

        public void Write(ushort address, byte value)
        {
            byte chunk = value;

            // Determine if ram banks where initialized
            if (address <= 0x1FFF)
            {
                if (_mbcType == MBCTypes.MBC1 || 
                    _mbcType == MBCTypes.MBC3 || 
                    _mbcType == MBCTypes.MBC5)
                {
                    if ((chunk) == 0x0A)
                        _enableRamBank = true;
                    else if (chunk == 0x0)
                        _enableRamBank = false;
                }
                else if (_mbcType == MBCTypes.MBC2)
                {
                    //bit 0 of upper byte must be 0
                    if (BitHelper.IsBitSet(address, 8) == false)
                    {
                        if ((chunk & 0xF) == 0xA)
                            _enableRamBank = true;
                        else if (chunk == 0x0)
                            _enableRamBank = false;
                    }
                }
            }
            // Writing to this area changes rom bank (YOLO NINTENDO)
            else if ((address >= 0x2000) && (address < 0x4000))
            {
                if (_mbcType == MBCTypes.MBC1)
                {
                    // Only 5 bits are used
                    _romBankIndex = (byte)(chunk & 0x1F);

                    // Not usable because of gameboy ram bug(fuck fuck fuck)
                    if (_romBankIndex == 0x00 || _romBankIndex == 0x20 || _romBankIndex == 0x40 || _romBankIndex == 0x60)
                    {
                        _romBankIndex++;
                    }
                }
                else if (_mbcType == MBCTypes.MBC3)
                {
                    // Only 7 bits are used
                    _romBankIndex = (byte)(chunk & 0x7F);
                    // Auto increase when rombank number zero
                    if (_romBankIndex == 0x00)
                        _romBankIndex++;
                }
                else if(_mbcType == MBCTypes.MBC5)
                {
                    // this might be retarted
                    if(address < 0x3000)
                    {
                        MBC5_LOWBIT = value;
                    }
                    else if (address < 0x4000)
                    {
                        MBC5_HIGHBIT = value;
                    }

                    _romBankIndex = MBC5_HIGHBIT + MBC5_LOWBIT;
                }    
            }
            // Writing to this area changes ram & rom bank if supported (YOLO NINTENDO)
            else if ((address >= 0x4000) && (address < 0x6000))
            {
                // 
                if (_mbcType == MBCTypes.MBC1)
                {
                    if (_memoryModel16_8 == true)
                    {
                        // Reset ram bank index
                        _ramBankIndex = 0;

                        // Last 5 bits are used
                        _romBankIndex |= (byte)(chunk & 0x3);

                        // Update rombank index
                        if (_romBankIndex == 0x00 || _romBankIndex == 0x20 || _romBankIndex == 0x40 || _romBankIndex == 0x60)
                            _romBankIndex++;
                    }
                    else
                    {
                        // Change rambank index
                        _ramBankIndex = (byte)(chunk & 0x3);
                    }
                }
                else if (_mbcType == MBCTypes.MBC3)
                {
                    // Check if value is vallid ram index or RTCTimer register
                    if (chunk >= 0x0 && chunk <= 0x3 || chunk >= 0x8 && chunk <= 0xC)
                    {
                        // Change rambank index
                        _ramBankIndex = (byte)(chunk);
                    }
                }
                else if (_mbcType == MBCTypes.MBC5)
                {
                    // Change rambank index
                    _ramBankIndex = (byte)(value & 0xF);
                }

            }
            //
            else if ((address >= 0x6000) && (address < 0x8000))
            {
                if (_mbcType == MBCTypes.MBC1)
                {
                    // we're only interested in the first bit
                    if ((chunk & 0x1) == 0x1)
                    {
                        //_ramBankIndex = 0;
                        _memoryModel16_8 = false;
                    }
                    else
                    {
                        _memoryModel16_8 = true;
                    }
                }
                else if (_mbcType == MBCTypes.MBC3) // fix this later
                {
                    //// Write 0 fist
                    //if (mRtcTimer.RTC_LE == 0x0 && value >= 0 && value <=1)
                    //{
                    //    // Update retard register
                    //    mRtcTimer.RTC_LE = 0x1;
                    //}
                    //else if(value == 0x1 && mRtcTimer.RTC_LE == 0x1)  // Latch Clock
                    //{
                        // Create new datetime and latch it to clock
                        DateTime now = DateTime.Now;
                        mRtcTimer.RTC_S = (byte)now.Second;
                        mRtcTimer.RTC_M = (byte)now.Minute;
                        mRtcTimer.RTC_H = (byte)now.Hour;
                        // Miscelanious reset Latch enabler
                        //mRtcTimer.RTC_LE = 0x0;
                        //mRtcTimer.RTC_DL = (byte)now.Day;
                    //}
                }
            }
            // Writing to external ram bank
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if ((_mbcType == MBCTypes.MBC1 || _mbcType == MBCTypes.MBC5) && _enableRamBank)
                {
                    WriteRamBankValue((ushort)(address - 0xA000), (byte)(value));      
                }
                else if (_mbcType == MBCTypes.MBC2 && (address < 0xA200))
                {
                    WriteRamBankValue((ushort)(address - 0xA000), (byte)(value));
                }
                else if (_mbcType == MBCTypes.MBC3 && _enableRamBank)
                {
                    switch (_ramBankIndex)
                    {
                        case 0x0:
                        case 0x1:
                        case 0x2:
                        case 0x3:
                            // Read address from Ram bank
                            WriteRamBankValue((ushort)(address - 0xA000), (byte)(value));
                            break;
                        case 0x8:
                            mRtcTimer.RTC_S = value;
                            break;
                        case 0x9:
                            mRtcTimer.RTC_M = value;
                            break;
                        case 0xA:
                            mRtcTimer.RTC_H = value;
                            break;
                        case 0xB:
                            mRtcTimer.RTC_DL = value;
                            break;
                        case 0xC:
                             mRtcTimer.RTC_DH = value;
                            break;
                    }
                }
            }
        }

        public bool IsWithinRange(ushort address)
        {
            if (address >= 0x0 && address <= 0x7FFF)
            {
                return true;
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                return true;
            }

            return false;
        }

        public byte ReadRomBankValue(ushort address)
        {
            int newBankIndex = _romBankIndex;
            ushort newAddress = address;

            if (_mbcType == MBCTypes.None)
            {
                // Switch bank index
                if (address >= 0x4000)
                {
                    newAddress -= 0x4000;
                    newBankIndex = 2;
                }

                // read bank index every time lol don't trust shit
                return _romBanks.ReadBankValue((byte)(newBankIndex - 1), newAddress);
            }
            else if (_mbcType == MBCTypes.MBC1 || _mbcType == MBCTypes.MBC3 || _mbcType == MBCTypes.MBC5)
            {
                // newBankIndex default = 1 because Bank 0 is fixed
                if (address < 0x4000)
                {
                    newBankIndex = 0;
                }
                else if (address >= 0x4000)
                {
                    newAddress -= 0x4000;
                }

                // read bank index every time lol don't trust your shit
                return _romBanks.ReadBankValue(newBankIndex, newAddress);
            }


            // read bank index every time lol don't trust your shit
            return 0;
        }

        public void WriteRomBank(int bankIndex, byte[] data)
        {
            _romBanks.WriteBank(bankIndex, data);
        }

        public void WriteRomBankValue(ushort address, byte data)
        {
            _romBanks.WriteBankValue(_romBankIndex, address, data);
        }

        public byte ReadRamBankValue(ushort address)
        {
            return _ramBanks.ReadBankValue((byte)_ramBankIndex, address);
        }

        public void WriteRamBankValue(ushort address, byte value)
        {
            _ramBanks.WriteBankValue((byte)_ramBankIndex, address, value);
        }
    }

    class RtcTimer
    {
        // Latch Enabler
        public byte RTC_LE { get; set; } = 0;

        public byte RTC_S {get;set;} // Seconds

        public byte RTC_M { get; set; } // Minutes

        public byte RTC_H { get; set; } // Hours

        public byte RTC_DL { get; set; }

        public byte RTC_DH { get; set; }
    }


}
