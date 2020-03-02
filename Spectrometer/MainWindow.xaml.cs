using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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
using static AO_Lib.AO_Devices;

namespace Spectrometer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private AO_Filter AOFilter = null;

        public MainWindow()
        {

        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                if (WindowState == WindowState.Maximized)
                {
                    var c = e.GetPosition(this);
                    double px1 = this.Top;
                    double py1 = this.Left;
                    double w1 = TitleBar.ActualWidth;
                    double h1 = TitleBar.ActualHeight;

                    WindowState = WindowState.Normal;

                    this.Left = px1 + 10;// c.X * (1 - TitleBar.ActualWidth / w1) + px1 - this.Left;
                    this.Top = py1 + 10;//c.Y * (1 - TitleBar.ActualHeight / h1) + py1 - this.Top;
                }

                this.DragMove();
            }
        }

        private void Button_Hide_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ToggleButton_MaxMin_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount > 1)
            {
                ToggleButton_MaxMin_Click(null, null);
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            SplitterBottom.Visibility = Visibility.Visible;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid_Bottom.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
            SplitterBottom.Visibility = Visibility.Collapsed;
        }

        #region AOF Controlling

        private bool attenuationAvailable = false;
        private bool devLoaded = false;

        private void ButtonLoadDev_Click(object sender, RoutedEventArgs e)
        {
            if (AOFilter != null)
            {
                OpenFileDialog dialog = new OpenFileDialog();

                if (dialog.ShowDialog() == true)
                {
                    int error = AOFilter.Read_dev_file(dialog.FileName);

                    if (error != 0) //0 = gut
                        MessageBox.Show(AOFilter.Implement_Error(error));
                    devLoaded = error == 0;

                    if (devLoaded)
                        InitSlidersByAOF();
                }
            }
            else
            {
                devLoaded = false;
            }
            State_AOF_DevLoad(devLoaded);
        }

        private void InitSlidersByAOF()
        {
            sliderWavelength.Minimum = AOFilter.WL_Min;
            sliderWavelength.Maximum = AOFilter.WL_Max;

            sliderFrequency.Minimum = AOFilter.HZ_Min;
            sliderFrequency.Maximum = AOFilter.HZ_Max;
            //sliderFrequency.Value = AOFilter.HZ_Current;
            //textBoxFrequency.Text = sliderFrequency.Value.ToString("F2", CultureInfo.InvariantCulture);

            sliderAttenuation.Maximum = 2500;
            sliderAttenuation.Minimum = 1700;
            //sliderAttenuation.Value = 2500;
            //textBoxAttenuation.Text = sliderAttenuation.Value.ToString("F2", CultureInfo.InvariantCulture);

            sliderWavenumber.Minimum = 1e7f / sliderWavelength.Maximum;
            sliderWavenumber.Maximum = 1e7f / sliderWavelength.Minimum;
            //sliderWavenumber.Value = 1e7f / sliderWavelength.Value;
            //textBoxWavenumber.Text = sliderWavenumber.Value.ToString("F2", CultureInfo.InvariantCulture);

            sliderWavelength.Value = (AOFilter.WL_Max + AOFilter.WL_Min) / 2;
            //textBoxWavelength.Text = sliderWavelength.Value.ToString("F2", CultureInfo.InvariantCulture);
            AutoAtten.IsEnabled = AOFilter.FilterType == FilterTypes.STC_Filter;
        }


        private void TextBoxWavelength_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double value = 0;
                if (double.TryParse(textBoxWavelength.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    sliderWavelength.Value = value;
                }
            }
        }

        private void TextBoxAttenuation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double value = 0;
                if (double.TryParse(textBoxAttenuation.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    sliderAttenuation.Value = value;
                }
            }
        }

        private void textBoxWavenumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double value = 0;
                if (double.TryParse(textBoxWavenumber.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    sliderWavenumber.Value = value;
                }
            }
        }

        private void textBoxFrequency_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double value = 0;
                if (double.TryParse(textBoxFrequency.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    sliderFrequency.Value = value;
                }
            }
        }

        private void ButtonConnectAOF_Click(object sender, RoutedEventArgs e)
        {
            if (AOFilter == null) //значит надо подключить
            {
                AOFilter = AO_Filter.Find_and_connect_AnyFilter();
                if (AOFilter == null)
                    MessageBox.Show("Фильтры не найдены");
                else
                {
                    attenuationAvailable = AOFilter.GetType() == typeof(STC_Filter);
                }
            }
            else //значит надо отключить
            {
                //начать с питания
                if (AOFilter.isPowered)
                    AOFilter.PowerOff();
                AOFilter = null;

                State_AOF_DevLoad(false);
                State_AOF_Power(false);
            }
            State_AOF_Connection(AOFilter != null);
        }

        private void State_AOF_Connection(bool connected)
        {
            buttonConnectAOF.Content = connected ? "Откл." : "Подкл.";

            buttonLoadDev.IsEnabled = connected;
            buttonPower.IsEnabled = false;
        }

        private void State_AOF_DevLoad(bool loaded)
        {
            buttonPower.IsEnabled = loaded;
            sliderFrequency.IsEnabled = loaded;
            sliderWavelength.IsEnabled = loaded;
            sliderWavenumber.IsEnabled = loaded;
            textBoxFrequency.IsEnabled = loaded;
            textBoxWavelength.IsEnabled = loaded;
            textBoxWavenumber.IsEnabled = loaded;
            AutoAtten.IsEnabled = loaded && (AOFilter.FilterType == FilterTypes.STC_Filter);

            sliderAttenuation.IsEnabled = attenuationAvailable;
            textBoxAttenuation.IsEnabled = attenuationAvailable;
        }

        private void State_AOF_Power(bool powered)
        {
            buttonPower.Content = powered ? "Откл. пит." : "Вкл. пит.";
        }

        private void ButtonPower_Click(object sender, RoutedEventArgs e)
        {
            if (AOFilter != null)
            {
                if (AOFilter.isPowered)
                    AOFilter.PowerOff();
                else
                    AOFilter.PowerOn();
                State_AOF_Power(AOFilter.isPowered);
            }
        }


        private void SliderWavelength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetViaWavelength((float)sliderWavelength.Value);
        }

        private void SliderWavenumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetViaWavelength((float)(1e7f / sliderWavenumber.Value));
        }

        private void SliderFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AOFilter != null)
                SetViaWavelength(AOFilter.Get_WL_via_HZ((float)sliderFrequency.Value));
        }

        private void SetViaWavelength(float wl)
        {
            if (AOFilter == null)
            {
                return;
            }

            if (wl >= AOFilter.WL_Min && wl <= AOFilter.WL_Max)
            {
                if (AOFilter.FilterType == FilterTypes.STC_Filter && AutoAtten.IsChecked == true)
                {
                    float attenuation = (float)sliderAttenuation.Value;

                    if (attenuation >= 1700 && attenuation <= 2500)
                        (AOFilter as STC_Filter).Set_Hz(AOFilter.Get_HZ_via_WL(wl), attenuation);

                    sliderAttenuation.ValueChanged -= SliderAttenuation_ValueChanged;
                    sliderAttenuation.Value = attenuation;
                    textBoxAttenuation.Text = attenuation.ToString("F2", CultureInfo.InvariantCulture);
                    sliderAttenuation.ValueChanged += SliderAttenuation_ValueChanged;
                }
                else
                {
                    AOFilter.Set_Wl(wl);
                }
            }

            UpdateSliders();
            UpdateSliderTextBoxes();
        }

        private void SliderAttenuation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AOFilter == null || AOFilter.FilterType != FilterTypes.STC_Filter)
            {
                return;
            }
            STC_Filter STCFilter = (STC_Filter)AOFilter;

            textBoxAttenuation.Text = sliderAttenuation.Value.ToString("F2", CultureInfo.InvariantCulture);
            float hz = (float)AOFilter.HZ_Current;
            float K = (float)sliderAttenuation.Value;
            if (hz >= AOFilter.HZ_Min && hz <= AOFilter.HZ_Max && K >= 1700 && K <= 2500)
            {
                STCFilter.Set_Hz(hz, K);
            }
        }

        private void UpdateSliders()
        {
            if (AOFilter == null)
            {
                return;
            }

            sliderWavelength.ValueChanged -= SliderWavelength_ValueChanged;
            sliderFrequency.ValueChanged -= SliderFrequency_ValueChanged;
            sliderWavenumber.ValueChanged -= SliderWavenumber_ValueChanged;

            try
            {
                sliderWavelength.Value = AOFilter.WL_Current;
                sliderFrequency.Value = AOFilter.HZ_Current;
                sliderWavenumber.Value = 1e7d / sliderWavelength.Value;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return;
            }
            finally
            {
                sliderWavelength.ValueChanged += SliderWavelength_ValueChanged;
                sliderFrequency.ValueChanged += SliderFrequency_ValueChanged;
                sliderWavenumber.ValueChanged += SliderWavenumber_ValueChanged;
            }
        }

        private void UpdateSliderTextBoxes()
        {
            if (AOFilter == null)
            {
                return;
            }

            try
            {
                textBoxWavelength.Text = sliderWavelength.Value.ToString("F2", CultureInfo.InvariantCulture);
                textBoxWavenumber.Text = sliderWavenumber.Value.ToString("F2", CultureInfo.InvariantCulture);
                textBoxFrequency.Text = sliderFrequency.Value.ToString("F3", CultureInfo.InvariantCulture);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return;
            }
        }

        private void TextBoxWavelength_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBoxWavenumber_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBoxFrequency_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBoxAttenuation_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #endregion
    }
}
