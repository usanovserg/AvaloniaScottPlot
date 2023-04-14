using Avalonia.Threading;
using AvaloniaApplication1.Entity;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottable;
using ScottPlot.Ticks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Timers;

namespace AvaloniaApplication1.ViewModels
{
    public class VM : ViewModelBase
    {
        public VM()
        {
            _count++;

            Debug.WriteLine("MainWindow = " + _count);

            Init();

            Timer timer = new Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 200;
            timer.Start();


        }

        static int _count = 0;

        #region Properties ==================================================================================================

        public AvaPlot AvaPlot => _avaPlot;

        public double MaximumScroll
        {
            get => _maximumScroll;

            set
            {
                _maximumScroll = value;
                OnPropertyChanged(nameof(MaximumScroll));
            }
        }
        double _maximumScroll = 0;

        public double ValueScroll
        {
            get => _valueScroll;

            set
            {
                _valueScroll = value;
                OnPropertyChanged(nameof(ValueScroll));

                if (ValueScroll == MaximumScroll)
                {
                    Auto = true;
                }
                else
                {
                    Auto = false;
                }
            }
        }
        double _valueScroll = 0;

        public double SizeScroll
        {
            get => _sizeScroll;

            set
            {
                _sizeScroll = value;
                OnPropertyChanged(nameof(SizeScroll));
            }
        }
        double _sizeScroll = 0;

        public bool Run
        {
            get => _run;

            set
            {
                _run = value;
                OnPropertyChanged(nameof(Run));
            }
        }
        bool _run = true;

        public bool Auto
        {
            get => _auto;

            set
            {
                if (IsPressedScrollBar)
                {
                    _auto = value;
                }
                
                OnPropertyChanged(nameof(Auto));
            }
        }
        bool _auto = true;


        #endregion

        #region Fields ======================================================================================================

        AvaPlot _avaPlot;

        //ScatterPlotList<OHLC> _plotList;

        double _lastPrice = 1000;

        Random _rnd = new Random();

        DateTime _dateTime = DateTime.Now;

        TimeSpan _tf = TimeSpan.FromMinutes(1);

        object _o = new object();

        double _yMax = 0;
        double _yMin = double.MaxValue;

        double _xMax = 0;
        double _xMin = double.MaxValue;

        List<Candle> _candles = new List<Candle>();

        string[] _dateTimes = new string[0];

        double[] _ints = new double[0];

        DateTime _lastScrollTime = DateTime.Now;

        public bool IsPressedScrollBar = false;

        #endregion

        #region Methods ======================================================================================================


        private void Init()
        {
            try
            {
                _avaPlot = new AvaPlot();

                _avaPlot.PointerWheelChanged += _avaPlot_PointerWheelChanged;

                _avaPlot.Configuration.Pan = false;
            }
            catch (Exception ex)
            {

            }
        }

        private void _avaPlot_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            _lastScrollTime = DateTime.Now;
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (!Run) return;
            
            OHLC candle = GetCandle();

            _candles.Add(new Candle(candle, _candles.Count));

            try
            {
                lock (_o)
                {
                    _avaPlot.Plot.RenderLock();

                    FinancePlot plt = _avaPlot.Plot.AddCandlesticks(new OHLC[1]
                    {
                        candle
                    });

                    _avaPlot.Plot.XAxis.ManualTickPositions(_ints, _dateTimes);

                    SetAxisLimits(candle);                    

                    _avaPlot.Plot.RenderUnlock();

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

            Array.Resize(ref _dateTimes, _dateTimes.Length + 1);
            Array.Resize(ref _ints, _ints.Length + 1);

            _dateTimes[_dateTimes.Length - 1] = _dateTime.ToShortTimeString();
            _ints[_ints.Length - 1] = _ints.Length;

            return new OHLC(_lastPrice, h, l, c, _dateTimes.Length, 1);
        }

        private void SetAxisLimits(OHLC candle)
        {
            if (_lastScrollTime.AddSeconds(2) >= DateTime.Now
                || IsPressedScrollBar) return; 

            MaximumScroll = _candles.Count;

            AxisLimits axisLimits = _avaPlot.Plot.GetAxisLimits();

            if (candle == null) return;

            double offsetY = 0;            

            _xMax = axisLimits.XMax;
            SizeScroll = axisLimits.XMax - axisLimits.XMin;

            if (_xMax < _ints.Length) _xMax = _ints.Length;

            GetMaxMinY();

            if (Auto)
            {
                ValueScroll = MaximumScroll;
                
                _avaPlot.Plot.SetAxisLimits(_xMax - SizeScroll, _xMax, _yMin, _yMax);
            }            
        }

        private void GetMaxMinY()
        {
            int size = (int)SizeScroll;
            int xMax = (int)_xMax;
            int xMin = xMax - size;

            double yMax = _yMax; // double.MinValue;
            double yMin = _yMin; //  double.MaxValue;

            for (int i= xMin; i< xMax; i++)
            {
                if (_candles[i].OHLC.High > yMax) yMax = _candles[i].OHLC.High;
                if (_candles[i].OHLC.Low < yMin) yMin = _candles[i].OHLC.Low;
            }

            _yMax = yMax;
            _yMin = yMin;
        }

        #endregion
    }
}
