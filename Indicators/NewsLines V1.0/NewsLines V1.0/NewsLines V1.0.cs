using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Linq;
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
        [FieldHidden()]
        public DateTime newsTime;



    }

    [Indicator(IsOverlay = true, TimeZone = TimeZones.EasternStandardTime, AccessRights = AccessRights.FullAccess)]
    public class NewsLines : Indicator
    {
        [Parameter("News File Path", DefaultValue = "C:\\Users\\jordand\\Documents\\highmediumlow.csv")]
        public string filePath { get; set; }

        [Parameter("Minutes Per Pip", DefaultValue = 1)]
        public int pipTime { get; set; }

        [Parameter("Future News Count", DefaultValue = 5)]
        public int newsCount { get; set; }

        [Parameter("Quiet Days", DefaultValue = 5)]
        public int quietDays { get; set; }

        public bool firstRun;
        public static FileHelperEngine<Fields> engine;
        public static Fields[] preres;
        public static List<Fields> res;
        public static Fields[] upcomingNews;

        public DateTime timeNow;
        public DateTime nextNews;
        public DateTime previousNews;
        public string nextNewsImpact;
        public int minutesToNews;
        public Colors color;
        public int lineWidth;
        public List<string> currencies;
        public int[] currencyActivityCount;
        public DateTime lastPrint;
        public string[] pairs = 
        {

            "EURUSD",
            "AUDUSD",
            "GBPUSD",
            "USDCAD",
            "USDCHF",
            "USDJPY",
            "AUDCAD",
            "AUDCHF",
            "AUDJPY",
            "AUDNZD",
            "CADCHF",
            "AUDSGD",
            "EURDKK",
            "EURHKD",
            "EURGBP",
            "CHFJPY",
            "EURCHF",
            "EURCAD",
            "EURJPY",
            "EURNOK",
            "EURNZD",
            "EURPLN",
            "EURSEK",
            "EURSGD",
            "EURTRY",
            "EURZAR",
            "GBPAUD",
            "GBPCAD",
            "GBPNZD",
            "GBPJPY",
            "GBPCHF",
            "GBPDKK",
            "GBPNOK",
            "GBPSEK",
            "GBPSGD",
            "NOKJPY",
            "NOKSEK",
            "NZDCAD",
            "NZDCHF",
            "NZDJPY",
            "NZDUSD",
            "SGDJPY",
            "USDMXN",
            "USDHKD",
            "USDDKK",
            "USDCZK",
            "USDHUF",
            "USDNOK",
            "USDPLN",
            "USDRUB",
            "USDSEK",
            "USDSGD",
            "USDTRY",
            "USDZAR"

        };
        public string quietPairs;

        protected override void Initialize()
        {
            // Initialize and create nested indicators
            nextNews = Server.Time;
            previousNews = Server.Time;
            timeNow = MarketSeries.OpenTime.LastValue;
            upcomingNews = new Fields[newsCount];
            currencies = new List<string>();
            lastPrint = Server.Time.AddMonths(-1);

            if (!TimeZoneInfo.Local.IsDaylightSavingTime(timeNow))
            {
                timeNow = timeNow.AddHours(1);
            }
            engine = new FileHelperEngine<Fields>();
            quietPairs = "";
            preres = engine.ReadFile(filePath);
            res = preres.ToList<Fields>();
            firstRun = true;
            readCSV();
            draw();
        }

        public override void Calculate(int index)
        {
            timeNow = Server.Time;

            if (!TimeZoneInfo.Local.IsDaylightSavingTime(timeNow))
            {
                timeNow = timeNow.AddHours(1);
            }

            draw();

        }



        public void draw()
        {
            readCSV();
            ChartObjects.RemoveAllObjects();





            foreach (Fields field in upcomingNews)
            {

                minutesToNews = (int)((field.newsTime - timeNow).TotalMinutes);

                if (field.impact == "High")
                {
                    color = Colors.Red;
                }
                else if (field.impact == "Medium")
                {
                    color = Colors.Orange;
                }
                else if (field.impact == "Low")
                {
                    color = Colors.Yellow;
                }

                if (Symbol.Code.IndexOf(field.currency) == -1)
                {
                    lineWidth = 1;
                }
                else
                {
                    lineWidth = 2;
                }



                var hAlign = HorizontalAlignment.Left;

                ChartObjects.DrawText("text" + minutesToNews, field.newsTime + " " + field.detail, MarketSeries.Close.Count - 1, Symbol.Bid + (minutesToNews * Symbol.PipSize) / pipTime, VerticalAlignment.Top, hAlign, color);

                if (this.Positions.Count == 0)
                {
                    ChartObjects.DrawText("Quiet Pairs", quietPairs, MarketSeries.Close.Count - 1, Symbol.Bid, VerticalAlignment.Bottom, hAlign, Colors.Aqua);
                }
                ChartObjects.DrawText("text2" + minutesToNews, field.newsTime + " " + field.detail, MarketSeries.Close.Count - 1, Symbol.Bid - (minutesToNews * Symbol.PipSize) / pipTime, VerticalAlignment.Bottom, hAlign, color);
                ChartObjects.DrawHorizontalLine("News " + minutesToNews, Symbol.Bid + (minutesToNews * Symbol.PipSize) / pipTime, color, lineWidth, LineStyle.Lines);
                ChartObjects.DrawHorizontalLine("News2 " + minutesToNews, Symbol.Bid - (minutesToNews * Symbol.PipSize) / pipTime, color, lineWidth, LineStyle.Lines);
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
            var index = 0;
            currencyActivityCount = new int[currencies.Count];
            foreach (Fields field in res)
            {
                if (firstRun)
                {

                    date = field.date;
                    currency = field.currency;
                    year = Int32.Parse(date.Substring(0, 4));
                    date = date.Substring(6);
                    monthname = date.Substring(0, date.IndexOf(" "));
                    if (currencies.IndexOf(field.currency) == -1)
                    {

                        currencies.Add(field.currency);
                    }

                    month = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList().IndexOf(monthname) + 1;

                    date = date.Substring(date.IndexOf(" ") + 1);
                    day = Int32.Parse(date.Substring(0, 2));
                    hour = Int32.Parse(date.Substring(4, 2));
                    minute = Int32.Parse(date.Substring(7));

                    field.newsTime = new DateTime(year, month, day, hour, minute, 0);
                }
                else
                {
                    if (field.newsTime > timeNow)
                    {
                        if ((field.newsTime - timeNow).TotalHours < quietDays * 24)
                        {

                            if (field.impact == "High")
                            {

                                currencyActivityCount[currencies.IndexOf(field.currency)] += 5;
                            }
                            else if (field.impact == "Medium")
                            {
                                currencyActivityCount[currencies.IndexOf(field.currency)] += 3;
                            }
                            else if (field.impact == "Low")
                            {
                                currencyActivityCount[currencies.IndexOf(field.currency)] += 1;
                            }
                        }
                        else if (index >= newsCount)
                        {

                            quietPairs = "";
                            foreach (string code in pairs)
                            {
                                var loudness = 0;
                                try
                                {
                                    loudness = currencyActivityCount[currencies.IndexOf(code.Substring(0, 3))] + currencyActivityCount[currencies.IndexOf(code.Substring(3))];
                                    Print(code + ": " + loudness);
                                    //Print(loudness + currencyActivityCount.Min());
                                    if (loudness == currencyActivityCount.Min() + 7)
                                    {
                                        quietPairs += " |" + code + "|";
                                    }
                                } catch (Exception e)
                                {
                                    //Print(code + " not found");
                                }

                            }
                            return;
                        }
                    }
                    if (field.newsTime > timeNow && (Symbol.Code.IndexOf(field.currency) != -1 || field.impact == "High"))
                    {
                        previousNews = nextNews;
                        nextNews = field.newsTime;
                        nextNewsImpact = field.impact;




                        if (index < newsCount)
                        {
                            upcomingNews[index] = field;

                        }
                        index++;
                    }
                }
            }


            if (firstRun)
            {
                currencyActivityCount = new int[currencies.Count];

            }



            firstRun = false;
        }
    }
}
