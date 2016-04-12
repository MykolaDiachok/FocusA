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
            this.ReceiptOfServiceReportIsOpened = (inByte & (1 << 0)) != 0;
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
            this.PrinterNotReady = (inByte & (1 << 0)) != 0;
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
        public byte ByteResult { get; }
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


    /// <summary>
    /// Информация по бумаге
    /// </summary>
    public struct PapStat 
    {
        /// <summary>
        /// ошибка связи с принтером
        /// </summary>
        public bool? ErrorOfConnectionWithPrinter; //ошибка связи с принтером
        /// <summary>
        /// чековая лента почти заканчивается
        /// </summary>
        public bool? ReceiptPaperIsAlmostEnded; //
        /// <summary>
        /// контрольная лента почти заканчивается 
        /// </summary>
        public bool? ControlPaperIsAlmostEnded; //       
        /// <summary>
        /// чековая лента закончилась
        /// </summary>
        public bool? ReceiptPaperIsFinished; //чековая лента закончилась
        /// <summary>
        /// контрольная лента закончилась
        /// </summary>
        public bool? ControlPaperIsFinished; //контрольная лента закончилась

        public PapStat(byte inputByte) : this()
        {
            BitArray _bit = new BitArray(new byte[] { inputByte });
            ErrorOfConnectionWithPrinter = _bit[0];
            ControlPaperIsAlmostEnded = _bit[2];
            ReceiptPaperIsAlmostEnded = _bit[3];
            ControlPaperIsFinished = _bit[5];
            ReceiptPaperIsFinished = _bit[6];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if ((ErrorOfConnectionWithPrinter!=null) &&((bool)ErrorOfConnectionWithPrinter))
                sb.Append("ошибка связи с принтером;");
            if ((ReceiptPaperIsAlmostEnded != null) && ((bool)ReceiptPaperIsAlmostEnded))
                sb.Append("чековая лента почти заканчивается;");
            if ((ControlPaperIsAlmostEnded != null) && ((bool)ControlPaperIsAlmostEnded))
                sb.Append("контрольная лента почти заканчивается;");
            if ((ReceiptPaperIsFinished != null) && ((bool)ReceiptPaperIsFinished))
                sb.Append("чековая лента закончилась;");
            if ((ControlPaperIsFinished != null) && ((bool)ControlPaperIsFinished))
                sb.Append("контрольная лента закончилась;");
            return sb.ToString();
        }


    }


    /// <summary>
    /// 
    /// </summary>
    public struct DayReport
    {
        public ReturnedStruct returnedStruct { get; set; }
        //BitConverter.ToUInt32 = 4 байта
        //BitConverter.ToUInt16 = 2 байта
        public DayReport(byte[] bytesReturn, byte[] bytesReturn0, byte[] bytesReturn1, byte[] bytesReturn2, byte[] bytesReturn3) : this()
        {
            #region bytesReturn
            int tst = 0;
            this.CounterOfSaleReceipts = BitConverter.ToUInt16(bytesReturn, tst); tst += 2;
            this.CounterOfSalesByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments(bytesReturn, ref tst);
            this.DailyMarkupBySale = BitConverter.ToUInt32(bytesReturn, tst); tst += 4;
            this.DailyDiscountBySale = BitConverter.ToUInt32(bytesReturn, tst); tst += 4;
            this.CounterOfPayoutReceipts = BitConverter.ToUInt16(bytesReturn, tst); tst += 2;
            this.CountersOfPayoutByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments(bytesReturn, ref tst);
            this.DailyMarkupByPayouts = BitConverter.ToUInt32(bytesReturn, tst); tst += 4;
            this.DailyDiscountByPayouts = BitConverter.ToUInt32(bytesReturn, tst); tst += 4;
            this.DailySumOfServiceCashGivingOut = BitConverter.ToUInt32(bytesReturn, tst); tst += 4;
            #endregion
            #region bytesReturn0
            tst = 0;
            this.CurrentNumberOfZReport = BitConverter.ToUInt16(bytesReturn0, tst); tst += 2;
            this.CounterOfSalesReceipt = BitConverter.ToUInt16(bytesReturn0, tst); tst += 2;
            this.CounterOfPaymentReceipt = BitConverter.ToUInt16(bytesReturn0, tst); tst += 2;
            {
                string hexday = bytesReturn0[tst].ToString("X"); tst++;
                int tday = Math.Min(Math.Max((int)Convert.ToInt16(hexday), 1), 31);

                string hexmonth = bytesReturn0[tst].ToString("X"); tst++;
                int tmonth = Math.Min(Math.Max((int)Convert.ToInt16(hexmonth), 1), 12);

                string hexyear = bytesReturn0[tst].ToString("X"); tst++;
                int tyear = Convert.ToInt16(hexyear);

                string hexhour = bytesReturn0[tst].ToString("X"); tst++;
                int thour = Convert.ToInt16(hexyear);

                string hexmin = bytesReturn0[tst].ToString("X"); tst++;
                int tmin = Convert.ToInt16(hexyear);
                this.DateTimeOfEndOfShift = new DateTime(2000 + tyear, tmonth, tday, thour, tmin, 0);
                this.DateOfEndOfShift = this.DateTimeOfEndOfShift.ToString("dd.MM.yy");
                this.TimeOfEndOfShift = this.DateTimeOfEndOfShift.ToString("HH:mm");
            }
            {
                string hexday = bytesReturn0[tst].ToString("X"); tst++;
                int tday = Math.Min(Math.Max((int)Convert.ToInt16(hexday), 1), 31);

                string hexmonth = bytesReturn0[tst].ToString("X"); tst++;
                int tmonth = Math.Min(Math.Max((int)Convert.ToInt16(hexmonth), 1), 12);

                string hexyear = bytesReturn0[tst].ToString("X"); tst++;
                int tyear = Convert.ToInt16(hexyear);
                this.dtDateOfTheLastDailyReport = new DateTime(2000 + tyear, tmonth, tday);
                this.DateOfTheLastDailyReport = this.dtDateOfTheLastDailyReport.ToString("dd.MM.yy");
            }

            this.CounterOfArticles = BitConverter.ToUInt16(bytesReturn, tst); tst += 2;
            #endregion
            #region bytesReturn1
            tst = 0;
            this.SumOfTaxByTaxGroupsForOverlayVAT = new SumTaxByTaxGroups(bytesReturn1, ref tst);
            #endregion
            #region bytesReturn2
            tst = 0;
            this.QuantityOfCancelSalesReceipt = BitConverter.ToUInt16(bytesReturn2, tst); tst += 2; //   2  bin
            this.QuantityOfCancelPaymentReceipt = BitConverter.ToUInt16(bytesReturn2, tst); tst += 2; //  2  bin
            this.SumOfCancelSalesReceipt = BitConverter.ToUInt32(bytesReturn2, tst); tst += 4;//   4  bin
            this.SumOfCancelPaymentReceipt = BitConverter.ToUInt32(bytesReturn2, tst); tst += 4; //  4  bin
            this.QuantityOfCancelSales = BitConverter.ToUInt16(bytesReturn2, tst); tst += 2; //   2  bin
            this.QuantityOfCancelPayments = BitConverter.ToUInt16(bytesReturn2, tst); tst += 2;//   2  bin
            this.SumOfCancelSales = BitConverter.ToUInt32(bytesReturn2, tst); tst += 4;  //4  bin        
            this.SumOfCancelPayments = BitConverter.ToUInt32(bytesReturn2, tst); tst += 4;  //4  bin
            #endregion
            #region bytesReturn3
            //TODO реализовать разбор КЛЕФ, при надобности
            #endregion

        }

        public DayReport(byte[] bytesReturn)
        {
            this.CounterOfSaleReceipts = 0;
            this.CounterOfSalesByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments();
            this.DailyMarkupBySale = 0;
            this.DailyDiscountBySale = 0;
            this.CounterOfPayoutReceipts = 0;
            this.CountersOfPayoutByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments();
            this.DailyMarkupByPayouts = 0;
            this.DailyDiscountByPayouts = 0;
            this.DailySumOfServiceCashGivingOut = 0;
            this.CurrentNumberOfZReport = 0;
            this.CounterOfSalesReceipt = 0;
            this.CounterOfPaymentReceipt = 0;

            this.DateTimeOfEndOfShift = DateTime.Now;
            this.DateOfEndOfShift = this.DateTimeOfEndOfShift.ToString("dd.MM.yy");
            this.TimeOfEndOfShift = this.DateTimeOfEndOfShift.ToString("HH:mm");                
            this.dtDateOfTheLastDailyReport = DateTime.Now;
            this.DateOfTheLastDailyReport = null;
            this.SumOfTaxByTaxGroupsForOverlayVAT = new SumTaxByTaxGroups();
            this.CounterOfArticles = 0;                      
            this.QuantityOfCancelSalesReceipt = 0;
            this.QuantityOfCancelPaymentReceipt = 0;
            this.SumOfCancelSalesReceipt = 0;
            this.SumOfCancelPaymentReceipt = 0;
            this.QuantityOfCancelSales = 0;
            this.QuantityOfCancelPayments = 0;
            this.SumOfCancelSales = 0;
            this.SumOfCancelPayments = 0;


            this.DailySumOfServiceCashEntering = 0;
            this.returnedStruct = new ReturnedStruct();
        }

        /// <summary>
        /// счетчик чеков продаж
        /// </summary>
        public int CounterOfSaleReceipts { get; }
        /// <summary>
        /// счетчики продаж по налоговым группам и формам оплат
        /// </summary>
        public SumTaxGroupsAndTypesOfPayments CounterOfSalesByTaxGroupsAndTypesOfPayments { get; }
        /// <summary>
        /// дневная наценка по продажам
        /// </summary>
        public UInt32 DailyMarkupBySale { get; }
        /// <summary>
        /// дневная скидка по продажам
        /// </summary>
        public UInt32 DailyDiscountBySale { get; }
        /// <summary>
        /// дневная сумма служебного вноса
        /// </summary>
        public UInt32 DailySumOfServiceCashEntering { get; }
        /// <summary>
        /// счетчик чеков выплат
        /// </summary>
        public int CounterOfPayoutReceipts { get; }
        /// <summary>
        /// счетчики выплат по налоговым группам и формам оплат 
        /// </summary>
        public SumTaxGroupsAndTypesOfPayments CountersOfPayoutByTaxGroupsAndTypesOfPayments { get; }
        /// <summary>
        /// дневная наценка по выплатам
        /// </summary>
        public UInt32 DailyMarkupByPayouts { get; }
        /// <summary>
        /// дневная скидка по выплатам 
        /// </summary>
        public UInt32 DailyDiscountByPayouts { get; }
        /// <summary>
        /// дневная сумма служебной выдачи
        /// </summary>
        public UInt32 DailySumOfServiceCashGivingOut { get; }
        /// <summary>
        /// текущий номер Z-отчета 
        /// </summary>
        public int CurrentNumberOfZReport { get; }
        /// <summary>
        /// счетчик чеков продаж 
        /// </summary>
        public int CounterOfSalesReceipt { get; }//  2  bin
        /// <summary>
        /// счетчик чеков выплат
        /// </summary>
        public int CounterOfPaymentReceipt { get; }//   2  bin
        /// <summary>
        /// дата конца смены в формате ДДММГГ
        /// </summary>
        public string DateOfEndOfShift { get; }// in format DDMMYY   3  BCD
        /// <summary>
        /// время конца смены в формате ЧЧММ 
        /// </summary>
        public string TimeOfEndOfShift { get; } //in format NNMM   2  BCD
        public DateTime DateTimeOfEndOfShift { get; }
        /// <summary>
        /// дата последнего дневного отчета в формате ДДММГГ 
        /// </summary>
        public string DateOfTheLastDailyReport { get; }// in format DDMMYY   3  BCD
        public DateTime dtDateOfTheLastDailyReport { get; }
        /// <summary>
        /// счетчик артикулов 
        /// </summary>
        public int CounterOfArticles { get; }//  2  bin

        /// <summary>
        /// суммы налогов по налоговым группам для наложенного НДС
        /// </summary>
        public SumTaxByTaxGroups SumOfTaxByTaxGroupsForOverlayVAT { get; } //   4*(6+6)  bin

        /// <summary>
        /// количество аннулированных чеков продаж
        /// </summary>
        public int QuantityOfCancelSalesReceipt { get; } //   2  bin
        /// <summary>
        /// количество аннулированных чеков выплат
        /// </summary>
        public int QuantityOfCancelPaymentReceipt { get; } //  2  bin
        /// <summary>
        /// сумма аннулированных чеков продаж
        /// </summary>
        public UInt32 SumOfCancelSalesReceipt { get; }//   4  bin
        /// <summary>
        /// сумма аннулированных чеков выплат
        /// </summary>
        public UInt32 SumOfCancelPaymentReceipt { get; } //  4  bin
        /// <summary>
        /// количество отказов продаж
        /// </summary>
        public int QuantityOfCancelSales { get; } //   2  bin
        /// <summary>
        /// количество отказов выплат
        /// </summary>
        public int QuantityOfCancelPayments { get; }//   2  bin
        /// <summary>
        /// сумма отказов продаж
        /// </summary>
        public UInt32 SumOfCancelSales { get; }  //4  bin
        /// <summary>
        /// сумма отказов выплат
        /// </summary>
        public UInt32 SumOfCancelPayments { get; }   //4  bin


    }

    public struct SumTaxGroupsAndTypesOfPayments
    {
        public SumTaxGroupsAndTypesOfPayments(byte[] inBytes, ref int ccounter) : this()
        {
            TaxA = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxB = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxC = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxD = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxE = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxF = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;

            Card = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            Credit = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            Check = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            Cash = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            Certificat = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            Voucher = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            ElectronicMoney = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            InsurancePayment = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            OverPayment = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            Payment = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
        }
        //TODO описать счетчики продаж по налоговым группам и формам оплат
        //4*(6+10) 
        /// <summary>
        /// По налогу А
        /// </summary>
        public UInt32 TaxA { get; set; }
        /// <summary>
        /// по налогу B
        /// </summary>
        public UInt32 TaxB;
        /// <summary>
        /// по налогу C
        /// </summary>
        public UInt32 TaxC;
        /// <summary>
        /// по налогу D
        /// </summary>
        public UInt32 TaxD;
        /// <summary>
        /// по налогу E
        /// </summary>
        public UInt32 TaxE;
        /// <summary>
        /// по налогу F
        /// </summary>
        public UInt32 TaxF;
        /// <summary>
        /// Карточка - 0
        /// </summary>
        public UInt32 Card;
        /// <summary>
        /// Кредит - 1
        /// </summary>
        public UInt32 Credit;
        /// <summary>
        /// Чек - 2
        /// </summary>
        public UInt32 Check;
        /// <summary>
        /// Наличка - 3 
        /// </summary>
        public UInt32 Cash;
        /// <summary>
        /// Сертификат -4
        /// </summary>
        public UInt32 Certificat;
        /// <summary>
        /// расписка, поручительство -5
        /// </summary>
        public UInt32 Voucher;
        /// <summary>
        /// электронные деньги -6
        /// </summary>
        public UInt32 ElectronicMoney;
        /// <summary>
        /// страховой платеж -7
        /// </summary>
        public UInt32 InsurancePayment;
        /// <summary>
        /// переплата -8
        /// </summary>
        public UInt32 OverPayment;
        /// <summary>
        /// Оплата
        /// </summary>
        public UInt32 Payment;
    }

    public struct SumTaxByTaxGroups
    {
        //TODO суммы налогов по налоговым группам для наложенного НДС
        //4*(6+6)
        public SumTaxByTaxGroups(byte[] inBytes, ref int ccounter) : this()
        {
            TaxA = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxB = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxC = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxD = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxE = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            TaxF = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;

            VatA = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            VatB = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            VatC = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            VatD = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            VatE = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
            VatF = BitConverter.ToUInt32(inBytes, ccounter); ccounter += 4;
        }

        /// <summary>
        /// По налогу А
        /// </summary>
        public UInt32 TaxA { get; set; }
        /// <summary>
        /// по налогу B
        /// </summary>
        public UInt32 TaxB;
        /// <summary>
        /// по налогу C
        /// </summary>
        public UInt32 TaxC;
        /// <summary>
        /// по налогу D
        /// </summary>
        public UInt32 TaxD;
        /// <summary>
        /// по налогу E
        /// </summary>
        public UInt32 TaxE;
        /// <summary>
        /// по налогу F
        /// </summary>
        public UInt32 TaxF;

        /// <summary>
        /// По вложенному налогу А
        /// </summary>
        public UInt32 VatA { get; set; }
        /// <summary>
        /// по вложенному налогу B
        /// </summary>
        public UInt32 VatB;
        /// <summary>
        /// по вложенному налогу C
        /// </summary>
        public UInt32 VatC;
        /// <summary>
        /// по вложенному налогу D
        /// </summary>
        public UInt32 VatD;
        /// <summary>
        /// по вложенному налогу E
        /// </summary>
        public UInt32 VatE;
        /// <summary>
        /// по вложенному налогу F
        /// </summary>
        public UInt32 VatF;
    }

    /// <summary>
    /// Описание налогов и что можно с ними делать
    /// </summary>
    public struct Taxes
    {
        public ReturnedStruct returnedStruct { get; set; }
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
        public ReturnedStruct returnedStruct { get; set; }
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
        public ReturnedStruct returnedStruct { get; set; }
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
