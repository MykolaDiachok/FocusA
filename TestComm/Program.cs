using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib;
using System.IO.Ports;

using System.Threading;
using CentralLib.Protocols;
using System.Net.Sockets;
using CentralLib.Helper;
using System.Net;
using System.IO;

namespace TestComm
{


    class Program
    {
        static int ConsecutiveNumber = 0;
        static ByteHelper byteHelper = new ByteHelper();


        static byte[] prepareForSend(byte[] BytesForSend, bool useCRC16 = false, bool repeatError = false) // тут передают только код и параметры, получают готовую строку для отправки
        {
            //this.glbytesPrepare = BytesForSend;

            byte[] prBytes = byteHelper.Combine(new byte[] { (byte)WorkByte.DLE, (byte)WorkByte.STX, (byte)ConsecutiveNumber }, BytesForSend);
            prBytes = byteHelper.Combine(prBytes, new byte[] { 0x00, (byte)WorkByte.DLE, (byte)WorkByte.ETX });
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
            //if (useCRC16)
            //    prBytes = returnArrayBytesWithCRC16(prBytes);

            ConsecutiveNumber++;
            return prBytes;

        }

        static byte getchecksum(byte[] buf)
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

        private static async void StartClient()
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse("192.168.1.98"), 4001);
            
