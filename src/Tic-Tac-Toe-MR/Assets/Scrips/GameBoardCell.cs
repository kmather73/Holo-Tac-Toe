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
