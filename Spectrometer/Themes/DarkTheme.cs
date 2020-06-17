using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrometer.Themes
{
    public class DarkTheme : AvalonDock.Themes.Vs2013DarkTheme
    {
        public override Uri GetResourceUri()
        {
            return new Uri("/Themes/DarkTheme.xaml", UriKind.Relative);
        }
    }
}
