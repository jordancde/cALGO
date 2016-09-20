using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class LineBreak : Robot
    {
        [Parameter(DefaultValue = 70)]
        public int wmaNum { get; set; }
        [Parameter(DefaultValue = 10)]
        public int TSShiftPips { get; set; }

        [Parameter(DefaultValue = 5)]
        public int TSTrailPips { get; set; }
        [Parameter(DefaultValue = 20)]
        public int takeProfitPips { get; set; }
        [Parameter(DefaultValue = 20)]
        public int stopLossPips { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int positionSize { get; set; }

        [Parameter(DefaultValue = 8)]
        public int timeStartHours { get; set; }

        [Parameter(DefaultValue = 10)]
        public int timeEndHours { get; set; }

        [Parameter(DefaultValue = false)]
        public bool candleColour { get; set; }


        private WeightedMovingAverage wma;
        public int index = 0;
        public bool modified = false;
        public int tradeTP = 0;
        public double entryPrice = 0;
        public bool taken = false;
        public bool crossUp = false;
        public bool crossDown = false;

        public DateTime timeNow;
        public DateTime startTime;
        public DateTime checkTime;
        public double startingBalance;
        protected override void OnStart()
        {
            // Put your initialization logic here
            wma = Indicators.WeightedMovingAverage(MarketSeries.Close, wmaNum);
            positionSize = (int)Symbol.NormalizeVolume(positionSize, RoundingMode.ToNearest);
            startTime = this.Time;
            Print(startTime.Year);
            startingBalance = Account.Balance;
            checkTime = startTime.AddMonths(1);


        }
        protected override void OnBar()
        {

            timeNow = MarketSeries.OpenTime.LastValue;
            timeNow = timeNow.AddHours(-4);
            /*if (timeNow > checkTime)
            {
                if (Account.Balance < startingBalance)
                {
                    Stop();
                }
            }*/
            if (timeNow.Hour < timeStartHours || timeNow.Hour >= timeEndHours)
            {

                taken = true;
                return;

            }
            //Print(taken + "1");
            index = MarketSeries.Close.Count - 1;
            if (this.Positions.Count == 0 && MarketSeries.Close[index - 2] < wma.Result[index - 2] && MarketSeries.Close[index - 1] > wma.Result[index - 1])
            {
                crossUp = true;
                crossDown = false;
                taken = false;
                //Print("CrossedUP");
            }
            //Print(MarketSeries.Close[index - 1] + " " + wma.Result[index - 1] + " " + MarketSeries.Close[index - 1] + " " + wma.Result[index]);
            if (this.Positions.Count == 0 && MarketSeries.Close[index - 2] > wma.Result[index - 2] && MarketSeries.Close[index - 1] < wma.Result[index - 1])
            {
                crossDown = true;
                crossUp = false;
                taken = false;
                //Print("CrossedDOWN");
            }
            //Print(taken + "2");
            if (!candleColour)
            {
                if (Positions.Count == 0 && MarketSeries.Low[index - 1] > wma.Result[index - 1] && timeNow.Hour >= timeStartHours && timeNow.Hour <= timeEndHours && !taken && crossUp)
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", stopLossPips, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    entryPrice = Symbol.Bid;
                    tradeTP = takeProfitPips;
                    taken = true;
                }
                else if (Positions.Count == 0 && MarketSeries.High[index - 1] < wma.Result[index - 1] && timeNow.Hour >= timeStartHours && timeNow.Hour < timeEndHours && !taken && crossDown)
                {
                    ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", stopLossPips, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    entryPrice = Symbol.Ask;
                    tradeTP = takeProfitPips;
                    taken = true;
                }
            }
            else
            {
                if (Positions.Count == 0 && MarketSeries.Low[index - 1] > wma.Result[index - 1] && MarketSeries.Close[index - 1] >= MarketSeries.Open[index - 1] && timeNow.Hour >= timeStartHours && timeNow.Hour <= timeEndHours && !taken && crossUp)
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", stopLossPips, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    entryPrice = Symbol.Bid;
                    tradeTP = takeProfitPips;
                    taken = true;
                }
                else if (Positions.Count == 0 && MarketSeries.High[index - 1] < wma.Result[index - 1] && MarketSeries.Close[index - 1] <= MarketSeries.Open[index - 1] && timeNow.Hour >= timeStartHours && timeNow.Hour < timeEndHours && !taken && crossDown)
                {
                    ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", stopLossPips, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    entryPrice = Symbol.Ask;
                    tradeTP = takeProfitPips;
                    taken = true;
                }
            }
            //Print(taken + "3");

        }
        protected override void OnTick()
        {


            if (this.Positions.Count == 0)
            {

                modified = false;
                return;

            }

            if (this.Positions.Count > 0 && this.Positions.Find("Buy") != null)
            {
                if (modified && Symbol.Bid - this.Positions.Find("Buy").StopLoss >= TSShiftPips * Symbol.PipSize)
                {
                    this.ModifyPosition(this.Positions.Find("Buy"), Symbol.Bid - TSTrailPips * Symbol.PipSize, null);

                }
                if (Symbol.Bid >= this.Positions.Find("Buy").EntryPrice + tradeTP * Symbol.PipSize && !modified)
                {
                    this.ModifyPosition(this.Positions.Find("Buy"), Symbol.Bid - TSTrailPips * Symbol.PipSize, null);
                    modified = true;

                }

            }
            else if (this.Positions.Count > 0 && this.Positions.Find("Sell") != null)
            {
                if (modified && this.Positions.Find("Sell").StopLoss - Symbol.Ask >= TSShiftPips * Symbol.PipSize)
                {
                    this.ModifyPosition(this.Positions.Find("Sell"), Symbol.Ask + TSTrailPips * Symbol.PipSize, null);

                }
                if (Symbol.Ask <= this.Positions.Find("Sell").EntryPrice - tradeTP * Symbol.PipSize && !modified)
                {
                    this.ModifyPosition(this.Positions.Find("Sell"), Symbol.Ask + TSTrailPips * Symbol.PipSize, null);
                    modified = true;

                }
            }



        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
