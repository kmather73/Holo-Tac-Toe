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
    using System.Threading.Tasks;

    public interface IDeviceLocator : INotifyPropertyChanged
    {
        event EventHandler<IRemoteDevice> DeviceFound;
        event EventHandler<IRemoteDevice> DeviceLost;
        event EventHandler<IRemoteDevice> DeviceUpdated;
        event EventHandler InitialDeviceEnumerationCompleted;

        IList<IRemoteDevice> RemoteDevices { get; }

        void StartSearchingForDevice();
        void StopSearchingForDevice();

        // Must be run on UI Thread
        Task<bool> ConnectDeviceAsync(IRemoteDevice device);
    }
}
