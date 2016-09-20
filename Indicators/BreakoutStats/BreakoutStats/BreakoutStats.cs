using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.EasternStandardTime, AccessRights = AccessRights.None)]
    public class BreakoutStats : Indicator
    {
        [Parameter("Periods", DefaultValue = 77)]
        public int maPeriod { get; set; }

        [Parameter("Channel Size (pips)", DefaultValue = 1)]
        public int bandDistance { get; set; }

        [Parameter("Pip Target", DefaultValue = 10)]
        public int pipTarget { get; set; }



        public WeightedMovingAverage wma;
        public bool[] success;
        public int arrayIndex;
        public int previousBreak;
        public int successRate;

        protected override void Initialize()
        {
            wma = Indicators.WeightedMovingAverage(MarketSeries.Close, maPeriod);
            arrayIndex = 0;
            success = new bool[MarketSeries.Close.Count];
            int previousClose = int.MinValue;


            for (int i = 1; i < MarketSeries.Close.Count; i++)
            {


                if (MarketSeries.Low[i] > wma.Result[i] + bandDistance * Symbol.PipSize && MarketSeries.Low[i - 1] < wma.Result[i - 1] + bandDistance * Symbol.PipSize)
                {

                    for (int j = 0; j + i < MarketSeries.Close.Count; j++)
                    {
                        if (i + j <= previousClose)
                        {
                            break;
                        }

                        if ((MarketSeries.Close[i + j] - MarketSeries.Open[i]) / Symbol.PipSize >= pipTarget)
                        {
                            success[arrayIndex] = true;
                            arrayIndex++;
                            previousClose = i + j;
                            Print("Top break at " + MarketSeries.OpenTime[i] + " Successful at " + MarketSeries.OpenTime[i + j]);
                            break;
                        }
                        else if (MarketSeries.Close[i + j] < wma.Result[i + j] - bandDistance * Symbol.PipSize)
                        {
                            success[arrayIndex] = false;
                            arrayIndex++;
                            previousClose = i + j;
                            Print("Top break at " + MarketSeries.OpenTime[i] + " Unsuccessful at " + MarketSeries.OpenTime[i + j]);
                            break;
                        }

                    }

                }
                if (MarketSeries.High[i] < wma.Result[i] - bandDistance * Symbol.PipSize && MarketSeries.High[i - 1] > wma.Result[i - 1] - bandDistance * Symbol.PipSize)
                {

                    for (int j = 0; j + i < MarketSeries.Close.Count; j++)
                    {
                        if (i + j <= previousClose)
                        {
                            break;
                        }
                        if ((MarketSeries.Close[i] - MarketSeries.Open[i + j]) / Symbol.PipSize >= pipTarget)
                        {
                            Print("Bottom break at " + MarketSeries.OpenTime[i] + " Successful at " + MarketSeries.OpenTime[i + j]);
                            success[arrayIndex] = true;
                            previousClose = i + j;
                            arrayIndex++;
                            break;
                        }
                        else if (MarketSeries.Open[i + j] > wma.Result[i + j] + bandDistance * Symbol.PipSize)
                        {

                            Print("Bottom break at " + MarketSeries.OpenTime[i] + " Unsuccessful at " + MarketSeries.OpenTime[i + j]);
                            success[arrayIndex] = false;
                            previousClose = i + j;
                            arrayIndex++;
                            break;
                        }

                    }

                }

            }

        }

        public override void Calculate(int index)
        {



            draw(index);
        }

        public void draw(int index)
        {
            int sum = 0;
            for (int i = 0; i < arrayIndex; i++)
            {
                if (success[i])
                {
                    sum += 1;
                }
            }
            DateTime firstBreak = MarketSeries.OpenTime[0];
            double perdays = Math.Round(arrayIndex * 1000 / (Server.Time - firstBreak).TotalDays) / 1000;
            successRate = sum * 100 / arrayIndex;
            ChartObjects.DrawText("Text", successRate.ToString() + "%", index, Symbol.Bid, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Aqua);
            ChartObjects.DrawText("Rate", perdays.ToString() + " Trades/Day", index, Symbol.Bid, VerticalAlignment.Bottom, HorizontalAlignment.Left, Colors.Red);
        }
    }
}
