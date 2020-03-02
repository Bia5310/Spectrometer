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
        
        static ValueControl()
        {
            FeatureNameProperty = DependencyProperty.Register("FeatureName", typeof(string), typeof(ValueControl), new FrameworkPropertyMetadata("Name",
                                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ValueFormatProperty = DependencyProperty.Register("ValueFormat", typeof(string), typeof(ValueControl), new FrameworkPropertyMetadata("F1",
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            UnitsProperty = DependencyProperty.Register("Units", typeof(string), typeof(ValueControl), new FrameworkPropertyMetadata(String.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(ValueControl), new FrameworkPropertyMetadata(0d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(ValueControl), new FrameworkPropertyMetadata(100d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(ValueControl), new FrameworkPropertyMetadata(0d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(ValueControl), new FrameworkPropertyMetadata(1d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            AutoVisibilityProperty = DependencyProperty.Register("AutoVisibility", typeof(Visibility), typeof(ValueControl), new FrameworkPropertyMetadata(Visibility.Collapsed,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            IsAutoProperty = DependencyProperty.Register("IsAuto", typeof(bool), typeof(ValueControl), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            IsLogarithmProperty = DependencyProperty.Register("IsLogarithm", typeof(bool), typeof(ValueControl), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
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
        }
    }
}
