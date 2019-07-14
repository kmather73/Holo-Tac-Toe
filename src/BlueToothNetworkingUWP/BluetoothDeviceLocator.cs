// ******************************************************************
// Copyright (c) Kevin Mather. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

namespace KMR.Communication.Devices
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
#if WINDOWS_UWP
    using Windows.Devices.Bluetooth;
    using Windows.Devices.Bluetooth.Rfcomm;
    using Windows.Devices.Enumeration;
#endif
    public class BluetoothDeviceLocator : IDeviceLocator, IDisposable
    {
#if WINDOWS_UWP
        private DeviceWatcher m_deviceWatcher;
#endif

        private object deviceListLock = new object();
        public event EventHandler<IRemoteDevice> DeviceFound;
        public event EventHandler<IRemoteDevice> DeviceLost;
        public event EventHandler<IRemoteDevice> DeviceUpdated;
        public event EventHandler InitialDeviceEnumerationCompleted;
        public event PropertyChangedEventHandler PropertyChanged;

        public IList<IRemoteDevice> RemoteDevices { get; }

        public async Task<bool> ConnectDeviceAsync(IRemoteDevice device)
        {
            await Task.CompletedTask;
            return false;
        }

        public void Dispose()
        {
#if WINDOWS_UWP
            if (m_deviceWatcher != null)
            {
                m_deviceWatcher.Stop();
                m_deviceWatcher.Added -= OnDeviceAdded;
                m_deviceWatcher.Removed -= OnDeviceLost;
                m_deviceWatcher.Updated -= OnDeviceUpdated;
                m_deviceWatcher.EnumerationCompleted -= OnInitialEnumerationCompleted;
                m_deviceWatcher = null;
            }
#endif
        }

        public void StartSearchingForDevice()
        {
#if WINDOWS_UWP
            var requestedProperties = new string[] { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
            // var aqs = BluetoothLEDevice.GetDeviceSelectorFromPairingState(false);
            var aqs = BluetoothDevice.GetDeviceSelectorFromPairingState(false);

            m_deviceWatcher = DeviceInformation.CreateWatcher(
                aqs,
                requestedProperties,
                DeviceInformationKind.AssociationEndpoint);

            m_deviceWatcher.Added += OnDeviceAdded;
            m_deviceWatcher.Removed += OnDeviceLost;
            m_deviceWatcher.Updated += OnDeviceUpdated;
            m_deviceWatcher.EnumerationCompleted += OnInitialEnumerationCompleted;
            m_deviceWatcher.Stopped += OnStopped;

            m_deviceWatcher.Start();
#endif
        }

        public void StopSearchingForDevice()
        {
#if WINDOWS_UWP
            m_deviceWatcher.Stop();
#endif
        }

#if WINDOWS_UWP
        private async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            var device = new RemoteBluetoothDevice(args);
            var success = await device.TryAndConnectToDevice();
            if (success)
            {
                 var id = RfcommServiceId.FromUuid(BluetoothMessageOrchestrator.ServiceUuid);
                 success = await device.HasService(id, BluetoothMessageOrchestrator.SdpServiceNameAttributeId);
            }

            if (success)
            {
                lock (RemoteDevices)
                {
                    RemoteDevices.Add(device);
                }

                DeviceFound.Invoke(this, device);
                RaisePropertyChanged(new PropertyChangedEventArgs(nameof(RemoteDevices)));
            }
        }

        private void OnDeviceLost(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            lock (RemoteDevices)
            {
                var device = RemoteDevices.FirstOrDefault(x => x.DeviceId == args.Id);
                if (device != null)
                {
                    RemoteDevices.Remove(device);
                    DeviceLost.Invoke(this, device);
                    RaisePropertyChanged(new PropertyChangedEventArgs(nameof(RemoteDevices)));
                }
            }
        }

        private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            DeviceUpdated.Invoke(this, null);
            RaisePropertyChanged(new PropertyChangedEventArgs(nameof(RemoteDevices)));
        }

        private void OnInitialEnumerationCompleted(DeviceWatcher sender, object args)
        {
            InitialDeviceEnumerationCompleted.Invoke(this, null);
        }

        private void OnStopped(DeviceWatcher sender, object args)
        {
        }
#endif
        private void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
