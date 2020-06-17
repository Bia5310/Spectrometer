using Microsoft.Win32;
using Spectrometer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static AO_Lib.AO_Devices;

namespace Spectrometer.ViewModels
{
    public class FilterViewModel : BaseViewModel
    {
        #region Events, Delegates

        #endregion

        #region Constructors

        public FilterViewModel(ApplicationViewModel applicationViewModel)
        {
            ApplicationViewModel = applicationViewModel;

            connectCommand = new Command(ConnectMethod);
            loadDevCommand = new Command(LoadDevMethod);
            powerToggleCommand = new Command(PowerToggleMethod);
        }

        #endregion

        #region Fields, variables

        public ApplicationViewModel ApplicationViewModel { get; set; }

        private AO_Filter aO_Filter = null;
        public AO_Filter AOFilter
        {
            get => aO_Filter;
            set
            {
                if (value == null)
                {
                    if(aO_Filter != null)
                    {
                        if (aO_Filter.isPowered)
                        {
                            aO_Filter.PowerOff();
                        }

                        aO_Filter.onSetHz -= AO_Filter_onSetHz;
                        aO_Filter.onSetWl -= AO_Filter_onSetWl;
                        aO_Filter.Dispose();
                        aO_Filter = null;
                    }
                }
                else
                {
                    aO_Filter = value;
                    aO_Filter.onSetHz += AO_Filter_onSetHz;
                    aO_Filter.onSetWl += AO_Filter_onSetWl;
                }

                OnPropertyChanged();
                OnPropertyChanged("isPowered");
            }
        }

        public double WL_Current
        {
            get => AOFilter?.WL_Current ?? 0;
            set
            {
                if(AOFilter != null)
                {
                    float wl = (float)value;
                    if(wl >= AOFilter.WL_Min && wl <= AOFilter.WL_Max)
                    {
                        AOFilter.Set_Wl(wl);
                    }
                }
            }
        }

        public double HZ_Current
        {
            get => AOFilter?.HZ_Current ?? 0;
            set
            {
                if (AOFilter != null)
                {
                    float hz = (float)value;
                    if (hz >= AOFilter.HZ_Min && hz <= AOFilter.HZ_Max)
                    {
                        AOFilter.Set_Hz(hz);
                    }
                }
            }
        }

        public bool isPowered
        {
            get => aO_Filter?.isPowered ?? false;
            set
            {
                if(aO_Filter != null)
                {
                    if(value)
                    {
                        aO_Filter.PowerOn();
                    }
                    else
                    {
                        aO_Filter.PowerOff();
                    }
                    OnPropertyChanged();
                }
            }
        }

        private Command connectCommand = null;
        private Command loadDevCommand = null;
        private Command powerToggleCommand = null;

        public Command ConnectCommand { get => connectCommand; }
        public Command LoadDevCommand { get => loadDevCommand; }
        public Command PowerToggleCommand { get => powerToggleCommand; }

        #endregion

        #region Functions
        private void AO_Filter_onSetWl(AO_Filter sender, float WL_now, float HZ_now)
        {
            OnPropertyChanged("WL_Current");
            OnPropertyChanged("HZ_Current");
        }

        private void AO_Filter_onSetHz(AO_Filter sender, float WL_now, float HZ_now)
        {
            OnPropertyChanged("WL_Current");
            OnPropertyChanged("HZ_Current");
        }

        private void PowerToggleMethod()
        {
            if(AOFilter != null)
            {
                if(!AOFilter.isPowered)
                {
                    AOFilter.PowerOn();
                    OnPropertyChanged("isPowered");
                }
                else
                {
                    AOFilter.PowerOff();
                    OnPropertyChanged("isPowered");
                }
            }
        }

        private void ConnectMethod()
        {
            try
            {
                DisconenctMethod();

                if (AOFilter == null)
                {
                    AO_Filter newAOFilter = AO_Filter.Find_and_connect_AnyFilter();

                    OpenFileDialog ofd = new OpenFileDialog();
                    
                    if(ofd.ShowDialog() == true)
                    {
                        int res = newAOFilter.Read_dev_file(ofd.FileName);
                        if(res != 0)
                        {
                            throw new Exception(newAOFilter.Implement_Error(res));
                        }

                        AOFilter = newAOFilter;
                    }
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public void DisconenctMethod()
        {
            AOFilter = null;
        }

        public void LoadDevMethod()
        {
            if(AOFilter != null)
            {
                OpenFileDialog ofd = new OpenFileDialog();

                if (ofd.ShowDialog() == true)
                {
                    int res = aO_Filter.Read_dev_file(ofd.FileName);
                    if (res != 0)
                    {
                        throw new Exception(aO_Filter.Implement_Error(res));
                    }

                    AOFilter = aO_Filter;
                }
            }

            OnPropertyChanged("isPowered");
        }

        #endregion
    }
}
