using CentralLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Helper
{
    public class ByteHelper : BitHelper,IByteHelper
    {
        public UInt32 Max3ArrayBytes = BitConverter.ToUInt32(new byte[] { 255, 255, 255, 0 }, 0);
        public UInt64 Max6ArrayBytes = BitConverter.ToUInt64(new byte[] { 255, 255, 255, 255, 255, 255, 0, 0 }, 0);
        public byte[] bytesBegin = { (byte)WorkByte.DLE, (byte)WorkByte.STX };
        public byte[] bytesEnd = { (byte)WorkByte.DLE, (byte)WorkByte.ETX };

        /// <summary>
        /// Для объединение массивов байт
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
        /// <summary>
        /// поиск в массиве байт первого вхождения из массива байт
        /// </summary>
        /// <param name="searchIn"></param>
        /// <param name="searchBytes"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0)
        {
            int found = -1;
            bool matched = false;
            //only look at this if we have a populated search array and search bytes with a sensible start 
            if (searchIn.Length > 0 && searchBytes.Length > 0 && start <= (searchIn.Length - searchBytes.Length) && searchIn.Length >= searchBytes.Length)
            {
                //iterate through the array to be searched 
                for (int i = start; i <= searchIn.Length - searchBytes.Length; i++)
                {
                    //if the start bytes match we will start comparing all other bytes 
                    if (searchIn[i] == searchBytes[0])
                    {
                        if (searchIn.Length > 1)
                        {
                            //multiple bytes to be searched we have to compare byte by byte 
                            matched = true;
                            for (int y = 1; y <= searchBytes.Length - 1; y++)
                            {
                                if (searchIn[i + y] != searchBytes[y])
                                {
                                    matched = false;
                                    break;
                                }
                            }
                            //everything matched up 
                            if (matched)
                            {
                                found = i;
                                break;
                            }

                        }
                        else
                        {
                            //search byte is only one bit nothing else to do 
                            found = i;
                            break; //stop the loop 
                        }

                    }
                }

            }
            return found;
        }
        /// <summary>
        /// Возврат массива байт без дубликатов DLE {DLE, DLE}
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public byte[] returnWithOutDublicateDLE(byte[] source)
        {
            return returnWithOutDublicate(source, new byte[] { (byte)WorkByte.DLE, (byte)WorkByte.DLE });
        }
        /// <summary>
        /// Возврат массива байт без дубликатов 
        /// </summary>
        /// <param name="source">массив байтов для поиска</param>
        /// /// <param name="pattern">патерн поиска</param>
        /// <returns></returns>
        public byte[] returnWithOutDublicate(byte[] source, byte[] pattern)
        {

            List<byte> tReturn = new List<byte>();
            int sLenght = source.Length;
            for (int i = 0; i < sLenght; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    tReturn.Add(source[i]);
                    i++;
                }
                else
                {
                    tReturn.Add(source[i]);
                }
            }
            return (byte[])tReturn.ToArray();
        }
        /// <summary>
        /// Поиск в массиве байт, исходя из массива байт
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public int? PatternAt(byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }
            return null;
        }
        /// <summary>
        /// Еще один метод вывода массива байтов в строку, не оптимальный
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// из массива байт получаем дату, используется 3 байта подряд
        /// </summary>
        /// <param name="inputByte">Массив бай</param>
        /// <param name="index">начальный идекс для 1 байта</param>
        /// <returns></returns>
        public DateTime returnDatefromByte(byte[] inputByte, int index = 0)
        {
            string hexday = inputByte[index].ToString("X");
            int _day = Math.Min(Math.Max((int)Convert.ToInt16(hexday), 1), 31);
            index++;
            string hexmonth = inputByte[index].ToString("X");
            int _month = Math.Min(Math.Max((int)Convert.ToInt16(hexmonth), 1), 12);
            index++;
            string hexyear = inputByte[index].ToString("X");
            int _year = Convert.ToInt16(hexyear);

            return new DateTime(2000 + _year, _month, _day, 0, 0, 0);
        }

        public byte[] ConvertUint32ToArrayByte3(UInt32 inputValue)
        {
            if (inputValue > Max3ArrayBytes)
            {
                throw new System.ArgumentOutOfRangeException("input value", "Превышение максимального значения");
            }
            byte[] tByte = BitConverter.GetBytes(inputValue);
            return new byte[] { tByte[0], tByte[1], tByte[2] };
        }

        public byte[] ConvertUint64ToArrayByte6(UInt64 inputValue)
        {
            if (inputValue > Max6ArrayBytes)
            {
                throw new System.ArgumentOutOfRangeException("input value", "Превышение максимального значения");
            }
            byte[] tByte = BitConverter.GetBytes(inputValue);
            return new byte[] { tByte[0], tByte[1], tByte[2], tByte[3], tByte[4], tByte[5] };
        }

        /// <summary>
        /// Для конвертации uint32 в массив из 6 байт
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="needCountArray"></param>
        /// <returns></returns>
        public byte[] ConvertTobyte(UInt32? inputValue, int needCountArray = 6)
        {
            UInt32 tValue = inputValue.GetValueOrDefault();


            byte[] forreturn = BitConverter.GetBytes(tValue);
            if (forreturn.Length != needCountArray)
            {
                byte[] addzerobyte = new Byte[needCountArray - forreturn.Length];
                forreturn = Combine(forreturn, addzerobyte);
            }
            return forreturn;
        }

        /// <summary>
        /// Строку кодируем в байты
        /// </summary>
        /// <param name="InputString">Входящая строка</param>
        /// <param name="MaxVal">Максимальная длина строка</param>
        /// <param name="length">Возвращаем количество байт после кодировки</param>
        /// <returns></returns>
        public byte[] CodingBytes(string InputString, UInt16 MaxVal, out byte length)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = InputString.Substring(0, Math.Min(MaxVal, InputString.Length));
            length = (byte)tempStr.Length;
            return cp866.GetBytes(tempStr);
        }

        /// <summary>
        /// Из строки формируем массив байт
        /// </summary>
        /// <param name="InputString">Строка для преобразования</param>
        /// <param name="MaxVal">Макс длина строки</param>
        /// <returns>Возврат массив байт из строки + вначале байт с длиной строки</returns>
        public byte[] CodingStringToBytesWithLength(string InputString, UInt16 MaxVal)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = InputString.Substring(0, Math.Min(MaxVal, InputString.Length));
            //length = (byte)tempStr.Length;

            return Combine(new byte[] { (byte)tempStr.Length }, cp866.GetBytes(tempStr));
        }

        /// <summary>
        /// Раскодируем массив байт и возвращаем строку
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string EncodingBytes(byte[] inputBytes, int index = 0, int length = 0)
        {
            if (length == 0)
                length = inputBytes.Length;
            Encoding cp866 = Encoding.GetEncoding(866);
            return cp866.GetString(inputBytes, index, length);
        }
    
        /// <summary>
        /// Ввыести массив байтов в строку, сделано для отладки
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string PrintByteArrayX(byte[] bytes)
        {
            if (bytes.Length > 0)
                return BitConverter.ToString(bytes).Replace("-", " ");
            else
                return "";
        }

        #region forProtocol
        public byte[] prepareForSend(int ConsecutiveNumber, byte[] BytesForSend, bool useCRC16 = false, bool repeatError = false) // тут передают только код и параметры, получают готовую строку для отправки
        {
            //this.glbytesPrepare = BytesForSend;

            byte[] prBytes = Combine(new byte[] { (byte)WorkByte.DLE, (byte)WorkByte.STX, (byte)ConsecutiveNumber }, BytesForSend);
            prBytes = Combine(prBytes, new byte[] { 0x00, (byte)WorkByte.DLE, (byte)WorkByte.ETX });
            prBytes[prBytes.Length - 3] = getchecksum(prBytes);

            for (int pos = 2; pos < prBytes.Length - 2; pos++)
            //for (int pos = 2; pos <= _out.Count - 3; pos++)
            {
                if (prBytes[pos] == (byte)WorkByte.DLE)
                {
                    prBytes = prBytes.Take(pos)
                    .Concat(new byte[] { (byte)WorkByte.DLE })
                    .Concat(prBytes.Skip(pos))
                    .ToArray();
                    //   prBytes..Insert(pos + 1, DLE);
                    pos++;
                }
            }
            if (useCRC16)
                prBytes = returnArrayBytesWithCRC16(prBytes);
            return prBytes;

        }


        public byte[] returnBytesWithoutSufixAndPrefix(byte[] inputbytes, bool useCRC16 = false)
        {
            int lenght = inputbytes.Length - 7 - 3 - ((useCRC16) ? 2 : 0);
            byte[] outputBytes = new byte[lenght];
            System.Buffer.BlockCopy(inputbytes, 7, outputBytes, 0, lenght);
            return outputBytes;
        }

        #region checksum
        public byte getchecksum(List<byte> buf)
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

        public byte getchecksum(byte[] buf)
        {
            int i, sum, n, res;
            byte lobyte, cs;

            n = buf.Length - 3;
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
            return (byte)(cs - 1);
        }

        public int getchecksum(byte[] buf, int len)
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
        private ushort[] table = new ushort[256];

        public ushort ComputeChecksum(params byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }

        public byte[] ComputeChecksumBytes(params byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public void initialCrc16()
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


        public byte[] returnArrayBytesWithCRC16(byte[] inBytes)
        {

            //byte[] bytebegin = { DLE, STX };
            //byte[] byteend = { DLE, ETX };

            byte[] tempb = returnWithOutDublicateDLE(inBytes);
            var searchBegin = PatternAt(tempb, bytesBegin);
            if (searchBegin == null)
                return null;

            var searchEnd = PatternAt(tempb, bytesEnd);
            if (searchEnd == null)
                return null;

            var newArr = tempb.Skip((int)searchBegin + 2).Take((int)searchEnd - 2).ToArray();

            byte[] a = new byte[newArr.Length + 1];
            newArr.CopyTo(a, 0);
            a[newArr.Length] = (byte)WorkByte.ETX;


            //var control = tempb.Skip((int)searchEnd + 2).Take(2).ToArray();


            byte[] crcBytes = ComputeChecksumBytes(a);
            byte[] retBytes = new byte[inBytes.Length + 2];
            inBytes.CopyTo(retBytes, 0);
            retBytes[retBytes.Length - 2] = crcBytes[0];
            retBytes[retBytes.Length - 1] = crcBytes[1];
            return retBytes;
        }

        #endregion
        #endregion
    }
}
