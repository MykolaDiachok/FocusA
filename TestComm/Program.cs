using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib;
using System.IO.Ports;

using System.Threading;
using CentralLib.DefaultPortCom;
using CentralLib.ConnectionFP;
using CentralLib.Protocols;

namespace TestComm
{
    class Program
    {
        static void Main(string[] args)
        {


            Protocols pr = new Protocols(4);
            bool op;

            op = pr.getStatus();
            for (int x = 0; x < 100000; x++)
            {
                
                if ((x % 2) == 0)
                {
                    op = pr.showBottomString("" + x.ToString());
                }
                else
                {
                    op = pr.showTopString("" + x.ToString());
                }
                if (!op)
                {                    
                        Console.WriteLine("Status:{0}, Result:{1}, Reserv:{2}, Error:{3}", pr.ByteStatus, pr.ByteResult, pr.ByteReserv, pr.errorInfo);                    
                    x--;
               }
                op = pr.getStatus();
                pr.showBottomString(pr.status.VersionOfSWOfECR);
                pr.showTopString(pr.fpDateTime.ToString());
            }
            pr.Dispose();

            //MainAsync();
            //Console.ReadKey();
            //  }


        }

        //static async Task MainAsync()
        //{
        //    DefaultPortCom initialPort = new DefaultPortCom(4);
        //    ConnectionFP connFP = new ConnectionFP(initialPort);
        //    connFP.Open();
        //    ////Provision proConn = new Provision(connFP);
        //    //Console.WriteLine(PrintByteArray(new byte[] { 16, 2, 0, 27, 1, 1, 97, 130, 16, 3, 28, 170 }));
        //    //Console.WriteLine(PrintByteArray(connFP.prepareForSend(new byte[] { 27, 1, 1, 97})));
        //    //new byte[] { 16, 2, 59, 27, 0, 8, 72, 101, 108, 108, 111, 51, 55, 52, 16, 16, 3, 13, 8, } - тут остановка
        //    Task<byte[]> otvet;
        //    byte[] ot;
        //    for (int x = 0; x < 100000;x++)
        //    {
        //        try {
        //            connFP.dataExchange(showTopString("Hello" + x.ToString()));
        //            //otvet = connFP.ExchangeFP(connFP.prepareForSend(showTopString("Hello" + x.ToString())));
        //            //otvet = connFP.dataExchange(showTopString("Hello" + x.ToString()));
        //            //ot = await otvet;
        //            //Console.Write("Текущая операция: {0} ответ аппарата:{1}", connFP.ConsecutiveNumber, PrintByteArray(ot));
        //            //Console.WriteLine("Статус:{0}, Результат:{1}, Резевр:{2}", connFP.ByteStatus, connFP.ByteResult, connFP.ByteReserv);
        //        }
        //        catch(Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //    //otvet = connFP.ExchangeFP(connFP.prepareForSend(new byte[] {14}));
        //    //ot = await otvet;
        //    //Console.WriteLine(PrintByteArray(ot));
        //    //otvet = connFP.ExchangeFP(connFP.prepareForSend(new byte[] { 48 }));
        //    //ot = await otvet;
        //    //Task<byte[]> otvet = connFP.ExchangeFP(new byte[] { 16, 2, 0, 27, 1, 1, 97, 130, 16, 3, 28, 170 });
        //    //byte[] ot = await otvet;
        //    //Console.WriteLine(PrintByteArray(ot));
        //    //connFP.WriteAsync(new byte[] { 16, 2, 0, 27, 1, 1, 97, 130, 16, 3, 28, 170 });

        //    ////proConn.ExchangeData(new byte[] { 16, 2, 0, 27, 1, 1, 97, 130, 16, 3, 28, 170 });


        //    ////proConn.Dispose();
        //    connFP.Close();
        //}


        static byte[] showTopString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));

           return Combine(new byte[] { 0x1b, 0x00, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            //_coding.Add(0x1b);
            //_coding.Add(0x00);
            //_coding.Add((byte)tempStr.Length);
            //byte[] strinfo = cp866.GetBytes(tempStr);
            //_coding.AddRange(strinfo);

            //bool resultcommand;
            //List<byte> rep = ReturnResult(out resultcommand, _coding);

            //return resultcommand;
        }

        static byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }

        static string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
           return sb.ToString();
        }

    }
}
