using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TimeFrames : Indicator
    {

        [Parameter("Short WMA", DefaultValue = 7, MinValue = 2)]
        public int WMAsmallnum { get; set; }
        [Parameter("Long WMA", DefaultValue = 14, MinValue = 2)]
        public int WMAbignum { get; set; }
        [Parameter("MA Type", DefaultValue = MovingAverageType.Weighted)]
        public MovingAverageType MAType { get; set; }
        [Parameter("Height", DefaultValue = 2, MinValue = 2)]
        public int height { get; set; }
        [Parameter("Width", DefaultValue = 42, MinValue = 2)]
        public int width { get; set; }
        [Parameter("Threshold", DefaultValue = 16, MinValue = 0)]
        public int threshold { get; set; }

        public TimeFrame[] timeframes;
        public int[] scores;
        private MovingAverage WMAsmall;
        private MovingAverage WMAbig;
        public Colors totalColor;

        protected override void Initialize()
        {
            // Initialize and create nested indicators
            timeframes = new TimeFrame[] 
            {
                TimeFrame.Minute3,
                TimeFrame.Minute5,
                TimeFrame.Minute15,
                TimeFrame.Hour,
                TimeFrame.Hour4,
                TimeFrame.Daily
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
            totalColor = Colors.White;

        }

        public override void Calculate(int index)
        {

            DataSeries ds;
            int totalscore = 0;
            int[] values = new int[scores.Length];
            for (int i = 0; i < timeframes.Length; i++)
            {
                ds = MarketData.GetSeries(timeframes[i]).Open;

                WMAsmall = Indicators.MovingAverage(ds, WMAsmallnum, MAType);
                WMAbig = Indicators.MovingAverage(ds, WMAbignum, MAType);

                if (WMAsmall.Result.LastValue > WMAbig.Result.LastValue)
                {
                    totalscore += scores[i];
                    values[i] = scores[i];
                }
                else
                {
                    totalscore -= scores[i];
                    values[i] = -1 * scores[i];
                }
            }

            //Result[index] = totalscore;

            drawLines(index, values);
        }

        public void drawLines(int index, int[] values)
        {

            ChartObjects.DrawLine("topline", index - width, Symbol.Bid, index, Symbol.Bid, totalColor, 1, LineStyle.Solid);

            for (int i = 0; i < scores.Length + 2; i++)
            {
                ChartObjects.DrawLine("line" + i, index - i * width / 7, Symbol.Bid, index - i * width / 7, Symbol.Bid - height * Symbol.PipSize, totalColor, 1, LineStyle.Solid);
            }

            ChartObjects.DrawLine("bottomline", index - width, Symbol.Bid - height * Symbol.PipSize, index, Symbol.Bid - height * Symbol.PipSize, totalColor, 1, LineStyle.Solid);


            for (int i = 0; i < scores.Length; i++)
            {
                Colors color = Colors.Red;
                if (values[i] > 0)
                {
                    color = Colors.Green;
                    ChartObjects.DrawText(timeframes[i].ToString() + "Value", "+" + values[i], index - (i + 2) * width / 7 + width / 14, Symbol.Bid - height * Symbol.PipSize, VerticalAlignment.Top, HorizontalAlignment.Center, color);
                }
                else if (values[i] < 0)
                {
                    color = Colors.Red;
                    ChartObjects.DrawText(timeframes[i].ToString() + "Value", "" + values[i], index - (i + 2) * width / 7 + width / 14, Symbol.Bid - height * Symbol.PipSize, VerticalAlignment.Top, HorizontalAlignment.Center, color);
                }
                else
                {
                    color = Colors.White;
                    ChartObjects.DrawText(timeframes[i].ToString() + "Value", "" + values[i], index - (i + 2) * width / 7 + width / 14, Symbol.Bid - height * Symbol.PipSize, VerticalAlignment.Top, HorizontalAlignment.Center, color);
                }
                ChartObjects.DrawText(timeframes[i].ToString() + "Text", timeframes[i].ToString(), index - (i + 2) * width / 7 + width / 14, Symbol.Bid, VerticalAlignment.Bottom, HorizontalAlignment.Center, color);

            }
            ChartObjects.DrawText("ScoreText", "Score", index - 1 * width / 7 + width / 14, Symbol.Bid, VerticalAlignment.Bottom, HorizontalAlignment.Center, Colors.White);
            int sum = 0;
            foreach (int i in values)
            {
                sum += i;
            }
            totalColor = Colors.Red;
            if (sum >= threshold)
            {
                totalColor = Colors.Green;
            }
            else if (sum <= threshold * (-1))
            {
                totalColor = Colors.Red;
            }
            else
            {
                totalColor = Colors.White;
            }
            ChartObjects.DrawText("ScoreValue", "" + sum, index - 1 * width / 7 + width / 14, Symbol.Bid - height * Symbol.PipSize, VerticalAlignment.Top, HorizontalAlignment.Center, totalColor);

        }
    }
}
