using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SchoolApplication.ViewModels
{
    public partial class ChartVm : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<double> _gaugeValues;

        public ChartVm()
        {

        }

        public void SetGaugeValue(double value)
        {

        }
    }
}