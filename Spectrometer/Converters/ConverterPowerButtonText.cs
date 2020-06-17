using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Spectrometer.Converters
{
    class ConverterPowerButtonText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(System.Convert.ToBoolean(value))
            {
                return "Выкл. пит.";
            }
            else
            {
                return "Вкл. пит.";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
            {
                return "Выкл. пит.";
            }
            else
            {
                return "Вклл. пит.";
            }
        }
    }
}
