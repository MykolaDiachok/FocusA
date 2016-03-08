using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Helper
{
    public class BitHelper : IBitHelper
    {
        public byte BitArrayToByte(BitArray ba)
        {
            byte result = 0;
            for (byte index = 0, m = 1; index < 8; index++, m *= 2)
                result += ba.Get(index) ? m : (byte)0;
            return result;
        }

        public byte[] ToByteArray(BitArray bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        public bool GetBit(byte val, int num)
        {
            if ((num > 7) || (num < 0))//Проверка входных данных
            {
                throw new ArgumentException();
            }
            return ((val >> num) & 1) > 0;//собственно все вычисления
        }

        public byte SetBit(byte val, int num, bool bit)
        {
            if ((num > 7) || (num < 0))//Проверка входных данных
            {
                throw new ArgumentException();
            }
            byte tmpval = 1;
            tmpval = (byte)(tmpval << num);//устанавливаем необходимый бит в единицу
            val = (byte)(val & (~tmpval));//сбрасываем в 0 необходимый бит

            if (bit)// если бит требуется установить в 1
            {
                val = (byte)(val | (tmpval));//то устанавливаем необходимый бит в 1
            }
            return val;
        }
               
        public UInt32 SetBitUInt32(UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            Value |= (UInt32)(1U << bit);
            return Value;
        }

        public UInt32 ClearBitUInt32(UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            Value &= ~(UInt32)(1U << bit);
            return Value;
        }

        public UInt32 WriteBitUInt32(UInt32 Value, byte bit, bool state)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            if (state)
            {
                Value |= (UInt32)(1U << bit);
            }
            else {
                Value &= ~(UInt32)(1U << bit);
            }

            return Value;
        }

        public UInt32 ToggleBitUInt32(UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            if ((Value & (1 << bit)) == (1 << bit))
            {
                Value &= ~(UInt32)(1U << bit);
            }
            else {
                Value |= (UInt32)(1U << bit);
            }

            return Value;
        }

        public bool ReadBitUInt32(UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            return ((Value & (1 << bit)) == (1 << bit));
        }



    }
}
