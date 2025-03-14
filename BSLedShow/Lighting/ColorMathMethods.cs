using System;
using System.Linq;

namespace BSLedShow.Lighting
{
    public static class ColorMathMethods
    {
        public static void MultiplyARGBComponentsByAlpha(float r, float g, float b, float a, ref float[] argbComponents)
        {
            argbComponents[0] = (float)Math.Pow(r * a, 2);
            argbComponents[1] = (float)Math.Pow(g * a, 2);
            argbComponents[2] = (float)Math.Pow(b * a, 2);
            float maxComponentValue = 0;
            if (1 > argbComponents[0] && 1 > argbComponents[1] && 1 > argbComponents[2])
            {
                maxComponentValue = 1;
            }
            else if (argbComponents[0] > argbComponents[1] && argbComponents[0] > argbComponents[2])
            {
                maxComponentValue = argbComponents[0];
            }
            else if (argbComponents[1] > argbComponents[0] && argbComponents[1] > argbComponents[2])
            {
                maxComponentValue = argbComponents[1];
            }
            else
            {
                maxComponentValue = argbComponents[2];
            }
            argbComponents[0] = argbComponents[0] / maxComponentValue;
            argbComponents[1] = argbComponents[1] / maxComponentValue;
            argbComponents[2] = argbComponents[2] / maxComponentValue;
            argbComponents[3] = 1f;
        }

        public static float[] MultiplyARGBComponentsByAlpha(float[] argbColor)
        {
            double[] argbComponents = new double[3] { Math.Pow(argbColor[0] * argbColor[3], 2), Math.Pow(argbColor[1] * argbColor[3], 2), Math.Pow(argbColor[2] * argbColor[3], 2) };
            double maxComponentValue = Math.Max(argbComponents.Max(), 1d);
            for (int i = 0; i < argbComponents.Length; i++)
            {
                argbComponents[i] = (argbComponents[i] / maxComponentValue);
            }
            return new float[] { (float)argbComponents[0], (float)argbComponents[1], (float)argbComponents[2], 1f };
        }

        public static byte[] GetRGBFromFloatRGBComponents(float[] rgbColor)
        {
            double[] argbComponents = new double[3] { Math.Pow(rgbColor[0], 2), Math.Pow(rgbColor[1], 2), Math.Pow(rgbColor[2], 2) };
            double maxComponentValue = Math.Max(argbComponents.Max(), 1d);
            byte[] rgb = new byte[3];
            for (int i = 0; i < argbComponents.Length; i++)
            {
                rgb[i] = (byte)(argbComponents[i] / maxComponentValue * 255);
            }
            return rgb;
        }
        
        public static void LerpARGBColor(float[] a, float[] b, float t, ref float[] result)
        {
            result[0] = a[0] + (b[0] - a[0]) * t;
            result[1] = a[1] + (b[1] - a[1]) * t;
            result[2] = a[2] + (b[2] - a[2]) * t;
            result[3] = a[3] + (b[3] - a[3]) * t;
        }

        public static void LerpARGBColor(float r1, float g1, float b1, float a1, float r2, float g2, float b2, float a2, float t, ref float[] result)
        {
            result[0] = r1 + (r2 - r1) * t;
            result[1] = g1 + (g2 - g1) * t;
            result[2] = b1 + (b2 - b1) * t;
            result[3] = a1 + (a2 - a1) * t;
        }


        public static void GetRGBFromFloatARGBComponents(float r, float g, float b, float a, ref byte[] color)
        {
            r = r * a;
            g = g * a;
            b = b * a;
            float maxComponentValue = 0;
            if (1 > r && 1 > g && 1 > b)
            {
                maxComponentValue = 1;
            }
            else if (r > g && r > b)
            {
                maxComponentValue = r;
            }
            else if (g > r && g > b)
            {
                maxComponentValue = g;
            }
            else
            {
                maxComponentValue = b;
            }
            color[0] = (byte)(r / maxComponentValue * 255);
            color[1] = (byte)(g / maxComponentValue * 255);
            color[2] = (byte)(b / maxComponentValue * 255);
        }
    }
}