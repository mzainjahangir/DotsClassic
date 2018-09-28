using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Dots
{
    /// <summary>
    ///     Responsible for any animations related to Dot.
    /// </summary>
    public class DotAnimator : MonoBehaviour
    {
        #region Members

        [SerializeField] private GameObject _maintImage;
        [SerializeField] private GameObject _highlightImage;

        private Color _mainColor;
        private RectTransform _mainRect;
        
        #endregion

        #region Tween Methods

        /// <summary>
        ///     Plays the Highlight animation.
        /// </summary>
        public void Highlight()
        {
            SetupHighlight();

            var highlightSize = new Vector3(1.3f, 1.3f, 1.3f);

            const float time = 0.7f;
            const iTween.EaseType easeType = iTween.EaseType.easeOutCubic;

            iTween.ScaleTo(_highlightImage, iTween.Hash(
                "name", "HighlightScaleUp",
                "scale", highlightSize,
                "time", time,
                "easetype", easeType
            ));

            iTween.ValueTo(gameObject, iTween.Hash(
                "name", "HighlightFadoOut",
                "from", _maintImage.GetComponent<Image>().color.a,
                "to", 0f,
                "time", time,
                "easetype", easeType,
                "onupdate", "ColorUpdate"
            ));
        }

        /// <summary>
        /// Moves the object to the specified empty slot.
        /// </summary>
        /// <param name="emptySlot">The empty slot.</param>
        public void Move(Transform emptySlot)
        {
            SetupMove();

            const float time = 0.5f;
            const iTween.EaseType easeType = iTween.EaseType.easeOutBounce;

            iTween.ValueTo(gameObject, iTween.Hash(
                "name", "MoveOffsetMin",
                "from", _mainRect.offsetMin,
                "to", new Vector2(GlobalConstants.DotToCellPadding, GlobalConstants.DotToCellPadding),
                "time", time,
                "easetype", easeType,
                "onupdate", "OffsetMinUpdate"
            ));

            iTween.ValueTo(gameObject, iTween.Hash(
                "name", "MoveOffsetMax",
                "from", _mainRect.offsetMax,
                "to", new Vector2(-GlobalConstants.DotToCellPadding, -GlobalConstants.DotToCellPadding),
                // - because offsetMax is inversed
                "time", time,
                "easetype", easeType,
                "onupdate", "OffsetMaxUpdate"
            ));
        }

        /// <summary>
        /// Scales the specified target object to zero.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        public void Disappear(GameObject targetObject)
        {
            const float time = 0.2f;
            const iTween.EaseType easeType = iTween.EaseType.easeOutCirc;

            iTween.ScaleTo(targetObject, iTween.Hash(
                "name", "ObjectScaleDown",
                "scale", Vector3.zero,
                "time", time,
                "easetype", easeType
            ));
        }

        [UsedImplicitly]
        private void ColorUpdate(float alpha)
        {
            _highlightImage.GetComponent<Image>().color = new Color(_mainColor.r, _mainColor.g, _mainColor.b,
                alpha);
        }

        [UsedImplicitly]
        private void OffsetMinUpdate(Vector2 deltaOffset)
        {
            _mainRect.offsetMin = deltaOffset;
        }

        [UsedImplicitly]
        private void OffsetMaxUpdate(Vector2 deltaOffset)
        {
            _mainRect.offsetMax = deltaOffset;
        }

        #endregion

        #region Helper Methods

        private void SetupMove()
        {
            transform.parent.localScale = Vector3.one;
            _mainRect = transform.parent.GetComponent<RectTransform>();
        }

        private void SetupHighlight()
        {
            _mainColor = _maintImage.GetComponent<Image>().color;
            _highlightImage.GetComponent<Image>().color = _mainColor;
            _highlightImage.transform.localScale = Vector3.one;
            _highlightImage.SetActive(true);
        }

        public void FinishAnimations()
        {
            iTween.Stop(_highlightImage);
        }

        #endregion
    }
}


