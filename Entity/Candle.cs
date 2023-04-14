using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Entity
{
    public class Candle
    {
        public Candle(OHLC ohlc, int index) 
        {
            OHLC = ohlc;
            Index = index;
        }

        public OHLC OHLC;

        public int Index;
    }
}
