using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Keltner : Robot
    {

        [Parameter(DefaultValue = 20)]
        public int stopLossPips { get; set; }
        [Parameter(DefaultValue = 10)]
        public int takeProfitPips { get; set; }
        [Parameter(DefaultValue = 70)]
        public int wmaNum { get; set; }
        [Parameter(DefaultValue = 20)]
        public int channelPips { get; set; }
        [Parameter(DefaultValue = 5)]
        public int TSTrailPips { get; set; }
        [Parameter(DefaultValue = 10)]
        public int TSShiftPips { get; set; }
        [Parameter(DefaultValue = 10)]
        public int positionSizePercent { get; set; }


        public DateTime crossUp;
        public DateTime crossDown;
        public WeightedMovingAverage wma;
        public DateTime tradeTime;
        public int index = 0;
        public bool modified = false;
        public int tradeTP;
        public double entryPrice;
        public int positionSize = 1000;
        public DateTime startDate;
        public double startingBalance;
        public bool notified = false;

        protected override void OnStart()
        {
            startingBalance = Account.Balance;
            startDate = this.Time;
            positionSize = (int)Symbol.NormalizeVolume(Account.Balance * (positionSizePercent / 100), RoundingMode.ToNearest);
            // Put your initialization logic here
            wma = Indicators.WeightedMovingAverage(MarketSeries.Close, wmaNum);
            tradeTime = Server.Time;
        }
        protected override void OnError(Error error)
        {
            //  Print the error to the log
            switch (error.Code)
            {
                case ErrorCode.BadVolume:
                    Print("Bad Volume");
                    break;
                case ErrorCode.TechnicalError:
                    Print("Technical Error");
                    break;
                case ErrorCode.NoMoney:
                    Print("No Money");
                    Notifications.SendEmail("gplytas@gmail.com", "4168732580@pcs.rogers.com", "No Money", "This will never happen");
                    break;
                case ErrorCode.Disconnected:
                    Print("Disconnected");
                    Notifications.SendEmail("gplytas@gmail.com", "4168732580@pcs.rogers.com", "Disconnect Warning", "This will never happen");
                    break;
                case ErrorCode.MarketClosed:
                    Print("Market Closed");
                    break;
            }
        }



        protected override void OnTick()
        {
            if (this.Positions.Count == 0)
            {

                modified = false;
                if (this.History.Count > 0 && !notified)
                {
                    if (this.History[this.History.Count - 1].NetProfit >= 0)
                    {
                        Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "Position Closed in Profit", "Open Time: " + this.History[this.History.Count - 1].EntryTime + "\nClose Time: " + this.History[this.History.Count - 1].ClosingTime + "\nPosition Size: " + this.History[this.History.Count - 1].Volume + "\nProfit: " + this.History[this.History.Count - 1].NetProfit + "\nCurrent Balance: " + Account.Balance);
                    }
                    else
                    {
                        Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "Position Closed in Loss", "Open Time: " + this.History[this.History.Count - 1].EntryTime + "\nClose Time: " + this.History[this.History.Count - 1].ClosingTime + "\nPosition Size: " + this.History[this.History.Count - 1].Volume + "\nLoss: " + this.History[this.History.Count - 1].NetProfit + "\nCurrent Balance: " + Account.Balance);
                    }
                    notified = true;
                }

            }

            if (this.Positions.Count > 0 && this.Positions.Find("Buy") != null)
            {
                if (modified && Symbol.Bid - this.Positions.Find("Buy").StopLoss >= TSShiftPips * Symbol.PipSize)
                {
                    this.ModifyPosition(this.Positions.Find("Buy"), Symbol.Bid - TSTrailPips * Symbol.PipSize, null);

                }
                if (Symbol.Bid >= this.Positions.Find("Buy").EntryPrice + tradeTP * Symbol.PipSize && !modified)
                {
                    Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "Take Profit Hit, Trailing", "Position Size: " + positionSize + "\nStop Loss Pips: " + TSTrailPips);
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
                    Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "Take Profit Hit, Trailing", "Position Size: " + positionSize + "\nStop Loss Pips: " + TSTrailPips);
                }
            }
            if (this.Positions.Count == 0)
            {

                //Print("crossUp: " + crossUp + ", crossDown: " + crossDown);
                if (Symbol.Bid > wma.Result.LastValue + channelPips * Symbol.PipSize && crossUp > tradeTime)
                {
                    Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "Buy Placed", "Position Size: " + positionSize + "\nTake Profit Pips: " + takeProfitPips + "\nStop Loss Pips: " + stopLossPips + "\nTrade Time: " + Server.Time);
                    Notifications.SendEmail("jordandearsley67@gmail.com", "jordandearsley67@gmail.com", "The Program has Begun", "");
                    ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "Buy", stopLossPips, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    notified = false;
                    entryPrice = Symbol.Bid;
                    tradeTP = takeProfitPips;
                    tradeTime = Server.Time;

                }
                else if (Symbol.Bid < wma.Result.LastValue - channelPips * Symbol.PipSize && crossDown > tradeTime)
                {
                    tradeTime = Server.Time;
                    Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "Sell Placed", "Position Size: " + positionSize + "\nTake Profit Pips: " + takeProfitPips + "\nStop Loss Pips: " + stopLossPips + "\nTrade Time: " + Server.Time);
                    ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "Sell", stopLossPips, null, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                    notified = false;
                    entryPrice = Symbol.Ask;
                    tradeTP = takeProfitPips;
                }
            }


        }
        protected override void OnBar()
        {
            /*if (startDate.AddMonths(monthTimeout) < this.Time)
            {
                if (Account.Balance < startingBalance)
                {
                    Stop();
                }
            }*/
            index = MarketSeries.Close.Count - 1;
            positionSize = (int)Symbol.NormalizeVolume(Account.Balance * (positionSizePercent / 100), RoundingMode.ToNearest);
            if (MarketSeries.Close[index - 1] > wma.Result[index - 1] + channelPips * Symbol.PipSize && MarketSeries.Close[index - 2] < wma.Result[index - 2] + channelPips * Symbol.PipSize)
            {
                crossUp = Server.Time;
                Print(crossUp);
            }
            else if (MarketSeries.Close[index - 1] < wma.Result[index - 1] - channelPips * Symbol.PipSize && MarketSeries.Close[index - 2] > wma.Result[index - 2] - channelPips * Symbol.PipSize)
            {
                crossDown = Server.Time;
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
