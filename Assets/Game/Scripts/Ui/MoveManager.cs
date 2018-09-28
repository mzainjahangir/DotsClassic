using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Dots.Utils;
using UnityEngine;

namespace Dots
{
    /// <summary>
    ///     Class responsible for managing the player moves.
    /// </summary>
    public class MoveManager : Singleton<MoveManager>
    {
        #region Members

        public bool HasMoveStarted { get; private set; }
        public Color StartedColor { get; private set; }

        /// <summary>
        /// Occurs when a dot is selected while the player is making a move.
        /// </summary>
        public event EventHandler DotSelected;
        
        /// <summary>
        /// Occurs when a dot is deselected while the player is making a move.
        /// </summary>
        public event EventHandler DotDeselected;

        /// <summary>
        /// Occurs when a square is Selected.
        /// </summary>
        public event EventHandler SquareSelected;

        /// <summary>
        /// Occurs when the move has ended
        /// </summary>
        public event EventHandler MoveEnded;

        private readonly List<Dot> _selectedDots = new List<Dot>();
        private bool _isSquareAnnounced;
        private int _lastSelectedDotNumber;
        private int _dotsRemoved;
        
        #endregion
        
        #region Initialization

        /// <summary>
        /// This is to be called after your board is setup and ready to detect moves.
        /// Adds the listeners to each dot on the board.
        /// </summary>
        public void Startup()
        {
            foreach (var dot in BoardManager.Instance.BoardPositions)
            {
                AddEventListeners(dot.ContainingDot);
            }
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// Removes the listeners subscribed to in the start.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!DotSpawner.HasInstance)
            {
                return;
            }
            foreach (var dot in BoardManager.Instance.BoardPositions)
            {
                RemoveEventListeners(dot.ContainingDot);
            }
        }

        #endregion

        #region Event Callbacks

        private void Dot_OnSelected(object sender, DotEventArgs dotEventArgs)
        {
            if (!HasMoveStarted)
            {
                StartMove(dotEventArgs.CurrentDot);
            }
        }

        private void Dot_OnDeselected(object sender, DotEventArgs dotEventArgs)
        {
            if (!HasMoveStarted)
            {
                return;
            }
            CheckMove();
            LinePainter.Instance.StopPainting();
        }

