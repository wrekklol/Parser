using System;
using System.Threading;

using static Parser.StaticLibrary.NativeMethods;
using static Parser.StaticLibrary.MathLibrary;

namespace Parser.StaticLibrary
{
    public static class MouseHelper
    {
        public static readonly int mouseSpeed = 15;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">X coord to move mouse to</param>
        /// <param name="y">Y coord to move mouse to</param>
        /// <param name="rx">Random variation of the end  of the X coord</param>
        /// <param name="ry">Random variation of the end  of the Y coord</param>
        public static void MoveMouse(int x, int y, int rx, int ry, Action OnMoveComplete = null)
        {
            GetCursorPos(out System.Drawing.Point c);

            Random r = new Random();

            x += r.Next(-rx, rx);
            y += r.Next(-rx, ry);

            double randomSpeed = Math.Max((r.Next(mouseSpeed) / 2.0 + mouseSpeed) / 10.0, 0.1);

            WindMouse(c.X, c.Y, x, y, 9.0, 3.0, 10.0 / randomSpeed, 15.0 / randomSpeed, 10.0 * randomSpeed, 10.0 * randomSpeed, OnMoveComplete);
        }

        public static void WindMouse(double xs, double ys, double xe, double ye,
            double gravity, double wind, double minWait, double maxWait,
            double maxStep, double targetArea, Action OnMoveComplete = null)
        {
            //Console.WriteLine(FormattableString.Invariant($"Start: {xs}, {ys}\nEnd: {xe}, {ye}\nGravity: {gravity}\nWind: {wind}\nWait: {minWait}, {maxWait}\nMaxStep: {maxStep}\nTargetArea: {targetArea}\n--------------------------------------------------------------------------"));

            Random r = new Random();

            double dist, windX = 0, windY = 0, veloX = 0, veloY = 0, randomDist, veloMag, step;
            int oldX, oldY, newX = (int)Math.Round(xs), newY = (int)Math.Round(ys);

            double waitDiff = maxWait - minWait;
            double sqrt2 = Math.Sqrt(2.0);
            double sqrt3 = Math.Sqrt(3.0);
            double sqrt5 = Math.Sqrt(5.0);

            dist = Hypot(xe - xs, ye - ys);

            while (dist > 1.0)
            {
                wind = Math.Min(wind, dist);

                if (dist >= targetArea)
                {
                    int w = r.Next((int)Math.Round(wind) * 2 + 1);
                    windX = windX / sqrt3 + (w - wind) / sqrt5;
                    windY = windY / sqrt3 + (w - wind) / sqrt5;
                }
                else
                {
                    windX = windX / sqrt2;
                    windY = windY / sqrt2;
                    if (maxStep < 3)
                        maxStep = r.Next(3) + 3.0;
                    else
                        maxStep = maxStep / sqrt5;
                }

                veloX += windX;
                veloY += windY;
                veloX = veloX + gravity * (xe - xs) / dist;
                veloY = veloY + gravity * (ye - ys) / dist;

                if (Hypot(veloX, veloY) > maxStep)
                {
                    randomDist = maxStep / 2.0 + r.Next((int)Math.Round(maxStep) / 2);
                    veloMag = Hypot(veloX, veloY);
                    veloX = (veloX / veloMag) * randomDist;
                    veloY = (veloY / veloMag) * randomDist;
                }

                oldX = (int)Math.Round(xs);
                oldY = (int)Math.Round(ys);
                xs += veloX;
                ys += veloY;
                dist = Hypot(xe - xs, ye - ys);
                newX = (int)Math.Round(xs);
                newY = (int)Math.Round(ys);

                if (oldX != newX || oldY != newY)
                    SetCursorPos(newX, newY);

                step = Hypot(xs - oldX, ys - oldY);
                int wait = (int)Math.Round(waitDiff * (step / maxStep) + minWait);
                Thread.Sleep(wait);
            }

            int endX = (int)Math.Round(xe);
            int endY = (int)Math.Round(ye);
            if (endX != newX || endY != newY)
                SetCursorPos(endX, endY);

            OnMoveComplete?.Invoke();
        }
    }
}