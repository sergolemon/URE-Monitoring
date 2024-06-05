using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI;

namespace URE.Extensions
{
    internal static class ColorExtensions
    {
        public static int ToInt(this Color color)
        {
            return (color.R << 0) | (color.G << 8) | (color.B << 16);
        }

        public static Color FromInt(int colorInt)
        {
            if (colorInt == 0) return Colors.Lime;

            var red = (byte)((colorInt >> 0) & 255);
            var green = (byte)((colorInt >> 8) & 255);
            var blue = (byte)((colorInt >> 16) & 255);

            return new Color() { A = 255, R = red, G = green, B = blue };
        }
    }
}
