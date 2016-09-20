using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TimeFramesHistorical : Indicator
    {


        [Parameter("Short WMA", DefaultValue = 7, MinValue = 2)]
        public int WMAsmallnum { get; set; }
        [Parameter("Long WMA", DefaultValue = 14, MinValue = 2)]
        public int WMAbignum { get; set; }
        [Parameter("MA Type", DefaultValue = MovingAverageType.Weighted)]
        public MovingAverageType MAType { get; set; }
        [Parameter("Scale", DefaultValue = "1")]
        public int scale { get; set; }
        [Parameter("Threshold", DefaultValue = "1")]
        public int threshold { get; set; }
        [Parameter("Number of Bars", DefaultValue = 20)]
        public int numBars { get; set; }

        public TimeFrame[] timeframes;
        public int[] scores;
        public string[] timeStrings;
        public int scoreTotal;
        public double[] barRatio;
        private MovingAverage WMAsmall;
        private MovingAverage WMAbig;
        public Colors totalColor;
        public int currentIndex;


        protected override void Initialize()
        {

            timeframes = new TimeFrame[] 
            {
                TimeFrame.Minute3,
                TimeFrame.Minute5,
                TimeFrame.Minute15,
                TimeFrame.Hour,
                TimeFrame.Hour4,
                TimeFrame.Daily
            };
            timeStrings = new string[] 
            {
                "M3",
                "M5",
                "M15",
                "H1",
                "H4",
                "D1"
            };
            scores = new int[] 
            {
                6,
                5,
                4,
                3,
                2,
                1
            };


        }
        public Colors getColor(int num, bool thresh)
        {
            if (thresh)
            {
                if (num >= threshold)
                {
                    return Colors.Green;
                }
                else if (num <= threshold * -1)
                {
                    return Colors.Red;
                }
                else
                {
                    return Colors.White;
                }
            }
            else
            {
                if (num > 0)
                {
                    return Colors.Green;
                }
                else if (num < 0)
                {
                    return Colors.Red;
                }
                else
                {
                    return Colors.White;
                }
            }
        }
        public override void Calculate(int index)
        {
            DataSeries ds;
            double lowPrice = double.MaxValue;
            for (int i = 0; i < numBars; i++)
            {
                if (MarketSeries.Low.Last(i) < lowPrice)
                {
                    lowPrice = MarketSeries.Low.Last(i);
                }
            }

            totalColor = Colors.White;

            int[,] values = new int[numBars, scores.Length + 1];
            //MarketSeries currentSeries = MarketSeries.
            for (int bar = 0; bar < numBars; bar++)
            {
                for (int i = 0; i < timeframes.Length; i++)
                {
                    ds = MarketData.GetSeries(timeframes[i]).Open;
                    int convertedIndex = MarketData.GetSeries(timeframes[i]).OpenTime.GetIndexByTime(MarketData.GetSeries(this.TimeFrame).OpenTime[index - bar]);
                    WMAsmall = Indicators.MovingAverage(ds, WMAsmallnum, MAType);
                    WMAbig = Indicators.MovingAverage(ds, WMAbignum, MAType);

                    if (WMAsmall.Result[convertedIndex] > WMAbig.Result[convertedIndex])
                    {
                        values[bar, scores.Length] += scores[i];
                        values[bar, i] = scores[i];
                        totalColor = Colors.Green;

                    }
                    else
                    {
                        values[bar, scores.Length] -= scores[i];
                        values[bar, i] = -1 * scores[i];
                        totalColor = Colors.Red;
                    }
                    ChartObjects.DrawText("ScoreValue" + bar + "" + i, "■", index - bar, lowPrice - i * scale * Symbol.PipSize, VerticalAlignment.Bottom, HorizontalAlignment.Center, totalColor);
                    if (bar == numBars - 1)
                    {
                        totalColor = getColor(values[0, i], false);
                        ChartObjects.DrawText("Labels" + bar + "" + i, timeStrings[i], index + 1, lowPrice - i * scale * Symbol.PipSize, VerticalAlignment.Bottom, HorizontalAlignment.Center, totalColor);
                    }

                    if (i == timeframes.Length - 1)
                    {


                        totalColor = getColor(values[bar, scores.Length], true);
                        ChartObjects.DrawText("FinalScoreValue" + bar + "" + i, "" + values[bar, scores.Length], index - bar, lowPrice - (i + 1) * scale * Symbol.PipSize, VerticalAlignment.Bottom, HorizontalAlignment.Center, totalColor);
                        if (bar == numBars - 1)
                        {
                            totalColor = getColor(values[0, scores.Length], true);
                            ChartObjects.DrawText("ScoreLabel" + bar + "" + i, "#", index + 1, lowPrice - (i + 1) * scale * Symbol.PipSize, VerticalAlignment.Bottom, HorizontalAlignment.Center, totalColor);
                        }
                    }

                }

            }



        }







    }


}
