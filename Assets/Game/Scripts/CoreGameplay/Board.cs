using UnityEngine;

namespace Dots
{
    /// <summary>
    ///     Class used to define the structure of a board.
    /// </summary>
    [CreateAssetMenu(order = 4000)]
    public class Board : ScriptableObject
    {
        [SerializeField] private int _rows;
        [SerializeField] private int _columns;

        /// <summary>
        ///     Gets the number of rows of the board.
        /// </summary>
        public int Rows
        {
            get { return _rows; }
        }

        /// <summary>
        ///     Gets the number of columns of the board.
        /// </summary>
        public int Columns
        {
            get { return _columns; }
        }
    }
}
