namespace TicTacToeMR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    public class GameBoard : MonoBehaviour
    {
        private GameObject GameCellPrefab;
        private List<List<GameObject>> m_gameBoard;
        private const int GridSize = 3;
        private const float GridLength = 0.5f;

        private void Awake()
        {
            m_gameBoard = new List<List<GameObject>>();
            for(var i = 0; i < GridSize; ++i)
            {
                m_gameBoard.Add(new List<GameObject>());
                for(var j = 0; j < GridSize; ++j)
                {
                    m_gameBoard[i].Add(Instantiate(GameCellPrefab));
                    m_gameBoard[i][j].transform.localPosition = new Vector2(i * GridLength, j * GridLength);
                }
            }
        }

        public bool CheckForWinner(string player)
        {
            var winner = false;
            winner |= AllSame(player, 0, 0, 1, 1);
            winner |= AllSame(player, GridSize - 1, 0, -1, 1);

            for (int i = 0; i < GridSize && !winner; ++i)
            {
                winner |= AllSame(player, 0, i, 1, 0);
                winner |= AllSame(player, i, 0, 0, 1);
            }

            return winner;
        }

        private bool AllSame(string player, int x0, int y0, int dx, int dy)
        {
            var same = true;
            for(int i = 1; i < GridSize; ++i)
            {
                same = same && m_gameBoard[x0][y0] == m_gameBoard[x0 + i * dx][y0 + i * dy];
            }

            return same;
        }
    }
}
