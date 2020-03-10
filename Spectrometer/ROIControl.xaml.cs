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
    /// Логика взаимодействия для POIControl.xaml
    /// </summary>
    public partial class ROIControl : UserControl
    {
        public static DependencyProperty ROIWidthProperty;
        public static DependencyProperty ROIHeightProperty;
        public static DependencyProperty ROIOffsetXProperty;
        public static DependencyProperty ROIOffsetYProperty;

        public static DependencyProperty ROIWidthMinProperty;
        public static DependencyProperty ROIWidthMaxProperty;
        public static DependencyProperty ROIHeightMinProperty;
        public static DependencyProperty ROIHeightMaxProperty;
        public static DependencyProperty ROIOffsetXMinProperty;
        public static DependencyProperty ROIOffsetXMaxProperty;
        public static DependencyProperty ROIOffsetYMinProperty;
        public static DependencyProperty ROIOffsetYMaxProperty;

        static ROIControl()
        {
            ROIWidthProperty = DependencyProperty.Register("ROIWidth", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(1,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
            ROIHeightProperty = DependencyProperty.Register("ROIHeight", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(1,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
            ROIOffsetXProperty = DependencyProperty.Register("ROIOffsetX", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(0,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
            ROIOffsetYProperty = DependencyProperty.Register("ROIOffsetY", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(0,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

            ROIWidthMinProperty = DependencyProperty.Register("ROIWidthMin", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(1,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
            ROIWidthMaxProperty = DependencyProperty.Register("ROIWidthMax", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(100,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

            ROIHeightMinProperty = DependencyProperty.Register("ROIHeightMin", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(1,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
            ROIHeightMaxProperty = DependencyProperty.Register("ROIHeightMax", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(100,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

            ROIOffsetXMinProperty = DependencyProperty.Register("ROIOffsetXMin", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(0,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
            ROIOffsetXMaxProperty = DependencyProperty.Register("ROIOffsetXMax", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(100,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

            ROIOffsetYMinProperty = DependencyProperty.Register("ROIOffsetYMin", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(0,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
            ROIOffsetYMaxProperty = DependencyProperty.Register("ROIOffsetYMax", typeof(int), typeof(ROIControl), new FrameworkPropertyMetadata(100,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        }

        public int ROIWidth
        {
            get => (int)GetValue(ROIWidthProperty);
            set => SetValue(ROIWidthProperty, value);
        }

        public int ROIHeight
        {
            get => (int)GetValue(ROIHeightProperty);
            set => SetValue(ROIHeightProperty, value);
        }

        public int ROIOffsetX
        {
            get => (int)GetValue(ROIOffsetXProperty);
            set => SetValue(ROIOffsetXProperty, value);
        }

        public int ROIOffsetY
        {
            get => (int)GetValue(ROIOffsetYProperty);
            set => SetValue(ROIOffsetYProperty, value);
        }

        public int ROIWidthMin
        {
            get => (int)GetValue(ROIWidthMinProperty);
            set => SetValue(ROIWidthMinProperty, value);
        }

        public int ROIWidthMax
        {
            get => (int)GetValue(ROIWidthMaxProperty);
            set => SetValue(ROIWidthMaxProperty, value);
        }

        public int ROIHeightMin
        {
            get => (int)GetValue(ROIHeightMinProperty);
            set => SetValue(ROIHeightMinProperty, value);
        }
        public int ROIHeightMax
        {
            get => (int)GetValue(ROIHeightMaxProperty);
            set => SetValue(ROIHeightMaxProperty, value);
        }

        public int ROIOffsetXMin
        {
            get => (int)GetValue(ROIOffsetXMinProperty);
            set => SetValue(ROIOffsetXMinProperty, value);
        }
        public int ROIOffsetXMax
        {
            get => (int)GetValue(ROIOffsetXMaxProperty);
            set => SetValue(ROIOffsetXMaxProperty, value);
        }

        public int ROIOffsetYMin
        {
            get => (int)GetValue(ROIOffsetYMinProperty);
            set => SetValue(ROIOffsetYMinProperty, value);
        }

        public int ROIOffsetYMax
        {
            get => (int)GetValue(ROIOffsetYMaxProperty);
            set => SetValue(ROIOffsetYMaxProperty, value);
        }

        public static void OnROIWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROIControl control = d as ROIControl;
            

        }

        public ROIControl()
        {
            InitializeComponent();
        }
    }
}
