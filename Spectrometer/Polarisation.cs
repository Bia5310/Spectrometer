using AVT.VmbAPINET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Spectrometer.MainWindow;

namespace Spectrometer
{
    public class Polarisation
    {
        public static string[] polarDataStrings = new string[] { "Полный кадр", "I0", "I135", "I45", "I90", "DOLP", "AOLP" };
        public enum PolarData : int { Idle, I0, I135, I45, I90, DOLP, AOLP };

        /// <summary>Вычисляет степень линейной поляризации</summary>
        /// <param name="frame">Frame</param>
        public static double[] ComputeDOLP(Frame frame)
        {
            int w_source = (int)frame.Width;
            int framesize = (int)(frame.Width * frame.Height);
            double[] dolp = new double[frame.Width / 2 * frame.Height / 2];

            unsafe
            {
                fixed (double* resultDoubleBuffer = dolp)
                fixed (byte* sourceByteBuffer = frame.Buffer)
                {
                    short* source = (short*)sourceByteBuffer;
                    double* result = (double*)resultDoubleBuffer;
                    double I0 = 0;
                    double I135 = 0;
                    double I45 = 0;
                    double I90 = 0;

                    double S0 = 0, S1 = 0, S2 = 0;
                    int j = 0;

                    for (int i = 0; i < framesize; i += (2 + Convert.ToInt32(i % w_source == w_source - 2) * w_source))
                    {
                        I0 = source[i];
                        I135 = source[i + 1];
                        j = i + w_source;
                        I45 = source[j];
                        I90 = source[j + 1];
                        S0 = I0 + I90;
                        S1 = I0 - I90;
                        S2 = I45 - I135;

                        result[0] = Math.Sqrt(S1 * S1 + S2 * S2) / S0;
                        result++;
                    }
                }
            }

            return dolp;
        }

        /// <summary>Вычисляет угол линейной поляризации</summary>
        /// <param name="frame">Frame</param>
        public static double[] ComputeAOLP(Frame frame)
        {
            int w_source = (int)frame.Width;
            int framesize = (int)(frame.Width * frame.Height);
            double[] aolp = new double[frame.Width / 2 * frame.Height / 2];

            unsafe
            {
                fixed (double* resultDoubleBuffer = aolp)
                fixed (byte* sourceByteBuffer = frame.Buffer)
                {
                    short* source = (short*)sourceByteBuffer;
                    double* result = (double*)resultDoubleBuffer;
                    double I0 = 0;
                    double I135 = 0;
                    double I45 = 0;
                    double I90 = 0;

                    double S1 = 0, S2 = 0;
                    int j = 0;

                    for (int i = 0; i < framesize; i += (2 + Convert.ToInt32(i % w_source == w_source - 2) * w_source))
                    {
                        I0 = source[i];
                        I135 = source[i + 1];
                        j = i + w_source;
                        I45 = source[j];
                        I90 = source[j + 1];
                        S1 = I0 - I90;
                        S2 = I45 - I135;

                        result[0] = 0.5d * Math.Atan2(S2, S1);
                        result++;
                    }
                }
            }

            return aolp;
        }

        public static Bitmap DoublesToBitmap(double[] sourceArray, int w, int h)
        {
            Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format32bppRgb);
            BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            double max = sourceArray.Max();
            double min = sourceArray.Min();
            double dif = max - min;

            unsafe
            {
                fixed (double* src = sourceArray)
                {
                    double* source = src;
                    byte* result = (byte*)bd.Scan0;
                    byte v = 0;

                    for (int i = 0; i < sourceArray.Length; i++)
                    {
                        v = Convert.ToByte((source[i] - min) / dif * 255);
                        result[0] = v;
                        result[1] = v;
                        result[2] = v;
                        result += 4;
                    }
                }
            }

            bitmap.UnlockBits(bd);
            bd = null;
            return bitmap;
        }

