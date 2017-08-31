using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class StochasticPOP : Robot
    {
        [Parameter(DefaultValue = 100)]
        public int positionSizePercent { get; set; }

        [Parameter(DefaultValue = 6)]
        public int kPeriods { get; set; }

        [Parameter(DefaultValue = 1)]
        public int kSlowing { get; set; }

        [Parameter(DefaultValue = 12)]
        public int dPeriods { get; set; }

        [Parameter(DefaultValue = 160)]
        public int initialSL { get; set; }

        [Parameter(DefaultValue = 80)]
        public int initialTP { get; set; }

        [Parameter(DefaultValue = 15)]
        public int trailMAPeriods { get; set; }





        public StochasticOscillator _stochastic;
        public bool setUp;
        public bool setDown;
        public SimpleMovingAverage highTrail;
        public SimpleMovingAverage lowTrail;
        public int positionSize;

        protected override void OnStart()
        {
            _stochastic = Indicators.StochasticOscillator(kPeriods, kSlowing, dPeriods, MovingAverageType.Simple);
            setUp = false;
            setDown = false;
            highTrail = Indicators.SimpleMovingAverage(MarketSeries.High, trailMAPeriods);
            lowTrail = Indicators.SimpleMovingAverage(MarketSeries.Low, trailMAPeriods);
            positionSize = (int)Symbol.NormalizeVolume(Account.Balance * positionSizePercent / 100, RoundingMode.ToNearest);
        }

        protected override void OnBar()
        {
            positionSize = (int)Symbol.NormalizeVolume(Account.Balance * positionSizePercent / 100, RoundingMode.ToNearest);
            if (this.Positions.Count == 0)
            {
                if (_stochastic.PercentK.LastValue > 70)
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "POPbuy", initialSL, null);

                }



                if (_stochastic.PercentK.LastValue < 30)
                {
                    ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "POP", initialSL, null);

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

                        }
                    }
                    else if (p.TradeType == TradeType.Sell)
                    {
                        if (MarketSeries.Close.LastValue > highTrail.Result.LastValue)
                        {
                            ClosePosition(p);

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
