using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.EasternStandardTime, AccessRights = AccessRights.None)]
    public class CrossoverStats : Indicator
    {
        [Parameter("Short WMA", DefaultValue = 7)]
        public int shortWMAnum { get; set; }

        [Parameter("Long WMA", DefaultValue = 14)]
        public int longWMAnum { get; set; }

        [Parameter("Pip Target", DefaultValue = 10)]
        public int pipTarget { get; set; }


        public WeightedMovingAverage longWMA;
        public WeightedMovingAverage shortWMA;
        public bool[] success;
        public int arrayIndex;
        public int previousBreak;
        public int successRate;

        protected override void Initialize()
        {
            longWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, longWMAnum);
            shortWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, shortWMAnum);
            arrayIndex = 0;
            success = new bool[MarketSeries.Close.Count];
            previousBreak = int.MaxValue;

            for (int i = 1; i < MarketSeries.Close.Count; i++)
            {


                if (shortWMA.Result[i] > longWMA.Result[i] && shortWMA.Result[i - 1] < longWMA.Result[i - 1])
                {

                    for (int j = 0; j + i < MarketSeries.Close.Count; j++)
                    {

                        if ((MarketSeries.Close[i + j] - MarketSeries.Open[i]) / Symbol.PipSize >= pipTarget)
                        {
                            success[arrayIndex] = true;
                            arrayIndex++;
                            Print("Top cross at " + MarketSeries.OpenTime[i] + " Successful at " + MarketSeries.OpenTime[i + j]);
                            break;
                        }
                        else if (shortWMA.Result[i + j] < longWMA.Result[i + j])
                        {
                            success[arrayIndex] = false;
                            arrayIndex++;
                            Print("Top cross at " + MarketSeries.OpenTime[i] + " Unsuccessful at " + MarketSeries.OpenTime[i + j]);
                            break;
                        }

                    }

                }
                if (shortWMA.Result[i] < longWMA.Result[i] && shortWMA.Result[i - 1] > longWMA.Result[i - 1])
                {

                    for (int j = 0; j + i < MarketSeries.Close.Count; j++)
                    {

                        if ((MarketSeries.Open[i] - MarketSeries.Close[i + j]) / Symbol.PipSize >= pipTarget)
                        {
                            Print("Bottom cross at " + MarketSeries.OpenTime[i] + " Successful at " + MarketSeries.OpenTime[i + j]);
                            success[arrayIndex] = true;
                            arrayIndex++;
                            break;
                        }
                        else if (shortWMA.Result[i + j] > longWMA.Result[i + j])
                        {

                            Print("Bottom cross at " + MarketSeries.OpenTime[i] + " Unsuccessful at " + MarketSeries.OpenTime[i + j]);
                            success[arrayIndex] = false;
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
