using AO_Lib;
using AVT.VmbAPINET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using VimbaCameraNET;
using static AO_Lib.AO_Devices;
using static VimbaCameraNET.VimbaCamera;

namespace Spectrometer
{
    class ApplicationViewModel : INotifyPropertyChanged
    {
        private VimbaCamera _camera = null;
        private AO_Filter AOFilter = null;

        public ApplicationViewModel()
        {
            _camera = new VimbaCamera();
            _camera.FrameReceivedHandler = Camera_FrameReceived;
            VimbaCamera.OnCameraListChanged += VimbaCamera_OnCameraListChanged;
        }

        ~ApplicationViewModel()
        {
            DisconnectCamera();
            _camera = null;
        }

        public WriteableBitmap writeableBitmap = null;
        public WriteableBitmap WriteableBitmap
        {
            get => writeableBitmap;
        }

        private void Camera_FrameReceived(VimbaCamera vCamera, Frame frame)
        {
            
        }

        public ClickCommand TestImageTriggerCommand
        {
            get
            {
                return new ClickCommand(o =>
                {
                    
                    if(writeableBitmap == null || writeableBitmap.Width != 100 && writeableBitmap.Height != 300)
                    {
                        writeableBitmap = new WriteableBitmap(100, 300, 96, 96, PixelFormats.Bgr32, null);
                    }
                    writeableBitmap.Lock();
                    byte value = (byte)(DateTime.Now.Second * 255 / 60);
                    
                    unsafe
                    {
                        IntPtr ptr = writeableBitmap.BackBuffer;
                        for(int i = 0; i < 300; i++)
                        {
                            for(int j = 0; j < 100; j++)
                            {
                                ((byte*)ptr)[0] = value;
                                ((byte*)ptr)[1] = value;
                                ((byte*)ptr)[2] = value;
                                ptr += 4;
                            }
                        }
                    }
                    writeableBitmap.AddDirtyRect(new Int32Rect(0,0,(int)writeableBitmap.Width,(int)writeableBitmap.Height));
                    writeableBitmap.Unlock();

                    OnPropertyChanged("WriteableBitmap");
                });
            }
        }

        #region CameraRegion

        private void VimbaCamera_OnCameraListChanged(VmbUpdateTriggerType reason)
        {
            OnPropertyChanged("ConnectedCameras");
        }

        public CameraCollection ConnectedCameras
        {
            get => VimbaCamera.Cameras;
            set { }
        }

        private ClickCommand connectToCameraCommand = null;

        public ClickCommand ConnectToCameraCommand
        {
            get
            {
                return connectToCameraCommand ?? new ClickCommand(cameraInfo =>
                {
                    try
                    {
                        CameraCollection cameras = VimbaCamera.Cameras;
                        if (cameras.Count == 0)
                        {
                            return;
                        }

                        string targetID = "";

                        if (cameras.Count == 1)
                        {
                            targetID = cameras[0].Id;
                        }

                        if (cameras.Count > 1)
                        {
                            //Отобразим диалог и предложим выбрать камеру. Временно просто подкл. 1-ую камеру
                            targetID = cameras[0].Id;
                        }

                        ConnectToCamera(targetID);
                    }
                    catch (Exception) { }
                });
            }
        }

        private ClickCommand disconnectCameraCommand = null;
        public ClickCommand DisconnectCameraCommand
        {
            get
            {
                return disconnectCameraCommand ?? new ClickCommand((cameraInfo =>
                {
                    DisconnectCamera();
                }));
            }
        }

        public void DisconnectCamera()
        {
            _camera.StopContiniousAsyncAccusition();
            _camera.CloseCamera();
            UnsubscribeEvents();
            _camera.OnCameraConnectionStateChanged -= _camera_OnCameraConnectionStateChanged;
        }

        public void ConnectToCamera(string id)
        {
            try
            {
                if(_camera.IsOpened)
                {
                    DisconnectCamera();
                }

                _camera.OpenCamera(VimbaCamera.Cameras[0].Id);

                if (_camera.IsOpened)
                {
                    try
                    {
                        _camera.OnCameraConnectionStateChanged += _camera_OnCameraConnectionStateChanged;

                        SubscribeEvents();
                        UpdateAllCameraFeatures();
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.ToString());
                    }

                    _camera.RestoreFullROI(true);
                    _camera.StartContiniousAsyncAccusition();

                    //logger.Info("Подключена камера " + vimbaCamera.ToString());
                }
            }
            catch(Exception ex)
            {

            }

        }

