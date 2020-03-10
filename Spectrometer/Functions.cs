using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrometer
{
    class Functions
    {
        public static double Interp(double x, DataPoint p1, DataPoint p2)
        {
            return (p2.Y - p1.Y) / (p2.X - p1.X) * (x - p1.X) + p1.Y; 
        }

        public static double Interp(double x, double x1, double y1, double x2, double y2)
        {
            return (y2 - y1) / (x2 - x1) * (x - x1) + y1;
        }

        public static double Interp(double x, List<DataPoint> list)
        {
            if (x <= list[0].X)
                return list[0].Y;
            if (x >= list.Last().X)
                return list[0].Y;

            int i1 = 0;
            int i2 = list.Count-1;
            double x1 = list[i1].X;
            double x2 = list[i2].X;

            int i = Convert.ToInt32(Interp(x, x1, i1, x2, i2));

            while(true)
            {
                i = Convert.ToInt32(Interp(x, x1, i1, x2, i2));

                if(x >= list[i].X && x <= list[i+1].X)
                {
                    i1 = i;
                    i2 = i + 1;
                    x1 = list[i1].X;
                    x2 = list[i2].X;
                    break; //значит мы нашли i1, i2, x1, x2
                }
                else
                {
                    if(x < list[i].X)
                    {
                        i2 = i;
                        x2 = list[i2].X;
                    }
                    if(x > list[i+1].X)
                    {
                        i1 = i + 1;
                        x1 = list[i1].X;
                    }
                }
            }

            return Interp(x, list[i1], list[i2]);
        }
    }
}
