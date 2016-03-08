using System.Collections;

namespace CentralLib.Helper
{
    interface IBitHelper
    {
        byte BitArrayToByte(BitArray ba);
        uint ClearBitUInt32(uint Value, byte bit);
        bool GetBit(byte val, int num);
        bool ReadBitUInt32(uint Value, byte bit);
        byte SetBit(byte val, int num, bool bit);
        uint SetBitUInt32(uint Value, byte bit);
        byte[] ToByteArray(BitArray bits);
        uint ToggleBitUInt32(uint Value, byte bit);
        uint WriteBitUInt32(uint Value, byte bit, bool state);
    }
}