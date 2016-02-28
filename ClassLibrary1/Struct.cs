using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Protocols
{
    public struct Status
    {
        public bool usingCollection { get; private set; } //используются сборы
        public bool modeOfRegistrationsOfPayments { get; private set; } //режим регистраций оплат в чеке(запрещены все регистрации  кроме оплат и комментариев)
        public bool cashDrawerIsOpened { get; private set; } //закрыт денежный ящик
        public bool receiptSaleOrPayout { get; private set; } //чек: продажи/выплаты(0/1)
        public bool VATembeddedOrVATaddon { get; private set; } //НДС вложенный/НДС добавляемый(0/1)
        public bool sessionIsOpened { get; private set; } //смена открыта(были закрытые чеки; запрещены команды режима программирования)
        public bool receiptIsOpened { get; private set; } //открыт чек
        public bool usedFontB { get; private set; } //используется шрифт B
        public bool printingOfEndUserLogo { get; private set; } //печать логотипа торговой точки
        public bool paperCuttingForbidden { get; private set; } //запрет обрезчика бумаги
        public bool modeOfPrintingOfReceiptOfServiceReport { get; private set; } //режим печати чека служебного отчета
        public bool printerIsFiscalized { get; private set; } //принтер фискализирован
        public bool emergentFinishingOfLastCommand { get; private set; } //аварийное завершение последней команды
        public bool modeOnLineOfRegistrations { get; private set; } //режим OnLine регистраций
        public string serialNumber { get; private set; }
        public DateTime manufacturingDate { get; private set; }
        public DateTime DateTimeregistration { get; private set; }
        public string fiscalNumber { get; private set; }
        //public int LengthOfLine1OfAttributesOfTaxpayer { get; private set; } //длина строки 1 атрибутов налогоплательщика (= n1)
        public string Line1OfAttributesOfTaxpayer { get; private set; } //строка 1 атрибутов налогоплательщика
        //public int LengthOfLine2OfAttributesOfTaxpayer { get; private set; } //длина строки 2 атрибутов налогоплательщика (= n1)
        public string Line2OfAttributesOfTaxpayer { get; private set; } //строка 2 атрибутов налогоплательщика
        //public int LengthOfLine3OfAttributesOfTaxpayer { get; private set; } //длина строки 3 атрибутов налогоплательщика (= n1)
        public string Line3OfAttributesOfTaxpayer { get; private set; } //строка 3 атрибутов налогоплательщика
        //public int LengthOfLineOfTaxNumber { get; private set; } //  длина строки налогового номера
        public string LineOfTaxNumber { get; private set; } //строка налогового номера
        public string VersionOfSWOfECR { get; private set; } //версия ПО ЭККР (“ЕП-11”)
        public DateTimeOffset setTime;
        public int ConsecutiveNumber;

        public Status(byte[] bitBytes, string SerialAndDate, DateTime DateTimeregistration, string fiscalNumber
                , string Line1OfAttributesOfTaxpayer
                ,  string Line2OfAttributesOfTaxpayer
                ,  string Line3OfAttributesOfTaxpayer
                ,  string LineOfTaxNumber
                , string VersionOfSWOfECR
            ,int ConsecutiveNumber // номер операции что бы не обновлять часто
            )
        {
            this.ConsecutiveNumber = ConsecutiveNumber;
            this.setTime = new DateTimeOffset(DateTime.Now);
            BitArray _bit = new BitArray(bitBytes);
            this.usingCollection = _bit[0];
            this.modeOfRegistrationsOfPayments = _bit[1];
            this.cashDrawerIsOpened = _bit[2];
            this.receiptSaleOrPayout = _bit[3];
            this.VATembeddedOrVATaddon = _bit[4];
            this.sessionIsOpened = _bit[5];
            this.receiptIsOpened = _bit[6];
            this.usedFontB = _bit[8];
            this.printingOfEndUserLogo = _bit[9];
            this.paperCuttingForbidden = _bit[10];
            this.modeOfPrintingOfReceiptOfServiceReport = _bit[11];
            this.printerIsFiscalized = _bit[12];
            this.emergentFinishingOfLastCommand = _bit[13];
            this.modeOnLineOfRegistrations = _bit[14];
            this.serialNumber = SerialAndDate.Substring(0, 19 - 8 - 2);
            int year = Convert.ToInt16(SerialAndDate.Substring(17, 2));
            int month = Convert.ToInt16(SerialAndDate.Substring(14, 2));
            month = Math.Min(Math.Max(month, 1), 12);
            int day = Convert.ToInt16(SerialAndDate.Substring(11, 2));
            day = Math.Min(Math.Max(day, 1), 31);

            this.manufacturingDate = new DateTime(2000 + year, month, day);
            this.DateTimeregistration = DateTimeregistration;
            this.fiscalNumber = fiscalNumber;
            //this.LengthOfLine1OfAttributesOfTaxpayer = Line1OfAttributesOfTaxpayer.Length;
            this.Line1OfAttributesOfTaxpayer = Line1OfAttributesOfTaxpayer;

            //this.LengthOfLine2OfAttributesOfTaxpayer = Line2OfAttributesOfTaxpayer.Length;
            this.Line2OfAttributesOfTaxpayer = Line2OfAttributesOfTaxpayer;

            //this.LengthOfLine3OfAttributesOfTaxpayer = Line3OfAttributesOfTaxpayer.Length;
            this.Line3OfAttributesOfTaxpayer = Line3OfAttributesOfTaxpayer;

            //this.LengthOfLineOfTaxNumber = LineOfTaxNumber.Length;
            this.LineOfTaxNumber = LineOfTaxNumber;

            this.VersionOfSWOfECR = VersionOfSWOfECR;


        }


    }

    public struct PapStat
    {
        public bool? ErrorOfConnectionWithPrinter; //ошибка связи с принтером
        public bool? ReceiptPaperIsAlmostEnded; //чековая лента почти заканчивается
        public bool? ControlPaperIsAlmostEnded; //контрольная лента почти заканчивается        
        public bool? ReceiptPaperIsFinished; //чековая лента закончилась
        public bool? ControlPaperIsFinished; //контрольная лента закончилась

        public PapStat(byte inputByte):this()
        {
            BitArray _bit = new BitArray(inputByte);
            ErrorOfConnectionWithPrinter = _bit[0];
            ControlPaperIsAlmostEnded = _bit[2];
            ReceiptPaperIsAlmostEnded = _bit[3];
            ControlPaperIsFinished = _bit[5];
            ReceiptPaperIsAlmostEnded = _bit[6];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if ((bool)ErrorOfConnectionWithPrinter)
                sb.Append("ошибка связи с принтером;");
            if ((bool)ReceiptPaperIsAlmostEnded)
                sb.Append("чековая лента почти заканчивается;");
            if ((bool)ControlPaperIsAlmostEnded)
                sb.Append("контрольная лента почти заканчивается;");
            if ((bool)ReceiptPaperIsFinished)
                sb.Append("чековая лента закончилась;");
            if ((bool)ControlPaperIsFinished)
                sb.Append("контрольная лента закончилась;");
            return "";
        }
        

    }

    /// <summary>
    /// Описание налогов и что можно с ними делать
    /// </summary>
    public struct Taxes
    {
        public short MaxGroup;
        public DateTime DateSet;
        public ushort quantityOfDecimalDigitsOfMoneySum; // max=3
        public bool VAT; //0 – вложенный, 1 – наложенный
        public Tax TaxA, TaxB, TaxC, TaxD, TaxE;
        public bool ToProgramChargeRates; // = 1 – программировать ставки сборов
        public ushort ChargeRateOfGroupЕ;

    }

    public struct Tax
    {
        public byte TaxGroup;
        public ushort TaxNumber;
        public ushort TaxRate;
        public ushort ChargeRates; //ставки сборов(в 0,01 %) (бит 15 = 1 – НДС на сбор бит 14 = 1 – сбор на НДС)
        public bool VATAtCharge;
        public bool ChargeAtVAT;
        

        public Tax(byte TaxGroup, ushort TaxNumber, ushort TaxRate, ushort ChargeRates, bool VATAtCharge,bool ChargeAtVAT) 
        {
            this.TaxGroup = TaxGroup;
            this.TaxNumber = TaxNumber;
            this.TaxRate = TaxRate;
            this.ChargeRates = ChargeRates;
            this.VATAtCharge = VATAtCharge;
            this.ChargeAtVAT = ChargeAtVAT;

        }
      
    }

    /// <summary>
    /// Информация по продаже
    /// </summary>
    public struct ReceiptInfo
    {
        /// <summary>
        /// стоимость товара или услуги
        /// </summary>
        public Int32 CostOfGoodsOrService;

        /// <summary>
        /// сумма по чеку
        /// </summary>
        public Int32 SumAtReceipt;
    }

    /// <summary>
    /// Возврат после регистрация оплаты
    /// </summary>
    public struct PaymentInfo
    {
        public override string ToString()
        {
            return "Rest: " + Rest + " Renting:" + Renting + " NumberOfReceiptPackageInCPEF:" + NumberOfReceiptPackageInCPEF;
        }
        /// <summary>
        ///  сдача (бит 31 = 1 – сдача)
        /// </summary>
        public UInt32 Renting;
        /// <summary>
        /// остаток
        /// </summary>
        public UInt64 Rest;
        /// <summary>
        /// номер пакета чека в КЛЕФ
        /// </summary>
        public UInt32 NumberOfReceiptPackageInCPEF;
    }

}