        private void _camera_OnCameraConnectionStateChanged(bool connected)
        {
            IsCameraOpened = connected;
        }

        private bool isCameraOpened = false;
        public bool IsCameraOpened
        {
            get
            {
                return isCameraOpened;
            }
            set
            {
                isCameraOpened = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public void SetFeatureValue(FeatureType featureType, dynamic value, [CallerMemberName]string featureName = "")
        {
            if (_camera != null)
            {
                CameraFeature f = _camera.GetFeature(featureType);
                if (!f.IsReadonly)
                {
                    f.Value = value;
                    OnPropertyChanged();
                }
            }
        }

        public dynamic GetFeatureValue(FeatureType featureType, dynamic defValue = null)
        {
            CameraFeature f = _camera.GetFeature(featureType);
            if (f.IsAvailable)
                return f.Value;
            return defValue ?? 0;
        }

        public dynamic GetFeatureValueMin(FeatureType featureType, dynamic defValue = null)
        {
            CameraFeature f = _camera.GetFeature(featureType);
            if (f.IsAvailable)
                return f.MinValue;
            return defValue ?? 0;
        }

        public dynamic GetFeatureValueMax(FeatureType featureType, dynamic defValue = null)
        {
            CameraFeature f = _camera.GetFeature(featureType);
            if (f.IsAvailable)
                return f.MaxValue;
            return defValue ?? 1;
        }

        public double Exposure
        {
            get => GetFeatureValue(FeatureType.Exposure);
            set => SetFeatureValue(FeatureType.Exposure, value);
        }

        public double ExposureMin
        {
            get => GetFeatureValueMin(FeatureType.Exposure);
        }

        public double ExposureMax
        {
            get => GetFeatureValueMax(FeatureType.Exposure);
        }

        public bool ExposureAuto
        {
            set
            {
                _camera.Exposure.Auto = value;
                OnPropertyChanged();
            }
            get
            {
                return _camera.Exposure.Auto;
            }
        }

        public double Gain
        {
            get => GetFeatureValue(FeatureType.Gain);
            set => SetFeatureValue(FeatureType.Gain, value);
        }

        public double GainMin
        {
            get => GetFeatureValueMin(FeatureType.Gain);
        }

        public double GainMax
        {
            get => GetFeatureValueMax(FeatureType.Gain);
        }

        public bool GainAuto
        {
            set
            {
                _camera.Gain.Auto = value;
                OnPropertyChanged();
            }
            get
            {
                return _camera.Gain.Auto;
            }
        }

        public double Gamma
        {
            get => GetFeatureValue(FeatureType.Gamma);
            set => SetFeatureValue(FeatureType.Gamma, value);
        }

        public double GammaMin
        {
            get => GetFeatureValueMin(FeatureType.Gamma);
        }

        public double GammaMax
        {
            get => GetFeatureValueMax(FeatureType.Gamma);
        }

        public double Width
        {
            get => GetFeatureValue(FeatureType.Width);
            set => SetFeatureValue(FeatureType.Width, value);
        }

        public double WidthMin
        {
            get => GetFeatureValueMin(FeatureType.Width);
        }

        public double WidthMax
        {
            get => GetFeatureValueMax(FeatureType.Width);
        }


        public double Height
        {
            get => GetFeatureValue(FeatureType.Height);
            set => SetFeatureValue(FeatureType.Height, value);
        }

        public double HeightMin
        {
            get => GetFeatureValueMin(FeatureType.Height);
        }

        public double HeightMax
        {
            get => GetFeatureValueMax(FeatureType.Height);
        }


        public double OffsetX
        {
            get => GetFeatureValue(FeatureType.OffsetX);
            set => SetFeatureValue(FeatureType.OffsetX, value);
        }

        public double OffsetXMin
        {
            get => GetFeatureValueMin(FeatureType.OffsetX);
        }

        public double OffsetXMax
        {
            get => GetFeatureValueMax(FeatureType.OffsetX);
        }


        public double OffsetY
        {
            get => GetFeatureValue(FeatureType.OffsetY);
            set => SetFeatureValue(FeatureType.OffsetY, value);
        }

        public double OffsetYMin
        {
            get => GetFeatureValueMin(FeatureType.OffsetY);
        }

        public double OffsetYMax
        {
            get => GetFeatureValueMax(FeatureType.OffsetY);
        }


        public double BinningX
        {
            get => GetFeatureValue(FeatureType.BinningX);
            set => SetFeatureValue(FeatureType.BinningX, value);
        }

        public double BinningXMin
        {
            get => GetFeatureValueMin(FeatureType.BinningX);
        }

        public double BinningXMax
        {
            get => GetFeatureValueMax(FeatureType.BinningX);
        }


        public double BinningY
        {
            get => GetFeatureValue(FeatureType.BinningY);
            set => SetFeatureValue(FeatureType.BinningY, value);
        }

        public double BinningYMin
        {
            get => GetFeatureValueMin(FeatureType.BinningY);
        }

        public double BinningYMax
        {
            get => GetFeatureValueMax(FeatureType.BinningY);
        }


        public double BlackLevel
        {
            get => GetFeatureValue(FeatureType.BlackLevel);
            set => SetFeatureValue(FeatureType.BlackLevel, value);
        }

        public double BlackLevelMin
        {
            get => GetFeatureValueMin(FeatureType.BlackLevel);
        }

        public double BlackLevelMax
        {
            get => GetFeatureValueMax(FeatureType.BlackLevel);
        }

        private bool subscribed = false;

        private void SubscribeEvents()
        {
            if (subscribed)
                return;
            
            subscribed = true;
            if (_camera.Exposure.IsAvailable)
            {
                _camera.Exposure.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.Gain.IsAvailable)
            {
                _camera.Gain.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.Gamma.IsAvailable)
            {
                _camera.Gamma.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.Width.IsAvailable)
            {
                _camera.Width.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.Height.IsAvailable)
            {
                _camera.Height.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.OffsetX.IsAvailable)
            {
                _camera.OffsetX.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.OffsetY.IsAvailable)
            {
                _camera.OffsetY.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.BinningX.IsAvailable)
            {
                _camera.BinningX.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.BinningY.IsAvailable)
            {
                _camera.BinningY.OnFeatureChanged += FeatureChanged;
            }
            if (_camera.StatFrameRate.IsAvailable)
                _camera.StatFrameRate.OnFeatureChanged += FeatureChanged;
            
        }

        public double FrameRate
        {
            get => GetFeatureValue(FeatureType.FrameRate, -1);
        }

        private void FeatureChanged(FeatureType featureType)
        {
            string featureName = Enum.GetName(typeof(FeatureType), featureType);
            switch (featureType)
            {
                case FeatureType.Exposure:
                case FeatureType.Gain:
                case FeatureType.Gamma:
                case FeatureType.BlackLevel:
                case FeatureType.Width:
                case FeatureType.Height:
                case FeatureType.OffsetX:
                case FeatureType.OffsetY:
                case FeatureType.BinningX:
                case FeatureType.BinningY:
                    OnPropertyChanged(featureName);
                    OnPropertyChanged(featureName+"Max");
                    OnPropertyChanged(featureName+"Min");
                    break;
                case FeatureType.FrameRate:
                    OnPropertyChanged("FrameRate");
                    break;
            }
            switch(featureType)
            {
                case FeatureType.Exposure:
                case FeatureType.Gain:
                    OnPropertyChanged(featureName+"Auto");
                    break;
            }
        }

        private void UpdateAllCameraFeatures()
        {
            string[] names = Enum.GetNames(typeof(FeatureType));
                
            for(int i = 0; i < names.Length; i++)
            {
                OnPropertyChanged(names[i]);
            }
        }

        private void UnsubscribeEvents()
        {
            if (!subscribed)
                return;

            subscribed = false;
            if (_camera.Exposure.IsAvailable)
            {
                _camera.Exposure.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.Gain.IsAvailable)
            {
                _camera.Gain.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.Gamma.IsAvailable)
            {
                _camera.Gamma.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.Width.IsAvailable)
            {
                _camera.Width.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.Height.IsAvailable)
            {
                _camera.Height.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.OffsetX.IsAvailable)
            {
                _camera.OffsetX.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.OffsetY.IsAvailable)
            {
                _camera.OffsetY.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.BinningX.IsAvailable)
            {
                _camera.BinningX.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.BinningY.IsAvailable)
            {
                _camera.BinningY.OnFeatureChanged -= FeatureChanged;
            }
            if (_camera.StatFrameRate.IsAvailable)
                _camera.StatFrameRate.OnFeatureChanged -= FeatureChanged;
        }


        #endregion

        

    }
}
