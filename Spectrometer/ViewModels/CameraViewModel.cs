using AVT.VmbAPINET;
using Spectrometer.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VimbaCameraNET;

namespace Spectrometer.ViewModels
{
    public class CameraViewModel : BaseViewModel
    {
        #region Constructor

        public CameraViewModel(ApplicationViewModel applicationViewModel)
        {
            this.applicationViewModel = applicationViewModel;

            connectCommand = new Command(ConnectMethod);
            toggleLiveCommand = new Command(ToggleLiveMethod);
            
            Camera = new VimbaCamera();
            Camera.PropertyChanged += Camera_PropertyChanged;
        }

        private void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("");
            //OnPropertyChanged("Camera");
        }

        #endregion

        #region Events, Delegates



        #endregion

        #region Fields, variables

        private double alpha = Math.Log(3d) / 9d;

        private ApplicationViewModel applicationViewModel;

        private VimbaCamera m_camera = null;
        public VimbaCamera Camera
        {
            get => m_camera;
            set
            {
                m_camera = value;
                OnPropertyChanged();
            }
        }

        private WriteableBitmap image = null;
        public WriteableBitmap Image
        {
            get => image;
            set
            {
                image = value;
                OnPropertyChanged();
            }
        }

        private int zoom_tick = 0;
        public int ZoomTick
        {
            get => zoom_tick;
            set
            {
                zoom_tick = value;
                Zoom = ZoomMethod(zoom_tick);
                
                OnPropertyChanged();
            }
        }

        private double zoom = 1d;
        public double Zoom
        {
            get => zoom;
            private set
            {
                zoom = value;
                OnPropertyChanged();
                OnPropertyChanged("WidthZoomed");
                OnPropertyChanged("HeightZoomed");
            }
        }

        public double WidthZoomed
        {
            get
            {
                if (image != null)
                    return image.Width * zoom;
                else
                    return 0;
            }
        }

        public double HeightZoomed
        {
            get
            {
                if (image != null)
                    return image.Height * zoom;
                else
                    return 0;
            }
        }

        #region Commands
        private Command connectCommand = null;
        private Command toggleLiveCommand = null;
        private Command saveFrameCommand = null;
        private Command startVideoCommand = null;
        private Command stopVideoCommand = null;

        public Command ConnectCommand { get => connectCommand; }
        public Command ToggleLiveCommand { get => toggleLiveCommand; }
        public Command SaveFrameCommand { get => saveFrameCommand; }
        public Command StartVideoCommand { get => startVideoCommand; }
        public Command StopVideoCommand { get => stopVideoCommand; }

        #endregion

        private bool frameBusy = false;

        private byte[] lastFrameBuffer = null;
        private int frameWidth = 0;
        private int frameHeight = 0;
        private VmbPixelFormatType lastPixelFormatType;

        private int offsetX = 0;
        public double OffsetXZoomed
        {
            get => offsetX*zoom;
        }

        public double OffsetYZoomed
        {
            get => offsetY*zoom;
        }

        private int offsetY = 0;

        #endregion

        #region Functions

        private void StartLiveMethod()
        {
            try
            {
                if(Camera.IsOpened)
                {
                    Camera.StartContiniousAsyncAccusition();
                }
            }
            catch (Exception) { }
        }

        private void StopLiveMethod()
        {
            try
            {
                if (Camera.IsOpened)
                {
                    Camera.StopContiniousAsyncAccusition();
                }
            }
            catch (Exception) { }
        }

        public void ToggleLiveMethod()
        {
            try
            {
                if(Camera.IsOpened)
                {
                    if (Camera.Accusition)
                    {
                        Camera.StopContiniousAsyncAccusition();
                    }
                    else
                    {
                        Camera.StartContiniousAsyncAccusition();
                    }
                }
            }
            catch (Exception) { }
        }

        private void ConnectMethod()
        {
            try
            {
                if(Camera.IsOpened)
                {
                    Camera.CloseCamera();
                }

                if(VimbaCamera.Cameras.Count == 0)
                {
                    MessageBox.Show("Нет подключенных камер");
                    return;
                }
                else if(VimbaCamera.Cameras.Count == 1)
                {
                    Camera.OpenCamera(VimbaCamera.Cameras[0].Id);
                    Camera.Camera.OnFrameReceived += Camera_OnFrameReceived;
                }
                else if(VimbaCamera.Cameras.Count > 1)
                {
                    //выбор подключенной камеры из списка
                }

                StartLiveMethod();
            }
            catch (Exception) { }
        }

