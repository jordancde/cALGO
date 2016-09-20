
using System;
using System.Linq;
using cAlgo.API;
using System.Collections.Generic;

namespace cAlgo.Robots
{



    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]

    public class George : Robot
    {

        private const string Label = "George";

        [Parameter("Buy George Box", MinValue = 0, MaxValue = 700, DefaultValue = 70)]
        public int BuyGeorgeBox { get; set; }
        [Parameter("Sell George Box", MinValue = 0, MaxValue = 700, DefaultValue = 70)]
        public int SellGeorgeBox { get; set; }

        [Parameter("Buy Lot Size", MinValue = 1000, DefaultValue = 3000)]
        public int ParamBuyVolume { get; set; }
        [Parameter("Sell Lot Size", MinValue = 1000, DefaultValue = 3000)]
        public int ParamSellVolume { get; set; }

        [Parameter("Profit ($)", MinValue = 0, DefaultValue = 1.5)]
        public double profit { get; set; }

        [Parameter("Max Loss ($)", MinValue = 0, DefaultValue = 50)]
        public double maxLoss { get; set; }

        [Parameter("Stop After Profit", DefaultValue = false)]
        public bool stopAfterProfit { get; set; }

        [Parameter("Average Streak Time (min)", DefaultValue = 180)]
        public int streakTimeavg { get; set; }

        [Parameter("Average Streak Trades", DefaultValue = 30)]
        public int streakTradesAvg { get; set; }



        public double startingBalance;
        public int streakTrades;
        public DateTime startTime;
        public List<int> avgTimes;
        public List<int> avgTrades;
        public int streakTime;





        protected override void OnStart()
        {
            Positions.Closed += OnPositionClosed;
            Positions.Opened += OnPositionOpened;
            startingBalance = Account.Balance;
            streakTrades = 0;
            startTime = Server.Time;
            avgTimes = new List<int>();
            avgTrades = new List<int>();

        }

        protected bool BuySessionStartedFlag
        {





            get { return Positions.FindAll(Label, Symbol, TradeType.Buy).Any(); }
        }

        protected bool SellSessionStartedFlag
        {




            get { return Positions.FindAll(Label, Symbol, TradeType.Sell).Any(); }
        }

        protected double GetPipsToLevel(double entryPrice, double Level)
        {
            return (entryPrice - Level) / Symbol.PipSize;
        }

        protected double GetTPLevel(double level, TradeType tradeType)
        {
            double result = level;

            if (tradeType == TradeType.Buy)
            {
                result = result + (30 * Symbol.PipSize);
            }
            else if (tradeType == TradeType.Sell)
            {
                result = result - (30 * Symbol.PipSize);
            }
            return result;
        }

        protected double GetMedianTPLevel(Position[] positions)
        {
            double lotSum = 0.0;
            double sum = 0.0;

            foreach (var position in positions)
            {
                double lotSize = position.Volume / 100000.0;
                double price = position.EntryPrice;

                sum += (lotSize * price);
                lotSum += lotSize;
            }

            double resultSum = sum / lotSum;
            double result = GetTPLevel(resultSum, positions[0].TradeType);

            return result;
        }

        protected void SetTakeProfit(string label, TradeType tradeType)
        {
            var positions = Positions.FindAll(label, Symbol, tradeType);

            double? tpLevel;

            if (positions.Length == 1 || positions.Length == 2)
            {
                var firstPosition = positions[0];

                tpLevel = tradeType == TradeType.Buy ? firstPosition.EntryPrice + (BuyGeorgeBox * Symbol.PipSize) : firstPosition.EntryPrice - (SellGeorgeBox * Symbol.PipSize);
            }
            else
            {
                tpLevel = GetMedianTPLevel(positions);
            }

            var high = Math.Max(Symbol.Ask, Symbol.Bid);
            var low = Math.Min(Symbol.Ask, Symbol.Bid);

            if (tradeType == TradeType.Buy && tpLevel <= low)
            {
                tpLevel = high;
            }

            if (tradeType == TradeType.Sell && tpLevel >= high)
            {
                tpLevel = low;
            }

            foreach (var position in positions)
            {
                ModifyPosition(position, null, tpLevel);
            }
        }

        private void OnPositionOpened(PositionOpenedEventArgs positionClosedEventArgs)
        {
            try
            {
                var openedPosition = positionClosedEventArgs.Position;

                if (openedPosition.Label != Label || openedPosition.SymbolCode != Symbol.Code)
                    return;

                CreatePendingOrder(openedPosition);
                SetTakeProfit(Label, openedPosition.TradeType);
                streakTrades++;
            } catch (Exception e)
            {
                Print("Index Error Caught");
            }
        }

        private void CreatePendingOrder(Position position)
        {
            var pipsGeorge = position.TradeType == TradeType.Buy ? BuyGeorgeBox : SellGeorgeBox;

            var priceGeorge = pipsGeorge * Symbol.PipSize;

            var priceLevel = position.TradeType == TradeType.Buy ? position.EntryPrice - priceGeorge : position.EntryPrice + priceGeorge;

            var volume = position.TradeType == TradeType.Buy ? (int)ParamBuyVolume : (int)ParamSellVolume;

            PlaceLimitOrder(position.TradeType, Symbol, volume, priceLevel, Label);
        }

        private void OnPositionClosed(PositionClosedEventArgs positionOpenedEventArgs)
        {
            if (!BuySessionStartedFlag)
            {
                DeleteAllNormalPendingOrders(TradeType.Buy);
            }

            if (!SellSessionStartedFlag)
            {
                DeleteAllNormalPendingOrders(TradeType.Sell);
            }

        }

        protected void DeleteAllNormalPendingOrders(TradeType tradeType)
        {
            foreach (var order in PendingOrders)
            {
                if (order.TradeType == tradeType && order.SymbolCode == Symbol.Code && order.Label == Label)
                    CancelPendingOrder(order);
            }
        }


        public void draw()
        {

            ChartObjects.RemoveAllObjects();

            var hAlign = HorizontalAlignment.Left;
            var color = Colors.White;
            streakTime = (int)(Server.Time - startTime).TotalMinutes;
            var targetEquity = startingBalance + profit;
            ChartObjects.DrawText("text", "Equity: " + Account.Equity + "/" + targetEquity, MarketSeries.Close.Count - 1, Symbol.Bid, VerticalAlignment.Top, hAlign, color);
            ChartObjects.DrawText("text2", "Time (min): " + streakTime + "/" + streakTimeavg + " | Trades: " + streakTrades + "/" + streakTradesAvg, MarketSeries.Close.Count - 1, Symbol.Bid, VerticalAlignment.Bottom, hAlign, color);
            var netPosition = 0.0;
            foreach (Position pos in this.Positions)
            {
                if (pos.TradeType == TradeType.Buy)
                {
                    netPosition += pos.Volume;
                }
                else
                {
                    netPosition -= pos.Volume;
                }
            }
            var pips = ((targetEquity - Account.Equity) / (netPosition * Symbol.PipValue)) * Symbol.PipSize;
            ChartObjects.DrawHorizontalLine("TP", Symbol.Bid + pips, Colors.White, 2, LineStyle.Lines);
        }

        protected override void OnTick()
        {
            draw();
            if (!BuySessionStartedFlag)
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol, ParamBuyVolume, Label);
            }

            if (!SellSessionStartedFlag)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol, ParamSellVolume, Label);
            }

            if ((Account.Equity - startingBalance) >= profit || startingBalance - Account.Equity >= maxLoss)
            {
                DeleteAllNormalPendingOrders(TradeType.Buy);
                DeleteAllNormalPendingOrders(TradeType.Sell);
                Print("Hit Profit, closing all | " + streakTime + " minutes");
                foreach (var position in this.Positions.FindAll(Label))
                {
                    ClosePosition(position);
                }
                if ((Account.Equity - startingBalance) >= profit)
                {
                    avgTrades.Add(streakTrades);
                    avgTimes.Add(streakTime);
                }
                streakTrades = 0;
                startingBalance = Account.Balance;
                startTime = Server.Time;
                if (stopAfterProfit)
                {
                    Stop();
                }


            }

        }
        protected override void OnStop()
        {
            var tradeSum = 0.0;
            var timeSum = 0.0;
            foreach (int num in avgTrades)
            {
                tradeSum += num;
            }
            foreach (int num in avgTimes)
            {
                timeSum += num;
            }
            Print("Average Trades/Streak: " + ((int)(tradeSum / (avgTrades.Count))) + " | Average Minutes/Streak: " + ((int)(timeSum / (avgTimes.Count))));
        }
    }


}

