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

        public static bool IsWithin<T>(this T InValue, T InMin, T InMax) where T : IComparable<T>
        {
            if (InValue.CompareTo(InMin) < 0)
                return false;
            if (InValue.CompareTo(InMax) > 0)
                return false;

            return true;
        }

        public static bool LargerThanPercent(this double InValue, double InCompareTo, double InPercent)
        {
            return InValue > InCompareTo - InCompareTo * InPercent;
        }
    }
}
