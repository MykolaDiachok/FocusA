using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace CentralLib.DefaultPortCom
{
    public class DefaultPortCom
    {
        
        public byte portNumber;
        public int baudRate;
        public int readTimeOut;
        public int writeTimeOut;
        public StopBits stopBits;
        public Parity parity;
        public int dataBits;

        public string sPortNumber
        {
            get
            {
                return "Com" + portNumber.ToString();
            }
        }

        public DefaultPortCom(byte portNumber)
        {
            this.portNumber = portNumber;
            this.baudRate = 9600;
            this.parity = Parity.None;
            this.stopBits = StopBits.One;
            this.readTimeOut = 1000;
            this.writeTimeOut = 1000;
            this.dataBits = 8;

        }
    }
}
