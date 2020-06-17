using Spectrometer.Converters;
using System;
using System.Collections.Generic;
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

namespace Spectrometer.UserControls
{
    /// <summary>
    /// Логика взаимодействия для FeatureControl.xaml
    /// </summary>
    public partial class FeatureControl : UserControl
    {
        private static readonly DependencyProperty ValueFormatProperty = null;
        private static readonly DependencyProperty IsAutoProperty = null;
        private static readonly DependencyProperty IsLogarithmProperty = null;
        private static readonly DependencyProperty AutoVisibilityProperty = null;
        private static readonly DependencyProperty MinNUDWidthProperty = null;

        public delegate void ValueChangedHandler(object sender);

        static FeatureControl()
        {
            ValueFormatProperty = DependencyProperty.Register("ValueFormat", typeof(string), typeof(FeatureControl), new FrameworkPropertyMetadata("F1",
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            AutoVisibilityProperty = DependencyProperty.Register("AutoVisibility", typeof(Visibility), typeof(FeatureControl), new FrameworkPropertyMetadata(Visibility.Collapsed,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            IsAutoProperty = DependencyProperty.Register("IsAuto", typeof(bool), typeof(FeatureControl), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            IsLogarithmProperty = DependencyProperty.Register("IsLogarithm", typeof(bool), typeof(FeatureControl), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnLogarithmChanged));
            MinNUDWidthProperty = DependencyProperty.Register("MinNUDWidth", typeof(double), typeof(FeatureControl), new FrameworkPropertyMetadata(90d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        }

        private IValueConverter valueConverter = null;
        private IValueConverter valueLogConverter = new LogarithmConverter();
        public IValueConverter ValueConverter
        {
            get
            {
                if(IsLogarithm)
                {
                    return valueLogConverter;
                }
                else
                {
                    return valueConverter;
                }
            }
            set
            {
                valueConverter = value;
            }
        }

        private static void OnLogarithmChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FeatureControl control = d as FeatureControl;

            Binding sliderBinding = new Binding("Value");
            sliderBinding.ElementName = "doubleUpDown";
            sliderBinding.Mode = BindingMode.TwoWay;
            sliderBinding.Converter = new LogarithmConverter();
            sliderBinding.ConverterParameter = control.IsLogarithm;
            control.slider.SetBinding(Slider.ValueProperty, sliderBinding);

            Binding sliderBindingMin = new Binding("Minimum");
            sliderBindingMin.ElementName = "doubleUpDown";
            sliderBindingMin.Mode = BindingMode.TwoWay;
            sliderBindingMin.Converter = new LogarithmConverter();
            sliderBindingMin.ConverterParameter = control.IsLogarithm;
            control.slider.SetBinding(Slider.MinimumProperty, sliderBindingMin);

            Binding sliderBindingMax = new Binding("Maximum");
            sliderBindingMax.ElementName = "doubleUpDown";
            sliderBindingMax.Mode = BindingMode.TwoWay;
            sliderBindingMax.Converter = new LogarithmConverter();
            sliderBindingMax.ConverterParameter = control.IsLogarithm;
            control.slider.SetBinding(Slider.MaximumProperty, sliderBindingMax);
        }

        public FeatureControl()
        {
            InitializeComponent();
        }

        public string ValueFormat
        {
            get => (string)GetValue(ValueFormatProperty);
            set => SetValue(ValueFormatProperty, value);
        }

        public bool IsAuto
        {
            get => (bool)GetValue(IsAutoProperty);
            set => SetValue(IsAutoProperty, value);
        }

        public bool IsLogarithm
        {
            get => (bool)GetValue(IsLogarithmProperty);
            set => SetValue(IsLogarithmProperty, value);
        }

        public double MinNUDWidth
        {
            get => (double)GetValue(MinNUDWidthProperty);
            set => SetValue(MinNUDWidthProperty, value);
        }

        public Visibility AutoVisibility
        {
            get => (Visibility)GetValue(AutoVisibilityProperty);
            set => SetValue(AutoVisibilityProperty, value);
        }
    }
}
