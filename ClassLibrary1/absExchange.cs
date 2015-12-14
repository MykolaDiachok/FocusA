using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;


namespace CentralLib.ExchangeFP
{
    class Exchange
    {
        private byte[] inputByte;
        private byte[] outputByte;
        public bool statusOperation { get; private set; }
        public byte ByteStatus { get; private set; } // Возврат ФР статус
        public byte ByteResult { get; private set; } // Возврат ФР результат
        public byte ByteReserv { get; private set; } // Возврат ФР результат

        public Exchange()
        {
            initialCrc16();
        }

        private void sendByte(byte[] input)
        {

        }

        private void getByte(byte[] output)
        {

        }

        #region checksum
        byte getchecksum(List<byte> buf)
        {
            int i, n;
            byte lobyte, cs;
            uint sum, res;

            n = buf.Count - 3;
            sum = 0;
            cs = 0x00;
            lobyte = 0x00;

            for (i = 2; i < n; i++)
                sum += buf[i];

            do
            {
                res = sum + cs;
                cs++;
                lobyte = (byte)(res & 0xFF);
            }
            while (lobyte != 0x00);
            return (byte)(cs - 1);
        }

        int getchecksum(byte[] buf, int len)
        {
            int i, sum, n, res;
            byte lobyte, cs;

            n = len - 3;
            sum = 0;
            cs = 0x00;
            lobyte = 0x00;

            for (i = 2; i < n; i++)
                //for (i = 0; i < buf.Length; ++i)
                sum += Convert.ToInt16(buf[i]);

            do
            {
                res = sum + cs;
                cs++;
                lobyte = (byte)(res & 0xFF);
            }
            while (lobyte != 0x00);
            return cs - 1;
        }
        #endregion

        #region CRC16
        ushort[] table = new ushort[256];

        ushort ComputeChecksum(params byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }

        byte[] ComputeChecksumBytes(params byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        void initialCrc16()
        {
            ushort polynomial = (ushort)0x8408;
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }

        #endregion



        #region bit

        byte BitArrayToByte(BitArray ba)
        {
            byte result = 0;
            for (byte index = 0, m = 1; index < 8; index++, m *= 2)
                result += ba.Get(index) ? m : (byte)0;
            return result;
        }

        byte[] ToByteArray(BitArray bits)
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

        bool GetBit(byte val, int num)
        {
            if ((num > 7) || (num < 0))//Проверка входных данных
            {
                throw new ArgumentException();
            }
            return ((val >> num) & 1) > 0;//собственно все вычисления
        }

        byte SetBit(byte val, int num, bool bit)
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

        #endregion

    }
}
