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
    using Microsoft.MixedReality.Toolkit.Input;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;

    public class GameBoardCell : MonoBehaviour, IMixedRealityTouchHandler
    {
        private string m_player;
        private TextMeshPro m_cellValue;

        public string CellValue => m_cellValue.text;
        public void SetPlayerInformation(string player)
        {
            OnPlayerMakesMove();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
        }

        private void OnPlayerMakesMove()
        {
            if(m_cellValue != null)
            {
                m_cellValue.text = m_player;
            }
        }
    }
}
