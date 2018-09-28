using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dots
{
    /// <summary>
    ///     Basic dot class that defines the core components and functionality of a dot.
    /// </summary>
    public class Dot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {
        #region Members

        [SerializeField] private Image _mainImage;
        [SerializeField] private Image _highlightImage;
        [SerializeField] private DotAnimator _animator;

        /// <summary>
        /// Gets the animator for dot.
        /// </summary>
        public DotAnimator Animator
        {
            get { return _animator; }
        }

        /// <summary>
        /// Occurs when a any dot is selected.
        /// </summary>
        public event EventHandler<DotEventArgs> Selected;

        /// <summary>
        /// Occurs when any dot is deselected.
        /// </summary>
        public event EventHandler<DotEventArgs> Deselected;

        /// <summary>
        /// Occurs when a touch enters.
        /// </summary>
        public event EventHandler<DotEventArgs> Entered;

        /// <summary>
        /// Gets the row number.
        /// </summary>
        /// <value>
        /// The row number.
        /// </value>
        public int RowNumber { get; private set; }

        /// <summary>
        /// Gets the column number.
        /// </summary>
        /// <value>
        /// The column number.
        /// </value>
        public int ColumnNumber { get; private set; }

        /// <summary>
        /// Gets the type of the dot.
        /// </summary>
        /// <value>
        /// The type of the dot.
        /// </value>
        public DotTypes DotType { get; private set; }

        /// <summary>
        /// Gets the main image.
        /// </summary>
        /// <value>
        /// The main image.
        /// </value>
        public Image MainImage
        {
            get { return _mainImage; }
        }

        /// <summary>
        /// Gets the highlight image.
        /// </summary>
        /// <value>
        /// The highlight image.
        /// </value>
        public Image HighlightImage
        {
            get { return _highlightImage; }
        }

        #endregion

        #region Initialization

        protected virtual void OnEnable()
        {
            if (MainImage != null)
            {
                MainImage.gameObject.SetActive(true);
            }
            if (HighlightImage != null)
            {
                HighlightImage.gameObject.SetActive(false);
            }
        }

        protected virtual void OnDisable()
        {
            if (MainImage != null)
            {
                MainImage.gameObject.SetActive(false);
            }
            if (HighlightImage != null)
            {
                HighlightImage.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Core Functionality

        /// <summary>
        /// Setups the dot.
        /// </summary>
        /// <param name="dotType">Type of the dot.</param>
        /// <param name="cell">The cell.</param>
        public virtual void Setup(DotTypes dotType, BoardCell cell)
        {
            DotType = dotType;
            UpdateCoordinates(cell.Row, cell.Column);
        }

        /// <summary>
        /// Updates the coordinates.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void UpdateCoordinates(int row, int column)
        {
            // For ease of debugging we rename the transform to the assigned board position
            transform.name = string.Format("[{0},{1}]", row, column);
            RowNumber = row;
            ColumnNumber = column;
        }

        /// <summary>
        /// Sets the spawn position from where the dot is suppose to fall in from.
        /// </summary>
        /// <param name="spawningPosition">The spawning position.</param>
        public void SetSpawnPosition(Transform spawningPosition)
        {
            transform.SetParent(spawningPosition);
            transform.localScale = Vector3.one;
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(GlobalConstants.DotToCellPadding,
                GlobalConstants.DotToCellPadding);
            rectTransform.offsetMax = new Vector2(-GlobalConstants.DotToCellPadding,
                -GlobalConstants.DotToCellPadding);
        }

        /// <summary>
        /// Moves to the specified position.
        /// </summary>
        /// <param name="cellTransform">The cell transform.</param>
        public void MoveToPosition(Transform cellTransform)
        {
            transform.SetParent(cellTransform);
            transform.localScale = Vector3.one;
            Animator.Move(cellTransform);
        }

        /// <summary>
        /// Selects this instance.
        /// </summary>
        public virtual void Select()
        {
            OnSelected(new DotEventArgs(this));
        }

        /// <summary>
        /// De-selects this instance.
        /// </summary>
        public virtual void Deselect()
        {
            OnDeselected(new DotEventArgs(this));
        }

        #endregion

        #region Event Invocators

        protected virtual void OnDeselected(DotEventArgs e)
        {
            var handler = Deselected;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelected(DotEventArgs e)
        {
            var handler = Selected;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnEntered(DotEventArgs e)
        {
            var handler = Entered;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region Handlers

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Select();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Deselect();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEntered(new DotEventArgs(this));
        }

        #endregion
    }

    /// <summary>
    ///     Class used for sending the dot data through events.
    /// </summary>
    public class DotEventArgs : EventArgs
    {
        public Dot CurrentDot;

        public DotEventArgs(Dot currentDot)
        {
            CurrentDot = currentDot;
        }
    }
}