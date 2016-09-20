using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Linq;
using FileHelpers;
using System.Globalization;
using System.Collections.Generic;
using System.Net;


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
    public class NewsIndicator : Indicator
    {
        [Parameter("News File Path", DefaultValue = "C:\\Users\\jordand\\Documents\\highmediumlow.csv")]
        public string filePath { get; set; }

        [Parameter("Minutes Per Pip", DefaultValue = 1)]
        public int pipTime { get; set; }

        [Parameter("Future News Count", DefaultValue = 5)]
        public int newsCount { get; set; }

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


        protected override void Initialize()
        {
            // Initialize and create nested indicators
            nextNews = Server.Time;
            previousNews = Server.Time;
            timeNow = MarketSeries.OpenTime.LastValue;
            upcomingNews = new Fields[newsCount];


            if (!TimeZoneInfo.Local.IsDaylightSavingTime(timeNow))
            {
                timeNow = timeNow.AddHours(1);
            }
            engine = new FileHelperEngine<Fields>();

            preres = engine.ReadFile(filePath);
            res = preres.ToList<Fields>();
            firstRun = true;
            //cal();
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
                ChartObjects.DrawText("text2" + minutesToNews, field.newsTime + " " + field.detail, MarketSeries.Close.Count - 1, Symbol.Bid - (minutesToNews * Symbol.PipSize) / pipTime, VerticalAlignment.Bottom, hAlign, color);
                ChartObjects.DrawHorizontalLine("News " + minutesToNews, Symbol.Bid + (minutesToNews * Symbol.PipSize) / pipTime, color, lineWidth, LineStyle.Lines);
                ChartObjects.DrawHorizontalLine("News2 " + minutesToNews, Symbol.Bid - (minutesToNews * Symbol.PipSize) / pipTime, color, lineWidth, LineStyle.Lines);
            }
        }

        public void cal()
        {
            using (var client = new WebClient())
            {
                var uri = new Uri("http://www.myfxbook.com/forex-economic-calendar");
                string html = client.DownloadString(uri);
                var index = html.LastIndexOf("<td width = \"30\">");
                Print(html.Substring(index - 20), 20);
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

            foreach (Fields field in res)
            {
                if (firstRun)
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

                    field.newsTime = new DateTime(year, month, day, hour, minute, 0);
                }
                else
                {

                    if (field.newsTime > timeNow && (Symbol.Code.IndexOf(field.currency) != -1 || field.impact == "High"))
                    {
                        previousNews = nextNews;
                        nextNews = field.newsTime;
                        nextNewsImpact = field.impact;

                        upcomingNews[index] = field;
                        index++;

                        if (index >= newsCount)
                        {
                            return;
                        }
                    }
                }
            }




            firstRun = false;
        }
    }
}
