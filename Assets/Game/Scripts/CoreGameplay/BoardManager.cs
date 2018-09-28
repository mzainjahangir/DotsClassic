using System;
using Dots.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Dots
{
    /// <summary>
    ///     Class responsible for managing the board.
    /// </summary>
    public class BoardManager : Singleton<BoardManager>
    {
        #region Members

        [SerializeField] private Board _boardToBeUsed;
        [SerializeField] private BoardCell _boardCell;
        [SerializeField] private Transform _boardInScene;

        public BoardCell[,] BoardPositions { get; private set; }
        
        #endregion

        #region Initialization

        protected virtual void Start()
        {
            if (_boardToBeUsed == null)
            {
                Debug.LogError("Board Manager requires a Board reference to create a game board.");
            }
            else
            {
                BoardPositions = new BoardCell[_boardToBeUsed.Rows, _boardToBeUsed.Columns];

                var layout = _boardInScene.GetComponent<GridLayoutGroup>();

                if (layout == null)
                {
                    Debug.LogError("Board requires a Grid Layout to be added onto the board to manage it's positions.");
                }
                else
                {
                    layout.constraintCount = _boardToBeUsed.Columns;
                }

                CreateBoard();
            }

            MoveManager.Instance.SquareSelected += MoveManager_OnSquareSelected;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MoveManager.Instance.SquareSelected -= MoveManager_OnSquareSelected;
        }

        /// <summary>
        /// Creates the board.
        /// </summary>
        private void CreateBoard()
        {
            if ((_boardToBeUsed.Columns < 0) || (_boardToBeUsed.Columns > GlobalConstants.MaxColumns)
                || (_boardToBeUsed.Rows < 0) || (_boardToBeUsed.Rows > GlobalConstants.MaxRows))
            {
                Debug.LogError("Invalid board values.");
            }
            else
            {
                for (var i = 0; i < _boardToBeUsed.Rows; i++)
                {
                    for (var j = 0; j < _boardToBeUsed.Columns; j++)
                    {
                        CreateBoardPosition(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the board position.
        /// </summary>
        /// <param name="i">The row number.</param>
        /// <param name="j">The column number.</param>
        private void CreateBoardPosition(int i, int j)
        {
            var boardCell = Instantiate(_boardCell.gameObject);
            boardCell.transform.SetParent(_boardInScene);
            boardCell.transform.localScale = Vector3.one;
            boardCell.transform.name = string.Format("Cell ({0}, {1})", i, j);
            var boardCellComponent = boardCell.GetComponent<BoardCell>();
            boardCellComponent.Row = i;
            boardCellComponent.Column = j;
            boardCellComponent.ContainingDot = null;
            BoardPositions[i, j] = boardCellComponent;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Refreshes the board by going through each board position and filling it up.
        /// </summary>
        public void Refresh()
        {
            for (var i = _boardToBeUsed.Rows - 1; i >= 0; i--)
            {
                for (var j = _boardToBeUsed.Columns - 1; j >= 0; j--)
                {
                    if (BoardPositions[i, j].IsEmpty)
                    {
                        FillEmptyCell(i, j);
                    }
                }
            }
            DotSpawner.Instance.RemovedSquareColor = Color.clear;
        }

        /// <summary>
        /// Fills the empty cell starting from bottom going upwards.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        private void FillEmptyCell(int row, int column)
        {
            if (row != 0)
            {
                for (var i = row - 1; i >= 0; i--)
                {
                    if (BoardPositions[i, column].IsEmpty)
                    {
                        continue;
                    }
                    BoardPositions[row, column].ContainingDot = BoardPositions[i, column].ContainingDot;
                    BoardPositions[i, column].ContainingDot = null;
                    BoardPositions[row, column].ContainingDot.UpdateCoordinates(row, column);
                    BoardPositions[row, column].ContainingDot.MoveToPosition(BoardPositions[row, column].transform);
                    return;
                }
            }
            DotSpawner.Instance.CreateDot(BoardPositions[row, column]);
        }

        #endregion
        
        #region Event Callbacks

        private void MoveManager_OnSquareSelected(object sender, EventArgs eventArgs)
        {
            foreach (var cell in BoardPositions)
            {
                switch (cell.ContainingDot.DotType)
                {
                    case DotTypes.None:
                        Debug.LogError("Invalid Dot Type");
                        break;
                    case DotTypes.Normal:
                        var dot = (NormalDot)cell.ContainingDot;
                        if (dot.DotColor == MoveManager.Instance.StartedColor)
                        {
                            dot.Select();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion

    }
}