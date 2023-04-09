using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace AvaloniaApplication1.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            double[] dataY = new double[] { 1, 4, 9, 16, 25 };

            Timer timer = new Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 200;
            timer.Start();

            //OHLC[] prices = DataGen.RandomStockPrices(null, 60, TimeSpan.FromDays(1));

            
            _avaPlot.Plot.XAxis.DateTimeFormat(true);

            _avaPlot.Plot.SetAxisLimits(DateTime.Now, DateTime.Now.AddMinutes(30), 900, 1100);
            //_avaPlot.SaveFig("finance_dateTimeAxis.png");

            //_plotList = _avaPlot.Plot.AddScatterList<OHLC>();
        }

        

        public AvaPlot AvaPlot => _avaPlot;

        AvaPlot _avaPlot = new AvaPlot();

        //ScatterPlotList<OHLC> _plotList;

        double _lastPrice = 1000;

        Random _rnd = new Random();

        DateTime _dateTime = DateTime.Now;

        TimeSpan _tf = TimeSpan.FromMinutes(1);

        object _o = new object();

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            OHLC candle = GetCandle();

            try
            {
                lock (_o)
                {
                    _avaPlot.Plot.AddCandlesticks(new OHLC[1]
                            {
                                candle
                            });

                    

                    _avaPlot.Refresh();

                    _lastPrice = candle.Close;
                }
                
            }
            catch (Exception ex) { }
        }

        private OHLC GetCandle()
        {
            double h = _lastPrice + 50 - _rnd.Next(1, 50);
            double c = h - _rnd.Next(1, 50);
            double l = c - _rnd.Next(1, 50);

            if (_lastPrice < l) l = _lastPrice;

            _dateTime = _dateTime.AddMinutes(1);

            return new OHLC(_lastPrice, h, l, c, _dateTime, _tf);
        }

        private void SetAxisLimits()
        {
            AxisLimits axisLimits = _avaPlot.Plot.GetAxisLimits();


            double yMax = axisLimits.YMax;
            double yMin = axisLimits.YMin;
        }
    }
}