        private void Dot_OnEntered(object sender, DotEventArgs dotEventArgs)
        {
            if (!HasMoveStarted)
            {
                return;
            }

            // The dot you just entered
            var currentDot = dotEventArgs.CurrentDot;

            switch (currentDot.DotType)
            {
                case DotTypes.None:
                    Debug.LogError("Invalid Dot Type");
                    break;
                case DotTypes.Normal:
                    if (IsValid((NormalDot)currentDot))
                    {
                        if ((_selectedDots.Count >= 2) && IsDeselecting(currentDot))
                        {
                            _selectedDots[_lastSelectedDotNumber].Animator.FinishAnimations();
                            _selectedDots.RemoveAt(_lastSelectedDotNumber);
                            _lastSelectedDotNumber--;
                            LinePainter.Instance.RemoveLastPosition();
                            OnDotDeselected();
                        }
                        else
                        {
                            _isSquareAnnounced = IsSquare();
                            if ((!IsSquare() && _selectedDots.Contains(currentDot)) ||
                                !_selectedDots.Contains(currentDot))
                            {
                                currentDot.Select();
                                _selectedDots.Add(currentDot);
                                _lastSelectedDotNumber++;
                                LinePainter.Instance.AddPosition(currentDot.transform.position);
                                OnDotSelected();
                            }
                            if (IsSquare() && !_isSquareAnnounced)
                            {
                                _isSquareAnnounced = true;
                                OnSquareSelected();
                            }
                        }
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        #endregion

        #region Core Functionality

        /// <summary>
        /// Adds the event listeners.
        /// </summary>
        /// <param name="dot">The dot.</param>
        public void AddEventListeners(Dot dot)
        {
            dot.Selected += Dot_OnSelected;
            dot.Entered += Dot_OnEntered;
            dot.Deselected += Dot_OnDeselected;
        }

        /// <summary>
        /// Removes the event listeners.
        /// </summary>
        /// <param name="dot">The dot.</param>
        public void RemoveEventListeners(Dot dot)
        {
            dot.Selected -= Dot_OnSelected;
            dot.Entered -= Dot_OnEntered;
            dot.Deselected -= Dot_OnDeselected;
        }

        /// <summary>
        /// Starts the move.
        /// </summary>
        /// <param name="currentDot">The current dot.</param>
        private void StartMove(Dot currentDot)
        {
            switch (currentDot.DotType)
            {
                case DotTypes.None:
                    Debug.LogError("Invalid Dot Type");
                    break;
                case DotTypes.Normal:
                    StartedColor = ((NormalDot) currentDot).DotColor;
                    _selectedDots.Add(currentDot);
                    _lastSelectedDotNumber = 0;
                    HasMoveStarted = true;
                    OnDotSelected();
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
            LinePainter.Instance.SetColor(StartedColor);
            LinePainter.Instance.AddPosition(currentDot.transform.position);
        }

        /// <summary>
        /// Checks the move upon finish.
        /// </summary>
        private void CheckMove()
        {
            if (_selectedDots.Count > 1) // If more than one dot was selected then we need to check the move
            {
                _dotsRemoved = 0;
                if (IsSquare())
                {
                    DotSpawner.Instance.RemovedSquareColor = StartedColor;
                    AddSameColoredDotsToSelected();
                }

                foreach (var dot in _selectedDots)
                {
                    dot.Animator.FinishAnimations();
                    dot.Animator.Disappear(dot.gameObject);
                    BoardManager.Instance.BoardPositions[dot.RowNumber, dot.ColumnNumber].ContainingDot = null;
                    RemoveEventListeners(dot);
                    StartCoroutine(WaitForDisappear(dot.gameObject));
                }
            }
            else
            {
                EndMove();
            }
        }

        /// <summary>
        /// Ends the move by clearing and reseting all local fields.
        /// </summary>
        private void EndMove()
        {
            StartedColor = Color.clear;
            HasMoveStarted = false;
            _isSquareAnnounced = false;
            _selectedDots.Clear();
            OnMoveEnded();
            BoardManager.Instance.Refresh();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds the same colored dots to selected before the move ends so they can be removed.
        /// </summary>
        private void AddSameColoredDotsToSelected()
        {
            foreach (var cell in BoardManager.Instance.BoardPositions)
            {
                if (cell == null || cell.ContainingDot == null)
                {
                    continue;
                }
                switch (cell.ContainingDot.DotType)
                {
                    case DotTypes.None:
                        Debug.Log("Invalid Dot Type");
                        break;
                    case DotTypes.Normal:
                        var dot = (NormalDot)cell.ContainingDot;
                        if (dot.DotColor == StartedColor && !_selectedDots.Contains(dot))
                        {
                            _selectedDots.Add(dot);
                        }
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            }
        }

        /// <summary>
        /// Waits for the dots to disappear before ending the move.
        /// </summary>
        /// <param name="dotObject">The dot object.</param>
        private IEnumerator WaitForDisappear(GameObject dotObject)
        {
            yield return new WaitUntil(() => iTween.Count(dotObject) == 0);

            PoolManager.Instance.DestroyObject(dotObject);
            _dotsRemoved++;

            if (_dotsRemoved == _selectedDots.Count)
            {
                EndMove();
            }
        }
        
        /// <summary>
        /// Determines whether the current dot is the next-to-last selected dot.
        /// </summary>
        /// <param name="currentDot">The current dot.</param>
        /// <returns>
        ///   <c>true</c> if the specified current dot is next-to-last selected; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDeselecting(Dot currentDot)
        {
            return currentDot == _selectedDots[_lastSelectedDotNumber - 1];
        }
        
        /// <summary>
        /// Checks to see if the dot just entered is a valid dot.
        /// </summary>
        /// <param name="dot">The dot.</param>
        /// <returns>
        ///   <c>true</c> If the specified dot is of the same color and is a neighbor; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValid(NormalDot dot)
        {
            return (StartedColor != Color.clear) && (dot.DotColor == StartedColor) && IsNeighbor(dot);
        }

        /// <summary>
        /// Determines whether the specified dot is neighbor to the last selected dot.
        /// </summary>
        /// <param name="dot">The dot to check.</param>
        /// <returns>
        ///   <c>true</c> If the specified dot is neighbor; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNeighbor(Dot dot)
        {
            if (_selectedDots[_lastSelectedDotNumber].ColumnNumber == dot.ColumnNumber)
            {
                return (dot.RowNumber == _selectedDots[_lastSelectedDotNumber].RowNumber + 1) ||
                       (dot.RowNumber == _selectedDots[_lastSelectedDotNumber].RowNumber - 1);
            }

            if (_selectedDots[_lastSelectedDotNumber].RowNumber == dot.RowNumber)
            {
                return (dot.ColumnNumber == _selectedDots[_lastSelectedDotNumber].ColumnNumber + 1) ||
                       (dot.ColumnNumber == _selectedDots[_lastSelectedDotNumber].ColumnNumber - 1);
            }
            return false;
        }

        /// <summary>
        /// Determines whether a square is selected or not.
        /// </summary>
        /// <returns>
        ///   <c>true</c> If this instance is square; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSquare()
        {
            var selected = new HashSet<Dot>();

            foreach (var dot in _selectedDots)
            {
                if (selected.Contains(dot))
                {
                    return true;
                }
                selected.Add(dot);
            }
            return false;
        }

        #endregion
        
        #region EventInvocators

        protected virtual void OnSquareSelected()
        {
            var handler = SquareSelected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnDotSelected()
        {
            var handler = DotSelected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnDotDeselected()
        {
            var handler = DotDeselected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnMoveEnded()
        {
            var handler = MoveEnded;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
