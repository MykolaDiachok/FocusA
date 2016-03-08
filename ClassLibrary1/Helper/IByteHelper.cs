using System;

namespace CentralLib.Helper
{
    interface IByteHelper
    {
        int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0);
        byte[] CodingBytes(string InputString, ushort MaxVal, out byte length);
        byte[] CodingStringToBytesWithLength(string InputString, ushort MaxVal);
        byte[] Combine(byte[] a, byte[] b);
        byte[] ConvertTobyte(uint? inputValue, int needCountArray = 6);
        byte[] ConvertUint32ToArrayByte3(uint inputValue);
        byte[] ConvertUint64ToArrayByte6(ulong inputValue);
        string EncodingBytes(byte[] inputBytes, int index = 0, int length = 0);
        int? PatternAt(byte[] source, byte[] pattern);
        string PrintByteArray(byte[] bytes);
        DateTime returnDatefromByte(byte[] inputByte, int index = 0);
        byte[] returnWithOutDublicate(byte[] source, byte[] pattern);
        byte[] returnWithOutDublicateDLE(byte[] source);
    }
}