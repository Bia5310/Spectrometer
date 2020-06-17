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
using Spectrometer.Commands;
using Spectrometer.ViewModels;

namespace Spectrometer.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Fields, variables

        private CameraViewModel cameraViewModel;
        public CameraViewModel CameraViewModel
        {
            get => cameraViewModel;
            set
            {
                cameraViewModel = value;
                OnPropertyChanged();
            }
        }

        private FilterViewModel filterViewModel;
        public FilterViewModel FilterViewModel
        {
            get => filterViewModel;
            set
            {
                filterViewModel = value;
                OnPropertyChanged();
            }
        }

        private Command openSettingsCommand = null;

        #endregion

        #region Constructor, Destructor

        public ApplicationViewModel()
        {
            CameraViewModel = new CameraViewModel(this);
            FilterViewModel = new FilterViewModel(this);
        }

        ~ApplicationViewModel()
        {

        }

        #endregion

        #region Functions

        /*public ClickCommand TestImageTriggerCommand
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
        }*/

        #endregion
    }
}
