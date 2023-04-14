using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AvaloniaApplication1.Entity
{
    internal class LineDateTime
    {
        internal LineDateTime()
        {
            Init();
        }

        #region Fields ===========================================================================================

        Dictionary<int, string[]> _lines= new Dictionary<int, string[]>();

        /// <summary>
        /// Номера свечей
        /// </summary>
        public double[] Ints = new double[0];


        #endregion

        #region Methods =======================================================================================

        public string[]? GetLineDateTimes(int number)
        {
            string[]? lines;

            _lines.TryGetValue(number, out lines);

            return lines;
        }

        public int AddCandleTime(DateTime candleTime)
        {
            string time = candleTime.ToShortTimeString();
            
            Array.Resize(ref Ints, Ints.Length + 1);

            foreach (var lineDt in _lines)
            {
                string[] strs = lineDt.Value;

                Array.Resize(ref strs, strs.Length + 1);

                _lines[lineDt.Key] = strs;
            }

            int count = Ints.Length;

            Ints[Ints.Length - 1] = count;

            string[] line;

            foreach (var val in _lines)
            {
                if (_lines.TryGetValue(val.Key, out line))
                {
                    if (val.Key == 1)
                    {
                        line[line.Length - 1] = time;

                        Debug.WriteLine(line.Length + " = " + time);
                    }
                    else if (val.Key == 60
                            && candleTime.Minute == 0)
                    {
                        line[line.Length - 1] = time;
                    }
                    else
                    {
                        if (candleTime.Minute % val.Key == 0)
                        {
                            line[line.Length - 1] = time;
                        }
                        else
                        {
                            line[line.Length - 1] = string.Empty;
                        }
                    }                                       
                }
            }
            

            //if (count % 5 == 0) _dateTimes5[_dateTimes5.Length - 1] = time;
            //else _dateTimes5[_dateTimes5.Length - 1] = string.Empty;

            //if (count % 10 == 0) _dateTimes10[_dateTimes10.Length - 1] = time;
            //else _dateTimes10[_dateTimes10.Length - 1] = string.Empty;

            //if (count % 50 == 0) _dateTimes50[_dateTimes50.Length - 1] = time;
            //else _dateTimes50[_dateTimes50.Length - 1] = string.Empty;

            //if (count % 100 == 0) _dateTimes100[_dateTimes100.Length - 1] = time;
            //else _dateTimes100[_dateTimes100.Length - 1] = string.Empty;


            return count;
        }

        private void Init()
        {
            _lines.Add(1, new string[0]);
            _lines.Add(5, new string[0]);
            _lines.Add(10, new string[0]);
            _lines.Add(30, new string[0]);
            _lines.Add(60, new string[0]);
        }

        #endregion
    }
}