        private void RefillImage()
        {
            //fillData

            if (image == null || (image.Width != frameWidth && image.Height != frameHeight))
            {
                image = new WriteableBitmap(frameWidth, frameHeight, 96, 96, PixelFormats.Bgr32, null);
            }

            image.Lock();

            unsafe
            {
                IntPtr ptr = image.BackBuffer;
                fixed (byte* framePtr = lastFrameBuffer)
                {
                    switch (lastPixelFormatType)
                    {
                        case VmbPixelFormatType.VmbPixelFormatMono8:
                            FillBufferMono8to16(framePtr, (byte*)ptr, frameWidth, frameHeight, 8);
                            break;
                        case VmbPixelFormatType.VmbPixelFormatMono10:
                            FillBufferMono8to16(framePtr, (byte*)ptr, frameWidth, frameHeight, 10);
                            break;
                        case VmbPixelFormatType.VmbPixelFormatMono12:
                            FillBufferMono8to16(framePtr, (byte*)ptr, frameWidth, frameHeight, 12);
                            break;
                        case VmbPixelFormatType.VmbPixelFormatMono14:
                            FillBufferMono8to16(framePtr, (byte*)ptr, frameWidth, frameHeight, 14);
                            break;
                        case VmbPixelFormatType.VmbPixelFormatMono16:
                            FillBufferMono8to16(framePtr, (byte*)ptr, frameWidth, frameHeight, 16);
                            break;
                    }
                }

                image.AddDirtyRect(new Int32Rect(0, 0, (int)image.Width, (int)image.Height));
                image.Unlock();

                OnPropertyChanged("Image");
                OnPropertyChanged("WidthZoomed");
                OnPropertyChanged("HeightZoomed");
            }
        }

        private void Camera_OnFrameReceived(Frame frame)
        {
            try
            {
                if (!frameBusy && frame.ReceiveStatus == VmbFrameStatusType.VmbFrameStatusComplete)
                {
                    frameBusy = true;

                    if (lastFrameBuffer == null || lastFrameBuffer.Length != frame.Buffer.Length)
                    {
                        lastFrameBuffer = new byte[frame.Buffer.Length];
                    }
                    Array.Copy(frame.Buffer, 0, lastFrameBuffer, 0, lastFrameBuffer.Length);
                    
                    //lastFrameBuffer = (byte[])frame.Buffer.Clone();
                    /*for(int i = 0; i < frame.Buffer.Length; i++)
                    {
                        lastFrameBuffer[i] = frame.Buffer[i];
                    }*/

                    lastPixelFormatType = frame.PixelFormat;
                    frameWidth = Convert.ToInt32(frame.Width);
                    frameHeight = Convert.ToInt32(frame.Height);
                    offsetX = Convert.ToInt32(frame.OffsetX);
                    offsetY = Convert.ToInt32(frame.OffsetY);

                    Camera.Camera.QueueFrame(frame);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RefillImage();
                        frameBusy = false;
                    }));

                }
                else
                {
                    Camera.Camera.QueueFrame(frame);
                }
            }
            catch (Exception ex)
            {
                frameBusy = false;
            }
            finally { }
        }

        static unsafe void FillBufferMono8to16(byte* srcPtr, byte* destPtr, int w, int h, int bpp)
        {
            byte value = 0;
            
            if(bpp == 8)
            {
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        value = srcPtr[0];

                        destPtr[0] = value;
                        destPtr[1] = value;
                        destPtr[2] = value;

                        destPtr += 4;
                        srcPtr++;
                    }
                }
            }
            else
            {
                int temp = 0;
                int max = (1 << bpp) - 1;
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        temp = (srcPtr[0] + srcPtr[1] * 256)*255/max;
                        value = Convert.ToByte(temp);

                        destPtr[0] = value;
                        destPtr[1] = value;
                        destPtr[2] = value;

                        destPtr += 4;
                        srcPtr += 2;
                    }
                }
            }
        }

        public void IninDafaultImage()
        {
            if (image == null || image.Width != 100 && image.Height != 300)
            {
                image = new WriteableBitmap(100, 300, 96, 96, PixelFormats.Bgr32, null);
            }
            image.Lock();
            byte value = 255;//(byte)(DateTime.Now.Second * 255 / 60);

            unsafe
            {
                IntPtr ptr = image.BackBuffer;
                for (int i = 0; i < 300; i++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        ((byte*)ptr)[0] = value;
                        ((byte*)ptr)[1] = value;
                        ((byte*)ptr)[2] = value;
                        ptr += 4;
                    }
                }
            }
            image.AddDirtyRect(new Int32Rect(0, 0, (int)image.Width, (int)image.Height));
            image.Unlock();
            
            OnPropertyChanged("Image");
        }

        private double ZoomMethod(int x)
        {
            return Math.Exp(alpha*x);
        }

        #endregion
    }
}
