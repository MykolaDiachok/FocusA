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



    }
}
