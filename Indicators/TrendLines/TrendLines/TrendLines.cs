using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TrendLines : Indicator
    {




        [Parameter(DefaultValue = 14, MinValue = 2)]
        public int Periods { get; set; }

        [Parameter(DefaultValue = 100, MinValue = 2)]
        public int PreviousBars { get; set; }

        [Parameter(DefaultValue = true)]
        public bool EnableTrendLines { get; set; }

        [Parameter(DefaultValue = true)]
        public bool EnableTrendChannel { get; set; }

        public WeightedMovingAverage wma;
        public bool[] upOrDown;

        protected override void Initialize()
        {
            // Initialize and create nested indicators
            wma = Indicators.WeightedMovingAverage(MarketSeries.Close, Periods);

        }

        public override void Calculate(int index)
        {
            if (EnableTrendLines)
            {
                for (int i = 0; i < PreviousBars; i++)
                {
                    if (wma.Result.Last(i) >= wma.Result.Last(i + 1))
                    {
                        double high = double.MinValue;
                        double offset = 0;
                        for (int j = i; j < PreviousBars; j++)
                        {
                            if (MarketSeries.High.Last(j) > high)
                            {
                                high = MarketSeries.High.Last(j);
                                offset = high - wma.Result.Last(j);
                            }
                            if (wma.Result.Last(j) <= wma.Result.Last(j + 1))
                            {
                                ChartObjects.DrawLine("trend" + i, index - i, wma.Result.Last(i), index - j, wma.Result.Last(j), Colors.Green, 2, LineStyle.Solid);
                                if (EnableTrendChannel)
                                {
                                    //ChartObjects.DrawLine("trendhigh" + i, index - i, wma.Result.Last(i) + offset, index - j, wma.Result.Last(j) + offset, Colors.White, 1, LineStyle.Solid);
                                    ChartObjects.DrawLine("trendlow" + i, index - i, wma.Result.Last(i) - offset, index - j, wma.Result.Last(j) - offset, Colors.White, 1, LineStyle.DotsRare);
                                }
                                i = j;
                                break;
                            }
                        }
                    }
                    else if (wma.Result.Last(i) <= wma.Result.Last(i + 1))
                    {
                        double low = double.MaxValue;
                        double offset = 0;
                        for (int j = i; j < PreviousBars; j++)
                        {
                            if (MarketSeries.Low.Last(j) < low)
                            {
                                low = MarketSeries.Low.Last(j);
                                offset = wma.Result.Last(j) - low;
                            }
                            if (wma.Result.Last(j) >= wma.Result.Last(j + 1))
                            {
                                ChartObjects.DrawLine("trend" + i, index - i, wma.Result.Last(i), index - j, wma.Result.Last(j), Colors.Red, 2, LineStyle.Solid);
                                if (EnableTrendChannel)
                                {
                                    ChartObjects.DrawLine("trendhigh" + i, index - i, wma.Result.Last(i) + offset, index - j, wma.Result.Last(j) + offset, Colors.White, 1, LineStyle.DotsRare);
                                    //ChartObjects.DrawLine("trendlow" + i, index - i, wma.Result.Last(i) - offset, index - j, wma.Result.Last(j) - offset, Colors.White, 1, LineStyle.Solid);
                                }
                                i = j;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