        public static byte[] ExtractPolarisation(Frame frame, PolarData polarisation, int bits)
        {
            int w = (int)frame.Width / 2;
            int h = (int)frame.Height / 2;

            byte[] resultBuffer = null;

            unsafe
            {
                if (bits == 8)
                    resultBuffer = new byte[w * h];
                if (bits > 8)
                    resultBuffer = new byte[2 * w * h];

                if (bits > 8)
                {
                    fixed (byte* res = resultBuffer, src = frame.Buffer)
                    {
                        if (bits > 8)
                        {
                            short* result = (short*)res;
                            short* source = (short*)src;
                            if (polarisation == PolarData.I135 || polarisation == PolarData.I90)
                                source += 1;
                            if (polarisation == PolarData.I45 || polarisation == PolarData.I90)
                                source += frame.Width;

                            for (int j = 0; j < frame.Height; j += 2)
                            {
                                for (int i = 0; i < frame.Width; i += 2)
                                {
                                    result[0] = source[i];
                                    result++;
                                }
                                source += 2*frame.Width;
                            }
                        }
                        if (bits == 8)
                        {
                            byte* result = res;
                            byte* source = src;
                            if (polarisation == PolarData.I135 || polarisation == PolarData.I90)
                                source += 1;
                            if (polarisation == PolarData.I45 || polarisation == PolarData.I90)
                                source += frame.Width;

                            for (int j = 0; j < frame.Height; j += 2)
                            {
                                for (int i = 0; i < frame.Width; i += 2)
                                {
                                    result[0] = source[i];
                                    result++;
                                }
                                source += 2*frame.Width;
                            }
                        }
                    }
                }
            }

            return resultBuffer;
        }

        /// <summary>Позволяет получить битмап группы пикселей с определенным направлением поляризатора</summary>
        /// <param name="frame">Исходный буффер</param>
        /// <param name="polarisation">Ориентация поляризатора I0, I45, I90, I135</param>
        /// <param name="bits">Глубина цвета</param>
        /// <returns></returns>
        public static Bitmap ExtractPolarisationBitmap(Frame frame, PolarData polarisation, int bits)
        {
            return ExtractPolarisationBitmap(frame.Buffer, (int)frame.Width, (int)frame.Height, polarisation, bits);
        }

