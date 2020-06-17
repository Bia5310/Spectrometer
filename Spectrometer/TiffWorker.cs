using AVT.VmbAPINET;
using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Spectrometer.Polarisation;

namespace Spectrometer
{
    public class TiffWorker
    {
        //Дополнительные тэги для TIFF
        public const TiffTag TIFFTAG_WAVELENGTH_METADATA = (TiffTag)800; //nm
        public const TiffTag TIFFTAG_EXPOSURE_METADATA = (TiffTag)801;
        public const TiffTag TIFFTAG_GAIN_METADATA = (TiffTag)802;
        public const TiffTag TIFFTAG_TIMESTAMP_METADATA = (TiffTag)803;
        public const TiffTag TIFFTAG_GPS_METADATA = (TiffTag)804;
        public const TiffTag TIFFTAG_GAMMA_METADATA = (TiffTag)805;
        public const TiffTag TIFFTAG_OffsetX_METADATA = (TiffTag)806; //ROI offset X
        public const TiffTag TIFFTAG_OffsetY_METADATA = (TiffTag)807; //ROI offset Y
        public const TiffTag TIFFTAG_BinningX_METADATA = (TiffTag)808; //Binning X
        public const TiffTag TIFFTAG_BinningY_METADATA = (TiffTag)809; //Binning Y
        public const TiffTag TIFFTAG_WAVENUMBER_METADATA = (TiffTag)810; //WaveNumber (1/cm)
        public const TiffTag TIFFTAG_BITS_METADATA = (TiffTag)811; //Bits per pixel
        public const TiffTag TIFFTAG_FREQUENCYMHZ_METADATA = (TiffTag)812; //Frequence (MHz)

        /// <summary>
        /// Сохраняет VimbaAPI Frame в *.Tiff файл
        /// </summary>
        /// <param name="frame">Vimba's Frame</param>
        /// <param name="filename">Имя файла</param>
        /// <param name="tiffWorker"></param>
        public static void SaveFrameToTiff(Frame frame, string filename, TiffWorker tiffWorker)
        {
            Tiff.SetTagExtender(TagExtender); //parentExtender = Tiff.SetTagExtender(TagExtender);
            using (var tiff = Tiff.Open(filename, "w"))
            {
                tiff.SetField(TiffTag.IMAGEWIDTH, frame.Width);
                tiff.SetField(TiffTag.IMAGELENGTH, frame.Height);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 16);
                tiff.SetField(TiffTag.ROWSPERSTRIP, frame.Height);
                tiff.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);

                //Set custom tags:
                
                tiff.SetField(TIFFTAG_WAVELENGTH_METADATA, tiffWorker.Wavelength);
                tiff.SetField(TIFFTAG_EXPOSURE_METADATA, tiffWorker.Exposure);
                tiff.SetField(TIFFTAG_GAIN_METADATA, tiffWorker.Gain);
                tiff.SetField(TIFFTAG_GAMMA_METADATA, tiffWorker.Gamma);
                tiff.SetField(TIFFTAG_TIMESTAMP_METADATA, tiffWorker.Timestamp);
                tiff.SetField(TIFFTAG_GPS_METADATA, tiffWorker.GPS);
                tiff.SetField(TIFFTAG_OffsetX_METADATA, tiffWorker.OffsetX);
                tiff.SetField(TIFFTAG_OffsetY_METADATA, tiffWorker.OffsetY);
                tiff.SetField(TIFFTAG_BinningX_METADATA, tiffWorker.BinningX);
                tiff.SetField(TIFFTAG_BinningY_METADATA, tiffWorker.BinningY);
                tiff.SetField(TIFFTAG_WAVENUMBER_METADATA, tiffWorker.Wavenumber);
                tiff.SetField(TIFFTAG_BITS_METADATA, tiffWorker.Bits);

                int rawLength = (int)frame.Width * 2;   //длина ряда 16-битных пикселей в байтах
                byte[] buffer = new byte[rawLength];

