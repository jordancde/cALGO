using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using FileHelpers;
using System.Globalization;
using System.Collections.Generic;

namespace cAlgo
{
    [DelimitedRecord(","), IgnoreFirst(1), IgnoreEmptyLines()]
    public class Fields
    {
        [FieldQuoted()]
        public string date;
        [FieldQuoted()]
        public string detail;
        public string impact;
        public string previous;
        public string consensus;
        public string actual;
        public string currency;

    }

    [Robot(TimeZone = TimeZones.EasternStandardTime, AccessRights = AccessRights.FullAccess)]
    public class NewsBot3 : Robot
    {




        public int test { get; set; }
        [Parameter(DefaultValue = 30)]
        public int minutesBefore { get; set; }
        [Parameter(DefaultValue = 30)]
        public int minutesAfter { get; set; }
        [Parameter(DefaultValue = 30)]
        public int positionSizePercent { get; set; }
        [Parameter(DefaultValue = 30)]
        public int profitPips { get; set; }
        [Parameter(DefaultValue = 20)]
        public int stopLossPips { get; set; }
        [Parameter(DefaultValue = 10)]
        public int TSTrailPips { get; set; }
        [Parameter(DefaultValue = 10)]
        public int TSShiftPips { get; set; }
        [Parameter(DefaultValue = 10)]
        public int trialMonths { get; set; }
        [Parameter(DefaultValue = 1)]


        public int tradeTP;
        public int positionSize;
        public double entryPrice;
        public bool modified = false;
        public bool ran;
        public static int hour;
        public static int min;
        public static int year;
        public static int month;
        public static int day;
        public static FileHelperEngine<Fields> engine;
        public static Fields[] preres;
        public static List<Fields> res;
        public DateTime timeNow;
        public DateTime nextNews;
        public DateTime tradeTime;
        public DateTime previousNews;
        public string[] highImpact;
        public string description;
        public string lastDescription;
        public bool printed = false;
        public int fieldindex = 0;
        public DateTime startTime;
        public DateTime checkTime;
        public double startingBalance;
        public int[] newsPips;
        public int newsIndex = -1;


        protected override void OnStart()
        {

            timeNow = MarketSeries.OpenTime.LastValue;
            if (!TimeZoneInfo.Local.IsDaylightSavingTime(timeNow))
            {
                timeNow = timeNow.AddHours(1);
            }

            startTime = this.Time;

            startingBalance = Account.Balance;
            checkTime = startTime.AddMonths(trialMonths);


            engine = new FileHelperEngine<Fields>();
            highImpact = new string[] 
            {
                //"Interest",
                "Retail",
                //"Unemploy",
                //"Sentiment",
                //"Confidence",
                //"Trade Balance",
                //" Manufacturing",
                "Nonfarm"
                //"FOMC",
                //"Consumer Price Index"

            };
            newsPips = new int[] 
            {
                //23,
                19,
                //10,
                //8,
                //7,
                //14,
                //8,
                33
                //9,
                //11
            };

            preres = engine.ReadFile("C:\\Users\\jordand\\Documents\\calendar.csv");
            res = preres.ToList<Fields>();

            readCSV();


        }

        protected override void OnBar()
        {

            if (timeNow > checkTime && Positions.Count == 0)
            {
                if (Account.Balance < startingBalance)
                {
                    Stop();
                }
            }
            timeNow = MarketSeries.OpenTime.LastValue;


            if (!TimeZoneInfo.Local.IsDaylightSavingTime(timeNow))
            {
                timeNow = timeNow.AddHours(1);
            }

            if (this.Positions.Count > 0 && (((!TimeZoneInfo.Local.IsDaylightSavingTime(timeNow) && timeNow >= Positions[0].EntryTime.AddHours(1).AddMinutes(minutesBefore + minutesAfter))) || ((TimeZoneInfo.Local.IsDaylightSavingTime(timeNow) && timeNow >= Positions[0].EntryTime.AddMinutes(minutesBefore + minutesAfter)))))
            {

                if (this.Positions.Count == 2)
                {
                    ClosePosition(this.Positions[0]);
                    ClosePosition(this.Positions[0]);
                }
                else
                {
                    ClosePosition(this.Positions[0]);
                }
            }
            readCSV();



        }

