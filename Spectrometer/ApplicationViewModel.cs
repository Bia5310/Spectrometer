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
using VimbaCameraNET;
using static AO_Lib.AO_Devices;
using static VimbaCameraNET.VimbaCamera;

namespace Spectrometer
{
    class ApplicationViewModel : INotifyPropertyChanged
    {
        private VimbaCamera _camera = null;
        private AO_Filter AOFilter = null;

        public ObservableCollection<VimbaCamera> ConnectedCameras { get; set; }

        public ApplicationViewModel()
        {

        }

        private ClickCommand connectToCameraCommand;
        public ClickCommand ConnectToCameraCommand
        {
            get
            {
                return connectToCameraCommand ??
                    new ClickCommand(cameraInfo =>
                    { 
                        if(cameraInfo == null)
                        {
                            //Connect to selected
                        }
                        else
                        {
                            //Autoconnect to first
                        }
                    });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void _camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        #region CameraRegion

        public void SetValue(FeatureType featureType, dynamic value, [CallerMemberName]string featureName = "")
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

        public dynamic GetValue(FeatureType featureType, dynamic defValue = null)
        {
            if (_camera != null)
            {
                CameraFeature f = _camera.GetFeature(featureType);
                if (f.IsAvailable)
                    return f.Value;
            }
            return defValue ?? 0;
        }

        public dynamic GetValueMin(FeatureType featureType, dynamic defValue = null)
        {
            if (_camera != null)
            {
                CameraFeature f = _camera.GetFeature(featureType);
                if (f.IsAvailable)
                    return f.MinValue;
            }
            return defValue ?? 0;
        }

        public dynamic GetValueMax(FeatureType featureType, dynamic defValue = null)
        {
            if (_camera != null)
            {
                CameraFeature f = _camera.GetFeature(featureType);
                if(f.IsAvailable)
                    return f.MaxValue;
            }
            return defValue ?? 1;
        }

        public double Exposure
        {
            get => GetValue(FeatureType.Exposure);
            set => SetValue(FeatureType.Exposure, value);
        }

        public double ExposureMin
        {
            get => GetValueMin(FeatureType.Exposure);
        }

        public double ExposureMax
        {
            get => GetValueMax(FeatureType.Exposure);
        }


        public double Gain
        {
            get => GetValue(FeatureType.Gain);
            set => SetValue(FeatureType.Gain, value);
        }

        public double GainMin
        {
            get => GetValueMin(FeatureType.Gain);
        }

        public double GainMax
        {
            get => GetValueMax(FeatureType.Gain);
        }


        public double Gamma
        {
            get => GetValue(FeatureType.Gamma);
            set => SetValue(FeatureType.Gamma, value);
        }

        public double GammaMin
        {
            get => GetValueMin(FeatureType.Gamma);
        }

        public double GammaMax
        {
            get => GetValueMax(FeatureType.Gamma);
        }


        public double Width
        {
            get => GetValue(FeatureType.Width);
            set => SetValue(FeatureType.Width, value);
        }

        public double WidthMin
        {
            get => GetValueMin(FeatureType.Width);
        }

        public double WidthMax
        {
            get => GetValueMax(FeatureType.Width);
        }


        public double Height
        {
            get => GetValue(FeatureType.Height);
            set => SetValue(FeatureType.Height, value);
        }

        public double HeightMin
        {
            get => GetValueMin(FeatureType.Height);
        }

        public double HeightMax
        {
            get => GetValueMax(FeatureType.Height);
        }


        public double OffsetX
        {
            get => GetValue(FeatureType.OffsetX);
            set => SetValue(FeatureType.OffsetX, value);
        }

        public double OffsetXMin
        {
            get => GetValueMin(FeatureType.OffsetX);
        }

        public double OffsetXMax
        {
            get => GetValueMax(FeatureType.OffsetX);
        }


        public double OffsetY
        {
            get => GetValue(FeatureType.OffsetY);
            set => SetValue(FeatureType.OffsetY, value);
        }

        public double OffsetYMin
        {
            get => GetValueMin(FeatureType.OffsetY);
        }

        public double OffsetYMax
        {
            get => GetValueMax(FeatureType.OffsetY);
        }


        public double BinningX
        {
            get => GetValue(FeatureType.BinningX);
            set => SetValue(FeatureType.BinningX, value);
        }

        public double BinningXMin
        {
            get => GetValueMin(FeatureType.BinningX);
        }

        public double BinningXMax
        {
            get => GetValueMax(FeatureType.BinningX);
        }


        public double BinningY
        {
            get => GetValue(FeatureType.BinningY);
            set => SetValue(FeatureType.BinningY, value);
        }

        public double BinningYMin
        {
            get => GetValueMin(FeatureType.BinningY);
        }

        public double BinningYMax
        {
            get => GetValueMax(FeatureType.BinningY);
        }


        public double BlackLevel
        {
            get => GetValue(FeatureType.BlackLevel);
            set => SetValue(FeatureType.BlackLevel, value);
        }

        public double BlackLevelMin
        {
            get => GetValueMin(FeatureType.BlackLevel);
        }

        public double BlackLevelMax
        {
            get => GetValueMax(FeatureType.BlackLevel);
        }

        #endregion


    }
}
