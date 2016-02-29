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
                , string Line2OfAttributesOfTaxpayer
                , string Line3OfAttributesOfTaxpayer
                , string LineOfTaxNumber
                , string VersionOfSWOfECR
            , int ConsecutiveNumber // номер операции что бы не обновлять часто
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

    /// <summary>
    /// Значение битов байта Резерва
    /// </summary>
    public struct strByteReserv
    {
        public byte Reserv { get; }
        /// <summary>
        /// открыт чек служебного отчета
        /// </summary>
        public bool? ReceiptOfServiceReportIsOpened { get; }
        /// <summary>
        /// состояние аварии (команда завершится после устранения ошибки)
        /// </summary>
        public bool? StatusOfEmergency { get; }
        /// <summary>
        /// отсутствие бумаги, если принтер не готов
        /// </summary>
        public bool? PaperIsAbsentInCaseIfPrinterIsntReady { get; }
        /// <summary>
        /// чек: продажи/выплаты (0/1)
        /// </summary>
        public bool? ReceiptSalePayment { get; }
        /// <summary>
        /// принтер фискализирован
        /// </summary>
        public bool? PrinterIsFiscalized { get; }
        /// <summary>
        /// смена открыта
        /// </summary>
        public bool? SessionIsOpened { get; }
        /// <summary>
        /// открыт чек
        /// </summary>
        public bool? ReceiptIsOpened { get; }
        /// <summary>
        /// ЭККР не персонализирован
        /// </summary>
        public bool? ECRIsNotPersonalized { get; }

        public strByteReserv(byte inByte) : this()
        {
            this.Reserv = inByte;
            this.ReceiptOfServiceReportIsOpened = (inByte & (1 << 0 )) != 0;
            this.StatusOfEmergency = (inByte & (1 << 1)) != 0;
            this.PaperIsAbsentInCaseIfPrinterIsntReady = (inByte & (1 << 2)) != 0;
            this.ReceiptSalePayment = (inByte & (1 << 3)) != 0;
            this.PrinterIsFiscalized = (inByte & (1 << 4)) != 0;
            this.SessionIsOpened = (inByte & (1 << 5)) != 0;
            this.ReceiptIsOpened = (inByte & (1 << 6)) != 0;
            this.ECRIsNotPersonalized = (inByte & (1 << 7)) != 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if ((bool)ReceiptOfServiceReportIsOpened)
                sb.Append("открыт чек служебного отчета; ");
            if ((bool)StatusOfEmergency)
                sb.Append("состояние аварии (команда завершится после устранения ошибки); ");
            if ((bool)PaperIsAbsentInCaseIfPrinterIsntReady)
                sb.Append("отсутствие бумаги, если принтер не готов; ");
            if ((bool)ReceiptSalePayment)
                sb.Append("чек: продажи/выплаты (0/1); ");
            if ((bool)PrinterIsFiscalized)
                sb.Append("принтер фискализирован; ");
            if ((bool)SessionIsOpened)
                sb.Append("смена открыта; ");
            if ((bool)ReceiptIsOpened)
                sb.Append("открыт чек; ");
            if ((bool)ECRIsNotPersonalized)
                sb.Append("ЭККР не персонализирован; ");
            return sb.ToString();
        }

    }

    /// <summary>
    /// Описание структуры статуса ФР
    /// </summary>
    public struct strByteStatus
    {
        public byte ByteStatus { get; }
        /// <summary>
        /// принтер не готов
        /// </summary>
        public bool? PrinterNotReady { get; }
        /// <summary>
        /// ошибка модема
        /// </summary>
        public bool? ModemError { get; }
        /// <summary>
        /// ошибка или переполнение фискальной памяти
        /// </summary>
        public bool? ErrorOrFiscalMemoryOverflow { get; }
        /// <summary>
        ///неправильная дата или ошибка часов 
        /// </summary>
        public bool? IncorrectDateOrClockError { get; }
        /// <summary>
        /// ошибка индикатора
        /// </summary>
        public bool? DisplayError { get; }
        /// <summary>
        /// превышение продолжительности смены
        /// </summary>
        public bool? ExceedingOfWorkingShiftDuration { get; }
        /// <summary>
        /// снижение рабочего напряжения питания
        /// </summary>
        public bool? LoweringOfWorkingSupplyVoltage { get; }
        /// <summary>
        /// команда не существует или запрещена в данном режиме
        /// </summary>
        public bool? CommandDoesNotExistOrIsForbiddenInCurrentMode { get; }

        public strByteStatus(byte inByte) : this()
        {
            this.ByteStatus = inByte;
            this.PrinterNotReady = (inByte & (1 <<0)) != 0;
            this.ModemError = (inByte & (1 << 1)) != 0;
            this.ErrorOrFiscalMemoryOverflow = (inByte & (1 << 2)) != 0;
            this.IncorrectDateOrClockError = (inByte & (1 << 3)) != 0;
            this.DisplayError = (inByte & (1 << 4)) != 0;
            this.ExceedingOfWorkingShiftDuration = (inByte & (1 << 5)) != 0;
            this.LoweringOfWorkingSupplyVoltage = (inByte & (1 << 6)) != 0;
            this.CommandDoesNotExistOrIsForbiddenInCurrentMode = (inByte & (1 << 7)) != 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if ((bool)PrinterNotReady)
                sb.Append("принтер не готов.Рекомендуется проверить принтер на предмет заклинивания печатающего механизма и плотного; ");
            if ((bool)ModemError)
                sb.Append("ошибка модема; ");
            if ((bool)ErrorOrFiscalMemoryOverflow)
                sb.Append("ошибка или переполнение фискальной памяти; ");
            if ((bool)IncorrectDateOrClockError)
                sb.Append("неправильная дата или ошибка часов; ");
            if ((bool)DisplayError)
                sb.Append("ошибка индикатора;");
            if ((bool)ExceedingOfWorkingShiftDuration)
                sb.Append("превышение продолжительности смены; ");
            if ((bool)LoweringOfWorkingSupplyVoltage)
                sb.Append("снижение рабочего напряжения питания; ");
            if ((bool)CommandDoesNotExistOrIsForbiddenInCurrentMode)
                sb.Append("команда не существует или запрещена в данном режиме;");
            return sb.ToString();
        }
    }

    public struct strByteResult
    {
        public byte ByteResult { get;  }
        public strByteResult(byte inByte) : this()
        {
            this.ByteResult = inByte;
        }
        public override string ToString()
        {
            if (ByteResult == 0)
                return "нормальное завершение";
            if (ByteResult == 1)
                return "ошибка принтера";
            if (ByteResult == 2)
                return "закончилась бумага";
            if (ByteResult == 3)
                return "";
            if (ByteResult == 4)
                return "сбой фискальной памяти ";
            if (ByteResult == 5)
                return "";
            if (ByteResult == 6)
                return "снижение напряжения питания";
            if (ByteResult == 7)
                return "";
            if (ByteResult == 8)
                return "фискальная память переполнена";
            if (ByteResult == 9)
                return "";
            if (ByteResult == 10)
                return "не было персонализации";
            if (ByteResult == 11)
                return "";
            if (ByteResult == 12)
                return "";
            if (ByteResult == 13)
                return "";
            if (ByteResult == 14)
                return "";
            if (ByteResult == 15)
                return "";
            if (ByteResult == 16)
                return "команда запрещена в данном режиме";
            if (ByteResult == 17)
                return "";
            if (ByteResult == 18)
                return "";
            if (ByteResult == 19)
                return "ошибка программирования логотипа";
            if (ByteResult == 20)
                return "неправильная длина строки";
            if (ByteResult == 21)
                return "неправильный пароль";
            if (ByteResult == 22)
                return "несуществующий номер (пароля, строки)";
            if (ByteResult == 23)
                return "налоговая группа не существует или не установлена, налоги не вводились";
            if (ByteResult == 24)
                return "тип оплат не существует";
            if (ByteResult == 25)
                return "недопустимые коды символов";
            if (ByteResult == 26)
                return "превышение количества налогов";
            if (ByteResult == 27)
                return "отрицательная продажа больше суммы предыдущих продаж чека";
            if (ByteResult == 28)
                return "ошибка в описании артикула";
            if (ByteResult == 29)
                return "";
            if (ByteResult == 30)
                return "ошибка формата даты/времени";
            if (ByteResult == 31)
                return "превышение регистраций в чеке";
            if (ByteResult == 32)
                return "превышение разрядности вычисленной стоимости";
            if (ByteResult == 33)
                return "переполнение регистра дневного оборота";
            if (ByteResult == 34)
                return "переполнение регистра оплат";
            if (ByteResult == 35)
                return "сумма \"выдано\" больше, чем в денежном ящике";
            if (ByteResult == 36)
                return "дата младше даты последнего z-отчета";
            if (ByteResult == 37)
                return "открыт чек выплат, продажи запрещены";
            if (ByteResult == 38)
                return "открыт чек продаж, выплаты запрещены";
            if (ByteResult == 39)
                return "команда запрещена, чек не открыт";
            if (ByteResult == 40)
                return "переполнение памяти артикулов";
            if (ByteResult == 41)
                return "команда запрещена до Z-отчета";
            if (ByteResult == 42)
                return "команда запрещена до фискализации";
            if (ByteResult == 43)
                return "сдача с  этой оплаты запрещена";
            if (ByteResult == 44)
                return "команда запрещена, чек открыт";
            if (ByteResult == 45)
                return "скидки/наценки запрещены, не было продаж";
            if (ByteResult == 46)
                return "команда запрещена после начала оплат";
            if (ByteResult == 47)
                return "превышение продолжительности отправки данный больше 72 часа";
            if (ByteResult == 48)
                return "нет ответа от модема";


            return "";
        }

    }


    public struct PapStat
    {
        public bool? ErrorOfConnectionWithPrinter; //ошибка связи с принтером
        public bool? ReceiptPaperIsAlmostEnded; //чековая лента почти заканчивается
        public bool? ControlPaperIsAlmostEnded; //контрольная лента почти заканчивается        
        public bool? ReceiptPaperIsFinished; //чековая лента закончилась
        public bool? ControlPaperIsFinished; //контрольная лента закончилась

        public PapStat(byte inputByte) : this()
        {
            BitArray _bit = new BitArray(inputByte);
            ErrorOfConnectionWithPrinter = _bit[0];
            ControlPaperIsAlmostEnded = _bit[2];
            ReceiptPaperIsAlmostEnded = _bit[3];
            ControlPaperIsFinished = _bit[5];
            ReceiptPaperIsFinished = _bit[6];
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
            return sb.ToString();
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


        public Tax(byte TaxGroup, ushort TaxNumber, ushort TaxRate, ushort ChargeRates, bool VATAtCharge, bool ChargeAtVAT)
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
