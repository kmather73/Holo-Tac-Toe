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
namespace TicTacToeMR
{
    using KMR.Communication.Devices;
    using Microsoft.MixedReality.Toolkit.Input;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    public class GameSelectManager : MonoBehaviour
    {
        private IMessageOrchestrator m_messageOrchestrator;
        private BluetoothDeviceLocator m_deviceLocator;
        private bool m_hasOtherPlayer;

        public async Task CreateSession()
        {
            m_messageOrchestrator = new BluetoothMessageOrchestrator(BluetoothMessageOrchestrator.Mode.Server);
            await m_messageOrchestrator.InitializeAsync();
        }

        public void LookForSession()
        {
            m_deviceLocator = new BluetoothDeviceLocator();
            m_deviceLocator.StartSearchingForDevice();
            m_deviceLocator.InitialDeviceEnumerationCompleted += OnFoundAllNearbyDeviceForNow;
        }

        private void OnEnable()
        {
            m_hasOtherPlayer = false;
        }

        private void OnDisable()
        {
            m_messageOrchestrator = null;
            m_hasOtherPlayer = false;
        }

        private void Update()
        {
            if (m_hasOtherPlayer)
            {

            }
        }

        private async void OnFoundAllNearbyDeviceForNow(object sender, EventArgs e)
        {
            m_messageOrchestrator = new BluetoothMessageOrchestrator(BluetoothMessageOrchestrator.Mode.Client);
            await m_messageOrchestrator.InitializeAsync();

            foreach (var device in m_deviceLocator.RemoteDevices)
            {
                var couldConnect = await m_messageOrchestrator.ConnectToServerAsync(device);
                if (couldConnect)
                {
                    break;
                }
            }
        }
    }
}
