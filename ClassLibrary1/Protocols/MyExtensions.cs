using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Protocols
{
    public static class MyExtensions
    {

        public static UInt16 returnUint16FromBytes(this byte[] inbytes, int index, int step)
        {
            if (step == 2)
            {
                return BitConverter.ToUInt16(inbytes, index);
            }
            if (step < 2)
            {
                byte[] c = new byte[2];
                System.Buffer.BlockCopy(inbytes, index, c, 0, step);
                return BitConverter.ToUInt16(c, 0);
            }            
            throw new ArgumentOutOfRangeException("Привышение допустимого для преобразования в int");
        }


        public static UInt32 returnUint32FromBytes(this byte[] inbytes, int index, int step)
        {
            if (step==4)
            {
                return BitConverter.ToUInt32(inbytes, index);
            }
            if (step<4)
            {
                byte[] c = new byte[4];
                System.Buffer.BlockCopy(inbytes, index, c, 0, step);
                return BitConverter.ToUInt32(c, 0);
            }
            else if (step==5)
            {
                byte[] c = new byte[8];
                System.Buffer.BlockCopy(inbytes, index, c, 0, step);
                return (UInt32)BitConverter.ToUInt64(c,0);
            }
            throw new ArgumentOutOfRangeException("Привышение допустимого для преобразования в int");
        }

        public static byte[] Combine(this byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }

        public static bool GetBit(this byte val, int num)
        {
            if ((num > 7) || (num < 0))//Проверка входных данных
            {
                throw new ArgumentException();
            }
            return ((val >> num) & 1) > 0;//собственно все вычисления
        }

        public static byte SetBit(this byte val, int num, bool bit)
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

        public static UInt32 ClearBitUInt32(this UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            Value &= ~(UInt32)(1U << bit);
            return Value;
        }

    }
}
