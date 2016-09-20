using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class NewcBot : Robot
    {
        [Parameter(DefaultValue = 0)]
        public int longWMAnum { get; set; }

        [Parameter(DefaultValue = 0)]
        public int shortWMAnum { get; set; }

        [Parameter(DefaultValue = 0)]
        public int positionSize { get; set; }

        [Parameter(DefaultValue = 0)]
        public int pipsProfit { get; set; }

        [Parameter(DefaultValue = 0)]
        public int SLpips { get; set; }

        public WeightedMovingAverage longWMA;
        public WeightedMovingAverage shortWMA;


        protected override void OnStart()
        {
            longWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, longWMAnum);
            shortWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, shortWMAnum);
        }

        protected override void OnBar()
        {
            int index = MarketSeries.Close.Count - 1;
            /*if (this.Positions.Count == 0)
            {*/
            if (shortWMA.Result[index - 1] > longWMA.Result[index - 1] && shortWMA.Result[index - 2] < longWMA.Result[index - 2])
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", SLpips, pipsProfit, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
            }
            else if (shortWMA.Result[index - 1] < longWMA.Result[index - 1] && shortWMA.Result[index - 2] > longWMA.Result[index - 2])
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", SLpips, pipsProfit, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
            }
        }
            /*
            }
            else
            {
            }*/
                /*if (shortWMA.Result[index] > longWMA.Result[index] && shortWMA.Result[index - 1] < longWMA.Result[index - 2])
                {
                    //ClosePosition(this.Positions[0]);
                    ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", SLpips, pipsProfit, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                }
                else if (shortWMA.Result[index] < longWMA.Result[index] && shortWMA.Result[index - 1] > longWMA.Result[index - 2])
                {
                    //ClosePosition(this.Positions[0]);
                    ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", SLpips, pipsProfit, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                }*/

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
