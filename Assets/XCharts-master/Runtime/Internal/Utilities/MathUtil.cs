using System;
using UnityEngine;

namespace XCharts.Runtime
{
    public static class MathUtil
    {
        public static double Abs(double d)
        {
            return d > 0 ? d : -d;
        }

        public static double Clamp(double d, double min, double max)
        {
            if (d >= min && d <= max) return d;
            if (d < min) return min;
            return max;
        }

        public static bool Approximately(double a, double b)
        {
            return Math.Abs(b - a) < Math.Max(0.000001f * Math.Max(Math.Abs(a), Math.Abs(b)), Mathf.Epsilon * 8);
        }

        public static double Clamp01(double value)
        {
            if (value < 0F)
                return 0F;
            if (value > 1F)
                return 1F;
            return value;
        }

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * Clamp01(t);
        }

        public static bool IsInteger(double value)
        {
            if (value == 0) return true;
            if (value >= -1 && value <= 1) return false;
            return Math.Abs(value % 1) <= double.Epsilon * 100;
        }

        public static int GetPrecision(double value)
        {
            if (IsInteger(value)) return 0;
            var count = 1;
            var intvalue = value * Mathf.Pow(10, count);
            while (!IsInteger(intvalue) && count < 38)
            {
                count++;
                intvalue = value * Mathf.Pow(10, count);
            }

            return count;
        }
    }
}