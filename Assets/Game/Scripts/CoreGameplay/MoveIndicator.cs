using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dots
{
    /// <summary>
    ///     Class responsible for showing any visual aid regarding moves made.
    ///     Also contains the hand held vibration upon square selection.
    /// </summary>
    public class MoveIndicator : MonoBehaviour
    {
        [SerializeField] private Image _squareOverlay;
        [SerializeField] private LineRenderer _topIndicator;
        [SerializeField] private LineRenderer _bottomIndicator;
        [SerializeField] private List<PredefinedPositions> _steps;

        private int _selectedCount;

        protected virtual void Start()
        {
            _selectedCount = 0;

            if (!MoveManager.HasInstance)
            {
                return;
            }
            MoveManager.Instance.DotSelected += MoveManager_OnDotSelected;
            MoveManager.Instance.DotDeselected += MoveManager_OnDotDeselected;
            MoveManager.Instance.SquareSelected += MoveManager_OnSquareSelected;
            MoveManager.Instance.MoveEnded += MoveManager_OnMoveEnded;
        }

        protected virtual void OnDestroy()
        {
            if (!MoveManager.HasInstance)
            {
                return;
            }
            MoveManager.Instance.DotSelected -= MoveManager_OnDotSelected;
            MoveManager.Instance.DotDeselected -= MoveManager_OnDotDeselected;
            MoveManager.Instance.SquareSelected -= MoveManager_OnSquareSelected;
            MoveManager.Instance.MoveEnded -= MoveManager_OnMoveEnded;
        }

        private void MoveManager_OnDotSelected(object sender, EventArgs eventArgs)
        {
            _selectedCount++;
            UpdateIndicator();
        }

        private void MoveManager_OnDotDeselected(object sender, EventArgs eventArgs)
        {
            _selectedCount--;
            UpdateIndicator();
        }

        private void MoveManager_OnSquareSelected(object sender, EventArgs eventArgs)
        {
            Handheld.Vibrate();
            UpdateIndicator();
        }

        private void MoveManager_OnMoveEnded(object sender, EventArgs eventArgs)
        {
            _selectedCount = 0;
            UpdateIndicator();
        }

        private void UpdateIndicator()
        {
            var startedColor = MoveManager.Instance.StartedColor;
            var isSquare = MoveManager.Instance.IsSquare();

            if (_selectedCount == 0)
            {
                Clear();
            }
            else
            {
                if (isSquare)
                {
                    _squareOverlay.color = new Color(startedColor.r, startedColor.g, startedColor.b, 0.2f);
                }

                _squareOverlay.gameObject.SetActive(isSquare);
                _topIndicator.material.color = startedColor;
                _bottomIndicator.material.color = startedColor;

                // If the player has selected 10 or more or has selected a square show the max indicator
                if ((_selectedCount > _steps.Count) || isSquare)
                {
                    DrawLines(_steps.Count - 1);
                    return;
                }

                DrawLines(_selectedCount - 1);
            }
        }

        /// <summary>
        ///     Draws the lines.
        /// </summary>
        /// <param name="index">The index of steps list you want to draw from.</param>
        private void DrawLines(int index)
        {
            _topIndicator.numPositions = _steps[index].UpperCoordinates.Length;
            _bottomIndicator.numPositions = _steps[index].LowerCoordinates.Length;
            _topIndicator.SetPositions(_steps[index].UpperCoordinates);
            _bottomIndicator.SetPositions(_steps[index].LowerCoordinates);
        }

        /// <summary>
        ///     Clears this indicators.
        /// </summary>
        private void Clear()
        {
            _topIndicator.numPositions = 0;
            _bottomIndicator.numPositions = 0;
            _squareOverlay.gameObject.SetActive(false);
        }
    }

    /// <summary>
    ///     Structure used to define the steps of side indicator.
    /// </summary>
    [Serializable]
    internal struct PredefinedPositions
    {
        // Realized there was a typo after setting up the values... :)
        [FormerlySerializedAs("_upperCorodinates"), SerializeField] private Vector3[] _upperCoordinates;

        public Vector3[] UpperCoordinates
        {
            get { return _upperCoordinates; }
            set { _upperCoordinates = value; }
        }

        [FormerlySerializedAs("_upperCorodinates"), SerializeField] private Vector3[] _lowerCoordinates;

        public Vector3[] LowerCoordinates
        {
            get { return _lowerCoordinates; }
            set { _lowerCoordinates = value; }
        }
    }
}