            using (var networkStream = client.GetStream())         
            {
                //byte[] msg = prepareForSend(new byte[] { 9, 0,0 });
                byte[] msg = prepareForSend(new byte[] { 0 });
                byte[] tr = byteHelper.Combine(msg, new byte[1024]);
                await networkStream.WriteAsync(msg, 0, msg.Length);
                Thread.Sleep(0);
                byte[] buffer = new byte[1024];
                //while (networkStream.DataAvailable)
                while(true)
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    Console.WriteLine("{0}", byteHelper.PrintByteArrayX(buffer.Take(bytesRead).ToArray()));
                    if (bytesRead <= 0)
                        break;
                    Thread.Sleep(40);
                }                                    
            }
            if (client != null)
            {
                client.Close();
            }

        }


        static void Main(string[] args)
        {
            //byte[] bytes = new byte[1024];

            byteHelper = new ByteHelper();
            ConsecutiveNumber = 1;
            //StartClient();

            //подготовка к отправке: 10 02 00 1C 00 1E C6 10 03
            //подготовка к отправке: 10020100FF1003
            //подготовка к отправке: 1002000D0000F31003
            //подготовка к отправке: 10 02 01 0F F0 10 03
            //подготовка к отправке: 10 02 02 09 00 00 F5 10 03





            BaseProtocol pr = SingletonProtocol.Instance(4).GetProtocols();
            //BaseProtocol pr = SingletonProtocol.Instance("192.168.1.98",4001).GetProtocols();            
            pr.setFPCplCutter(true);
            pr.FPNullCheck();
            pr.FPNullCheck();
            pr.setFPCplCutter(false);
            pr.FPNullCheck();
            pr.FPNullCheck();
            pr.setFPCplCutter(true);
            pr.FPDayClrReport();
            pr.setFPCplCutter(false);
            ////pr.FPResetOrder();
            pr.FPDayReport();
            pr.Dispose();


            ////pr.FPDayClrReport();
            ////bool op;
            ////pr.FPNullCheck();
            //// 
            //var tR = pr.FPSaleEx(1, 0, false, 1000, 0, false, "123456789012345678901234567890123456789012345678901234567890", 6, false);
            //var ss = pr.FPPayment(3, 5000, false, true);
            ////Console.WriteLine("in box: {0}", pr.GetMoneyInBox());

            ////pr.FPResetOrder();
            ////tR = pr.FPPayMoneyEx(1, 0, false, 995, 0, false, "Сигарети L&M Loft Sea Blue  Харкiв п. МРЦ 10,00", 3,false);
            ////ss = pr.FPPayment(3, 995, false, true);
            ////Console.WriteLine("in box: {0}", pr.GetMoneyInBox());
            ////pr.FPCplCutter();
            ////Console.WriteLine("{0}",pr.FPPayment(1, 1000, false, true));
            ////Console.WriteLine("{0}", pr.FPPayment(2, 1000, false, true));
            ////Console.WriteLine("{0}", pr.FPPayment(3, 3000, true, true));

            ////pr.FPCplOnline();

            //////pr.FPGetTaxRate();
            ////Taxes tax = new Taxes();
            ////tax.MaxGroup = 4;
            ////tax.quantityOfDecimalDigitsOfMoneySum = 2;
            ////tax.ToProgramChargeRates = false;
            ////tax.VAT = false;

            ////tax.DateSet = new DateTime();
            ////tax.TaxA.TaxGroup = (byte)FPTaxgroup.A;
            ////tax.TaxA.TaxNumber = 1;
            ////tax.TaxA.TaxRate = 2000;
            //////tax.TaxA.ChargeRates = 500;            
            //////tax.TaxA.VATAtCharge = false;
            //////tax.TaxA.ChargeAtVAT = true;


            ////tax.TaxB.TaxGroup = (byte)FPTaxgroup.B;
            ////tax.TaxB.TaxNumber = 2;
            ////tax.TaxB.TaxRate = 0;
            ////////tax.TaxB.ChargeRates = 1;
            ////////tax.TaxB.VATAtCharge = true;

            ////tax.TaxC.TaxGroup = (byte)FPTaxgroup.C;
            ////tax.TaxC.TaxNumber = 3;
            ////tax.TaxC.TaxRate = 0;            
            //////tax.TaxC.ChargeRates = 101;
            //////tax.TaxC.VATAtCharge = true;
            //////tax.TaxC.ChargeAtVAT = false;

            ////tax.TaxD.TaxGroup = (byte)FPTaxgroup.D;
            ////tax.TaxD.TaxNumber = 4;
            ////tax.TaxD.TaxRate = 700;            
            ////////tax.TaxD.ChargeRates = 0;
            ////////tax.TaxD.VATAtCharge = false;
            ////////tax.TaxD.VATAtCharge = false;

            ////////tax.TaxE.TaxGroup = (byte)FPTaxgroup.E;
            ////////tax.TaxE.TaxNumber = 5;
            ////////tax.TaxE.TaxRate = 7000;            
            ////////tax.TaxE.ChargeRates = 0;
            ////////tax.TaxE.VATAtCharge = false;
            ////////tax.TaxE.VATAtCharge = false;
            //////tax.ChargeRateOfGroupЕ = 500;
            ////pr.FPSetTaxRate(0, tax);


            ////Console.WriteLine("{0}", pr.status.fiscalNumber);
            ////Console.WriteLine("{0}", pr.status.manufacturingDate);
            ////Console.WriteLine("{0}", pr.status.paperCuttingForbidden);
            ////Console.WriteLine("{0}", pr.status.printerIsFiscalized);
            ////Console.WriteLine("{0}", pr.FPGetPayName((byte)FPTypePays.Splata4));
            ////pr.FPRegisterCashier(0, "Master", 0);
            ////pr.FPDayClrReport();
            ////pr.FPSetHeadLine(0, "The Galactic Empire", true, true, "The Death Star", true, false, "The captian cabin", false, true, "123456789012", false);
            ////pr.FPPrintZeroReceipt();
            ////pr.FPLineFeed();
            ////pr.FPPrintZeroReceipt();
            ////pr.FPLineFeed();
            ////pr.FPOpenBox(150);
            ////pr.FPPrintVer();
            //// Console.WriteLine("{0}", pr.FPInToCash(10000));
            ////Console.WriteLine("{0}", pr.FPOutOfCash(10000));

            ////pr.FPDayReport();
            ////pr.FPPeriodicReport(0, DateTime.Now.AddDays(-60), DateTime.Now);
            ////pr.FPPeriodicReportShort(0, DateTime.Now.AddDays(-60), DateTime.Now);
            ////pr.FPPeriodicReport2(0, 0, 1);

            ////Console.WriteLine("ErrorOfConnectionWithPrinter:{0}", pr.papStat.ErrorOfConnectionWithPrinter);
            ////Console.WriteLine("ReceiptPaperIsAlmostEnded:{0}", pr.papStat.ReceiptPaperIsAlmostEnded);
            ////Console.WriteLine("ReceiptPaperIsFinished:{0}", pr.papStat.ReceiptPaperIsFinished);
            ////Console.WriteLine("{0}", pr.FPDayClrReport());
            //// //op = pr.showBottomString("Begin");
            //// //op = pr.getStatus();
            //// //op = pr.showBottomString(pr.fpDateTime.ToString());
            //// DateTime t= DateTime.Now.AddMinutes(1);
            //// int count = 0;
            ////while(DateTime.Now<t)
            //// {
            ////     pr.fpDateTime = DateTime.Now;
            ////     count++;
            ////     op = pr.showTopString(pr.fpDateTime.ToString());
            ////     pr.showBottomString(count.ToString());
            ////     //    if ((x % 2) == 0)
            ////     //    {
            ////     //        op = pr.showBottomString("" + x.ToString());
            ////     //    }
            ////     //    else
            ////     //    {
            ////     //        op = pr.showTopString("" + x.ToString());
            ////     //    }
            ////     //    if (!op)
            ////     //    {                    
            ////     //            Console.WriteLine("Status:{0}, Result:{1}, Reserv:{2}, Error:{3}", pr.ByteStatus, pr.ByteResult, pr.ByteReserv, pr.errorInfo);                    
            ////     //        x--;
            ////     //   }
            ////     //    op = pr.getStatus();
            ////     //  pr.showBottomString(pr.status.VersionOfSWOfECR);
            ////     //                pr.showTopString(pr.fpDateTime.ToString());
            ////}
            //pr.Dispose();

            //MainAsync();
            Console.ReadKey();
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



    }
}
