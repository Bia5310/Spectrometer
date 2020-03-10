using Accord.Video.FFMPEG;
using AVT.VmbAPINET;
using NLog;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using VimbaCameraNET;
using static AO_Lib.AO_Devices;
using static Spectrometer.Polarisation;

namespace Spectrometer
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Структура содержит информацию о пути для сохранения изображений, количестве кадров и наборе тэгов
        /// </summary>
        private struct CaptureInfo
        {
            public string SavePath;

            public decimal StartWL;
            public decimal EndWL;
            public decimal StepWL;

            public decimal StartWN;
            public decimal EndWN;
            public decimal StepWN;

            public TiffWorker TagsData;
        }

        #region переменные

        Logger logger = null;

        //Polarisation
        private PolarData polarDataSelector = PolarData.Idle;

        private readonly object lockerBitmap = new object();
        private Bitmap bitmap = null;

        int pixelvars = 4096; //2^pixelDeph

        private Units.UnitsEnum currentUnit;

        private readonly object zoomlock = new object();
        double zoom_selected_item = 1d;

        /// <summary> Экземпляр класса VimbaCamera позволяет управлять подключенной и с его помощью открытой камерой на основе Vimba </summary>
        VimbaCamera vimbaCamera = new VimbaCamera();
        /// <summary>Аккусто-оптический фильтр</summary>
        AO_Filter AOFilter = null;


        VideoFileWriter videoFileWriter = null;
        DateTime videocapStartTime = new DateTime(0);
        bool videoCapturing = false;
        private readonly object lockerVideoWriter = new object();

        //
        int bitmapW = 0, bitmapH = 0;

        //данные по корректировке
        private double corrCurveExposure;
        private double corrMaxExposure;
        private double corrCurveMaxY;
        private double corrCurveMinY;
        private bool corrByExposure;
        private bool corrByGain;
        private bool useCorrection;
        private bool corrCurveLoaded = false;
        List<DataPoint> corrCurveList = new List<DataPoint>();

        /// <summary>
        /// Protector for m_ImageInUse. Украдено из VimbaHelper
        /// </summary>
        private readonly object m_ImageInUseSyncLock = new object();

        /// <summary>
        /// Signal of picture box that image is used
        /// </summary>
        private bool m_ImageInUse = false;

        /// <summary>
        /// Gets or sets a value indicating whether an image is displayed or not
        /// </summary>
        private bool ImageInUse
        {
            get
            {
                lock (m_ImageInUseSyncLock)
                {
                    return m_ImageInUse;
                }
            }
            set
            {
                lock (m_ImageInUseSyncLock)
                {
                    m_ImageInUse = value;
                }
            }
        }

        private ushort[] sbuffer = new ushort[0];
        private int[] hist = new int[4096];
        private bool histDrawing = false;
        private readonly object lockerHistDrawing = new object();

        DateTime timeCounterLimitFPS = DateTime.Now;
        DateTime timeCounterHist = DateTime.Now;

        private bool sequenceCapturing = false;
        DateTime startTime = DateTime.Now;

        DateTime start_time;
        DateTime end_time;
        List<DateTime> timesTrReList = new List<DateTime>();
        List<DateTime> timesTrList = new List<DateTime>();

        ///<summary>содержит информацию о кадрах для сохранения их в Tiff</summary>
        private List<TiffWorker> tiffWorkers = new List<TiffWorker>();
        double wavelength = 0;
        double startWavelength = 0;
        double endWavelength = 0;
        double stepWavelength = 0;

        double wavenumber = 0;
        double startWavenumber = 0;
        double endWavenumber = 0;
        double stepWavenumber = 0;

        int frameNumber = 0;
        int framesCount = 0;

        bool canScroll = true;

        private static readonly object _logLocker = new object();

        bool subscrided = false;


        List<DateTime> timestamps = new List<DateTime>();
        List<TimeSpan> timespans = new List<TimeSpan>();

        public bool flagToStopCaptureCycle = false;
        public bool flagMultispectralCaptureEnded = false;

        private struct CapPoint
        {
            public decimal wavelength;
            public decimal time;
        }
        private List<CapPoint> capPoints = new List<CapPoint>();

        bool einmalflag = false;

        #endregion

        private void AutoconnectToFirstCamera()
        {
            try
            {
                if (VimbaCamera.Cameras.Count > 0)
                {
                    vimbaCamera.CloseCamera();
                    UnsubscribeEvents();

                    vimbaCamera.OpenCamera(VimbaCamera.Cameras[0].Id);
                    CameraControlsGroup(vimbaCamera.IsOpened);
                    pixelvars = (int)Math.Pow(2, VimbaCamera.PixelFormatToBits(vimbaCamera.PixelFormat));

                    if (vimbaCamera.IsOpened)
                    {
                        try
                        {
                            SetupFeaturesControls();
                            SubscribeEvents();
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.ToString());
                        }

                        vimbaCamera.RestoreFullROI(true);
                        vimbaCamera.StartContiniousAsyncAccusition();
                        //FillTriggerFeatures();

                        logger.Info("Подключена камера " + vimbaCamera.ToString());
                    }
                }
            }
            catch (Exception exc)
            {
                LogWrite(exc.Message);
                logger.Error(exc, "Autoconnect to camera failed");
            }
        }

        private void DrawHistogram()
        {
            /*if (pixelvars != hist.Length)
                Array.Resize(ref hist, pixelvars);

            Array.Clear(hist, 0, hist.Length);
            for (int i = 0; i < sbuffer.Length; i++)
            {
                hist[sbuffer[i]]++;
            }

            BeginInvoke(new Action(() => {
                chart_histogram.Series[0].Points.Clear();
                for (int i = 0; i < hist.Length; i++)
                {
                    chart_histogram.Series[0].Points.AddXY(i, hist[i]);
                }

                groupBox_Histogram.Text = "Гистограмма " + "(пикселей в насыщении: " + hist[hist.Length - 1].ToString() + ")";

                timeCounterHist = DateTime.Now;

                lock (lockerHistDrawing)
                    histDrawing = false;
            }));*/
        }

        private void LogWrite(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Action act = () => LogWrite(message);
                Dispatcher.Invoke(act);
                return;
            }
            else
            {
                lock (_logLocker)
                {
                    int index = listBoxLog.Items.Add(message);
                }
            }
        }

        private void SetupFeaturesControls()
        {
            /*if (vimbaCamera != null && vimbaCamera.IsOpened)
            {
                if (vimbaCamera.Exposure.IsAvailable)
                {
                    trackBarAdvancedExposure.SetValueMinMax(vimbaCamera.Exposure.Value, vimbaCamera.Exposure.MinValue, vimbaCamera.Exposure.MaxValue);
                    trackBarAdvancedExposure.Increment = vimbaCamera.Exposure.ValueIncrement;
                    trackBarAdvancedExposure.UseIncrement = vimbaCamera.Exposure.HasValueIncrement;
                    checkBoxAutoExposure.Checked = vimbaCamera.Exposure.Auto;
                }
                trackBarAdvancedExposure.Enabled = vimbaCamera.Exposure.IsAvailable && !vimbaCamera.Exposure.IsReadonly && !vimbaCamera.Exposure.Auto;
                checkBoxAutoExposure.Enabled = vimbaCamera.Exposure.IsAvailable;

                if (vimbaCamera.Gain.IsAvailable)
                {
                    trackBarAdvancedGain.SetValueMinMax(vimbaCamera.Gain.Value, vimbaCamera.Gain.MinValue, vimbaCamera.Gain.MaxValue);
                    trackBarAdvancedGain.Increment = vimbaCamera.Gain.ValueIncrement;
                    trackBarAdvancedGain.UseIncrement = vimbaCamera.Gain.HasValueIncrement;
                    //trackBarAdvancedGain.TickStyle = vimbaCamera.Gain.HasValueIncrement ? TickStyle.BottomRight : TickStyle.None ;
                    checkBoxAutoGain.Checked = vimbaCamera.Gain.Auto;
                }
                trackBarAdvancedGain.Enabled = vimbaCamera.Gain.IsAvailable && !vimbaCamera.Gain.IsReadonly && !vimbaCamera.Gain.Auto;
                checkBoxAutoGain.Enabled = vimbaCamera.Gain.IsAvailable;

                if (vimbaCamera.Gamma.IsAvailable)
                {
                    trackBarAdvancedGamma.SetValueMinMax(vimbaCamera.Gamma.Value, vimbaCamera.Gamma.MinValue, vimbaCamera.Gamma.MaxValue);
                }
                trackBarAdvancedGamma.Enabled = vimbaCamera.Gamma.IsAvailable;

                if (vimbaCamera.Width.IsAvailable)
                {
                    trackBarAdvancedWidth.SetValueMinMax(vimbaCamera.Width.Value, vimbaCamera.Width.MinValue, vimbaCamera.Width.MaxValue);
                    trackBarAdvancedWidth.Increment = vimbaCamera.Width.ValueIncrement;
                    trackBarAdvancedWidth.UseIncrement = true;
                }
                trackBarAdvancedWidth.Enabled = vimbaCamera.Width.IsAvailable;

                if (vimbaCamera.Height.IsAvailable)
                {
                    trackBarAdvancedHeight.SetValueMinMax(vimbaCamera.Height.Value, vimbaCamera.Height.MinValue, vimbaCamera.Height.MaxValue);
                    trackBarAdvancedHeight.Increment = vimbaCamera.Height.ValueIncrement;
                    trackBarAdvancedHeight.UseIncrement = true;
                }
                trackBarAdvancedHeight.Enabled = vimbaCamera.Height.IsAvailable;

                if (vimbaCamera.OffsetX.IsAvailable)
                {
                    trackBarAdvancedOffsetX.SetValueMinMax(vimbaCamera.OffsetX.Value, vimbaCamera.OffsetX.MinValue, vimbaCamera.OffsetX.MaxValue);
                    trackBarAdvancedOffsetX.Increment = vimbaCamera.OffsetX.ValueIncrement;
                    trackBarAdvancedOffsetX.UseIncrement = true;
                }
                trackBarAdvancedOffsetX.Enabled = vimbaCamera.OffsetX.IsAvailable;

                if (vimbaCamera.OffsetY.IsAvailable)
                {
                    trackBarAdvancedOffsetY.SetValueMinMax(vimbaCamera.OffsetY.Value, vimbaCamera.OffsetY.MinValue, vimbaCamera.OffsetY.MaxValue);
                    trackBarAdvancedOffsetY.Increment = vimbaCamera.OffsetY.ValueIncrement;
                    trackBarAdvancedOffsetY.UseIncrement = true;
                }
                trackBarAdvancedOffsetY.Enabled = vimbaCamera.OffsetY.IsAvailable;

                if (vimbaCamera.BinningX.IsAvailable)
                {
                    trackBarAdvancedBinningX.SetValueMinMax(vimbaCamera.BinningX.Value, vimbaCamera.BinningX.MinValue, vimbaCamera.BinningX.MaxValue);
                    trackBarAdvancedBinningX.Increment = vimbaCamera.BinningX.ValueIncrement;
                    trackBarAdvancedBinningX.UseIncrement = true;
                }
                trackBarAdvancedBinningX.Enabled = vimbaCamera.BinningX.IsAvailable;

                if (vimbaCamera.BinningY.IsAvailable)
                {
                    trackBarAdvancedBinningY.SetValueMinMax(vimbaCamera.BinningY.Value, vimbaCamera.BinningY.MinValue, vimbaCamera.BinningY.MaxValue);
                    trackBarAdvancedBinningY.Increment = vimbaCamera.BinningY.ValueIncrement;
                    trackBarAdvancedBinningY.UseIncrement = true;
                }
                trackBarAdvancedBinningY.Enabled = vimbaCamera.BinningY.IsAvailable;

                if (vimbaCamera.StatFrameRate.IsAvailable)
                    toolStripLabelFrameRate.Text = "Frame Rate: " + vimbaCamera.StatFrameRate.Value.ToString("F3");
                else
                    toolStripLabelFrameRate.Text = "Frame Rate: --";
            }*/
        }

        private void SubscribeEvents()
        {
            /*if (subscrided)
                return;

            subscrided = true;
            if (vimbaCamera.Exposure.IsAvailable)
            {
                vimbaCamera.Exposure.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.Gain.IsAvailable)
            {
                vimbaCamera.Gain.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.Gamma.IsAvailable)
            {
                vimbaCamera.Gamma.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.Width.IsAvailable)
            {
                vimbaCamera.Width.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.Height.IsAvailable)
            {
                vimbaCamera.Height.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.OffsetX.IsAvailable)
            {
                vimbaCamera.OffsetX.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.OffsetY.IsAvailable)
            {
                vimbaCamera.OffsetY.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.BinningX.IsAvailable)
            {
                vimbaCamera.BinningX.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.BinningY.IsAvailable)
            {
                vimbaCamera.BinningY.OnFeatureChanged += FeatureChanged;
            }
            if (vimbaCamera.StatFrameRate.IsAvailable)
                vimbaCamera.StatFrameRate.OnFeatureChanged += FeatureChanged;*/
        }

        private void UnsubscribeEvents()
        {
            if (!subscrided)
                return;

            subscrided = false;
            if (vimbaCamera.Exposure.IsAvailable)
            {
                vimbaCamera.Exposure.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.Gain.IsAvailable)
            {
                vimbaCamera.Gain.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.Gamma.IsAvailable)
            {
                vimbaCamera.Gamma.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.Width.IsAvailable)
            {
                vimbaCamera.Width.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.Height.IsAvailable)
            {
                vimbaCamera.Height.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.OffsetX.IsAvailable)
            {
                vimbaCamera.OffsetX.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.OffsetY.IsAvailable)
            {
                vimbaCamera.OffsetY.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.BinningX.IsAvailable)
            {
                vimbaCamera.BinningX.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.BinningY.IsAvailable)
            {
                vimbaCamera.BinningY.OnFeatureChanged -= FeatureChanged;
            }
            if (vimbaCamera.StatFrameRate.IsAvailable)
                vimbaCamera.StatFrameRate.OnFeatureChanged -= FeatureChanged;
        }

        private void FeatureChanged(VimbaCamera.FeatureType featureType)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new VimbaCamera.CameraFeature.OnFeatureChangedHandler(FeatureChanged), featureType);
                return;
            }
            else
            {
                switch (featureType)
                {
                    case VimbaCamera.FeatureType.Exposure:
                        //trackBarAdvancedExposure.SetValueMinMax(vimbaCamera.Exposure.Value, vimbaCamera.Exposure.MinValue, vimbaCamera.Exposure.MaxValue);
                        ExposureControl.MinValue = vimbaCamera.Exposure.MinValue;
                        ExposureControl.MaxValue = vimbaCamera.Exposure.MaxValue;
                        ExposureControl.Value = vimbaCamera.Exposure.Value;
                        break;
                    case VimbaCamera.FeatureType.Gain:
                        //trackBarAdvancedGain.SetValueMinMax(vimbaCamera.Gain.Value, vimbaCamera.Gain.MinValue, vimbaCamera.Gain.MaxValue);
                        GainControl.MinValue = vimbaCamera.Gain.MinValue;
                        GainControl.MaxValue = vimbaCamera.Gain.MaxValue;
                        GainControl.Value = vimbaCamera.Gain.Value;
                        break;
                    case VimbaCamera.FeatureType.Gamma:
                        //trackBarAdvancedGamma.SetValueMinMax(vimbaCamera.Gamma.Value, vimbaCamera.Gamma.MinValue, vimbaCamera.Gamma.MaxValue);
                        GammaControl.MinValue = vimbaCamera.Gamma.MinValue;
                        GammaControl.MaxValue = vimbaCamera.Gamma.MaxValue;
                        GammaControl.Value = vimbaCamera.Gamma.Value;
                        break;
                    case VimbaCamera.FeatureType.Width:
                        //trackBarAdvancedWidth.SetValueMinMax(vimbaCamera.Width.Value, vimbaCamera.Width.MinValue, vimbaCamera.Width.MaxValue);
                        ROI_Control.ROIWidth = vimbaCamera.Width.Value;
                        ROI_Control.ROIWidthMin = vimbaCamera.Width.MinValue;
                        ROI_Control.ROIWidthMax = vimbaCamera.Width.MaxValue;
                        //trackBarAdvancedWidth.Enabled = !vimbaCamera.Width.IsReadonly;
                        break;
                    case VimbaCamera.FeatureType.Height:
                        //trackBarAdvancedHeight.SetValueMinMax(vimbaCamera.Height.Value, vimbaCamera.Height.MinValue, vimbaCamera.Height.MaxValue);
                        ROI_Control.ROIHeight = vimbaCamera.Height.Value;
                        ROI_Control.ROIHeightMin = vimbaCamera.Height.MinValue;
                        ROI_Control.ROIHeightMax = vimbaCamera.Height.MaxValue;
                        //trackBarAdvancedHeight.Enabled = !vimbaCamera.Height.IsReadonly;
                        break;
                    case VimbaCamera.FeatureType.OffsetX:
                        //trackBarAdvancedOffsetX.SetValueMinMax(vimbaCamera.OffsetX.Value, vimbaCamera.OffsetX.MinValue, vimbaCamera.OffsetX.MaxValue);
                        ROI_Control.ROIOffsetX = vimbaCamera.OffsetX.Value;
                        ROI_Control.ROIOffsetXMin = vimbaCamera.OffsetX.MinValue;
                        ROI_Control.ROIOffsetXMax = vimbaCamera.OffsetX.MaxValue;
                        //trackBarAdvancedOffsetX.Enabled = !vimbaCamera.OffsetX.IsReadonly;
                        break;
                    case VimbaCamera.FeatureType.OffsetY:
                        //trackBarAdvancedOffsetY.SetValueMinMax(vimbaCamera.OffsetY.Value, vimbaCamera.OffsetY.MinValue, vimbaCamera.OffsetY.MaxValue);
                        ROI_Control.ROIOffsetY = vimbaCamera.OffsetY.Value;
                        ROI_Control.ROIOffsetYMin = vimbaCamera.OffsetY.MinValue;
                        ROI_Control.ROIOffsetYMax = vimbaCamera.OffsetY.MaxValue;
                        //trackBarAdvancedOffsetY.Enabled = !vimbaCamera.OffsetY.IsReadonly;
                        break;
                    case VimbaCamera.FeatureType.BinningX:
                        //trackBarAdvancedBinningX.SetValueMinMax(vimbaCamera.BinningX.Value, vimbaCamera.BinningX.MinValue, vimbaCamera.BinningX.MaxValue);

                        break;
                    case VimbaCamera.FeatureType.BinningY:
                        //trackBarAdvancedBinningY.SetValueMinMax(vimbaCamera.BinningY.Value, vimbaCamera.BinningY.MinValue, vimbaCamera.BinningY.MaxValue);
                        break;
                    case VimbaCamera.FeatureType.FrameRate:
                        //toolStripLabelFrameRate.Text = "Frame Rate: " + vimbaCamera.StatFrameRate.Value.ToString("F3");
                        break;
                }
            }
        }

        private void CameraControlsGroup(bool active)
        {
            /*groupBoxCamera.Enabled = active;
            setupROIToolStripMenuItem.Enabled = active;
            restoreROIToolStripMenuItem.Enabled = active;*/
        }

        

        /// <summary>
        /// Обработчик события приема кадра
        /// </summary>
        /// <param name="frame"></param>
        private void Camera_FrameReceived(VimbaCamera vCamera, Frame frame)
        {
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Camera_FrameReceived(vCamera, frame);
                }));
                return;
            }
            else
            {
                if (viewportControl1.Image == null || (viewportControl1.Image.Width != frame.Width &&
                        viewportControl1.Image.Height != frame.Height))
                {
                    //viewportControl1.WriteableBitmap = null;//image.Source = null;
                    viewportControl1.Image = new System.Windows.Media.Imaging.WriteableBitmap(
                        (int)frame.Width, (int)frame.Height, 96, 96, PixelFormats.Bgr32, null);
                }

                try
                {
                    viewportControl1.Image.Lock();

                    unsafe
                    {
                        IntPtr intPtr = viewportControl1.Image.BackBuffer;
                        byte* dest = (byte*)intPtr;
                        byte value = (byte)(DateTime.Now.Second * 4.22d);

                        for (int i = 0; i < frame.BufferSize; i += 2)
                        {
                            value = (byte) (BitConverter.ToInt16(frame.Buffer, i)*255/4095);
                            dest[0] = dest[1] = dest[2] = value;
                            dest += 4;
                        }
                    }
                    viewportControl1.Image.AddDirtyRect(new Int32Rect(0, 0, (int)frame.Width, (int)frame.Height));
                }
                catch (Exception) { }
                finally
                {
                    viewportControl1.Image.Unlock();
                }
                //viewportControl1.Image
                //viewportControl1.WriteableBitmap.WritePixels(new Int32Rect(0,0,(int)frame.Width,(int)frame.Height), frame.Buffer, (int)frame.Width*2, 0);
            }
            vCamera.Camera.QueueFrame(frame);

            /*try
            {
                bool miss_this_frame = GlobalSettings.FPS_Limiting && DateTime.Now.Subtract(timeCounterLimitFPS).TotalMilliseconds < 1000d / GlobalSettings.FPS_Limit; //ограничение по фпс
                if (!viewportControl1.IsImageInUse() && !miss_this_frame)
                {
                    lock (lockerBitmap)
                    {
                        if (frame.ReceiveStatus == VmbFrameStatusType.VmbFrameStatusComplete)
                        {
                            if (bitmap != null)
                            {
                                bitmap.Dispose();
                                bitmap = null;
                            }

                            //Блок обработкт изображения
                            switch (polarDataSelector)
                            {
                                case PolarData.I0:
                                case PolarData.I135:
                                case PolarData.I45:
                                case PolarData.I90:
                                    bitmap = Polarisation.ExtractPolarisationBitmap(frame, polarDataSelector, VimbaCamera.PixelFormatToBits(vimbaCamera.PixelFormat));
                                    break;
                                case PolarData.AOLP:
                                    bitmap = Polarisation.ComputeAOLPBitmap(frame, colorbarControl.Colorbar);//DoublesToBitmap(ComputeAOLP(frame), (int)frame.Width/2, (int)frame.Height/2);
                                    break;
                                case PolarData.DOLP:
                                    bitmap = Polarisation.ComputeDOLPBitmap(frame, colorbarControl.Colorbar);
                                    break;
                                default:
                                    bitmap = new Bitmap((int)frame.Width, (int)frame.Height, PixelFormat.Format32bppRgb);//GetBitmap8FromFrame(frame); //null;//new Bitmap((int)frame.Width, (int)frame.Height, PixelFormat.Format24bppRgb);
                                    frame.Fill(ref bitmap);
                                    break;
                            }

                            //Нужны при записи видео. Необходимо задать сразу после изменений в bitmap
                            bitmapW = bitmap.Width;
                            bitmapH = bitmap.Height;

                            bool hist_drawing = false;
                            lock (lockerHistDrawing)
                                hist_drawing = histDrawing;

                            //Draw histogram
                            if (DateTime.Now.Subtract(timeCounterHist).TotalMilliseconds > 100 && !hideHistogram && !hist_drawing)
                            {
                                lock (lockerHistDrawing)
                                    histDrawing = true;

                                int length = (int)(frame.Width * frame.Height);
                                if (sbuffer.Length != length)
                                    Array.Resize(ref sbuffer, length);

                                Buffer.BlockCopy(frame.Buffer, 0, sbuffer, 0, length * 2);

                                Parallel.Invoke(DrawHistogram);
                            }

                            //Writing bitmap in video
                            try
                            {
                                lock (lockerVideoWriter)
                                    if (videoCapturing)
                                    {
                                        TimeSpan timeSpan = DateTime.Now.Subtract(videocapStartTime);
                                        lock (lockerBitmap)
                                            videoFileWriter.WriteVideoFrame(bitmap, timeSpan);
                                    }
                            }
                            catch (Exception vex)
                            {
                                StopVideoCapture();
                                BeginInvoke(new Action(() => {
                                    StartVideoToolStripMenuItem.Enabled = true;
                                    StopVideoToolStripMenuItem.Enabled = false;
                                    LogWrite(vex.Message);
                                }));
                            }

                            lock (lockerBitmap)
                            {
                                viewportControl1.Image = bitmap;
                            }
                        }
                    }

                    timeCounterLimitFPS = DateTime.Now;
                }
            }
            catch (Exception exc)
            {
                LogWrite(exc.Message);
                logger.Error(exc, "FrameReceived error");
            }
            finally
            {
                try
                {
                    vCamera.Camera.QueueFrame(frame);
                }
                catch (Exception exc)
                {
                    LogWrite(exc.Message);
                    logger.Error(exc, "FrameReceived QueueFrame");
                }
            }*/
        }
    }
}
