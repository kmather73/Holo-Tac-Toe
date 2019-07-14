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
#if WINDOWS_UWP
namespace KMR.Communication.Devices
{
    using System;
    using System.Threading.Tasks;
    using Windows.Storage.Streams;

    public class BluethoothMessageService : IMessageService
    {
        private IMessageOrchestrator m_orchestrator;

        public BluethoothMessageService(IMessageOrchestrator orchestrator)
        {
            m_orchestrator = orchestrator;
            m_orchestrator.RegisterMessageService(this);
        }

        public short ServiceId => 0;
        public bool HasMessageToSend => false;
        public bool AreNotificationSupported { get; private set; }

        public bool ReadMessage(IRemoteMessage message)
        {
            // Do something with this
            return true;
        }

        public bool SendMessage(IRemoteMessage message)
        {
            return m_orchestrator.TransmitMessage(message);
        }
    }
}
#endif
