using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.EasternStandardTime, AccessRights = AccessRights.None)]
    public class CrossoverBreakout : Robot
    {

        [Parameter("Position Size", DefaultValue = 1000)]
        public int positionSize { get; set; }

        [Parameter("Long WMA Periods", DefaultValue = 8)]
        public int longWMANum { get; set; }

        [Parameter("Short WMA Periods", DefaultValue = 5)]
        public int shortWMANum { get; set; }

        [Parameter("WMA Crossover TimeFrame")]
        public TimeFrame WMAtimeframe { get; set; }

        [Parameter("Stop Loss (pips)", DefaultValue = 0)]
        public int stopLossPips { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 0)]
        public int TPPips { get; set; }

        [Parameter("WMA Pip Movement Max", DefaultValue = 0)]
        public int PipThreshold { get; set; }

        [Parameter("Channel Period", DefaultValue = 77)]
        public int maPeriod { get; set; }

        [Parameter("Channel Pips", DefaultValue = 1)]
        public int bandDistance { get; set; }

        public WeightedMovingAverage longWMA;
        public WeightedMovingAverage shortWMA;
        public WeightedMovingAverage channelWMA;

        protected override void OnStart()
        {
            longWMA = Indicators.WeightedMovingAverage(MarketData.GetSeries(WMAtimeframe).Close, longWMANum);
            shortWMA = Indicators.WeightedMovingAverage(MarketData.GetSeries(WMAtimeframe).Close, shortWMANum);
            channelWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, maPeriod);
        }

        protected override void OnTick()
        {
            int crossIndex = 0;
            // Put your core logic here
            if (this.Positions.Count == 0)
            {
                //finds last cross index
                if (shortWMA.Result.LastValue > longWMA.Result.LastValue)
                {
                    for (int i = 0; i < MarketSeries.Close.Count; i++)
                    {
                        if (shortWMA.Result.Last(i) < longWMA.Result.Last(i))
                        {
                            crossIndex = MarketSeries.Close.Count - i;
                            break;
                        }
                    }
                }
                else if (shortWMA.Result.LastValue < longWMA.Result.LastValue)
                {
                    for (int i = 0; i < MarketSeries.Close.Count; i++)
                    {
                        if (shortWMA.Result.Last(i) > longWMA.Result.Last(i))
                        {
                            crossIndex = MarketSeries.Close.Count - i;
                            break;
                        }
                    }
                }

                double pipsMoved = Math.Abs(Symbol.Bid - MarketSeries.Close[crossIndex]) / Symbol.PipSize;

                if (pipsMoved < PipThreshold)
                {
                    if (Symbol.Bid > channelWMA.Result.LastValue + bandDistance * Symbol.PipSize)
                    {

                        ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", stopLossPips, TPPips, 3);
                    }
                    else if (Symbol.Bid < channelWMA.Result.LastValue - bandDistance * Symbol.PipSize)
                    {

                        ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", stopLossPips, TPPips, 3);
                    }
                }


            }
        }

        protected override void OnBar()
        {

        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
