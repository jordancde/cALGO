using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Gap : Robot
    {
        [Parameter(DefaultValue = 1000)]
        public int positionSize { get; set; }
        [Parameter(DefaultValue = 20)]
        public int SLpips { get; set; }
        [Parameter(DefaultValue = 10)]
        public int TPpips { get; set; }

        public bool buySetUp;
        public bool sellSetUp;
        public DateTime setUpTime;

        protected override void OnStart()
        {
            // Put your initialization logic here
            buySetUp = false;
            sellSetUp = false;
            setUpTime = DateTime.Now;
        }

        protected override void OnTick()
        {
            if ((DateTime.Now - setUpTime).Days >= 1)
            {
                buySetUp = false;
                sellSetUp = false;
            }
            if (!buySetUp && !sellSetUp)
            {
                Print(MarketData.GetSeries(TimeFrame.Daily).Open.LastValue + " " + MarketData.GetSeries(TimeFrame.Daily).Open.Last(1));
                if (MarketData.GetSeries(TimeFrame.Daily).Open.LastValue > MarketData.GetSeries(TimeFrame.Daily).Open.LastValue)
                {
                    sellSetUp = true;
                    setUpTime = DateTime.Now;
                }
                else if (MarketData.GetSeries(TimeFrame.Daily).Open.LastValue < MarketData.GetSeries(TimeFrame.Daily).Low.LastValue)
                {
                    buySetUp = true;
                    setUpTime = DateTime.Now;
                }
            }

        }
        protected override void OnBar()
        {
            Print(buySetUp + "" + sellSetUp);

            if ((this.History.FindLast("buyGap") == null || (DateTime.Now - this.History.FindLast("buyGap").EntryTime).Days >= 1) && buySetUp && MarketSeries.Close.LastValue > MarketData.GetSeries(TimeFrame.Daily).Low.LastValue)
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol, positionSize, "buyGap", SLpips, TPpips);
            }
            else if ((this.History.FindLast("sellGap") == null || (DateTime.Now - this.History.FindLast("sellGap").EntryTime).Days >= 1) && sellSetUp && MarketSeries.Close.LastValue < MarketData.GetSeries(TimeFrame.Daily).High.LastValue)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol, positionSize, "sellGap", SLpips, TPpips);
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
