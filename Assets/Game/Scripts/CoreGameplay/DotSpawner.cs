using System.Collections.Generic;
using System.ComponentModel;
using Dots.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace Dots
{
    /// <summary>
    ///     Class responsible spawning the dots.
    /// </summary>
    public class DotSpawner : Singleton<DotSpawner>
    {
        /// <summary>
        ///     The colors to be used for colored dots.
        /// </summary>
        [SerializeField] private List<Color> _colors;

        /// <summary>
        ///     The spawning positions for new added dots.
        /// </summary>
        [SerializeField] private List<Transform> _spawningPositions;

        public Color RemovedSquareColor { set; get; }

        protected virtual void Start()
        {
            RemovedSquareColor = Color.clear;
            Assert.IsTrue(_colors.Count == GlobalConstants.NumberOfColors,
                "The amount of colors defined and setup on the Spawner are not the same.");
            CreateDotsInitially();
        }

        private void CreateDotsInitially()
        {
            BoardManager.Instance.Refresh();
            MoveManager.Instance.Startup();
        }

        /// <summary>
        ///     Method used to create a dot.
        /// </summary>
        /// <param name="cell">The cell you want to move the dot to.</param>
        /// <param name="type">The type of dot you want to create.</param>
        public void CreateDot(BoardCell cell, DotTypes type = DotTypes.Normal)
        {
            Dot dotCreated = null;
            switch (type)
            {
                case DotTypes.None:
                    Debug.LogError("Invalid Dot Type.");
                    break;
                case DotTypes.Normal:
                    dotCreated = PoolManager.Instance.GetPoolableObject().GetComponent<NormalDot>();
                    dotCreated.Setup(DotTypes.Normal, cell);
                    
                    // to prevent the same color as square from getting created right after the square is found.
                    int colorIndex;
                    do
                    {
                        colorIndex = Random.Range(0, _colors.Count);

                    } while (RemovedSquareColor == _colors[colorIndex]);

                    ((NormalDot) dotCreated).DotColor = _colors[colorIndex];
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            if (dotCreated == null)
            {
                Debug.LogError("No Dot created");
                return;
            }
            
            cell.ContainingDot = dotCreated;
            cell.ContainingDot.SetSpawnPosition(_spawningPositions[dotCreated.ColumnNumber]);
            cell.ContainingDot.MoveToPosition(cell.transform);
            MoveManager.Instance.AddEventListeners(dotCreated);
        }
    }
}