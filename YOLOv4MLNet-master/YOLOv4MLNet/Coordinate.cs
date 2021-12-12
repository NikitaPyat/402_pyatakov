using System;
using System.Collections.Generic;
using System.Text;

namespace YOLOv4MLNet
{
    public class Coordinate
    {
        double x1;
        double y1;
        double x2;
        double y2;
        public Coordinate(double x_1, double y_1, double x_2, double y_2)
        {
            x1 = x_1;
            y1 = y_1;
            x2 = x_2;
            y2 = y_2;
        }

        public double getX1()
        {
            return x1;
        }

        public double getY1()
        {
            return y1;
        }

        public double getX2()
        {
            return x2;
        }

        public double getY2()
        {
            return y2;
        }

        public double getX2minusX1()
        {
            return x2 - x1;
        }

        public double getY2minusY1()
        {
            return y2 - y1;
        }
    }
}
