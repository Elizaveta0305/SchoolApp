using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SchoolApplication.ViewModels
{
    public partial class ChartVm : INotifyPropertyChanged
    {
        private double _gaugeValue;

        public double GaugeValue
        {
            get => _gaugeValue;
            set
            {
                if (_gaugeValue != value)
                {
                    _gaugeValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ChartVm()
        {
            // Установим начальное значение для примера
            GaugeValue = 2.5;
        }
    }
}