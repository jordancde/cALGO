using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class LucrumLR : Robot
    {

        [Parameter(DefaultValue = 10)]
        public int shortWMAnum { get; set; }
        [Parameter(DefaultValue = 120)]
        public int longWMAnum { get; set; }
        [Parameter(DefaultValue = 10)]
        public int positionSizePercent { get; set; }
        [Parameter(DefaultValue = 30)]
        public int OGpipsProfit { get; set; }
        [Parameter(DefaultValue = 10)]
        public int profitPercent { get; set; }
        [Parameter(DefaultValue = 30)]
        public int maxLossPercentOG { get; set; }
        [Parameter(DefaultValue = 100)]
        public int pipsMax { get; set; }
        [Parameter(DefaultValue = 100)]
        public int RecSLPercent { get; set; }
        [Parameter(DefaultValue = 2)]
        public int multiplier { get; set; }
        [Parameter(DefaultValue = true)]
        public bool scaling { get; set; }
        [Parameter(DefaultValue = 5)]
        public int TSTrailPips { get; set; }
        [Parameter(DefaultValue = 10)]
        public int TSShiftPips { get; set; }
        [Parameter(DefaultValue = false)]
        public bool roundingToNearest { get; set; }
        [Parameter(DefaultValue = 3)]
        public int lossWait { get; set; }



        private WeightedMovingAverage longWMA;
        private WeightedMovingAverage shortWMA;
        public double boxTop = double.MinValue;
        public double boxBottom = double.MaxValue;
        public double prevBoxBottom = 0;
        public double prevBoxTop = 0;
        public double startBalance;
        public bool crossUp = false;
        public bool crossDown = false;
        public double maxLossPercent;

        public double lossTotal = 0;
        public int pipsProfit;
        public int positionSize;
        public int OGpositionSize;
        public double nextLoss = 0;
        public int tradeTP = 0;
        public bool modified = false;

        public bool recoveryMode = false;


        public int fakeLosses = 0;
        public bool fakeTrading = false;
        public double entryPrice = 0;
        public bool fakeCrossUp = false;
        public bool fakeCrossDown = false;
        public bool fakeSell = false;
        public bool fakeBuy = false;


        //public int multiWait = 0;
        protected override void OnStart()
        {
            OGpositionSize = (int)(Account.Balance * (positionSizePercent / 100));
            positionSize = OGpositionSize;
            pipsProfit = OGpipsProfit;
            longWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, longWMAnum);
            shortWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, shortWMAnum);
            startBalance = Account.Balance;
            maxLossPercent = maxLossPercentOG;
        }
        protected override void OnBar()
        {
            if (Positions.Count == 0)
            {
                lossWaitBar();
            }
            if (fakeLosses <= lossWait && Positions.Count == 0)
            {
                return;
            }
            int index = MarketSeries.Close.Count - 1;
            //If the short crosses over long going up
            if (shortWMA.Result[index - 1] > longWMA.Result[index - 1] && shortWMA.Result[index - 2] < longWMA.Result[index - 2])
            {
                if (!recoveryMode)
                {
                    prevBoxBottom = boxBottom;
                }
                if (this.Positions.Count == 0)
                {
                    fakeLosses = 0;
                    lossTotal = 0;

                    pipsProfit = OGpipsProfit;

                    positionSize = OGpositionSize;
                    //multiWait = (int)Math.Round(Math.Log(Symbol.VolumeMin / (Account.Balance * positionSizePercent / 100), 2));
                    boxTop = double.MinValue;
                    boxBottom = double.MaxValue;
                    tradeTP = pipsProfit;
                    ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", null, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                }

                crossUp = true;
                crossDown = false;


            }
            //If the short crosses over long going down
            else if (shortWMA.Result[index - 1] < longWMA.Result[index - 1] && shortWMA.Result[index - 2] > longWMA.Result[index - 2])
            {
                if (!recoveryMode)
                {
                    prevBoxTop = boxTop;
                }
                if (this.Positions.Count == 0)
                {
                    lossTotal = 0;
                    fakeLosses = 0;
                    pipsProfit = OGpipsProfit;
                    positionSize = OGpositionSize;
                    //multiWait = (int)Math.Round(Math.Log(Symbol.VolumeMin / (Account.Balance * positionSizePercent / 100), 2));
                    tradeTP = pipsProfit;
                    ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", null, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    boxTop = double.MinValue;
                    boxBottom = double.MaxValue;
                }

                crossUp = false;
                crossDown = true;

            }

        }

        protected override void OnTick()
        {
            if (Positions.Count == 0)
            {
                lossWaitTick();
            }
            if (fakeLosses <= lossWait && Positions.Count == 0)
            {
                return;
            }

            drawLines();
            if (Account.Equity <= Symbol.PipValue * positionSize * 5)
            {
                ClosePosition(this.Positions[0]);
                Stop();
            }
            if (scaling)
            {
                if (roundingToNearest)
                {
                    OGpositionSize = (int)Symbol.NormalizeVolume(Account.Balance * positionSizePercent / 100, RoundingMode.ToNearest);
                }
                else
                {
                    OGpositionSize = (int)Symbol.NormalizeVolume(Account.Balance * positionSizePercent / 100, RoundingMode.Down);
                }
            }
            else
            {
                OGpositionSize = 1000;
                maxLossPercent = startBalance / Account.Balance * maxLossPercentOG;

            }

            //Manual TP
            if (recoveryMode && this.Positions.Find("Buy") != null)
            {
                if (Symbol.Bid >= this.Positions[0].EntryPrice + tradeTP * Symbol.PipSize)
                {
                    ClosePosition(this.Positions[0]);
                }
            }
            else if (recoveryMode && this.Positions.Find("Sell") != null)
            {
                if (Symbol.Bid <= this.Positions[0].EntryPrice - tradeTP * Symbol.PipSize)
                {
                    ClosePosition(this.Positions[0]);
                }
            }
            if (this.Positions.Count > 0 && this.Positions.Find("Buy") != null && !recoveryMode)
            {
                if (modified && Symbol.Bid - this.Positions[0].StopLoss >= TSShiftPips * Symbol.PipSize)
                {
                    this.ModifyPosition(this.Positions[0], Symbol.Bid - TSTrailPips * Symbol.PipSize, null);
                }
                if (Symbol.Bid >= this.Positions[0].EntryPrice + tradeTP * Symbol.PipSize && !modified)
                {
                    this.ModifyPosition(this.Positions[0], Symbol.Bid - TSTrailPips * Symbol.PipSize, null);
                    modified = true;
                }

            }
            else if (this.Positions.Count > 0 && this.Positions.Find("Sell") != null && !recoveryMode)
            {
                if (modified && this.Positions[0].StopLoss - Symbol.Bid >= TSShiftPips * Symbol.PipSize)
                {
                    this.ModifyPosition(this.Positions[0], Symbol.Bid + TSTrailPips * Symbol.PipSize, null);
                }
                if (Symbol.Bid <= this.Positions[0].EntryPrice - tradeTP * Symbol.PipSize && !modified)
                {
                    this.ModifyPosition(this.Positions[0], Symbol.Bid + TSTrailPips * Symbol.PipSize, null);
                    modified = true;
                }
            }

            int index = MarketSeries.Close.Count - 1;
            //If the short crosses over long going up


            if (crossUp)
            {
                if (Symbol.Bid > boxTop)
                {
                    boxTop = Symbol.Bid;
                }
                if (this.Positions.Find("Sell") != null && Symbol.Bid > prevBoxTop)
                {

                    ClosePosition(this.Positions.Find("Sell"));
                    Print(History[History.Count - 1].NetProfit);
                    lossTotal += History[History.Count - 1].NetProfit;

                    positionSize = positionSize * multiplier;

                    nextLoss = (Symbol.Bid - prevBoxBottom) * positionSize / Symbol.Bid;



                    if (nextLoss < Account.Equity * maxLossPercent / 100)
                    {
                        pipsProfit = (int)((((-1) * lossTotal + (-1) * lossTotal * profitPercent / 100 + positionSize * 16 / 1000000) / positionSize) / Symbol.PipValue);
                    }
                    else
                    {
                        pipsProfit = (int)((((-1) * lossTotal + positionSize * 16 / 1000000) / positionSize) / Symbol.PipValue);
                    }
                    //If the pips profit is too high
                    if (pipsProfit > pipsMax)
                    {
                        pipsProfit = pipsMax;
                        //sets position size for profit at pipsMax and calculates new potential loss
                        positionSize = (int)(((-1) * lossTotal + (-1) * lossTotal * profitPercent / 100 + positionSize * 16 / 1000000) / (pipsProfit * Symbol.PipValue));
                        nextLoss = (Symbol.Bid - prevBoxBottom) * positionSize / Symbol.Bid;

                        //If potential loss is too great, 
                        if (nextLoss > Account.Equity * maxLossPercent / 100)
                        {
                            //positionSize = (int)(((-1) * lossTotal + positionSize * 16 / 1000000) / (pipsProfit * Symbol.PipSize));
                            pipsProfit = (int)((((-1) * lossTotal + positionSize * 16 / 1000000) / positionSize) / Symbol.PipValue);

                        }
                        positionSize = (int)Symbol.NormalizeVolume(positionSize, RoundingMode.ToNearest);

                    }
                    /*if (nextLoss > Account.Balance * maxLossPercent / 100)
                    {
                        pipsProfit = (int)((((-1) * lossTotal + positionSize * 16 / 1000000) / positionSize) / Symbol.PipSize);
                    }*/

                    //ChartObjects.DrawText("line", prevBoxBottom.ToString(), index, prevBoxBottom, VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Red);
                    if (nextLoss > Account.Equity * maxLossPercent / 100)
                    {
                        prevBoxBottom = Symbol.Bid - pipsProfit * Symbol.PipSize * RecSLPercent / 100;
                        recoveryMode = true;
                        Print("Recovery");
                    }
                    Print("Executing Buy, TP: " + pipsProfit + ", PositionSize: " + positionSize + ", LossTotal: " + lossTotal + ", TP$: " + pipsProfit * Symbol.PipValue * positionSize);

                    ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", null, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    tradeTP = pipsProfit;
                }
            }
            if (crossDown)
            {
                if (Symbol.Bid < boxBottom)
                {
                    boxBottom = Symbol.Bid;
                }
                if (this.Positions.Find("Buy") != null && Symbol.Bid < prevBoxBottom)
                {

                    ClosePosition(this.Positions.Find("Buy"));
                    Print(History[History.Count - 1].NetProfit);
                    lossTotal += History[History.Count - 1].NetProfit;

                    positionSize = positionSize * multiplier;

                    nextLoss = (prevBoxTop - Symbol.Bid) * positionSize / Symbol.Bid;

                    if (nextLoss < Account.Equity * maxLossPercent / 100)
                    {
                        pipsProfit = (int)((((-1) * lossTotal + (-1) * lossTotal * profitPercent / 100 + positionSize * 16 / 1000000) / positionSize) / Symbol.PipValue);
                    }
                    else
                    {
                        pipsProfit = (int)((((-1) * lossTotal + positionSize * 16 / 1000000) / positionSize) / Symbol.PipValue);
                    }

                    if (pipsProfit > pipsMax)
                    {
                        pipsProfit = pipsMax;
                        positionSize = (int)(((-1) * lossTotal + (-1) * lossTotal * profitPercent / 100 + positionSize * 16 / 1000000) / (pipsProfit * Symbol.PipValue));
                        nextLoss = (prevBoxTop - Symbol.Bid) * positionSize / Symbol.Bid;



                        if (nextLoss > Account.Equity * maxLossPercent / 100)
                        {
                            //positionSize = (int)(((-1) * lossTotal + positionSize * 16 / 1000000) / (pipsProfit * Symbol.PipSize));
                            pipsProfit = (int)((((-1) * lossTotal + positionSize * 16 / 1000000) / positionSize) / Symbol.PipValue);
                        }


                        positionSize = (int)Symbol.NormalizeVolume(positionSize, RoundingMode.ToNearest);

                    }
                    /*if (nextLoss > Account.Balance * maxLossPercent / 100)
                    {
                        pipsProfit = (int)((((-1) * lossTotal + positionSize * 16 / 1000000) / positionSize) / Symbol.PipSize);

                    }*/

                    //ChartObjects.DrawText("line", prevBoxTop.ToString(), index, prevBoxTop, VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Red);

                    if (nextLoss > Account.Equity * maxLossPercent / 100)
                    {
                        prevBoxTop = Symbol.Bid + pipsProfit * Symbol.PipSize * RecSLPercent / 100;
                        recoveryMode = true;
                        Print("Recovery");
                    }
                    Print(pipsProfit * Symbol.PipValue * RecSLPercent / 100 + " " + pipsProfit * Symbol.PipSize * RecSLPercent / 100);


                    Print("Executing Sell, TP: " + pipsProfit + ", PositionSize: " + positionSize + ", LossTotal: " + lossTotal + ", TP$: " + pipsProfit * Symbol.PipValue * positionSize);

                    ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", null, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    tradeTP = pipsProfit;
                }
            }
            if (this.Positions.Count == 0)
            {
                recoveryMode = false;
                modified = false;

            }
        }




        private void drawLines()
        {
            ChartObjects.RemoveAllObjects();
            if (this.Positions.Find("Buy") != null && !modified)
            {
                if (recoveryMode)
                {
                    ChartObjects.DrawHorizontalLine("TP", this.Positions[0].EntryPrice + tradeTP * Symbol.PipSize, Colors.Yellow, 2, LineStyle.Lines);
                }
                else
                {
                    ChartObjects.DrawHorizontalLine("TP", this.Positions[0].EntryPrice + tradeTP * Symbol.PipSize, Colors.Green, 2, LineStyle.Lines);
                    ChartObjects.DrawHorizontalLine("Recovery", this.Positions[0].EntryPrice + tradeTP / (1 + profitPercent / 100) * Symbol.PipSize, Colors.Yellow, 2, LineStyle.Lines);
                }
            }
            else if (this.Positions.Find("Sell") != null && !modified)
            {
                if (recoveryMode)
                {
                    ChartObjects.DrawHorizontalLine("TP", this.Positions[0].EntryPrice - tradeTP * Symbol.PipSize, Colors.Yellow, 2, LineStyle.Lines);
                }
                else
                {
                    ChartObjects.DrawHorizontalLine("TP", this.Positions[0].EntryPrice - tradeTP * Symbol.PipSize, Colors.Green, 2, LineStyle.Lines);
                    ChartObjects.DrawHorizontalLine("Recovery", this.Positions[0].EntryPrice - tradeTP / (1 + profitPercent / 100) * Symbol.PipSize, Colors.Yellow, 2, LineStyle.Lines);
                }
            }
            if (this.Positions.Find("Buy") != null)
            {

                ChartObjects.DrawHorizontalLine("boxBottom", prevBoxBottom, Colors.Red, 2, LineStyle.Lines);
            }
            else if (this.Positions.Find("Sell") != null)
            {
                ChartObjects.DrawHorizontalLine("boxTop", prevBoxTop, Colors.Red, 2, LineStyle.Lines);
            }

        }

        private void lossWaitBar()
        {
            int index = MarketSeries.Close.Count - 1;
            //If the short crosses over long going up
            if (shortWMA.Result[index - 1] > longWMA.Result[index - 1] && shortWMA.Result[index - 2] < longWMA.Result[index - 2])
            {
                prevBoxBottom = boxBottom;
                if (!fakeTrading)
                {
                    boxTop = double.MinValue;
                    boxBottom = double.MaxValue;
                    entryPrice = Symbol.Bid;
                    fakeTrading = true;
                    fakeSell = false;
                    fakeBuy = true;

                }
                fakeCrossUp = true;
                fakeCrossDown = false;
            }
            else if (shortWMA.Result[index - 1] < longWMA.Result[index - 1] && shortWMA.Result[index - 2] > longWMA.Result[index - 2])
            {
                prevBoxTop = boxTop;
                if (!fakeTrading)
                {
                    boxTop = double.MinValue;
                    boxBottom = double.MaxValue;
                    entryPrice = Symbol.Bid;
                    fakeTrading = true;
                    fakeSell = true;
                    fakeBuy = false;
                }
                fakeCrossUp = false;
                fakeCrossDown = true;
            }
        }

        private void lossWaitTick()
        {
            if (fakeCrossUp)
            {
                if (Symbol.Bid > boxTop)
                {
                    boxTop = Symbol.Bid;
                }
                if (fakeSell && Symbol.Bid > prevBoxTop)
                {
                    fakeLossTotal += (prevBoxTop - entryPrice) * positionSize / Symbol.Bid;
                    fakeLosses++;
                }
                else if (fakeBuy && Symbol.Bid > entryPrice + pipsProfit * Symbol.PipSize)
                {
                    fakeTrading = false;
                    fakeBuy = false;
                    fakeSell = false;
                    fakeLosses = 0;
                }
            }
            else if (fakeCrossDown)
            {
                if (Symbol.Bid < boxBottom)
                {
                    boxBottom = Symbol.Bid;
                }
                if (fakeBuy && Symbol.Bid < prevBoxBottom)
                {
                    fakeLossTotal += (entryPrice - prevBoxBottom) * positionSize / Symbol.Bid;
                    fakeLosses++;
                }
                else if (fakeSell && Symbol.Bid < entryPrice - pipsProfit * Symbol.PipSize)
                {
                    fakeTrading = false;
                    fakeBuy = false;
                    fakeSell = false;
                    fakeLosses = 0;
                }
            }

        }



        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
