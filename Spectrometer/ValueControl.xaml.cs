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

namespace Spectrometer
{
    /// <summary>
    /// Логика взаимодействия для ValueControl.xaml
    /// </summary>
    public partial class ValueControl : UserControl
    {
        private static readonly DependencyProperty FeatureNameProperty = null;
        private static readonly DependencyProperty ValueFormatProperty = null;
        private static readonly DependencyProperty ValueProperty = null;
        private static readonly DependencyProperty IncrementProperty = null;
        private static readonly DependencyProperty MaxValueProperty = null;
        private static readonly DependencyProperty MinValueProperty = null;
        private static readonly DependencyProperty IsAutoProperty = null;
        private static readonly DependencyProperty IsLogarithmProperty = null;
        private static readonly DependencyProperty AutoVisibilityProperty = null;
        private static readonly DependencyProperty UnitsProperty = null;

        public delegate void ValueChangedHandler(object sender);
        public event ValueChangedHandler ValueChanged;

        private Binding sliderBinding = null;

        static ValueControl()
        {
            FeatureNameProperty = DependencyProperty.Register("FeatureName", typeof(string), typeof(ValueControl), new FrameworkPropertyMetadata("Name",
                                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ValueFormatProperty = DependencyProperty.Register("ValueFormat", typeof(string), typeof(ValueControl), new FrameworkPropertyMetadata("F1",
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            UnitsProperty = DependencyProperty.Register("Units", typeof(string), typeof(ValueControl), new FrameworkPropertyMetadata(String.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(ValueControl), new FrameworkPropertyMetadata(1d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(ValueControl), new FrameworkPropertyMetadata(100d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(ValueControl), new FrameworkPropertyMetadata(1d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(ValueControl), new FrameworkPropertyMetadata(1d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            AutoVisibilityProperty = DependencyProperty.Register("AutoVisibility", typeof(Visibility), typeof(ValueControl), new FrameworkPropertyMetadata(Visibility.Collapsed,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            IsAutoProperty = DependencyProperty.Register("IsAuto", typeof(bool), typeof(ValueControl), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnIsAutoChanged));
            IsLogarithmProperty = DependencyProperty.Register("IsLogarithm", typeof(bool), typeof(ValueControl), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnLogarithmChanged));
        }


        private static void OnIsAutoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueControl control = d as ValueControl;
            control.slider.IsEnabled = !control.IsAuto;
            control.doubleUpDown.IsEnabled = !control.IsAuto;
        }

        private static void OnLogarithmChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueControl control = d as ValueControl;

            Binding sliderBinding = new Binding("Value");
            sliderBinding.Mode = BindingMode.TwoWay;
            //sliderBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            sliderBinding.ElementName = "UserControl";
            sliderBinding.Converter = new LogarithmConverter();
            sliderBinding.ConverterParameter = control.IsLogarithm;
            control.slider.SetBinding(Slider.ValueProperty, sliderBinding);

            Binding sliderBindingMin = new Binding("MinValue");
            sliderBindingMin.Mode = BindingMode.TwoWay;
            //sliderBindingMin.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            sliderBindingMin.ElementName = "UserControl";
            sliderBindingMin.Converter = new LogarithmConverter();
            sliderBindingMin.ConverterParameter = control.IsLogarithm;
            control.slider.SetBinding(Slider.MinimumProperty, sliderBindingMin);

            Binding sliderBindingMax = new Binding("MaxValue");
            sliderBindingMax.Mode = BindingMode.TwoWay;
            //sliderBindingMax.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            sliderBindingMax.ElementName = "UserControl";
            sliderBindingMax.Converter = new LogarithmConverter();
            sliderBindingMax.ConverterParameter = control.IsLogarithm;
            control.slider.SetBinding(Slider.MaximumProperty, sliderBindingMax);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueControl control = d as ValueControl;
            
            /*if(control.IsInitialized)
            {
                control.slider.ValueChanged -= control.slider_ValueChanged;
                if (control.IsLogarithm)
                {
                    control.slider.Value = Math.Log10(control.Value);
                }
                else
                {
                    control.slider.Value = control.Value;
                }
                control.slider.ValueChanged += control.slider_ValueChanged;
            }
            */
            control.ValueChanged?.Invoke(control);
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueControl control = d as ValueControl;
            if(control.IsInitialized)
            {
                if(control.IsLogarithm)
                {
                    control.slider.Maximum = Math.Log10(control.MaxValue);
                }
                else
                {
                    control.slider.Maximum = control.MaxValue;
                }
            }
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueControl control = d as ValueControl;
            if(control.IsInitialized)
            {
                if (control.IsLogarithm)
                {
                    control.slider.Minimum = Math.Log10(control.MinValue);
                }
                else
                {
                    control.slider.Minimum = control.MinValue;
                }
            }
        }

        public string FeatureName
        {
            get => (string)GetValue(FeatureNameProperty);
            set => SetValue(FeatureNameProperty, value);
        }

        public string ValueFormat
        {
            get => (string)GetValue(ValueFormatProperty);
            set => SetValue(ValueFormatProperty, value);
        }

        public string Units
        {
            get => (string)GetValue(UnitsProperty);
            set => SetValue(UnitsProperty, value);
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

        public Visibility AutoVisibility
        {
            get => (Visibility)GetValue(AutoVisibilityProperty);
            set => SetValue(AutoVisibilityProperty, value);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set
            {
               SetValue(ValueProperty, Math.Round(value/Increment)*Increment);
            }
        }

        public double Increment
        {
            get => (double)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public ValueControl()
        {
            InitializeComponent();
            Binding sliderBinding = new Binding("Value");
            sliderBinding.Mode = BindingMode.TwoWay;
            //sliderBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            sliderBinding.ElementName = "UserControl";
            sliderBinding.Converter = new LogarithmConverter();
            sliderBinding.ConverterParameter = false;
            slider.SetBinding(Slider.ValueProperty, sliderBinding);

            Binding sliderBindingMin = new Binding("MinValue");
            sliderBindingMin.Mode = BindingMode.TwoWay;
            //sliderBindingMin.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            sliderBindingMin.ElementName = "UserControl";
            sliderBindingMin.Converter = new LogarithmConverter();
            sliderBindingMin.ConverterParameter = false;
            slider.SetBinding(Slider.MinimumProperty, sliderBindingMin);

            Binding sliderBindingMax = new Binding("MaxValue");
            sliderBindingMax.Mode = BindingMode.TwoWay;
            //sliderBindingMax.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            sliderBindingMax.ElementName = "UserControl";
            sliderBindingMax.Converter = new LogarithmConverter();
            sliderBindingMax.ConverterParameter = false;
            slider.SetBinding(Slider.MaximumProperty, sliderBindingMax);
        }
    }
}
