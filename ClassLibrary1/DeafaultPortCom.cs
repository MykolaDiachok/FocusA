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
        
        public byte portNumber { get; protected set; }
        public int baudRate { get; protected set; }
        public int readTimeOut { get; protected set; }
        public int writeTimeOut { get; protected set; }
        public StopBits stopBits { get; protected set; }
        public Parity parity { get; protected set; }
        public int dataBits { get; protected set; }
        public int waiting { get; protected set; }

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
            this.waiting = 200;

        }
    }
}
