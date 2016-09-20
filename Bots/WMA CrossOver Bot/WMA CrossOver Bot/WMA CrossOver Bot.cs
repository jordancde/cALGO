using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class WMACrossOverBot : Robot
    {
        [Parameter(DefaultValue = 0)]
        public int longWMANum { get; set; }

        [Parameter(DefaultValue = 0)]
        public int shortWMANum { get; set; }

        [Parameter(DefaultValue = 0)]
        public int stopLossPips { get; set; }

        [Parameter(DefaultValue = 0)]
        public int initialTPPips { get; set; }

        [Parameter(DefaultValue = 0)]
        public int TSShiftPips { get; set; }

        [Parameter(DefaultValue = 0)]
        public int TSTrailPips { get; set; }

        public WeightedMovingAverage shortWMA;
        public WeightedMovingAverage longWMA;

        protected override void OnStart()
        {
            // Put your initialization logic here
            shortWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, shortWMANum);
            longWMA = Indicators.WeightedMovingAverage(MarketSeries.Close, longWMANum);
        }

        protected override void OnTick()
        {





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
