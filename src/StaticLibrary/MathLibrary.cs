using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.StaticLibrary
{
    public static class MathLibrary
    {
        public static double Hypot(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
