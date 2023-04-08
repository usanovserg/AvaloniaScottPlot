using ScottPlot;
using ScottPlot.Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaApplication1.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            double[] dataY = new double[] { 1, 4, 9, 16, 25 };
            _avaPlot = new ScottPlot.Plot(600, 400);
            OHLC[] prices = DataGen.RandomStockPrices(null, 60, TimeSpan.FromDays(1));

            _avaPlot.AddCandlesticks(prices);
            _avaPlot.XAxis.DateTimeFormat(true);
            _avaPlot.SaveFig("finance_dateTimeAxis.png");
        }

        public ScottPlot.Plot AvaPlot
        {
            get => _avaPlot;

            set
            {
                _avaPlot = value;
            }
        }
        ScottPlot.Plot _avaPlot;
    }
}
