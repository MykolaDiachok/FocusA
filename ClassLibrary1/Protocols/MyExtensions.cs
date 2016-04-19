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
    }
}