        /// <summary>Позволяет получить битмап группы пикселей с определенным направлением поляризатора</summary>
        /// <param name="frameBuffer">Буффер байтов кадра</param>
        /// <param name="frameWidth">Ширина исходного кадра</param>
        /// <param name="frameHeight">Высота исходного кадра</param>
        /// <param name="polarisation">Какую компоненту поляризации выделить</param>
        /// <param name="bits">Сколько бит на пиксель</param>
        /// <returns></returns>
        public static Bitmap ExtractPolarisationBitmap(byte[] frameBuffer, int frameWidth, int frameHeight, PolarData polarisation, int bits)
        {
            int w = frameWidth / 2;
            int h = frameHeight / 2;
            int maxval = Convert.ToInt32(Math.Pow(2, bits)) - 1;

            Bitmap resBitmap = new Bitmap(w, h, PixelFormat.Format32bppRgb);
            BitmapData bd = resBitmap.LockBits(new Rectangle(0, 0, resBitmap.Width, resBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            unsafe
            {
                byte b = 0;
                fixed (byte* src = frameBuffer)
                {
                    byte* result = (byte*)bd.Scan0;
                    if (bits > 8)
                    {
                        short* source = (short*)src;
                        if (polarisation == PolarData.I135 || polarisation == PolarData.I90)
                            source += 1;
                        if (polarisation == PolarData.I45 || polarisation == PolarData.I90)
                            source += frameWidth;

                        for (int j = 0; j < frameHeight; j += 2)
                        {
                            for (int i = 0; i < frameWidth; i += 2)
                            {
                                b = (byte)(source[i] * 255 / maxval);
                                result[0] = result[1] = result[2] = b;
                                result += 4;
                            }
                            source += 2 * frameWidth;
                        }
                    }
                    if (bits == 8)
                    {
                        byte* source = src;
                        if (polarisation == PolarData.I135 || polarisation == PolarData.I90)
                            source += 1;
                        if (polarisation == PolarData.I45 || polarisation == PolarData.I90)
                            source += frameWidth;

                        for (int j = 0; j < frameHeight; j += 2)
                        {
                            for (int i = 0; i < frameWidth; i += 2)
                            {
                                result[0] = result[1] = result[2] = source[i];
                                result += 4;
                            }
                            source += 2 * frameWidth;
                        }
                    }
                }
            }
            resBitmap.UnlockBits(bd);
            return resBitmap;
        }

        /// <summary>Вычисляет степень линейной поляризации</summary>
        /// <param name="frame">Кадр в виде массива байтов</param>
        /// <param name="colormap">Цветовая палитра</param>
        /// <returns></returns>
        public static Bitmap ComputeDOLPBitmap(Frame frame, int[] colormap)
        {
            return ComputeDOLPBitmap(frame.Buffer, (int)frame.Width, (int)frame.Height, colormap);
        }

        /// <summary>Вычисляет степень линейной поляризации</summary>
        /// <param name="frame">Frame</param>
        /// <param name="colormap">Цветовая палитра</param>
        public static Bitmap ComputeDOLPBitmap(byte[] frameBuffer, int frameWidth, int frameHeight, int[] colormap)
        {
            int w = frameWidth / 2;
            int h = frameHeight / 2;

            Bitmap resBitmap = new Bitmap(w, h, PixelFormat.Format32bppRgb);
            BitmapData bd = resBitmap.LockBits(new Rectangle(0, 0, resBitmap.Width, resBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            unsafe
            {
                fixed (byte* sourceByteBuffer = frameBuffer)
                {
                    short* source = (short*)sourceByteBuffer;
                    int* result = (int*)bd.Scan0;
                    double I0 = 0;
                    double I135 = 0;
                    double I45 = 0;
                    double I90 = 0;

                    double S0 = 0, S1 = 0, S2 = 0;

                    for (int j = 0; j < frameHeight; j += 2)
                    {
                        for (int i = 0; i < frameWidth; i += 2)
                        {
                            I0 = source[i];
                            I135 = source[i + 1];
                            I45 = source[i + frameWidth];
                            I90 = source[i + frameWidth + 1];

                            S0 = I0 + I90;
                            S1 = I0 - I90;
                            S2 = I45 - I135;

                            result[0] = colormap[(byte)(Math.Sqrt(S1 * S1 + S2 * S2) / S0 * 250)];
                            result++;
                        }
                        source += 2 * frameWidth;
                    }
                }
            }

            resBitmap.UnlockBits(bd);
            return resBitmap;
        }

        /// <summary></summary>
        /// <param name="frame"></param>
        /// <param name="colormap"></param>
        /// <returns></returns>
        public static Bitmap ComputeAOLPBitmap(Frame frame, int[] colormap)
        {
            return ComputeAOLPBitmap(frame.Buffer, (int)frame.Width, (int)frame.Height, colormap);
        }

        /// <summary>Вычисляет азимут линейной поляризации</summary>
        /// <param name="frame">Frame</param>
        /// <param name="colormap">Цветовая палитра</param>
        public static Bitmap ComputeAOLPBitmap(byte[] frameBuffer, int frameWidth, int frameHeight, int[] colormap)
        {
            int w = frameWidth / 2;
            int h = frameHeight / 2;

            Bitmap resBitmap = new Bitmap(w, h, PixelFormat.Format32bppRgb);
            BitmapData bd = resBitmap.LockBits(new Rectangle(0, 0, resBitmap.Width, resBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            unsafe
            {
                fixed (byte* sourceByteBuffer = frameBuffer)
                {
                    short* source = (short*)sourceByteBuffer;
                    int* result = (int*)bd.Scan0;
                    double I0 = 0;
                    double I135 = 0;
                    double I45 = 0;
                    double I90 = 0;

                    double S0 = 0, S1 = 0, S2 = 0;

                    for (int j = 0; j < frameHeight; j += 2)
                    {
                        for (int i = 0; i < frameWidth; i += 2)
                        {
                            I0 = source[i];
                            I135 = source[i + 1];
                            I45 = source[i + frameWidth];
                            I90 = source[i + frameWidth + 1];

                            S1 = I0 - I90;
                            S2 = I45 - I135;

                            result[0] = colormap[(byte)((0.5d * Math.Atan2(S2, S1) + Math.PI / 2) * 255 / Math.PI)];
                            result++;
                        }
                        source += 2 * frameWidth;
                    }
                }
            }

            resBitmap.UnlockBits(bd);
            return resBitmap;
        }
    }
}
