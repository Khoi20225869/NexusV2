using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SoulForge.UI
{
    public sealed class ViewerTargetSurface : MonoBehaviour, IPointerClickHandler
    {
        private const string ViewportName = "Viewport";

        [SerializeField] private RectTransform viewportRect;
        [SerializeField] private ViewerTargetingController targetingController;
        [SerializeField] private TMP_Text clickFeedbackText;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (targetingController == null || !targetingController.IsTargeting || viewportRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewportRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return;
            }

            Rect rect = viewportRect.rect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
            targetingController.SubmitViewportTarget(new Vector2(normalizedX, normalizedY));

            if (clickFeedbackText != null)
            {
                clickFeedbackText.text = $"Placed at {normalizedX:0.00}, {normalizedY:0.00}";
            }
        }

        private void EnsureReferences()
        {
            RectTransform namedViewport = ResolveNamedViewportRect();
            if (namedViewport != null && namedViewport != viewportRect)
            {
                viewportRect = namedViewport;
            }
            else if (viewportRect == null || viewportRect.rect.width <= 0f || viewportRect.rect.height <= 0f)
            {
                viewportRect = ResolveViewportRect();
            }
        }

        private RectTransform ResolveViewportRect()
        {
            if (transform is RectTransform selfRect && selfRect.rect.width > 0f && selfRect.rect.height > 0f)
            {
                return selfRect;
            }

            RectTransform[] rects = FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
            for (int i = 0; i < rects.Length; i++)
            {
                RectTransform rect = rects[i];
                if (rect != null && rect.name == ViewportName && rect.gameObject.scene.IsValid())
                {
                    return rect;
                }
            }

            return transform as RectTransform;
        }

        private RectTransform ResolveNamedViewportRect()
        {
            RectTransform[] rects = FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
            for (int i = 0; i < rects.Length; i++)
            {
                RectTransform rect = rects[i];
                if (rect != null && rect.name == ViewportName && rect.gameObject.scene.IsValid())
                {
                    return rect;
                }
            }

            return null;
        }
    }
}
