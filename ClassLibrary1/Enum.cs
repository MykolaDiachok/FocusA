using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib
{
    public enum WorkByte:byte
    {
        //Коды служебных символов
        DLE = (byte)0x10,
        STX = (byte)0x02,
        ETX = (byte)0x03,
        ACK = (byte)0x06,
        NAK = (byte)0x15,
        SYN = (byte)0x16,
        ENQ = (byte)0x05
    }

    public enum WorkProtocol
    {
        unknown,
        EP06, //http://www.ics-market.com.ua/ru/oborudovanie/fiskalnoe-oborudovanie/download/2179/456/40
        EP07,
        EP08,
        EP09,
        EP11 //http://www.ics-market.com.ua/ru/oborudovanie/fiskalnoe-oborudovanie/fiskalnye-registratory/download/2771/1874/40
    }

    public enum FPSpeed
    {
        S9600 = 0,
        S19200 = 1,
        S38400 = 2
    }

    public enum FPSpeedUSB
    {
        S2400=0,
        S4800=1,
        S9600=2,
        S19200=3,
        S38400=4,
        S57600=5,
        S115200=6,
    }
   
    public enum FPTypePays
    {
        Card = 0x00,
        Credit = 0x01,
        Check = 0x02,
        Cash = 0x03        
    }

    public enum FPTaxgroup
    {
        A=0x80,
        B=0x81,
        C=0x82,
        D=0x83,
        E=0x84
            
    }

}
