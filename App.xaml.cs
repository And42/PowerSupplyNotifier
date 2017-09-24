using System.IO;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using PowerSupplyNotifier.Code;
using PowerSupplyNotifier.Localizations;
using PowerSupplyNotifier.Properties;
using PowerSupplyNotifier.Windows;

using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using PowerLineStatus = System.Windows.Forms.PowerLineStatus;

namespace PowerSupplyNotifier
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static NotifyIcon NotifyIcon { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Settings.Default.HaveToUpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.HaveToUpdateSettings = false;
                Settings.Default.Save();
            }

            NotifyIcon = new NotifyIcon();

            InitNotifyIcon(NotifyIcon);

            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                NotifyIcon.Icon = PowerSupplyNotifier.Properties.Resources.app_icon_battery;

            SystemEvents.PowerModeChanged += SystemEventsOnPowerModeChanged;
        }

        private void InitNotifyIcon(NotifyIcon icon)
        {
            icon.Icon = PowerSupplyNotifier.Properties.Resources.app_icon;
            icon.Visible = true;

            icon.ContextMenuStrip = new ContextMenuStrip
            {
                Items =
                {
                    new ToolStripMenuItem(StringResources.Settings_Title, null, (_, __) => SettingsWindow.ShowUnique()),
                    new ToolStripMenuItem(StringResources.AboutProgram_Title, null, (_, __) => AboutProgram.ShowUnique()),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem(StringResources.Exit, null, (_, __) => Shutdown(0))
                }
            };
        }

        private void SystemEventsOnPowerModeChanged(object sender, PowerModeChangedEventArgs args)
        {
            if (args.Mode == PowerModes.StatusChange)
            {
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                {
                    NotifyIcon.Icon = PowerSupplyNotifier.Properties.Resources.app_icon_battery;

                    if (Settings.Default.SoundNotification)
                        PlaySoundIfCan();

                    if (Settings.Default.MessageNotification)
                    {
                        MessageBox.Show(StringResources.PowerSupplyChangedToBattery,
                            StringResources.PowerSupplyInformation, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                {
                    NotifyIcon.Icon = PowerSupplyNotifier.Properties.Resources.app_icon;

                    if (Settings.Default.SoundNotification)
                        PlaySoundIfCan();

                    if (Settings.Default.MessageNotification)
                    {
                        MessageBox.Show(StringResources.PowerSupplyChangedToNetwork,
                            StringResources.PowerSupplyInformation, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SystemEvents.PowerModeChanged -= SystemEventsOnPowerModeChanged;

            NotifyIcon.Dispose();
            Utils.Dispose();
        }

        private static void PlaySoundIfCan()
        {
            var (_, device) = Utils.GetDeviceByProductGuid(Settings.Default.SoundOutputGuid);

            var soundFile = Settings.Default.SoundFile;

            if (!string.IsNullOrEmpty(soundFile) && File.Exists(soundFile))
                Utils.PlaySound(soundFile, device);
        }
    }
}
