using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ExpMovingAverageChannel5MOptimized : Robot
    {
        [Parameter(DefaultValue = 18)]
        public int MAperiods { get; set; }

        [Parameter(DefaultValue = 25)]
        public int MOMperiods { get; set; }

        [Parameter(DefaultValue = 33)]
        public int MOMma { get; set; }

        [Parameter(DefaultValue = 100)]
        public int positionSizePercent { get; set; }

        [Parameter(DefaultValue = 80)]
        public int initialTP { get; set; }

        [Parameter(DefaultValue = 85)]
        public int initialSL { get; set; }

        [Parameter(DefaultValue = 14)]
        public int trailMAPeriods { get; set; }


        public SimpleMovingAverage highMA;
        public SimpleMovingAverage lowMA;

        public MomentumOscillator momentum;
        public SimpleMovingAverage momentumMA;

        public bool buySetup;
        public bool sellSetup;
        public SimpleMovingAverage highTrail;
        public SimpleMovingAverage lowTrail;
        public int positionSize;

        protected override void OnStart()
        {
            highMA = Indicators.SimpleMovingAverage(MarketSeries.High, MAperiods);
            lowMA = Indicators.SimpleMovingAverage(MarketSeries.Low, MAperiods);

            momentum = Indicators.MomentumOscillator(MarketSeries.Close, MOMperiods);
            momentumMA = Indicators.SimpleMovingAverage(momentum.Result, MOMma);

            highTrail = Indicators.SimpleMovingAverage(MarketSeries.High, trailMAPeriods);
            lowTrail = Indicators.SimpleMovingAverage(MarketSeries.Low, trailMAPeriods);

            buySetup = false;
            sellSetup = false;
        }

        protected override void OnBar()
        {
            positionSize = (int)Symbol.NormalizeVolume(Account.Balance * positionSizePercent / 100, RoundingMode.ToNearest);
            if (this.Positions.Count > 0)
            {
                return;
            }

            if (buySetup)
            {
                if (MarketSeries.Low.Last(1) < highMA.Result.Last(1))
                {
                    buySetup = false;
                }
                else
                {
                    if (momentum.Result.Last(0) > momentumMA.Result.Last(0) && momentum.Result.Last(1) > momentumMA.Result.Last(1))
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "buy", initialSL, null);
                    }
                }
            }
            if (sellSetup)
            {
                if (MarketSeries.High.Last(1) > lowMA.Result.Last(1))
                {
                    sellSetup = false;
                }
                else
                {
                    if (momentum.Result.Last(0) < momentumMA.Result.Last(0) && momentum.Result.Last(1) < momentumMA.Result.Last(1))
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "sell", initialSL, null);
                    }
                }
            }

            if (this.Positions.Find("buy") == null)
            {
                if (MarketSeries.Low.Last(0) > highMA.Result.Last(0) & MarketSeries.Low.Last(1) > highMA.Result.Last(1))
                {
                    buySetup = true;
                }
            }
            if (this.Positions.Find("sell") == null)
            {
                if (MarketSeries.High.Last(0) < lowMA.Result.Last(0) & MarketSeries.High.Last(1) < lowMA.Result.Last(1))
                {
                    sellSetup = true;
                }
            }


        }

        protected override void OnTick()
        {
            foreach (Position p in this.Positions)
            {

                if (p.StopLoss != null && p.NetProfit > initialTP * positionSize * Symbol.PipValue)
                {
                    if (p.TradeType == TradeType.Buy)
                    {
                        ModifyPosition(p, null, null);
                    }
                    else if (p.TradeType == TradeType.Sell)
                    {
                        ModifyPosition(p, null, null);
                    }

                }
                if (p.StopLoss == null)
                {
                    if (p.TradeType == TradeType.Buy)
                    {
                        if (MarketSeries.Close.LastValue < lowTrail.Result.LastValue)
                        {
                            ClosePosition(p);
                            buySetup = false;
                        }
                    }
                    else if (p.TradeType == TradeType.Sell)
                    {
                        if (MarketSeries.Close.LastValue > highTrail.Result.LastValue)
                        {
                            ClosePosition(p);
                            sellSetup = false;
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
