using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static VimbaCameraNET.VimbaCamera;

namespace Spectrometer.Converters
{
    public class ConverterFeatureRangeText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                CameraFeature feature = (CameraFeature)value;
                return String.Format("[{0:F2}...{1:F2}]", feature.MinValue, feature.MaxValue);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