                for (int j = 0; j < frame.Height; j++)
                {
                    int srcOffset = rawLength * j;        //offset in source buffer в байтах
                    
                    Buffer.BlockCopy(frame.Buffer, srcOffset, buffer, 0, buffer.Length); //заполняем буффер ряда

                    tiff.WriteScanline(buffer, j); //записываем ряд j в файл
                }
                tiff.Close();
                tiff.Dispose();
            }
            //Tiff.SetTagExtender(parentExtender);
            
        }

        /// <summary>
        /// Сохраняет VimbaAPI Frame в *.Tiff файл
        /// </summary>
        /// <param name="frame">Vimba's Frame</param>
        /// <param name="filename">Имя файла</param>
        /// <param name="tiffWorker"></param>
        public static void SavePolarisationFrameToTiff(Frame frame, string filename, TiffWorker tiffWorker)
        {
            Tiff.SetTagExtender(TagExtender); //parentExtender = Tiff.SetTagExtender(TagExtender);
            using (var tiff = Tiff.Open(filename, "w"))
            {
                tiff.SetField(TiffTag.IMAGEWIDTH, frame.Width);
                tiff.SetField(TiffTag.IMAGELENGTH, frame.Height);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 16);
                tiff.SetField(TiffTag.ROWSPERSTRIP, frame.Height);
                tiff.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);

                //Set custom tags:

                tiff.SetField(TIFFTAG_WAVELENGTH_METADATA, tiffWorker.Wavelength);
                tiff.SetField(TIFFTAG_EXPOSURE_METADATA, tiffWorker.Exposure);
                tiff.SetField(TIFFTAG_GAIN_METADATA, tiffWorker.Gain);
                tiff.SetField(TIFFTAG_GAMMA_METADATA, tiffWorker.Gamma);
                tiff.SetField(TIFFTAG_TIMESTAMP_METADATA, tiffWorker.Timestamp);
                tiff.SetField(TIFFTAG_GPS_METADATA, tiffWorker.GPS);
                tiff.SetField(TIFFTAG_OffsetX_METADATA, tiffWorker.OffsetX);
                tiff.SetField(TIFFTAG_OffsetY_METADATA, tiffWorker.OffsetY);
                tiff.SetField(TIFFTAG_BinningX_METADATA, tiffWorker.BinningX);
                tiff.SetField(TIFFTAG_BinningY_METADATA, tiffWorker.BinningY);
                tiff.SetField(TIFFTAG_WAVENUMBER_METADATA, tiffWorker.Wavenumber);
                tiff.SetField(TIFFTAG_BITS_METADATA, tiffWorker.Bits);

                //Main Page
                tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                tiff.SetField(TiffTag.PAGENAME, "Full");
                tiff.SetField(TiffTag.PAGENUMBER, 0, 5);

                int rawLength = (int)frame.Width * 2;   //длина ряда 16-битных пикселей в байтах
                byte[] buffer = new byte[rawLength];
                int srcOffset = 0;

                for (int j = 0; j < frame.Height; j++)
                {
                    srcOffset = rawLength * j;        //offset in source buffer в байтах

                    Buffer.BlockCopy(frame.Buffer, srcOffset, buffer, 0, buffer.Length); //заполняем буффер ряда

                    tiff.WriteScanline(buffer, j); //записываем ряд j в файл
                }
                tiff.WriteDirectory();

                //Write Polarisation Components pages
                int w = (int) frame.Width / 2;
                int h = (int) frame.Height / 2;
                rawLength = (int)frame.Width;
                buffer = new byte[rawLength];
                
                PolarData[] polars = new PolarData[] {PolarData.I0, PolarData.I135, PolarData.I45, PolarData.I90};
                byte[] I = null;

                for (int k = 0; k < polars.Length; k++)
                {
                    tiff.SetField(TiffTag.IMAGEWIDTH, w);
                    tiff.SetField(TiffTag.IMAGELENGTH, h);
                    tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    tiff.SetField(TiffTag.BITSPERSAMPLE, 16);
                    tiff.SetField(TiffTag.ROWSPERSTRIP, h);
                    tiff.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                    tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    tiff.SetField(TiffTag.PAGENAME, polars[k].ToString());
                    tiff.SetField(TiffTag.PAGENUMBER, k+1, 5);

                    I = ExtractPolarisation(frame, polars[k], tiffWorker.Bits);

                    for (int j = 0; j < h; j++)
                    {
                        srcOffset = rawLength * j;

                        Buffer.BlockCopy(I, srcOffset, buffer, 0, buffer.Length);

                        tiff.WriteScanline(buffer, j);
                    }
                    tiff.WriteDirectory();
                }

                tiff.Close();
                tiff.Dispose();
            }
            //Tiff.SetTagExtender(parentExtender);

        }

        private static Tiff.TiffExtendProc parentExtender;

        public static void TagExtender(Tiff tif)
        {
            TiffFieldInfo[] tiffFieldInfo =
            {
                    new TiffFieldInfo(TIFFTAG_WAVELENGTH_METADATA, 1, 1, TiffType.DOUBLE, FieldBit.Custom, true, false, "Wavelength"),
                    new TiffFieldInfo(TIFFTAG_WAVENUMBER_METADATA, 1, 1, TiffType.DOUBLE, FieldBit.Custom, true, false, "Wavenumber"),
                    new TiffFieldInfo(TIFFTAG_EXPOSURE_METADATA, 1, 1, TiffType.DOUBLE, FieldBit.Custom, true, false, "Exposure"),
                    new TiffFieldInfo(TIFFTAG_GAIN_METADATA, 1, 1, TiffType.DOUBLE, FieldBit.Custom, true, false, "Gain"),
                    new TiffFieldInfo(TIFFTAG_GAMMA_METADATA, 1, 1, TiffType.DOUBLE, FieldBit.Custom, true, false, "Gamma"),
                    new TiffFieldInfo(TIFFTAG_GPS_METADATA, 1, 1, TiffType.DOUBLE, FieldBit.Custom, true, false, "GPS"),
                    new TiffFieldInfo(TIFFTAG_TIMESTAMP_METADATA, 1, 1, TiffType.DOUBLE, FieldBit.Custom, true, false, "Timestamp"), //записываем ULONG как массив из 8-и байт
                    new TiffFieldInfo(TIFFTAG_OffsetX_METADATA, 1, 1, TiffType.SLONG, FieldBit.Custom, true, false, "OffsetX"),
                    new TiffFieldInfo(TIFFTAG_OffsetY_METADATA, 1, 1, TiffType.SLONG, FieldBit.Custom, true, false, "OffsetY"),
                    new TiffFieldInfo(TIFFTAG_BinningX_METADATA, 1, 1, TiffType.SLONG, FieldBit.Custom, true, false, "BinningX"),
                    new TiffFieldInfo(TIFFTAG_BinningY_METADATA, 1, 1, TiffType.SLONG, FieldBit.Custom, true, false, "BinningY"),
                    new TiffFieldInfo(TIFFTAG_BITS_METADATA, 1, 1, TiffType.SLONG, FieldBit.Custom, true, false, "BitsPerPixel"),
            };

            tif.MergeFieldInfo(tiffFieldInfo, tiffFieldInfo.Length);
            
            /*if (parentExtender != null)
                parentExtender(tif);*/
        }

        public double Wavelength;
        public double Wavenumber;
        public double Frequency = 0;
        public double Exposure;
        public double Gain;
        public double GPS;
        public double Timestamp;
        public double Gamma;
        public int OffsetX;
        public int OffsetY;
        public int BinningX;
        public int BinningY;
        public int Width;
        public int Height;
        public int Bits;

        public string[] GetStrings()
        {
            return new string[] {
                "Width: " + Width.ToString(),
                "Height: " + Height.ToString(),
                "OffsetX: " + OffsetX.ToString(),
                "OffsetY: " + OffsetY.ToString(),
                "BinningX: " + BinningX.ToString(),
                "BinningY: " + BinningY.ToString(),
                "Exposure: " + Exposure.ToString("F"),
                "Gain: " + Gain.ToString("F"),
                "Gamma: " + Gamma.ToString("F"),
                "Timestamp: "+ Timestamp.ToString(),
                "GPS: " + GPS.ToString("F"),
                "Bits: " + Bits.ToString(),
                "Wavelength: " + Wavelength.ToString(),
            };
        }
    }
}
