using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib.Helper;

namespace CentralLib.Protocols
{
    class Protocol_EP06 : BaseProtocol, IProtocols
    {

        //UInt16 MaxStringLenght = 75;

        public Protocol_EP06(int serialPort):base(serialPort)
        {
            MaxStringLenght = 75;
            useCRC16 = false;
            //initial();
        }

        public Protocol_EP06(CentralLib.Connections.DefaultPortCom dComPort):base(dComPort)
        {
            MaxStringLenght = 75;
            useCRC16 = false;
        }

        public Protocol_EP06(string IpAdress, int port):base(IpAdress,port)
        {
            MaxStringLenght = 75;
            useCRC16 = false;
        }


        /// <summary>
        /// Установка обрезчика, в начале запрашиваем данные о состоянии, после включаем
        /// </summary>
        /// <param name="Enable">Если true то включаем, если false то нет</param>
        public override bool setFPCplCutter(bool Enable)
        {
            byte[] forsending = new byte[] { 28, 0x1A, 0x30 };
            byte[] answer = ExchangeWithFP(forsending);
            bool csetCutter = byteHelper.GetBit(answer[0], 3);
            if ((Enable) && (csetCutter))
                FPCplCutter();
            else if ((!Enable)&&(!csetCutter))
                FPCplCutter();
            return statusOperation;
        }

    }
}
