using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiaSharpTestApp
{
    public static class ColorTools
    {
        /// <summary>
        /// Shades color (make lighter or darker).
        /// </summary>
        /// <param name="color"></param>
        /// <param name="percent">any value from -1.0 to 1.0</param>
        /// <returns>Shaded color</returns>
        static public SKColor ShadeRGBColor(SKColor color, float percent)
        {
            // https://github.com/PimpTrizkit/PJs/wiki/12.-Shade,-Blend-and-Convert-a-Web-Color-(pSBC.js)#stackoverflow-archive-begin

            byte t = (byte)(percent < 0 ? 0 : 255);
            var p = percent < 0 ? percent * -1 : percent;
            var R = color.Red;
            var G = color.Green;
            var B = color.Blue;

            var rr = (byte)(Math.Round((t - R) * p) + R);
            var gg = (byte)(Math.Round((t - G) * p) + G);
            var bb = (byte)(Math.Round((t - B) * p) + B);

            return new SKColor(rr, gg, bb);
        }
    }
}
