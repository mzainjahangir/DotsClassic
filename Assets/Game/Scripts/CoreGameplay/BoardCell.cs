using UnityEngine;

namespace Dots
{
    /// <summary>
    ///     The class representing the board cell that will hold the dot.
    /// </summary>
    public class BoardCell : MonoBehaviour
    {
        /// <summary>
        /// To see if the slot is empty or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> If the ContainingDot is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return ContainingDot == null; }
        }

        /// <summary>
        /// Gets or sets the row.
        /// </summary>
        /// <value>
        /// The row.
        /// </value>
        public int Row { get; set; }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets the containing dot.
        /// </summary>
        /// <value>
        /// The containing dot.
        /// </value>
        public Dot ContainingDot { get; set; }
    }
}
