using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib;
using System.IO.Ports;

using System.Threading;
using CentralLib.Protocols;

namespace TestComm
{
    class Program
    {
        static void Main(string[] args)
        {


           

            IProtocols pr = SingletonProtocol.Instance(4).GetProtocols();
            //bool op;
            //pr.FPResetOrder();
            //pr.FPSaleEx(1, 0, false, 3000, 0, false, "X-fiter", 1,false);
            //Console.WriteLine("{0}",pr.FPPayment(1, 1000, false, true));
            //Console.WriteLine("{0}", pr.FPPayment(2, 1000, false, true));
            //Console.WriteLine("{0}", pr.FPPayment(3, 3000, true, true));

            //pr.FPCplOnline();

            ////pr.FPGetTaxRate();
            //Taxes tax = new Taxes();
            //tax.MaxGroup = 4;
            //tax.quantityOfDecimalDigitsOfMoneySum = 2;
            //tax.ToProgramChargeRates = false;
            //tax.VAT = false;

            //tax.DateSet = new DateTime();
            //tax.TaxA.TaxGroup = (byte)FPTaxgroup.A;
            //tax.TaxA.TaxNumber = 1;
            //tax.TaxA.TaxRate = 2000;
            ////tax.TaxA.ChargeRates = 500;            
            ////tax.TaxA.VATAtCharge = false;
            ////tax.TaxA.ChargeAtVAT = true;


            //tax.TaxB.TaxGroup = (byte)FPTaxgroup.B;
            //tax.TaxB.TaxNumber = 2;
            //tax.TaxB.TaxRate = 0;
            //////tax.TaxB.ChargeRates = 1;
            //////tax.TaxB.VATAtCharge = true;

            //tax.TaxC.TaxGroup = (byte)FPTaxgroup.C;
            //tax.TaxC.TaxNumber = 3;
            //tax.TaxC.TaxRate = 0;            
            ////tax.TaxC.ChargeRates = 101;
            ////tax.TaxC.VATAtCharge = true;
            ////tax.TaxC.ChargeAtVAT = false;

            //tax.TaxD.TaxGroup = (byte)FPTaxgroup.D;
            //tax.TaxD.TaxNumber = 4;
            //tax.TaxD.TaxRate = 700;            
            //////tax.TaxD.ChargeRates = 0;
            //////tax.TaxD.VATAtCharge = false;
            //////tax.TaxD.VATAtCharge = false;

            //////tax.TaxE.TaxGroup = (byte)FPTaxgroup.E;
            //////tax.TaxE.TaxNumber = 5;
            //////tax.TaxE.TaxRate = 7000;            
            //////tax.TaxE.ChargeRates = 0;
            //////tax.TaxE.VATAtCharge = false;
            //////tax.TaxE.VATAtCharge = false;
            ////tax.ChargeRateOfGroupЕ = 500;
            //pr.FPSetTaxRate(0, tax);


            //Console.WriteLine("{0}", pr.status.fiscalNumber);
            //Console.WriteLine("{0}", pr.status.manufacturingDate);
            //Console.WriteLine("{0}", pr.status.paperCuttingForbidden);
            //Console.WriteLine("{0}", pr.status.printerIsFiscalized);
            //Console.WriteLine("{0}", pr.FPGetPayName((byte)FPTypePays.Splata4));
            //pr.FPRegisterCashier(0, "Master", 0);
            //pr.FPDayClrReport();
            //pr.FPSetHeadLine(0, "The Galactic Empire", true, true, "The Death Star", true, false, "The captian cabin", false, true, "123456789012", false);
            //pr.FPPrintZeroReceipt();
            //pr.FPLineFeed();
            //pr.FPPrintZeroReceipt();
            //pr.FPLineFeed();
            //pr.FPOpenBox(150);
            //pr.FPPrintVer();
            // Console.WriteLine("{0}", pr.FPInToCash(10000));
            //Console.WriteLine("{0}", pr.FPOutOfCash(10000));

            pr.FPDayReport();
            //pr.FPPeriodicReport(0, DateTime.Now.AddDays(-60), DateTime.Now);
            //pr.FPPeriodicReportShort(0, DateTime.Now.AddDays(-60), DateTime.Now);
            //pr.FPPeriodicReport2(0, 0, 1);

            //Console.WriteLine("ErrorOfConnectionWithPrinter:{0}", pr.papStat.ErrorOfConnectionWithPrinter);
            //Console.WriteLine("ReceiptPaperIsAlmostEnded:{0}", pr.papStat.ReceiptPaperIsAlmostEnded);
            //Console.WriteLine("ReceiptPaperIsFinished:{0}", pr.papStat.ReceiptPaperIsFinished);
            //Console.WriteLine("{0}", pr.FPDayClrReport());
            // //op = pr.showBottomString("Begin");
            // //op = pr.getStatus();
            // //op = pr.showBottomString(pr.fpDateTime.ToString());
            // DateTime t= DateTime.Now.AddMinutes(1);
            // int count = 0;
            //while(DateTime.Now<t)
            // {
            //     pr.fpDateTime = DateTime.Now;
            //     count++;
            //     op = pr.showTopString(pr.fpDateTime.ToString());
            //     pr.showBottomString(count.ToString());
            //     //    if ((x % 2) == 0)
            //     //    {
            //     //        op = pr.showBottomString("" + x.ToString());
            //     //    }
            //     //    else
            //     //    {
            //     //        op = pr.showTopString("" + x.ToString());
            //     //    }
            //     //    if (!op)
            //     //    {                    
            //     //            Console.WriteLine("Status:{0}, Result:{1}, Reserv:{2}, Error:{3}", pr.ByteStatus, pr.ByteResult, pr.ByteReserv, pr.errorInfo);                    
            //     //        x--;
            //     //   }
            //     //    op = pr.getStatus();
            //     //  pr.showBottomString(pr.status.VersionOfSWOfECR);
            //     //                pr.showTopString(pr.fpDateTime.ToString());
            //}
            pr.Dispose();

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
