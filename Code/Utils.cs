using System;
using NAudio.Wave;

namespace PowerSupplyNotifier.Code
{
    internal static class Utils
    {
        private static WaveFileReader _waveReader;
        private static WaveOut _waveOut;

        internal static (WaveOutCapabilities? deviceInfo, int deviceNumber) GetDeviceByProductGuid(Guid productGuid)
        {
            if (productGuid == Guid.Empty)
                return (null, -1);

            var deviceCount = WaveOut.DeviceCount;

            for (int i = 0; i < deviceCount; i++)
            {
                var cap = WaveOut.GetCapabilities(i);

                if (cap.ProductGuid == productGuid)
                    return (cap, i);
            }

            return (null, -1);
        }

        internal static void PlaySound(string fileName, int deviceNumber = -1)
        {
            if (_waveReader != null)
            {
                _waveReader.Dispose();
                _waveReader = null;
            }

            if (_waveOut != null)
            {
                _waveOut.Dispose();
                _waveOut = null;
            }

            _waveReader = new WaveFileReader(fileName);
            _waveOut = new WaveOut { DeviceNumber = deviceNumber };

            _waveOut.Init(_waveReader);
            _waveOut.Play();
        }

        internal static void Dispose()
        {
            if (_waveReader != null)
            {
                _waveReader.Dispose();
                _waveReader = null;
            }

            if (_waveOut != null)
            {
                _waveOut.Dispose();
                _waveOut = null;
            }
        }
    }
}
