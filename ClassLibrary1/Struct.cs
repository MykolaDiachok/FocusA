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
        public int LengthOfLine1OfAttributesOfTaxpayer { get; private set; } //длина строки 1 атрибутов налогоплательщика (= n1)
        public string Line1OfAttributesOfTaxpayer { get; private set; } //строка 1 атрибутов налогоплательщика
        public int LengthOfLine2OfAttributesOfTaxpayer { get; private set; } //длина строки 2 атрибутов налогоплательщика (= n1)
        public string Line2OfAttributesOfTaxpayer { get; private set; } //строка 2 атрибутов налогоплательщика
        public int LengthOfLine3OfAttributesOfTaxpayer { get; private set; } //длина строки 3 атрибутов налогоплательщика (= n1)
        public string Line3OfAttributesOfTaxpayer { get; private set; } //строка 3 атрибутов налогоплательщика
        public int LengthOfLineOfTaxNumber { get; private set; } //  длина строки налогового номера
        public string LineOfTaxNumber { get; private set; } //строка налогового номера
        public string VersionOfSWOfECR { get; private set; } //версия ПО ЭККР (“ЕП-11”)
        public DateTimeOffset setTime;
        public int ConsecutiveNumber;

        public Status(byte[] bitBytes, string SerialAndDate, DateTime DateTimeregistration, string fiscalNumber
                , int LengthOfLine1OfAttributesOfTaxpayer, string Line1OfAttributesOfTaxpayer
                , int LengthOfLine2OfAttributesOfTaxpayer, string Line2OfAttributesOfTaxpayer
                , int LengthOfLine3OfAttributesOfTaxpayer, string Line3OfAttributesOfTaxpayer
                , int LengthOfLineOfTaxNumber, string LineOfTaxNumber
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
            this.LengthOfLine1OfAttributesOfTaxpayer = LengthOfLine1OfAttributesOfTaxpayer;
            this.Line1OfAttributesOfTaxpayer = Line1OfAttributesOfTaxpayer;

            this.LengthOfLine2OfAttributesOfTaxpayer = LengthOfLine2OfAttributesOfTaxpayer;
            this.Line2OfAttributesOfTaxpayer = Line2OfAttributesOfTaxpayer;

            this.LengthOfLine3OfAttributesOfTaxpayer = LengthOfLine3OfAttributesOfTaxpayer;
            this.Line3OfAttributesOfTaxpayer = Line3OfAttributesOfTaxpayer;

            this.LengthOfLineOfTaxNumber = LengthOfLineOfTaxNumber;
            this.LineOfTaxNumber = LineOfTaxNumber;

            this.VersionOfSWOfECR = VersionOfSWOfECR;


        }


    }
}
