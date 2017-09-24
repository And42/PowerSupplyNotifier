using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using PowerSupplyNotifier.Annotations;
using PowerSupplyNotifier.Code;
using PowerSupplyNotifier.LongStringResources;

namespace PowerSupplyNotifier.Windows
{
    /// <summary>
    /// Логика взаимодействия для AboutProgram.xaml
    /// </summary>
    public partial class AboutProgram : Window, INotifyPropertyChanged
    {
        private static AboutProgram Instance { get; set; }

        private ComponentLicense _currentLicense;

        public AboutProgram()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            AppVersion = $"{version.Major}.{version.Minor}";

            InitializeComponent();

            Licenses.Add(
                new ComponentLicense(
                    "Icons",
                    "Icons made by Freepik (http://www.freepik.com) from https://www.flaticon.com is licensed by Creative Commons BY 3.0"
                )
            );
            Licenses.Add(
                new ComponentLicense(
                    "NAudio",
                    LicenseResources.NAudio_License
                )
            ); 
        }

        public string AppVersion { get; }

        public ObservableCollection<ComponentLicense> Licenses { get; } = new ObservableCollection<ComponentLicense>();

        public ComponentLicense CurrentLicense
        {
            get => _currentLicense;
            set
            {
                _currentLicense = value;
                OnPropertyChanged(nameof(CurrentLicense));
            }
        }

        public static void ShowUnique()
        {
            if (Instance == null)
            {
                Instance = new AboutProgram();
                Instance.Show();
            }
            else
            {
                Instance.Focus();
            }
        }

        private void AboutProgram_OnClosed(object sender, EventArgs e)
        {
            Instance = null;
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
