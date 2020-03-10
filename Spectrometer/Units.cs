using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrometer
{
    public static class Units
    {
        public enum UnitsEnum : int { NanoMeters = 0, WaveNumber = 1 };

        public const char lambda = 'λ';
        public const char nu = 'ν';
        public const string revcm = "см⁻¹";

        public static double WavelengthToWavenumber(double wavelength) //wl - nm, wn - 1/cm
        {
            return 10e7d / wavelength;
        }

        public static decimal WavelengthToWavenumber(decimal wavelength) //wl - nm, wn - 1/cm
        {
            return 10e7m / wavelength;
        }

        public static double WavenumberToWavelength(double wavenumber)
        {
            return 10e7d / wavenumber;
        }
        public static decimal WavenumberToWavelength(decimal wavenumber)
        {
            return 10e7m / wavenumber;
        }

        public static string ToString(UnitsEnum ue)
        {
            switch(ue)
            {
                case UnitsEnum.NanoMeters:
                    return "нм";
                case UnitsEnum.WaveNumber:
                    return revcm;
            }
            return "";
        }
    }
}