        protected override void OnTick()
        {

            int index = MarketSeries.Close.Count - 1;
            //If the short crosses over long going up
            positionSize = (int)Symbol.NormalizeVolume(Account.Balance * (positionSizePercent / 100), RoundingMode.ToNearest);

            if (this.Positions.Count > 0 && this.Positions.Find("Buy") != null)
            {
                if (modified && Symbol.Bid - this.Positions.Find("Buy").StopLoss >= TSShiftPips * Symbol.PipSize)
                {
                    this.ModifyPosition(this.Positions.Find("Buy"), Symbol.Bid - TSTrailPips * Symbol.PipSize, null);

                }
                if (Symbol.Bid >= this.Positions.Find("Buy").EntryPrice + tradeTP * Symbol.PipSize && !modified)
                {
                    this.ModifyPosition(this.Positions.Find("Buy"), Symbol.Bid - TSTrailPips * Symbol.PipSize, null);
                    modified = true;

                }

            }
            else if (this.Positions.Count > 0 && this.Positions.Find("Sell") != null)
            {
                if (modified && this.Positions.Find("Sell").StopLoss - Symbol.Ask >= TSShiftPips * Symbol.PipSize)
                {
                    this.ModifyPosition(this.Positions.Find("Sell"), Symbol.Ask + TSTrailPips * Symbol.PipSize, null);

                }
                if (Symbol.Ask <= this.Positions.Find("Sell").EntryPrice - tradeTP * Symbol.PipSize && !modified)
                {
                    this.ModifyPosition(this.Positions.Find("Sell"), Symbol.Ask + TSTrailPips * Symbol.PipSize, null);
                    modified = true;

                }
            }
            if (this.Positions.Count == 0)
            {
                modified = false;
                if (History.Count > 0 && this.History.FindLast("Buy").NetProfit + this.History.FindLast("Sell").NetProfit < 0 && !printed)
                {
                    Print("Loss on '" + lastDescription + "'");
                    printed = true;
                }
            }

            if (this.Positions.Count == 0 && timeNow >= tradeTime && previousNews != nextNews)
            {

                stopLossPips = newsPips[newsIndex] - profitPips;
                ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", stopLossPips, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", stopLossPips, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                lastDescription = description;
                printed = false;
                entryPrice = Symbol.Bid;
                tradeTP = newsPips[newsIndex];
                previousNews = nextNews;



            }


        }



        public void readCSV()
        {


            var date = "";
            var currency = "";
            var year = 0;
            var month = 0;
            var day = 0;
            var hour = 0;
            var minute = 0;
            var monthname = "";
            var fielddesc = "";
            DateTime fieldTime;




            foreach (Fields field in res.Skip(fieldindex))
            {
                date = field.date;
                currency = field.currency;
                year = Int32.Parse(date.Substring(0, 4));
                date = date.Substring(6);
                monthname = date.Substring(0, date.IndexOf(" "));

                month = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList().IndexOf(monthname) + 1;

                date = date.Substring(date.IndexOf(" ") + 1);
                day = Int32.Parse(date.Substring(0, 2));
                hour = Int32.Parse(date.Substring(4, 2));
                minute = Int32.Parse(date.Substring(7));
                fielddesc = field.detail;
                description = field.detail;
                fieldTime = new DateTime(year, month, day, hour, minute, 0);



                if (fieldTime > timeNow && Symbol.Code.IndexOf(currency) != -1)
                {

                    foreach (string s in highImpact)
                    {
                        if (fielddesc.Contains(s))
                        {
                            nextNews = fieldTime;


                            tradeTime = nextNews.AddMinutes((-1) * minutesBefore);
                            newsIndex = Array.IndexOf(highImpact, s);



                            fieldindex = res.IndexOf(field);


                            return;
                        }
                    }

                }


            }


        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
