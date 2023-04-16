using Avalonia.Threading;
using AvaloniaApplication1.Entity;
using ControllerExChanges.Controller;
using ControllerExChanges.Entity;
using ControllerExChanges.Enums;
using ControllerExChanges.Interfaces;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottable;
using ScottPlot.Ticks;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //timer.Start();

            ILogger log = BuildLogger();
            _logger = log.ForContext<VM>();

            Controller controller = Controller.GetController(log);

            _connector = controller.CreateConnector(ExchangeType.LiveFutures);

            if (_connector != null)
            {
                Task.Run(() =>
                {
                    ConnectParametrs parametrs = _connector.ConnectParametrs;

                    parametrs.ExchangeType = ExchangeType.RandomConnector;
                    parametrs.Login = "serg-225@mail.ru";
                    parametrs.Password = "=Ss*#]Vt";
                    parametrs.Path = "http://193.161.214.142:8081";


                    _connector.ConnectStatusChangeEvent += _connector_ConnectStatusChangeEvent;

                    _connector.NewTradeEvent += _connector_NewTradeEvent;

                    _connector.SecuritiesChangeEvent += _connector_SecuritiesChangeEvent;
                    State = _connector.Connect().Result;
                });
            }            
        }

        

        static int _count = 0;

        #region Properties ==================================================================================================

        public AvaPlot AvaPlot => _avaPlot;

        public ConnectStatus State
        {
            get => _state;

            set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
        ConnectStatus _state = ConnectStatus.Disconnect;

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

                if (_lastValueScroll + 1 < value
                    || _lastValueScroll - 1 > value)
                {
                    _lastValueScroll = value;
                    
                    if (_candles.Count > 0
                        && IsPressedScrollBar)
                    {
                        Refresh(_candles.Last().OHLC);
                    }
                }
            }
        }
        double _valueScroll = 0;
        double _lastValueScroll = 0;

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

        ILogger _logger;

        IConnector _connector;

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

        LineDateTime _lineDateTime = new LineDateTime();

        

        DateTime _lastScrollTime = DateTime.Now;

        public bool IsPressedScrollBar = false;

        #endregion

        #region Methods ======================================================================================================

        private void _connector_ConnectStatusChangeEvent(ConnectStatus status)
        {
            State = status;
        }

        private void _connector_NewTradeEvent(Trade trade)
        {
            
        }

        private async void _connector_SecuritiesChangeEvent(System.Collections.Concurrent.ConcurrentDictionary<string, Security> securities)
        {
            if (securities == null
                || securities.Count == 0) return;

            Security? security;

            if (securities.TryGetValue("2678455", out security))
            {
                bool res = await _connector.SubscribeToSecurity(security);

                Debug.WriteLine("SubscribeToSecurity " + security.Name + " res = " + res);
            }
        }

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

            Refresh(candle);
        }

        private void Refresh(OHLC candle)
        {
            lock (_o)
            {
                try
                {
                    _avaPlot.Plot.RenderLock();

                    FinancePlot plt = _avaPlot.Plot.AddCandlesticks(new OHLC[1]
                    {
                        candle
                    });

                    SetDateTimes();

                    SetAxisLimits(candle);

                    try
                    {
                        _avaPlot.Refresh();
                    }
                    catch (Exception ex)
                    {

                    }

                    _avaPlot.Plot.RenderUnlock();

                    _lastPrice = candle.Close;
                }
                catch (Exception ex)
                {
                    _avaPlot.Plot.RenderUnlock();
                }
            }            
        }

        private OHLC GetCandle()
        {
            int vola = _rnd.Next(30, 50);
            int vola2 = 1;

            double h = _lastPrice + vola*1.02 - _rnd.Next(vola2, vola);
            double c = h - _rnd.Next(vola2, vola);
            double l = c - _rnd.Next(vola2, vola);

            if (_lastPrice < l) l = _lastPrice;

            _dateTime = _dateTime.AddMinutes(1);            

            int count = _lineDateTime.AddCandleTime(_dateTime);

            return new OHLC(_lastPrice, h, l, c, count, 1);
        }

        private void SetAxisLimits(OHLC candle)
        {
            try
            {
                if (_lastScrollTime.AddSeconds(1) >= DateTime.Now
                ) return;  // || IsPressedScrollBar

                MaximumScroll = _candles.Count;

                AxisLimits axisLimits = _avaPlot.Plot.GetAxisLimits();

                if (candle == null) return;

                double offsetY = 0;

                _xMax = axisLimits.XMax;

                SizeScroll = axisLimits.XMax - axisLimits.XMin;

                if (Auto)
                {
                    if (_xMax < MaximumScroll) _xMax = MaximumScroll;

                    ValueScroll = MaximumScroll;
                }

                GetMaxMinY();

                if (ValueScroll > SizeScroll)
                {
                    double offsetRight = (SizeScroll * 0.05);

                    //_avaPlot.Plot.XAxis.SetOffset(offsetRight);

                    _avaPlot.Plot.SetAxisLimits(ValueScroll - SizeScroll + offsetRight, ValueScroll + offsetRight, _yMin, _yMax);
                }                
            }
            catch (Exception ex)
            {

            }
        }

        private void GetMaxMinY()
        {
            try
            {
                int size = (int)SizeScroll + 1;

                int xMax = (int)ValueScroll;

                if (xMax > _candles.Count) xMax = _candles.Count;

                int xMin = xMax - size;

                if (xMin < 0) xMin = 0;

                double yMax = double.MinValue;
                double yMin = double.MaxValue;

                for (int i = xMin; i < xMax; i++)
                {
                    if (_candles[i].OHLC.High > yMax) yMax = _candles[i].OHLC.High;
                    if (_candles[i].OHLC.Low < yMin) yMin = _candles[i].OHLC.Low;
                }

                double offsetY = (yMax - yMin) / 20;

                _yMax = yMax + offsetY;
                _yMin = yMin - offsetY;
            }
            catch(Exception ex)
            {

            }

            
        }

        private void SetDateTimes()
        {
            if (SizeScroll <= 0) return;
            
            double koef = _avaPlot.Plot.XAxis.Dims.DataSizePx / SizeScroll;

            if (koef > 25) SetDateTimes(1);
            else if (koef > 7 && koef <= 25) SetDateTimes(5);
            else if (koef > 3 && koef <= 7) SetDateTimes(10);
            else if (koef > 2 && koef <= 3) SetDateTimes(30);
            else if (koef > 1 && koef <= 2) SetDateTimes(60);
        }

        private void SetDateTimes(int count)
        {
            _avaPlot.Plot.XAxis.ManualTickPositions(_lineDateTime.Ints, _lineDateTime.GetLineDateTimes(count));
        }

        private ILogger BuildLogger()
        {
            if (!Directory.Exists(@"Log"))
            {
                Directory.CreateDirectory(@"Log");
            }

            ILogger log = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.File(new CompactJsonFormatter(),
                                        @"Log\" + DateTime.Now.ToShortDateString() + "_file.log", LevelAlias.Minimum, 104857600L)
                        .CreateLogger();

            return log;
        }

        #endregion
    }
}
