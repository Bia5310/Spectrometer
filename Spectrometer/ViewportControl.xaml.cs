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
        public ViewportControl()
        {
            InitializeComponent();
            
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateImagePosition();
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            return base.MeasureOverride(constraint);
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeBounds)
        {
            return base.ArrangeOverride(arrangeBounds);
        }

        public static readonly DependencyProperty ZoomProperty = null;
        public static readonly DependencyProperty ScrollXProperty = null;
        public static readonly DependencyProperty ScrollYProperty = null;
        public static readonly DependencyProperty ImageProperty = null;

        public static readonly DependencyProperty ImageWidthProperty = null;
        public static readonly DependencyProperty ImageHeightProperty = null;
        public static readonly DependencyProperty ImagePositionXProperty = null;
        public static readonly DependencyProperty ImagePositionYProperty = null;

        public double ImagePositionX
        {
            get => (double)GetValue(ImagePositionXProperty);
            set => SetValue(ImagePositionXProperty, value);
        }

        public double ImagePositionY
        {
            get => (double)GetValue(ImagePositionYProperty);
            set => SetValue(ImagePositionYProperty, value);
        }

        public double ImageWidth
        {
            get => (double)GetValue(ImageWidthProperty);
            set => SetValue(ImageWidthProperty, value);
        }

        public double ImageHeight
        {
            get => (double)GetValue(ImageHeightProperty);
            set => SetValue(ImageHeightProperty, value);
        }

        public WriteableBitmap Image
        {
            get => (WriteableBitmap)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public void UpdateImagePosition()
        {
            if (Image != null)
            {
                ImageWidth = Zoom * image.Source.Width;
                ImageHeight = Zoom * image.Source.Height;
                ImagePositionY = ActualHeight / 2d + image.Source.Height * Zoom * (ScrollY - 0.5d);
                ImagePositionX = ActualWidth / 2d + image.Source.Width * Zoom * (ScrollX - 0.5d);
            }
        }

        /*public WriteableBitmap WriteableBitmap
        {
            get => (WriteableBitmap)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }*/

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
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnZoomChanged));
            ScrollXProperty = DependencyProperty.Register("ScrollX", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnScrollXChanged));
            ScrollYProperty = DependencyProperty.Register("ScrollY", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnScrollYChanged));
            ImageProperty = DependencyProperty.Register("Image", typeof(WriteableBitmap), typeof(ViewportControl), new FrameworkPropertyMetadata(new WriteableBitmap(1,1,96,96,PixelFormats.Bgr32, null),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnImageChanged));

            ImagePositionXProperty = DependencyProperty.Register("ImagePositionX", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ImagePositionYProperty = DependencyProperty.Register("ImagePositionY", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ImageWidthProperty = DependencyProperty.Register("ImageWidth", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(1d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            ImageHeightProperty = DependencyProperty.Register("ImageHeight", typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(1d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        }

        private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ViewportControl c = d as ViewportControl;
            if (c.image.Source != null)
                c.UpdateImagePosition();
        }

        private static void OnScrollYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ViewportControl c = d as ViewportControl;
            if (c.image.Source != null)
                c.ImagePositionY = c.ActualHeight / 2d + c.image.Source.Height * c.Zoom * (c.ScrollY-0.5d);
        }

        private static void OnScrollXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ViewportControl c = d as ViewportControl;
            if (c.image.Source != null)
                c.ImagePositionX = c.ActualWidth / 2d + c.image.Source.Width * c.Zoom * (c.ScrollX - 0.5d);
        }

        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ViewportControl c = d as ViewportControl;
            if(c.image.Source != null)
            {
                c.UpdateImagePosition();
                //c.ImageWidth = c.image.Source.Width * c.Zoom;
                //c.ImageHeight = c.image.Source.Height * c.Zoom;
            }
        }

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        private void scrollBarX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void scrollBarY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
