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
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading.Tasks;
#if WINDOWS_UWP
    using Windows.Devices.Bluetooth;
    using Windows.Devices.Bluetooth.Advertisement;
    using Windows.Devices.Bluetooth.GenericAttributeProfile;
    using Windows.Devices.Bluetooth.Rfcomm;
    using Windows.Devices.Enumeration;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
#endif 

    public class BluetoothMessageOrchestrator : IMessageOrchestrator
    {
        public enum Mode
        {
            Client,
            Server,
        }

#pragma warning disable CS0414
        private static readonly byte SdpServiceNameAttributeType = (4 << 3) | 5;
#pragma warning restore CS0414
        public static readonly Guid ServiceUuid = Guid.Parse("FCCBFF23-8335-4C70-B430-E32DE0E57177");
        public static readonly ushort SdpServiceNameAttributeId = 0x100;
        public static readonly string SdpServiceName = "KMR Bluetooth Rfcomm Service v1";

        private Mode m_orchestratorMode;
#if WINDOWS_UWP
        private RfcommDeviceService m_rfService;
        private RfcommServiceProvider m_rfcommService;
        private StreamSocketListener m_socketListener;

        private StreamSocket m_socket;
        private DataWriter m_messageOutstream;
        private DataReader m_messageInStream;
#endif
        private Dictionary<short, IMessageService> m_messageServices = new Dictionary<short, IMessageService>();

        public BluetoothMessageOrchestrator(Mode mode)
        {
            m_orchestratorMode = mode;
        }

        public bool RegisterMessageService(IMessageService messageService)
        {
            var wasRegisterd = false;
            if (!m_messageServices.ContainsKey(messageService.ServiceId))
            {
                m_messageServices.Add(messageService.ServiceId, messageService);
                wasRegisterd = true;
            }

            return wasRegisterd;
        }

        public bool DeregisterMessageService(IMessageService messageService)
        {
            var wasDeregistered = false;
            if (m_messageServices.ContainsKey(messageService.ServiceId))
            {
                m_messageServices.Remove(messageService.ServiceId);
                wasDeregistered = true;
            }

            return wasDeregistered;
        }

        public bool TransmitMessage(IRemoteMessage message)
        {
#if WINDOWS_UWP
            if (m_socket != null)
            {
                m_messageOutstream.WriteUInt16((ushort)message.MessageHeader);
                var sizeofmessage = message.Message.Length;
                m_messageOutstream.WriteInt32(sizeofmessage);
                m_messageOutstream.WriteBytes(message.Message);
            }

            return m_socket != null;
#else
            return false;
#endif
        }

        public async Task InitializeAsync()
        {
            if (m_orchestratorMode == Mode.Server)
            {
#if WINDOWS_UWP
                await InitializeRfcommServer();
#else
                await Task.CompletedTask;
#endif
            }
        }

        public Task<bool> ConnectToServerAsync(IRemoteDevice remoteServer)
        {
#if WINDOWS_UWP
            return InitializeRfcommClient(remoteServer);
#else
            return Task.FromResult(false);
#endif
        }

#if WINDOWS_UWP
        private async Task<bool> OnMessageReceived()
        {
            if (m_socket != null)
            {
                try
                {
                    var totalHeaderSize = (uint) sizeof(short) + sizeof(int);
                    var readLength = await m_messageInStream.LoadAsync(totalHeaderSize);
                    if (readLength < totalHeaderSize)
                    {
                        return false;
                    }

                    var header = m_messageInStream.ReadInt16();
                    var messageSize = m_messageInStream.ReadUInt32();

                    readLength = await m_messageInStream.LoadAsync(messageSize);

                    var bytes = new byte[messageSize];
                    m_messageInStream.ReadBytes(bytes);
                    if (m_messageServices.TryGetValue(header, out var service))
                    {
                        service.ReadMessage(new IRemoteMessage()
                        {
                            MessageHeader = header,
                            Message = bytes,
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                }
            }

            return m_socket != null;
        }

        private async Task InitializeRfcommServer()
        {
            try
            {
                m_rfcommService = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(ServiceUuid));
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x800710DF)
            {
                // Catch exception HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE).
                return;
            }

            m_socketListener = new StreamSocketListener();
            m_socketListener.ConnectionReceived += OnConnectionReceivedAsync;
            var rfcomm = m_rfcommService.ServiceId.AsString();

            await m_socketListener.BindServiceNameAsync(
                m_rfcommService.ServiceId.AsString(),
                SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            var sdpWriter = new DataWriter
            {
                UnicodeEncoding = UnicodeEncoding.Utf8
            };

            sdpWriter.WriteByte(SdpServiceNameAttributeType);
            sdpWriter.WriteByte((byte)SdpServiceName.Length);
            sdpWriter.WriteString(SdpServiceName);

            m_rfcommService.SdpRawAttributes.Add(SdpServiceNameAttributeId, sdpWriter.DetachBuffer());

            try
            {
                m_rfcommService.StartAdvertising(m_socketListener, true);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private async Task<bool> InitializeRfcommClient(IRemoteDevice remoteServer)
        {
            var accessStatus = DeviceAccessInformation.CreateFromId(remoteServer.DeviceId).CurrentStatus;
            if (accessStatus == DeviceAccessStatus.DeniedByUser)
            {
                Debug.Write("This app does not have access to connect to the remote device (please grant access in Settings > Privacy > Other Devices");
                return false;
            }

            var success = await remoteServer.TryAndConnectToDevice();
            var bluetoothDevice = (RemoteBluetoothDevice)remoteServer;

            m_socket = await bluetoothDevice.EstablishSocketEndPoint();

            m_messageOutstream = new DataWriter(m_socket.OutputStream);
            m_messageInStream = new DataReader(m_socket.InputStream);
            return success;
        }

        private async void OnConnectionReceivedAsync(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            m_socket = args.Socket;
            var remoteDevice = await BluetoothDevice.FromHostNameAsync(m_socket.Information.RemoteHostName);

            m_messageOutstream = new DataWriter(m_socket.OutputStream);
            m_messageInStream = new DataReader(m_socket.InputStream);

            var readSuccess = false;
            do
            {
                readSuccess = await OnMessageReceived();
            } while (readSuccess);
        }
#endif
    }
}
