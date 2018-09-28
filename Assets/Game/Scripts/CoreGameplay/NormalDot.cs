using UnityEngine;

namespace Dots
{
    /// <summary>
    ///     Class responsible for setting up a normal colored dot.
    /// </summary>
    public class NormalDot : Dot
    {
        private Color _dotColor;

        /// <summary>
        /// Gets or sets the color of the dot along with the color of the main texture and highlight.
        /// </summary>
        /// <value>
        /// The color of the dot.
        /// </value>
        public Color DotColor
        {
            get { return _dotColor; }
            set
            {
                _dotColor = value;
                MainImage.color = _dotColor;
                HighlightImage.color = _dotColor;
            }
        }

        public override void Select()
        {
            base.Select();
            Animator.Highlight();
        }

        public override void Deselect()
        {
            base.Deselect();
            Animator.FinishAnimations();
        }
    }
}

