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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    public class GameBoard : MonoBehaviour
    {
        [SerializeField]
        private GameObject GameCellPrefab = null;
        private List<List<GameBoardCell>> m_gameBoard;
        private const int GridSize = 3;
        private const float GridLength = 0.5f;

        private void Awake()
        {
            m_gameBoard = new List<List<GameBoardCell>>();
            for(var i = 0; i < GridSize; ++i)
            {
                m_gameBoard.Add(new List<GameBoardCell>());
                for(var j = 0; j < GridSize; ++j)
                {
                    var cell = Instantiate(GameCellPrefab);
                    cell.transform.parent = this.transform;
                    cell.transform.localPosition = new Vector2(i * GridLength, j * GridLength);
                    m_gameBoard[i].Add(cell.GetComponent<GameBoardCell>());
                }
            }
        }

        public bool CheckForWinner(out string winner)
        {
            var win = AllSame(GridSize - 1, 0, -1, 1, out winner) || AllSame(0, 0, 1, 1, out winner);

            for (int i = 0; i < GridSize && !win; ++i)
            {
                win = AllSame(0, i, 1, 0, out winner) || AllSame(i, 0, 0, 1, out winner);
            }

            return win;
        }

        private bool AllSame(int x0, int y0, int dx, int dy, out string winner)
        {
            var same = true;
            for(int i = 1; i < GridSize; ++i)
            {
                same = same && m_gameBoard[x0][y0].CellValue == m_gameBoard[x0 + i * dx][y0 + i * dy].CellValue;
            }
            winner = same ? m_gameBoard[x0][y0].CellValue : null;
            return same;
        }
    }
}
