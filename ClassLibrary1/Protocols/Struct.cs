using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CentralLib.Protocols
{
    public class Status
    {
        public ReturnedStruct returnedStruct { get; private set; }
        public strByteReserv infoReserv { get; private set; }
        public strByteStatus infoStatus { get; private set; }
        public strByteResult infoResult { get; private set; }

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
            , ReturnedStruct returnedStruct
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
            Match match = Regex.Match(SerialAndDate, @"([^\s]+)\s+(\d{2})-(\d{2})-(\d{2})");            
            if (match.Success)
            {

                this.serialNumber = match.Groups[1].Value;
                int tyear = Convert.ToInt16(match.Groups[4].Value);
                int tmonth = Convert.ToInt16(match.Groups[3].Value);
                tmonth = Math.Min(Math.Max(tmonth, 1), 12);
                int tday = Convert.ToInt16(match.Groups[2].Value);
                tday = Math.Min(Math.Max(tday, 1), 31);
                this.manufacturingDate = new DateTime(2000 + tyear, tmonth, tday);
            }

            //this.serialNumber = SerialAndDate.Substring(0, 19 - 9);
            
            //int year = Convert.ToInt16(SerialAndDate.Substring(17, 2));
            //int month = Convert.ToInt16(SerialAndDate.Substring(14, 2));
            
            //int day = Convert.ToInt16(SerialAndDate.Substring(11, 2));
            //day = Math.Min(Math.Max(day, 1), 31);

            
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

            this.returnedStruct = returnedStruct;
            this.infoReserv = new strByteReserv(returnedStruct.ByteReserv);
            this.infoStatus = new strByteStatus(returnedStruct.ByteStatus);
            this.infoResult = new strByteResult(returnedStruct.ByteResult);
        }


    }

    /// <summary>
    /// Значение битов байта Резерва
    /// </summary>
    public class strByteReserv
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

        public strByteReserv(byte inByte)
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
    public class strByteStatus
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

        public strByteStatus(byte inByte)
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

    public class strByteResult
    {
        public byte ByteResult { get; }
        public strByteResult(byte inByte)
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
    public class PapStat
    {
        public ReturnedStruct returnedStruct { get; private set; }
        public strByteReserv infoReserv { get; private set; }
        public strByteStatus infoStatus { get; private set; }
        public strByteResult infoResult { get; private set; }

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

        public PapStat(byte inputByte, ReturnedStruct returnedStruct)
        {
            BitArray _bit = new BitArray(new byte[] { inputByte });
            ErrorOfConnectionWithPrinter = _bit[0];
            ControlPaperIsAlmostEnded = _bit[2];
            ReceiptPaperIsAlmostEnded = _bit[3];
            ControlPaperIsFinished = _bit[5];
            ReceiptPaperIsFinished = _bit[6];

            this.returnedStruct = returnedStruct;
            this.infoReserv = new strByteReserv(returnedStruct.ByteReserv);
            this.infoStatus = new strByteStatus(returnedStruct.ByteStatus);
            this.infoResult = new strByteResult(returnedStruct.ByteResult);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if ((ErrorOfConnectionWithPrinter != null) && ((bool)ErrorOfConnectionWithPrinter))
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
    public class DayReport
    {
        public ReturnedStruct returnedStruct { get; private set; }
        public strByteReserv infoReserv { get; private set; }
        public strByteStatus infoStatus { get; private set; }
        public strByteResult infoResult { get; private set; }
        //BitConverter.ToUInt32 = 4 байта
        //BitConverter.ToUInt16 = 2 байта
        public DayReport(IProtocols protocol, ReturnedStruct returnedStruct)
        {
            this.returnedStruct = returnedStruct;
            this.infoReserv = new strByteReserv(returnedStruct.ByteReserv);
            this.infoStatus = new strByteStatus(returnedStruct.ByteStatus);
            this.infoResult = new strByteResult(returnedStruct.ByteResult);
        }

        public DayReport(IProtocols protocol, ReturnedStruct returnedStruct, params byte[][] inbytes)
        {
            this.returnedStruct = returnedStruct;
            this.infoReserv = new strByteReserv(returnedStruct.ByteReserv);
            this.infoStatus = new strByteStatus(returnedStruct.ByteStatus);
            this.infoResult = new strByteResult(returnedStruct.ByteResult);
            if (protocol.GetType() == typeof(Protocol_EP11))
            {
                DayReportEP11(inbytes[0], inbytes[1], inbytes[2], inbytes[3], inbytes[4]);
                return;
            }
            else if ((protocol.GetType() == typeof(Protocol_EP06)) && (inbytes.Length == 1))
            {
                DayReportEP06(inbytes[0]);
                return;
            }
            else if (protocol.GetType() == typeof(Protocol_EP06))
            {
                DayReportEP06_fromMemory(inbytes);
                return;
            }
            throw new ApplicationException("не удалось определить протокол");
        }


        public DayReport(IProtocols protocol, params byte[][] inbytes)
        {
            if (protocol.GetType() == typeof(Protocol_EP11))
            {
                DayReportEP11(inbytes[0], inbytes[1], inbytes[2], inbytes[3], inbytes[4]);
                return;
            }
            else if ((protocol.GetType() == typeof(Protocol_EP06)) && (inbytes.Length == 1))
            {
                DayReportEP06(inbytes[0]);
                return;
            }
            else if (protocol.GetType() == typeof(Protocol_EP06))
            {
                DayReportEP06_fromMemory(inbytes);
                return;
            }
            throw new ApplicationException("не удалось определить протокол");
        }

        public void DayReportEP06_fromMemory(params byte[][] inArrayBytes)
        {
            this.CounterOfSaleReceipts = inArrayBytes[0].returnUint16FromBytes(0, 2);
            this.CounterOfSalesByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments(inArrayBytes[1], 0, 5, true); //tst += 80;
            this.DailyMarkupBySale = inArrayBytes[2].returnUint32FromBytes(0, 5);
            this.DailyDiscountBySale = inArrayBytes[3].returnUint32FromBytes(0, 5);
            this.DailySumOfServiceCashEntering = inArrayBytes[4].returnUint32FromBytes(0, 5);
            this.CounterOfPayoutReceipts = inArrayBytes[5].returnUint16FromBytes(0, 2);
            this.CountersOfPayoutByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments(inArrayBytes[6], 0, 5, true); //tst += 80;
            this.DailyMarkupByPayouts = inArrayBytes[7].returnUint32FromBytes(0, 5);
            this.DailyDiscountByPayouts = inArrayBytes[8].returnUint32FromBytes(0, 5);
            this.DailySumOfServiceCashGivingOut = inArrayBytes[9].returnUint32FromBytes(0, 5);
            this.CurrentNumberOfZReport = inArrayBytes[10].returnUint16FromBytes(0, 2);
            this.CounterOfSalesReceipt = inArrayBytes[11].returnUint16FromBytes(0, 2); // BitConverter.ToUInt16(bytesReturn0, tst); tst += 2;
            this.CounterOfPaymentReceipt = inArrayBytes[12].returnUint16FromBytes(0, 2);// BitConverter.ToUInt16(bytesReturn0, tst); tst += 2;
            {
                int tst = 0;
                string hexday = inArrayBytes[13][tst].ToString("X"); tst++;
                int tday = 0;
                if (!int.TryParse(hexday, out tday))
                    tday = 1;
                else
                    tday = Math.Min(Math.Max(tday, 1), 31);

                string hexmonth = inArrayBytes[13][tst].ToString("X"); tst++;
                int tmonth = 0;
                if (!int.TryParse(hexmonth,out  tmonth))
                    tmonth = 1;
                else
                    tmonth = Math.Min(Math.Max(tmonth, 1), 12);

                string hexyear = inArrayBytes[13][tst].ToString("X"); tst++;
                int tyear = 0;
                if (!int.TryParse(hexyear, out tyear))
                    tyear = 1;
                else
                    tyear = Math.Min(tyear, 99);


                tst = 0;
                string hexmin = inArrayBytes[14][tst].ToString("X"); tst++;
                int tmin = 0;
                if (!int.TryParse(hexmin, out tmin))
                    tmin = 0;
                else
                    tmin = Math.Min(tmin, 59);
                //int tmin = Convert.ToInt16(hexmin);

                string hexhour = inArrayBytes[14][tst].ToString("X"); tst++;
                int thour = 0;
                if (!int.TryParse(hexhour, out thour))
                    thour = 0;
                else
                    thour = Math.Min(thour, 23);
                //Convert.ToInt16(hexhour);





                this.DateTimeOfEndOfShift = new DateTime(2000 + tyear, tmonth, tday, thour, tmin, 0);
                this.DateOfEndOfShift = this.DateTimeOfEndOfShift.ToString("dd.MM.yy");
                this.TimeOfEndOfShift = this.DateTimeOfEndOfShift.ToString("HH:mm");
            }
            {
                int tst = 0;
                string hexday = inArrayBytes[15][tst].ToString("X"); tst++;
                int tday = Math.Min(Math.Max((int)Convert.ToInt16(hexday), 1), 31);

                string hexmonth = inArrayBytes[15][tst].ToString("X"); tst++;
                int tmonth = Math.Min(Math.Max((int)Convert.ToInt16(hexmonth), 1), 12);

                string hexyear = inArrayBytes[15][tst].ToString("X"); tst++;
                int tyear = Convert.ToInt16(hexyear);
                this.dtDateOfTheLastDailyReport = new DateTime(2000 + tyear, tmonth, tday);
                this.DateOfTheLastDailyReport = this.dtDateOfTheLastDailyReport.ToString("dd.MM.yy");
            }
            this.CounterOfArticles = inArrayBytes[16].returnUint16FromBytes(0, 2); ;
            this.SumOfTaxByTaxGroupsForOverlayVAT = new SumTaxByTaxGroups(inArrayBytes[1], inArrayBytes[17], 0, 5);

            this.QuantityOfCancelSalesReceipt = inArrayBytes[18].returnUint16FromBytes(0, 2); //   2  bin
            this.QuantityOfCancelPaymentReceipt = inArrayBytes[19].returnUint16FromBytes(0, 2); //  2  bin
            this.SumOfCancelSalesReceipt = inArrayBytes[20].returnUint32FromBytes(0, 5);//   4  bin
            this.SumOfCancelPaymentReceipt = inArrayBytes[21].returnUint32FromBytes(0, 5); //  4  bin
            this.QuantityOfCancelSales = inArrayBytes[22].returnUint16FromBytes(0, 2); //   2  bin
            this.QuantityOfCancelPayments = inArrayBytes[23].returnUint16FromBytes(0, 2);//   2  bin
            this.SumOfCancelSales = inArrayBytes[24].returnUint32FromBytes(0, 5);  //4  bin        
            this.SumOfCancelPayments = inArrayBytes[25].returnUint32FromBytes(0, 5);  //4  bin
        }


        /// <summary>
        /// Заполняем класс исходя из данных для протокола ЕП11
        /// </summary>
        /// <param name="bytesReturn"></param>
        /// <param name="bytesReturn0"></param>
        /// <param name="bytesReturn1"></param>
        /// <param name="bytesReturn2"></param>
        /// <param name="bytesReturn3"></param>
        public void DayReportEP11(byte[] bytesReturn, byte[] bytesReturn0, byte[] bytesReturn1, byte[] bytesReturn2, byte[] bytesReturn3)
        {
            #region bytesReturn
            int tst = 0;
            this.CounterOfSaleReceipts = BitConverter.ToUInt16(bytesReturn, tst); tst += 2;
            this.CounterOfSalesByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments(bytesReturn, ref tst);
            this.DailyMarkupBySale = BitConverter.ToUInt32(bytesReturn, tst); tst += 4;
            this.DailyDiscountBySale = BitConverter.ToUInt32(bytesReturn, tst); tst += 4;
            this.DailySumOfServiceCashEntering = BitConverter.ToUInt32(bytesReturn, tst); tst += 4;
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
                int thour = Convert.ToInt16(hexhour);

                string hexmin = bytesReturn0[tst].ToString("X"); tst++;
                int tmin = Convert.ToInt16(hexmin);
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

        
        /// <summary>
        /// Если приход запрос на 42
        /// </summary>
        /// <param name="bytesReturn"></param>
        public void DayReportEP06(byte[] bytesReturn)
        {
            int tst = 0;
            this.CounterOfSaleReceipts = bytesReturn.returnUint16FromBytes(tst, 2); tst += 2;
            this.CounterOfSalesByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments(bytesReturn, ref tst, 5); //tst += 80;
            this.DailyMarkupBySale = bytesReturn.returnUint32FromBytes(tst, 5); tst += 5;
            this.DailyDiscountBySale = bytesReturn.returnUint32FromBytes(tst, 5); tst += 5;
            this.DailySumOfServiceCashEntering = bytesReturn.returnUint32FromBytes(tst, 5); tst += 5;
            this.CounterOfPayoutReceipts = BitConverter.ToUInt16(bytesReturn, tst); tst += 2;
            this.CountersOfPayoutByTaxGroupsAndTypesOfPayments = new SumTaxGroupsAndTypesOfPayments(bytesReturn, ref tst, 5); //tst += 80;
            this.DailyMarkupByPayouts = bytesReturn.returnUint32FromBytes(tst, 5); tst += 5;
            this.DailyDiscountByPayouts = bytesReturn.returnUint32FromBytes(tst, 5); tst += 5;
            this.DailySumOfServiceCashGivingOut = bytesReturn.returnUint32FromBytes(tst, 5); tst += 5;
        }

        public DayReport()
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
        public int CounterOfSaleReceipts { get; private set; }
        /// <summary>
        /// счетчики продаж по налоговым группам и формам оплат
        /// </summary>
        public SumTaxGroupsAndTypesOfPayments CounterOfSalesByTaxGroupsAndTypesOfPayments { get; private set; }
        /// <summary>
        /// дневная наценка по продажам
        /// </summary>
        public UInt32 DailyMarkupBySale { get; private set; }
        /// <summary>
        /// дневная скидка по продажам
        /// </summary>
        public UInt32 DailyDiscountBySale { get; private set; }
        /// <summary>
        /// дневная сумма служебного вноса
        /// </summary>
        public UInt32 DailySumOfServiceCashEntering { get; private set; }
        /// <summary>
        /// счетчик чеков выплат
        /// </summary>
        public int CounterOfPayoutReceipts { get; private set; }
        /// <summary>
        /// счетчики выплат по налоговым группам и формам оплат 
        /// </summary>
        public SumTaxGroupsAndTypesOfPayments CountersOfPayoutByTaxGroupsAndTypesOfPayments { get; private set; }
        /// <summary>
        /// дневная наценка по выплатам
        /// </summary>
        public UInt32 DailyMarkupByPayouts { get; private set; }
        /// <summary>
        /// дневная скидка по выплатам 
        /// </summary>
        public UInt32 DailyDiscountByPayouts { get; private set; }
        /// <summary>
        /// дневная сумма служебной выдачи
        /// </summary>
        public UInt32 DailySumOfServiceCashGivingOut { get; private set; }
        /// <summary>
        /// текущий номер Z-отчета 
        /// </summary>
        public int CurrentNumberOfZReport { get; private set; }
        /// <summary>
        /// счетчик чеков продаж 
        /// </summary>
        public int CounterOfSalesReceipt { get; private set; }//  2  bin
        /// <summary>
        /// счетчик чеков выплат
        /// </summary>
        public int CounterOfPaymentReceipt { get; private set; }//   2  bin
        /// <summary>
        /// дата конца смены в формате ДДММГГ
        /// </summary>
        public string DateOfEndOfShift { get; private set; }// in format DDMMYY   3  BCD
        /// <summary>
        /// время конца смены в формате ЧЧММ 
        /// </summary>
        public string TimeOfEndOfShift { get; private set; } //in format NNMM   2  BCD
        public DateTime DateTimeOfEndOfShift { get; private set; }
        /// <summary>
        /// дата последнего дневного отчета в формате ДДММГГ 
        /// </summary>
        public string DateOfTheLastDailyReport { get; private set; }// in format DDMMYY   3  BCD
        public DateTime dtDateOfTheLastDailyReport { get; private set; }
        /// <summary>
        /// счетчик артикулов 
        /// </summary>
        public int CounterOfArticles { get; private set; }//  2  bin

        /// <summary>
        /// суммы налогов по налоговым группам для наложенного НДС
        /// </summary>
        public SumTaxByTaxGroups SumOfTaxByTaxGroupsForOverlayVAT { get; private set; } //   4*(6+6)  bin

        /// <summary>
        /// количество аннулированных чеков продаж
        /// </summary>
        public int QuantityOfCancelSalesReceipt { get; private set; } //   2  bin
        /// <summary>
        /// количество аннулированных чеков выплат
        /// </summary>
        public int QuantityOfCancelPaymentReceipt { get; private set; } //  2  bin
        /// <summary>
        /// сумма аннулированных чеков продаж
        /// </summary>
        public UInt32 SumOfCancelSalesReceipt { get; private set; }//   4  bin
        /// <summary>
        /// сумма аннулированных чеков выплат
        /// </summary>
        public UInt32 SumOfCancelPaymentReceipt { get; private set; } //  4  bin
        /// <summary>
        /// количество отказов продаж
        /// </summary>
        public int QuantityOfCancelSales { get; private set; } //   2  bin
        /// <summary>
        /// количество отказов выплат
        /// </summary>
        public int QuantityOfCancelPayments { get; private set; }//   2  bin
        /// <summary>
        /// сумма отказов продаж
        /// </summary>
        public UInt32 SumOfCancelSales { get; private set; }  //4  bin
        /// <summary>
        /// сумма отказов выплат
        /// </summary>
        public UInt32 SumOfCancelPayments { get; private set; }   //4  bin


    }

    public class SumTaxGroupsAndTypesOfPayments
    {
        public SumTaxGroupsAndTypesOfPayments()
        { }

        public SumTaxGroupsAndTypesOfPayments(byte[] inBytes, ref int ccounter, int countstep = 5)
        {


            TaxA = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxB = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxC = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxD = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxE = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxF = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;

            Card = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Credit = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Check = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Cash = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Certificat = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Voucher = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            ElectronicMoney = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            InsurancePayment = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            OverPayment = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Payment = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
        }


        public SumTaxGroupsAndTypesOfPayments(byte[] inBytes, int ccounter, int countstep = 5, bool controlSum=false)
        {


            TaxA = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxB = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxC = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxD = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxE = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            TaxF = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;

            Card = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Credit = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Check = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            //TODO WARNING!!!! проверено на ОП-06 почемуто CASH не передается и происходит смещение
            //Cash = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Certificat = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Voucher = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            ElectronicMoney = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            InsurancePayment = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            OverPayment = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            Payment = inBytes.returnUint32FromBytes(ccounter, countstep); ccounter += countstep;
            if ((controlSum) &(sumTAX() - sumPayment()!=0)) // почему, я так и не понял, но аппарат не отдает сумму кеша
            {                
                Cash = sumTAX() - sumPayment();
            }
        }



        public SumTaxGroupsAndTypesOfPayments(byte[] inBytes, ref int ccounter)
        {
            int countstep = 4;
            TaxA = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxB = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxC = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxD = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxE = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxF = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;

            Card = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            Credit = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            Check = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            Cash = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            Certificat = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            Voucher = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            ElectronicMoney = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            InsurancePayment = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            OverPayment = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            Payment = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            //if (sumTAX() - sumPayment() != 0) // почему, я так и не понял, но аппарат не отдает сумму кеша
            //{
            //    Cash = sumTAX() - sumPayment();
            //}
        }


        private UInt32 sumTAX()
        {
            return TaxA
                + TaxB
                + TaxC
                + TaxD
                + TaxE
                + TaxF;
        }


        private UInt32 sumPayment()
        {
            return Card
                + Cash
                + Certificat
                + Check
                + Credit
                + ElectronicMoney
                + InsurancePayment
                + OverPayment
                + Payment
                + Voucher;
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

    public class SumTaxByTaxGroups
    {
        public SumTaxByTaxGroups() { }
        

        public SumTaxByTaxGroups(byte[] inBytesTax, byte[] inBytesVat, int ccounter, int countstep = 5)
        {
            int index = 0;
            VatA = inBytesVat.returnUint32FromBytes(index, countstep); index += countstep;
            VatB = inBytesVat.returnUint32FromBytes(index, countstep); index += countstep;
            VatC = inBytesVat.returnUint32FromBytes(index, countstep); index += countstep;
            VatD = inBytesVat.returnUint32FromBytes(index, countstep); index += countstep;
            VatE = inBytesVat.returnUint32FromBytes(index, countstep); index += countstep;
            VatF = inBytesVat.returnUint32FromBytes(index, countstep); index += countstep;
        }

        //TODO суммы налогов по налоговым группам для наложенного НДС
        //4*(6+6)
        public SumTaxByTaxGroups(byte[] inBytes, ref int ccounter, int countstep = 4)
        {
            TaxA = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxB = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxC = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxD = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxE = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            TaxF = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;

            VatA = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            VatB = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            VatC = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            VatD = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            VatE = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
            VatF = BitConverter.ToUInt32(inBytes, ccounter); ccounter += countstep;
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
    public class Taxes
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

    public class Tax
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
    public class ReceiptInfo
    {
        public ReceiptInfo() { }

        public ReceiptInfo(ReturnedStruct returnedStruct, Int32 CostOfGoodsOrService, Int32 SumAtReceipt)
        {
            this.returnedStruct = returnedStruct;
            this.CostOfGoodsOrService = CostOfGoodsOrService;
            this.SumAtReceipt = SumAtReceipt;
        }
        public ReturnedStruct returnedStruct { get; private set; }
        /// <summary>
        /// стоимость товара или услуги
        /// </summary>
        public Int32 CostOfGoodsOrService { get; private set;  }

        /// <summary>
        /// сумма по чеку
        /// </summary>
        public Int32 SumAtReceipt { get; private set; }
    }

    /// <summary>
    /// Возврат после регистрация оплаты
    /// </summary>
    public class PaymentInfo
    {
        public PaymentInfo(ReturnedStruct returnedStruct)
        {
            this.returnedStruct = returnedStruct;
            if ((returnedStruct.statusOperation) && (returnedStruct.bytesReturn.Length > 3))
            {

                UInt32 tinfo = BitConverter.ToUInt32(returnedStruct.bytesReturn, 0);
                if (returnedStruct.bytesReturn[3].GetBit(7))
                {
                    tinfo = tinfo.ClearBitUInt32(31);
                    Renting = tinfo;
                }
                else
                    Rest = tinfo;
                if (returnedStruct.bytesReturn.Length >= 8)
                    NumberOfReceiptPackageInCPEF = BitConverter.ToUInt32(returnedStruct.bytesReturn, 4);
               
            }
        }

        public PaymentInfo(ReturnedStruct returnedStruct, UInt32 Renting, UInt64 Rest, UInt32 NumberOfReceiptPackageInCPEF= 0)
        {
            this.returnedStruct = returnedStruct;
            this.Renting = Renting;
            this.Rest = Rest;
            this.NumberOfReceiptPackageInCPEF = NumberOfReceiptPackageInCPEF;
        }

        public ReturnedStruct returnedStruct { get; private set; }

        public override string ToString()
        {
            return "Rest: " + Rest + " Renting:" + Renting + " NumberOfReceiptPackageInCPEF:" + NumberOfReceiptPackageInCPEF;
        }
        /// <summary>
        ///  сдача (бит 31 = 1 – сдача)
        /// </summary>
        public UInt32 Renting { get; private set; }
        /// <summary>
        /// остаток
        /// </summary>
        public UInt64 Rest { get; private set; }
        /// <summary>
        /// номер пакета чека в КЛЕФ
        /// </summary>
        public UInt32 NumberOfReceiptPackageInCPEF { get; private set; }
    }

    public class DiscountInfo
    {
        public DiscountInfo() { }

        public DiscountInfo(ReturnedStruct returnedStruct)
        {
            this.returnedStruct = returnedStruct;
            if (returnedStruct.bytesReturn.Length!=8)
            {
                throw new ApplicationException(String.Format("не правильный ответ сервера на строку чека, нужно 8 байт - ответ {0}!!!!", returnedStruct.bytesReturn.Length));
            }
            this.ValueOfDiscountMarkup = BitConverter.ToInt32(returnedStruct.bytesReturn, 0);
            this.SumOfReceipt = BitConverter.ToInt32(returnedStruct.bytesReturn, 4);
            var bit = new BitArray(returnedStruct.bytesReturn.Take(4).ToArray());
            if (bit[31])
            {
                this.ValueOfDiscountMarkup = this.ValueOfDiscountMarkup ^ (1 << 31);
            }
        }

        public DiscountInfo(ReturnedStruct returnedStruct, Int32 ValueOfDiscountMarkup, Int32 SumOfReceipt)
        {
            this.returnedStruct = returnedStruct;
            this.ValueOfDiscountMarkup = ValueOfDiscountMarkup;
            this.SumOfReceipt = SumOfReceipt;
        }
        public ReturnedStruct returnedStruct { get; private set; }
        /// <summary>
        /// стоимость товара или услуги
        /// </summary>
        public Int32 ValueOfDiscountMarkup { get; private set; }

        /// <summary>
        /// сумма по чеку
        /// </summary>
        public Int32 SumOfReceipt { get; private set; }
    }

    public class KleffInfo
    {
        public UInt32 PacketFirst { get; private set; }
        public UInt32 PacketLast { get; private set; }
        public UInt16 FreeMem { get; private set; }
        public ReturnedStruct returnedStruct { get; private set; }

        public KleffInfo()
        {

        }

        public KleffInfo(ReturnedStruct inReturnedStruct)
        {
            returnedStruct = inReturnedStruct;
            PacketFirst = inReturnedStruct.bytesReturn.returnUint32FromBytes(0, 4);
            PacketLast = inReturnedStruct.bytesReturn.returnUint32FromBytes(4, 4);            
            FreeMem = inReturnedStruct.bytesReturn.returnUint16FromBytes(8, 2);
            if (PacketFirst == 0 && PacketLast == 0 && FreeMem == 0)
                FreeMem = 65535;
        }
    }


}
