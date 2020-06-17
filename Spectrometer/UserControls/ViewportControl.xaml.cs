using Spectrometer.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Логика взаимодействия для ViewportControl.xaml
    /// </summary>
    public partial class ViewportControl : UserControl
    {
        public static readonly DependencyProperty ZoomProperty = null;
        public static readonly DependencyProperty ScrollXProperty = null;
        public static readonly DependencyProperty ScrollYProperty = null;
        //public static readonly DependencyProperty ImageProperty = null;

        public ViewportControl()
        {
            InitializeComponent();
            
        }

        /*public WriteableBitmap Image
        {
            get => (WriteableBitmap)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }*/

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public double ScrollX
        {
            get => (double)GetValue(ScrollXProperty);
            set => SetValue(ScrollXProperty, value);
        }

        public double ScrollY
        {
            get => (double)GetValue(ScrollYProperty);
            set => SetValue(ScrollYProperty, value);
        }

        static ViewportControl()
        {
            ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(1d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ScrollXProperty = DependencyProperty.Register("ScrollX", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ScrollYProperty = DependencyProperty.Register("ScrollY", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            /*ImageProperty = DependencyProperty.Register("Image", typeof(WriteableBitmap), typeof(ViewportControl), new FrameworkPropertyMetadata(new WriteableBitmap(1,1,96,96,PixelFormats.Bgr32, null),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));*/

            /*ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(1d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnZoomChanged));
            ScrollXProperty = DependencyProperty.Register("ScrollX", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnScrollXChanged));
            ScrollYProperty = DependencyProperty.Register("ScrollY", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnScrollYChanged));
            ImageProperty = DependencyProperty.Register("Image", typeof(WriteableBitmap), typeof(ViewportControl), new FrameworkPropertyMetadata(new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgr32, null),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnImageChanged));*/
        }

        private void scrollBarX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void scrollBarY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                CameraViewModel cvm = DataContext as CameraViewModel;
                if(e.Delta > 0)
                {
                    if(cvm.ZoomTick < 100)
                        cvm.ZoomTick++;
                }
                if(e.Delta < 0)
                {
                    if(cvm.ZoomTick > -100)
                        cvm.ZoomTick--;
                }
            }
            catch (Exception) { }
        }

        /*protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            return base.MeasureOverride(constraint);
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeBounds)
        {
            return base.ArrangeOverride(arrangeBounds);
        }*/
    }
}
