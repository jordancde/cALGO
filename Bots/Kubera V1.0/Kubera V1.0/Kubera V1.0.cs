using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _CCR : Robot
    {

        private WeightedMovingAverage _wma77;
        private bool tradeActive;
        private bool tradeActiveBuy;
        private bool tradeActiveSell;
        private bool fakeSellActive;
        private bool fakeBuyActive;
        private int positionSize;
        private double upperBand;
        private double lowerBand;

        [Parameter(DefaultValue = 48)]
        public int pipsProfit { get; set; }
        [Parameter(DefaultValue = 14)]
        public int channelSubtractor { get; set; }
        [Parameter(DefaultValue = 120)]
        public int wmaNumber { get; set; }
        [Parameter(DefaultValue = 1000)]
        public int startingPositionSize { get; set; }
        [Parameter(DefaultValue = 2)]
        public int multiplier { get; set; }

        [Parameter(DefaultValue = 6)]
        public int lossWait { get; set; }

        int fakeTradeCount = 0;
        int tradeActiveBuy1 = 0;
        int tradeActiveSell1 = 0;
        int channelWidth = 0;
        bool fakeTradeActive;
        double EntryPrice1;
        int multiplied = 0;
        int PosWait = 0;
        private Position myPositionSell;
        private Position myPositionBuy;

        protected override void OnStart()
        {
            // Put your initialization logic here

            //Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "The Program has Begun", "");

            upperBand = 0;
            lowerBand = 0;
            tradeActive = false;
            tradeActiveBuy = false;
            fakeTradeActive = false;
            fakeSellActive = false;
            fakeBuyActive = false;
            tradeActiveSell = false;
            startingPositionSize = 1000;
            positionSize = startingPositionSize;
            _wma77 = Indicators.WeightedMovingAverage(MarketSeries.Close, wmaNumber);
            channelWidth = (pipsProfit / 2) - channelSubtractor;




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

            int index = MarketSeries.Close.Count - 1;
            upperBand = _wma77.Result[index] + (channelWidth * Symbol.PipSize);
            lowerBand = _wma77.Result[index] - (channelWidth * Symbol.PipSize);

            if (this.Positions.Count > 0 || fakeTradeActive)
            {



                myPositionSell = null;

                if (!fakeTradeActive)
                {

                    myPositionSell = this.Positions.Find("SellGJ");

                }



                if ((fakeSellActive || myPositionSell != null) && tradeActive == true && tradeActiveBuy == false && Symbol.Bid > EntryPrice1 + ((channelWidth * 2) * Symbol.PipSize))
                {
                    if (PosWait > lossWait)
                    {

                        if (!fakeTradeActive)
                        {


                            ClosePosition(myPositionSell);

                        }
                        else
                        {
                            fakeTradeActive = false;
                        }


                        multiplied++;





                        ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "BuyGJ", null, pipsProfit, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                        string message = "Position Size: " + positionSize + " \nEntry Price: " + this.LastResult.Position.EntryPrice + "\nTake Profit: " + this.LastResult.Position.TakeProfit + "\nStop Loss: " + (Symbol.Bid - ((channelWidth * 2) * Symbol.PipSize));
                        //Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "Buy Placed", message);
                        //Notifications.SendEmail("jordandearsley67@gmail.com", "jordandearsley67@gmail.com", "Buy Placed", message);
                        positionSize = positionSize * multiplier;
                        if (!fakeTradeActive)
                        {

                            EntryPrice1 = this.LastResult.Position.EntryPrice;

                        }
                        else
                        {
                            EntryPrice1 = Symbol.Bid;
                        }

                        tradeActiveBuy = true;
                        tradeActiveSell = false;
                    }
                    //must stop small trades from interfering with big trades
                    PosWait++;
                    return;
                }


                myPositionBuy = null;

                if (!fakeTradeActive)
                {
                    myPositionBuy = this.Positions.Find("BuyGJ");
                }

                if ((fakeBuyActive || myPositionBuy != null) && tradeActive && tradeActiveSell == false && Symbol.Bid < EntryPrice1 - ((channelWidth * 2) * Symbol.PipSize))
                {

                    if (PosWait > lossWait)
                    {
                        if (!fakeTradeActive)
                        {


                            ClosePosition(myPositionBuy);


                        }
                        else
                        {
                            fakeTradeActive = false;
                        }

                        multiplied++;




                        ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "SellGJ", null, pipsProfit, 3, (this.Symbol.Code + " " + this.TimeFrame.ToString()));
                        string message = "Position Size: " + positionSize + " \nEntry Price: " + this.LastResult.Position.EntryPrice + "\nTake Profit: " + this.LastResult.Position.TakeProfit + "\nStop Loss: " + (Symbol.Bid + ((channelWidth * 2) * Symbol.PipSize));
                        //Notifications.SendEmail("jordandearsley67@gmail.com", "4168732580@pcs.rogers.com", "Sell Placed", message);
                        //Notifications.SendEmail("jordandearsley@gmail.com", "jordandearsley67@gmail.com", "Sell Placed", message);
                        positionSize = positionSize * multiplier;
                        if (!fakeTradeActive)
                        {

                            EntryPrice1 = this.LastResult.Position.EntryPrice;

                        }
                        else
                        {
                            EntryPrice1 = Symbol.Bid;
                        }
                        tradeActiveSell = true;
                        tradeActiveBuy = false;
                    }
                    PosWait++;
                    return;
                }


            }

            if (this.Positions.Count == 0 && tradeActiveBuy == false && Symbol.Bid > upperBand)
            {
                //prevent taking multiple trades once a buy has already occurred

                if (tradeActiveBuy1 == 1)
                {
                    return;
                }

                positionSize = startingPositionSize;
                //ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "BuyGJ", null, pipsProfit, 3);
                EntryPrice1 = Symbol.Bid;
                channelWidth = (pipsProfit / 2) - channelSubtractor;
                fakeTradeCount++;
                fakeTradeActive = true;
                fakeBuyActive = true;
                fakeSellActive = false;
                tradeActive = true;
                tradeActiveBuy = true;
                tradeActiveSell = false;
                tradeActiveBuy1 = 1;
                tradeActiveSell1 = 0;
                PosWait = 0;
                multiplied = 0;

                return;
            }
            if (this.Positions.Count == 0 && tradeActiveSell == false && Symbol.Bid < lowerBand)
            {

                //prevent taking multiple trades once a buy has already occurred
                if (tradeActiveSell1 == 1)
                {
                    return;
                }


                positionSize = startingPositionSize;
                //ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "SellGJ", null, pipsProfit, 3);
                EntryPrice1 = Symbol.Bid;
                channelWidth = (pipsProfit / 2) - channelSubtractor;
                fakeTradeActive = true;
                fakeTradeCount++;
                tradeActive = true;
                fakeSellActive = true;
                fakeBuyActive = false;
                tradeActiveSell = true;
                tradeActiveBuy = false;
                tradeActiveSell1 = 1;
                tradeActiveBuy1 = 0;
                PosWait = 0;
                multiplied = 0;
                return;
            }

            if (this.Positions.Count == 0 && !fakeTradeActive)
            {
                tradeActiveSell = false;
                tradeActiveBuy = false;
                fakeTradeActive = false;
                fakeBuyActive = false;
                fakeSellActive = false;
                tradeActive = false;
                positionSize = startingPositionSize;
                channelWidth = (pipsProfit / 2) - channelSubtractor;
                PosWait = 0;
                multiplied = 0;
                return;
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}

