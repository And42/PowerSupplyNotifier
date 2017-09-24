using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using NAudio.Wave;
using PowerSupplyNotifier.Annotations;
using PowerSupplyNotifier.Localizations;
using PowerSupplyNotifier.Properties;

namespace PowerSupplyNotifier.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private static SettingsWindow Instance { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();

            FillAudioDevices();
        }

        private void FillAudioDevices()
        {
            if (AudioDevices.Count > 0)
                AudioDevices.Clear();
            
            var deviceCound = WaveOut.DeviceCount;

            for (int i = 0; i < deviceCound; i++)
                AudioDevices.Add(WaveOut.GetCapabilities(i));
        }

        public ObservableCollection<WaveOutCapabilities> AudioDevices { get; } = new ObservableCollection<WaveOutCapabilities>();

        public string SoundFile
        {
            get => Settings.Default.SoundFile;
            set
            {
                if (File.Exists(value))
                {
                    Settings.Default.SoundFile = value;
                    Settings.Default.Save();
                }
                else
                {
                    MessageBox.Show(
                        string.Format(StringResources.SoundFileNotFound, value), StringResources.Error,
                        MessageBoxButton.OK, MessageBoxImage.Warning
                    );
                }

                OnPropertyChanged(nameof(SoundFile));
            }
        }

        public static void ShowUnique()
        {
            if (Instance == null)
            {
                Instance = new SettingsWindow();
                Instance.Show();
            }
            else
            {
                Instance.Focus();
            }
        }

        public void PlaySound(int deviceNumber, string fileName)
        {
            var waveReader = new WaveFileReader(fileName);
            var waveOut = new WaveOut {DeviceNumber = deviceNumber};

            waveOut.Init(waveReader);
            waveOut.Play();
        }

        private void SettingsWindow_OnClosed(object sender, EventArgs e)
        {
            Instance = null;
        }

        private void ChooseSoundFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = StringResources.WavFilesFilter,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                SoundFile = dialog.FileName;
            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
