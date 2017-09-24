using System.IO;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using PowerSupplyNotifier.Code;
using PowerSupplyNotifier.Localizations;
using PowerSupplyNotifier.Properties;
using PowerSupplyNotifier.Windows;

using Application = System.Windows.Application;
using PowerLineStatus = System.Windows.Forms.PowerLineStatus;

namespace PowerSupplyNotifier
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static NotifyIcon NotifyIcon { get; private set; }

        private PowerLineStatus _previous = PowerLineStatus.Unknown;

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

        protected override void OnExit(ExitEventArgs e)
        {
            SystemEvents.PowerModeChanged -= SystemEventsOnPowerModeChanged;

            NotifyIcon.Dispose();
            Utils.Dispose();
        }

        private void SystemEventsOnPowerModeChanged(object sender, PowerModeChangedEventArgs args)
        {
            if (args.Mode == PowerModes.StatusChange)
            {
                if (SystemInformation.PowerStatus.PowerLineStatus == _previous)
                    return;

                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                {
                    _previous = PowerLineStatus.Offline;

                    NotifyIcon.Icon = PowerSupplyNotifier.Properties.Resources.app_icon_battery;

                    if (Settings.Default.SoundNotification)
                        PlaySoundIfCan(true);

                    if (Settings.Default.MessageNotification)
                        ShowMessage(StringResources.PowerSupplyChangedToBattery);
                }
                else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                {
                    _previous = PowerLineStatus.Online;

                    NotifyIcon.Icon = PowerSupplyNotifier.Properties.Resources.app_icon;

                    if (Settings.Default.SoundNotification)
                        PlaySoundIfCan(false);

                    if (Settings.Default.MessageNotification)
                        ShowMessage(StringResources.PowerSupplyChangedToNetwork);
                }
            }
        }

        private void InitNotifyIcon(NotifyIcon icon)
        {
            icon.Icon = PowerSupplyNotifier.Properties.Resources.app_icon;
            icon.Visible = true;

            icon.MouseClick += (_, args) =>
            {
                if (args.Button == MouseButtons.Left)
                    SettingsWindow.ShowUnique();
            };

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

        private static void PlaySoundIfCan(bool battery)
        {
            var (_, device) = Utils.GetDeviceByProductGuid(Settings.Default.SoundOutputGuid);

            var soundFile = battery ? Settings.Default.BatterySoundFile : Settings.Default.NetworkSoundFile;

            if (File.Exists(soundFile))
                Utils.PlaySound(soundFile, device);
        }

        private void ShowMessage(string message)
        {
            NotifyIcon.ShowBalloonTip(1000, StringResources.PowerSupplyInformation, message, ToolTipIcon.Info);
        }
    }
}
