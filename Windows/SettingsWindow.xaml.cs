using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private WaveOutCapabilities? _currentAudioDevice;

        public SettingsWindow()
        {
            InitializeComponent();

            FillAudioDevices();
            SelectCurrentDevice();
        }

        private void FillAudioDevices()
        {
            if (AudioDevices.Count > 0)
                AudioDevices.Clear();
            
            var deviceCount = WaveOut.DeviceCount;

            for (int i = 0; i < deviceCount; i++)
                AudioDevices.Add(WaveOut.GetCapabilities(i));
        }

        private void SelectCurrentDevice()
        {
            var guid = Settings.Default.SoundOutputGuid;

            if (guid != Guid.Empty)
            {
                var device = AudioDevices.FirstOrDefault(it => it.ProductGuid == guid);

                if (device.ProductGuid != Guid.Empty)
                    CurrentAudioDevice = device;
                else
                {
                    Settings.Default.SoundOutputGuid = Guid.Empty;
                    Settings.Default.Save();
                }
            }
            else
            {
                CurrentAudioDevice = null;
            }
        }

        public ObservableCollection<WaveOutCapabilities> AudioDevices { get; } = new ObservableCollection<WaveOutCapabilities>();

        public WaveOutCapabilities? CurrentAudioDevice
        {
            get => _currentAudioDevice;
            set
            {
                if (SetProperty(ref _currentAudioDevice, value))
                {
                    Settings.Default.SoundOutputGuid = value?.ProductGuid ?? Guid.Empty;
                    Settings.Default.Save();
                }
                
            }
        }

        public bool SoundNotification
        {
            get => Settings.Default.SoundNotification;
            set
            {
                Settings.Default.SoundNotification = value;
                Settings.Default.Save();

                OnPropertyChanged();
            }
        }

        public bool MessageNotification
        {
            get => Settings.Default.MessageNotification;
            set
            {
                Settings.Default.MessageNotification = value;
                Settings.Default.Save();

                OnPropertyChanged();
            }
        }

        public string BatterySoundFile
        {
            get => Settings.Default.BatterySoundFile;
            set
            {
                if (File.Exists(value))
                {
                    Settings.Default.BatterySoundFile = value;
                    Settings.Default.Save();
                }
                else
                {
                    MessageBox.Show(
                        string.Format(StringResources.SoundFileNotFound, value), StringResources.Error,
                        MessageBoxButton.OK, MessageBoxImage.Warning
                    );
                }

                OnPropertyChanged();
            }
        }

        public string NetworkSoundFile
        {
            get => Settings.Default.NetworkSoundFile;
            set
            {
                if (File.Exists(value))
                {
                    Settings.Default.NetworkSoundFile = value;
                    Settings.Default.Save();
                }
                else
                {
                    MessageBox.Show(
                        string.Format(StringResources.SoundFileNotFound, value), StringResources.Error,
                        MessageBoxButton.OK, MessageBoxImage.Warning
                    );
                }

                OnPropertyChanged();
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
                if (((FrameworkElement) sender).Tag.Equals("Battery"))
                    BatterySoundFile = dialog.FileName;
                else
                    NetworkSoundFile = dialog.FileName;
            }
        }

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;

            OnPropertyChanged(propertyName);

            return true;
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
