using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI;

namespace URE.ViewModels.Controls
{
    //конфиг для каждого отдельного детектора
    public class DetectorConfigVm : ObservableRecipient, INotifyDataErrorInfo
    {
        public DetectorConfigVm()
        {
            HistogramLineColor = Colors.Lime;
        }
        public int Id { get; set; }
        public int PortId { get; set; }
        //цвет графика детектора на гистограмме
        //private Color histogramLineColor = Color.FromArgb(255, 90, 90, 90);
        private Color histogramLineColor;
        public Color HistogramLineColor { get => histogramLineColor; set { SetProperty(ref histogramLineColor, value); HistogramLineColorBrush = new SolidColorBrush(value); } }
        private Brush histogramLineColorBrush;
        public Brush HistogramLineColorBrush { get => histogramLineColorBrush; set => SetProperty(ref histogramLineColorBrush, value); }
        ////имя детектора
        private string detectorName;
        public string DetectorName { get => detectorName; set { SetProperty(ref detectorName, value); } }
        ////включение/выключение детектора
        private bool detectorEnabled;
        public bool DetectorEnabled { get => detectorEnabled; set { SetProperty(ref detectorEnabled, value); } }
        ////сетевой адрес детектора
        private int detectorIdentifier;
        public int DetectorIdentifier { get => detectorIdentifier; set { ValidateDetectorIdentifier(value); SetProperty(ref detectorIdentifier, value); OnPropertyChanged(); } }
        ////серийный номер детектора
        private long detectorSerialNumber;
        public long DetectorSerialNumber { get => detectorSerialNumber; set { SetProperty(ref detectorSerialNumber, value); } }

        private double normalDoseRate;
        public double NormalDoseRate { get => normalDoseRate; set { ValidateDoseRate(value); SetProperty(ref normalDoseRate, value); OnPropertyChanged(); } }
        private double highDoseRate;
        public double HighDoseRate { get => highDoseRate; set { ValidateDoseRate(value); SetProperty(ref highDoseRate, value); OnPropertyChanged(); NormalDoseRate = NormalDoseRate; } }
        private double criticalDoseRate;
        public double CriticalDoseRate { get => criticalDoseRate; set { ValidateDoseRate(value); SetProperty(ref criticalDoseRate, value); OnPropertyChanged(); HighDoseRate = HighDoseRate; } }

        ////скорость передачи данных
        //private int detectorBoudRate;
        //public int DetectorBaudRate { get => detectorBoudRate; set { SetProperty(ref detectorBoudRate, value); } }
        public ICommand SelectThisDetectorCommand
        {
            get
            {
                return new RelayCommand<DetectorConfigVm>((detector) =>
                {
                    OnSelectThisDetector?.Invoke(detector!);
                });
            }
        }

        public event Action<DetectorConfigVm> OnSelectThisDetector;

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new();

        public bool HasErrors => _validationErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
                !_validationErrors.ContainsKey(propertyName))
                return null;

            return _validationErrors[propertyName];
        }

        private void SetErrors(string key, ICollection<string> errors)
        {
            if (errors.Any())
                _validationErrors[key] = errors;
            else
                _ = _validationErrors.Remove(key);

            OnErrorsChanged(key);
        }

        public void ClearErrors()
        {
            foreach (var key in _validationErrors.Keys)
            {
                SetErrors(key, new List<string>());
            }
        }

        public bool ValidateDetectorIdentifier(double identifier)
        {
            var errors = new List<string>(1);
            if (identifier < 0 || identifier > 255)
            {
                errors.Add("Ідентифікатор має бути від 0 до 255!");
            }

            SetErrors(nameof(DetectorIdentifier), errors);

            return !HasErrors;
        }

        public bool ValidateDoseRate(double doseRate, [CallerMemberName] string? propertyName = null)
        {
            var errors = new List<string>(1);
            if (doseRate < 0 || doseRate > 20000)
            {
                errors.Add("Показник має бути від 0,001 до 20000,000!");
            }
            else if (propertyName == nameof(NormalDoseRate) && doseRate >= HighDoseRate)
            {
                errors.Add("Нормальний показник має бути меншим за підвищений!");
            }
            else if (propertyName == nameof(HighDoseRate) && doseRate >= CriticalDoseRate)
            {
                errors.Add("Підвищений показник має бути меншим за критичний!");
            }

            SetErrors(propertyName, errors);

            return !HasErrors;
        }
    }
}
