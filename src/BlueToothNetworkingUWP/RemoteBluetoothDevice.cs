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
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
#if WINDOWS_UWP
    using Windows.Devices.Bluetooth;
    using Windows.Devices.Bluetooth.Rfcomm;
    using Windows.Devices.Enumeration;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
    using Windows.System;
#endif

    public class RemoteBluetoothDevice : IRemoteDevice
    {
#if WINDOWS_UWP
        private BluetoothDevice m_btDevice;
        private DeviceInformation m_deviceInformation;
        private RfcommDeviceService m_frCommService;

        public RemoteBluetoothDevice(DeviceInformation deviceInformation)
        {
            m_deviceInformation = deviceInformation;
        }
#endif
        public string DeviceId
        {
            get
            {
#if WINDOWS_UWP
                return m_deviceInformation.Id;
#else
                return string.Empty;
#endif
            }
        }


        public bool IsConnected
        {
            get
            {
#if WINDOWS_UWP
                return  m_btDevice != null && m_btDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
#else
                return false;
#endif
            }
        }
                public async Task<bool> TryAndConnectToDevice()
        {
#if WINDOWS_UWP
            try
            {
                m_btDevice = await BluetoothDevice.FromIdAsync(DeviceId);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return m_btDevice != null;
#else
            await Task.CompletedTask;
            return false;
#endif
        }

#if WINDOWS_UWP
        public async Task<bool> HasService(RfcommServiceId serviceId, ushort sdpServiceNameAttributeId)
        {
            var rfcommServices = await m_btDevice.GetRfcommServicesForIdAsync(
                serviceId,
                BluetoothCacheMode.Uncached);

            if (rfcommServices.Services.Count == 0)
            {
                // Could not discover the service on the remote device
                return false;
            }

            var m_frCommService = rfcommServices.Services[0];
            var attributes = await m_frCommService.GetSdpRawAttributesAsync();
            if (!attributes.ContainsKey(sdpServiceNameAttributeId))
            {
                // The service is using an unexpected format for the Service Name attribute.
                return false;
            }

            var attributeReader = DataReader.FromBuffer(attributes[sdpServiceNameAttributeId]);
            attributeReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

            var serviceNameLength = attributeReader.ReadByte();
            var serviceName = attributeReader.ReadString(serviceNameLength);

            return serviceName == BluetoothMessageOrchestrator.SdpServiceName;
        }

        public async Task<StreamSocket> EstablishSocketEndPoint()
        {
            var socket = new StreamSocket();

            try
            {
                await socket.ConnectAsync(m_frCommService.ConnectionHostName, m_frCommService.ConnectionServiceName);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return socket;
        }
#endif
        public void Dispose()
        {
#if WINDOWS_UWP
            if (m_btDevice != null)
            {
                m_btDevice.Dispose();
                m_btDevice = null;
            }
#endif
        }
    }
}
