using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class CandlePatterns : Indicator
    {
        [Parameter(DefaultValue = 0)]
        public int PreviousBars { get; set; }

        [Parameter(DefaultValue = true)]
        public bool SpinningTop { get; set; }

        [Parameter(DefaultValue = true)]
        public bool Marubozu { get; set; }

        [Parameter(DefaultValue = true)]
        public bool Doji { get; set; }

        [Parameter(DefaultValue = true)]
        public bool Hammer { get; set; }

        [Parameter(DefaultValue = true)]
        public bool InvertedHammer { get; set; }

        [Parameter(DefaultValue = true)]
        public bool HangingMan { get; set; }

        [Parameter(DefaultValue = true)]
        public bool ShootingStar { get; set; }

        [Parameter(DefaultValue = true)]
        public bool Engulfing { get; set; }

        [Parameter(DefaultValue = true)]
        public bool Tweezers { get; set; }

        [Parameter(DefaultValue = true)]
        public bool Stars { get; set; }

        [Parameter(DefaultValue = true)]
        public bool Threes { get; set; }

        [Parameter(DefaultValue = true)]
        public bool ThreeInside { get; set; }



        protected override void Initialize()
        {
            // Initialize and create nested indicators
        }

        public override void Calculate(int index)
        {
            for (int i = 0; i < PreviousBars; i++)
            {
                var open = MarketSeries.Open.Last(i);
                var close = MarketSeries.Close.Last(i);
                var high = MarketSeries.High.Last(i);
                var low = MarketSeries.Low.Last(i);

                var open2 = MarketSeries.Open.Last(i + 1);
                var close2 = MarketSeries.Close.Last(i + 1);
                var high2 = MarketSeries.High.Last(i + 1);
                var low2 = MarketSeries.Low.Last(i + 1);

                var open3 = MarketSeries.Open.Last(i + 2);
                var close3 = MarketSeries.Close.Last(i + 2);
                var high3 = MarketSeries.High.Last(i + 2);
                var low3 = MarketSeries.Low.Last(i + 2);


                var color = Colors.White;
                var thickness = 1;
                var drawBox = false;
                var rightOffset = 0;
                var leftOffset = 0;

                if (close > open)
                {
                    //Spinning Top
                    if (SpinningTop && (close - open) / (high - low) <= 0.17)
                    {
                        if (Math.Abs((high - close) - (open - low)) / high - low <= 0.2)
                        {
                            drawBox = true;
                        }
                    }

                    //White Marubozu
                    if (Marubozu && high == close && low == open)
                    {
                        drawBox = true;
                        color = Colors.LimeGreen;
                        thickness = 2;
                    }

                    //White Candles + Doji
                    if (Doji && (close3 - open3) / (high3 - low3) >= 0.7 && (low2 - open2) / (high2 - low2) >= 0.7)
                    {
                        if ((close - open) / (high - low) <= 0.3)
                        {
                            drawBox = true;
                            color = Colors.Tomato;
                            thickness = 2;
                            leftOffset = 2;
                        }
                    }

                    //Hammer
                    if (Hammer && (close - open) / (high - low) < 0.33 && (high - close) / (high - low) < 0.2)
                    {
                        drawBox = true;
                        color = Colors.LimeGreen;
                        thickness = 2;
                    }

                    //Inverted Hammer
                    if (InvertedHammer && (close - open) / (high - low) < 0.33 && (open - low) / (high - low) < 0.2)
                    {
                        drawBox = true;
                        color = Colors.LimeGreen;
                        thickness = 2;

                    }

                    //Bullish Englufing
                    if (Engulfing && open2 > close2 && (close - open) > (high2 - low))
                    {
                        drawBox = true;
                        color = Colors.LimeGreen;
                        thickness = 2;
                    }

                    //Tweezer Bottoms
                    if (Tweezers && (high - close) / (high - low) <= 0.2 && (high2 - open2) / (high2 - low2) <= 0.2 && close3 < open3 && (open - low) / (high - low) > 0.4 && (close2 - low2) / (high2 - low2) > 0.4)
                    {
                        drawBox = true;
                        color = Colors.LimeGreen;
                        thickness = 2;
                        leftOffset = 1;
                    }

                    //Morning Star
                    if (Stars && open3 > close3 && Math.Abs(close2 - open2) / (high2 - low2) <= 0.15 && close > (open3 + close3) / 2)
                    {
                        drawBox = true;
                        color = Colors.LimeGreen;
                        thickness = 2;
                        leftOffset = 2;
                    }

                    //Three White Soldiers
                    if (Threes && open3 < close3 && open2 < close2 && (high2 - close2) / (high2 - low2) <= 0.3 && close - open >= close2 - open2 && (close - open) / (high - low) > 0.7)
                    {
                        drawBox = true;
                        color = Colors.LimeGreen;
                        thickness = 2;
                        leftOffset = 2;
                    }

                    //Three Inside Up
                    if (ThreeInside && close3 < open3 && close2 > open2 && open3 - close3 > close2 - open2 && close2 > (open3 + close3) / 2 && close > high2)
                    {
                        drawBox = true;
                        color = Colors.LimeGreen;
                        thickness = 2;
                        leftOffset = 2;
                    }

                }
                else if (close < open)
                {
                    //Spinning Top
                    if (SpinningTop && (open - close) / (high - low) <= 0.17)
                    {
                        if (Math.Abs((high - open) - (close - low)) / (high - low) <= 0.2)
                        {
                            drawBox = true;
                        }
                    }

                    //Black Marubozu
                    if (Marubozu && low == close && high == open)
                    {
                        drawBox = true;
                        color = Colors.Tomato;
                        thickness = 2;
                    }

                    //Black Candles + Doji
                    if (Doji && (open3 - close3) / (high3 - low3) > 0.7 && (open2 - close2) / (high2 - low2) > 0.7)
                    {
                        if ((open - close) / (high - low) < 0.3)
                        {
                            drawBox = true;
                            color = Colors.LimeGreen;
                            thickness = 2;
                            leftOffset = 2;
                        }
                    }

                    //Hanging Man
                    if (HangingMan && (open - close) / (high - low) < 0.33 && (high - open) / (high - low) < 0.2)
                    {
                        drawBox = true;
                        color = Colors.Tomato;
                        thickness = 2;
                    }

                    //Shooting Star
                    if (ShootingStar && (open - close) / (high - low) < 0.33 && (close - low) / (high - low) < 0.2)
                    {
                        drawBox = true;
                        color = Colors.Tomato;
                        thickness = 2;
                    }

                    //Bearish Englufing
                    if (Engulfing && MarketSeries.Open.Last(i + 1) < MarketSeries.Close.Last(i + 1) && (open - close) > (MarketSeries.High.Last(i + 1) - MarketSeries.Low.Last(i + 1)))
                    {
                        drawBox = true;
                        color = Colors.Tomato;
                        thickness = 2;
                    }

                    //Tweezer Tops
                    if (Tweezers && (close - low) / (high - low) <= 0.2 && (high - open) / (high - low) > 0.4)
                    {
                        if ((MarketSeries.Open.Last(i + 1) - MarketSeries.Low.Last(i + 1)) / (MarketSeries.High.Last(i + 1) - MarketSeries.Low.Last(i + 1)) <= 0.3 && (MarketSeries.High.Last(i + 1) - MarketSeries.Close.Last(i + 1)) / (MarketSeries.High.Last(i + 1) - MarketSeries.Low.Last(i + 1)) > 0.4)
                        {
                            drawBox = true;
                            color = Colors.Tomato;
                            thickness = 2;
                            leftOffset = 1;

                        }
                    }

                    //Evening Star
                    if (Stars && open3 < close3 && Math.Abs(close2 - open2) / (high2 - low2) <= 0.15 && close < (open3 + close3) / 2)
                    {
                        drawBox = true;
                        color = Colors.Tomato;
                        thickness = 2;
                        leftOffset = 2;
                    }
                    //Three Black Crows
                    if (Threes && open3 > close3 && open2 > close2 && (close2 - low2) / (high2 - low2) <= 0.3 && open - close >= open2 - close2 && (open - close) / (high - low) > 0.7)
                    {
                        drawBox = true;
                        color = Colors.Tomato;
                        thickness = 2;
                        leftOffset = 2;
                    }

                    //Three Inside Down
                    if (ThreeInside && close3 > open3 && close2 < open2 && close3 - open3 > open2 - close2 && close2 < (open3 + close3) / 2 && close < low2)
                    {
                        drawBox = true;
                        color = Colors.Tomato;
                        thickness = 2;
                        leftOffset = 2;
                    }

                }
                if (drawBox)
                {
                    ChartObjects.DrawLine("boxbottom" + i, index - i - 1 - leftOffset, MarketSeries.Low.Last(i), index - i + 1 + rightOffset, MarketSeries.Low.Last(i), color, thickness);
                    ChartObjects.DrawLine("boxtop" + i, index - i - 1 - leftOffset, MarketSeries.High.Last(i), index - i + 1 + rightOffset, MarketSeries.High.Last(i), color, thickness);
                    ChartObjects.DrawLine("boxleft" + i, index - i - 1 - leftOffset, MarketSeries.Low.Last(i), index - i - 1 - leftOffset, MarketSeries.High.Last(i), color, thickness);
                    ChartObjects.DrawLine("boxright" + i, index - i + 1 + rightOffset, MarketSeries.Low.Last(i), index - i + 1 + rightOffset, MarketSeries.High.Last(i), color, thickness);

                }
            }
        }
    }
}

