using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ProfitWave : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public int pipsProfit { get; set; }
        [Parameter(DefaultValue = 0.0)]
        public int pipsSL { get; set; }
        public double[] prevUps = new double[3];
        public double[] prevDowns = new double[3];
        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnBar()
        {

            changePrev();

            if (Positions.Count == 0)
            {

                if (prevUps[0] > prevUps[1] && prevUps[1] > prevUps[2])
                {

                    ExecuteMarketOrder(TradeType.Sell, Symbol, 1000, "Sell", pipsSL, pipsProfit, 3);

                }
                else if (prevDowns[0] > prevDowns[1] && prevDowns[1] > prevDowns[2])
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, 1000, "Buy", pipsSL, pipsProfit, 3);
                }
                return;
            }

        }
        /*
            if (Positions.Count > 0)
            {
                if (Positions.Find("Sell") == null)
                {
                    if (prevUps[0] > prevUps[1] && prevUps[1] > prevUps[2])
                    {
                        ClosePosition(Positions.Find("Buy"));
                        ExecuteMarketOrder(TradeType.Sell, Symbol, 1000, "Sell", null, null, 3);
                    }
                }
                else
                {
                    if (prevDowns[0] > prevDowns[1] && prevDowns[1] > prevDowns[2])
                    {
                        ClosePosition(Positions.Find("Sell"));
                        ExecuteMarketOrder(TradeType.Sell, Symbol, 1000, "Buy", null, null, 3);
                    }
                }
            }*/

        public void changePrev()
        {
            prevUps = new double[3];
            prevDowns = new double[3];
            int previ = 0;
            int datai = 0;
            while (prevUps[2] == 0 && previ < 3)
            {
                if (MarketSeries.Close.Last(datai) > MarketSeries.Open.Last(datai))
                {
                    prevUps[previ] = MarketSeries.High.Last(datai) - MarketSeries.Close.Last(datai);

                    previ++;
                }
                datai++;
            }

            previ = 0;
            datai = 0;
            while (prevDowns[2] == 0 && previ < 3)
            {
                if (MarketSeries.Close.Last(datai) < MarketSeries.Open.Last(datai))
                {
                    prevDowns[previ] = MarketSeries.Close.Last(datai) - MarketSeries.Low.Last(datai);
                    //Print(MarketSeries.OpenTime.Last(datai));
                    previ++;
                }
                datai++;
            }


        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
