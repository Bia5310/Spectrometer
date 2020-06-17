using Microsoft.Win32;
using NLog;
using Spectrometer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VimbaCameraNET;
using static AO_Lib.AO_Devices;

namespace Spectrometer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ApplicationViewModel();
        }

        private void mainForm_Loaded(object sender, RoutedEventArgs e)
        {
            /*bottomGroup.DockHeight = new GridLength(250, GridUnitType.Pixel);
            rightSideGroup.DockHeight = new GridLength(450, GridUnitType.Pixel);*/
        }
    }
}
