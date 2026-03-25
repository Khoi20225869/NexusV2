using SoulForge.Data;
using SoulForge.Viewer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    public sealed class ViewerTargetingController : MonoBehaviour
    {
        [SerializeField] private ViewerWebSocketClient viewerWebSocketClient;
        [SerializeField] private TMP_Text targetingHintText;
        [SerializeField] private Image targetingOverlay;
        [SerializeField] private Button cancelButton;

        private ViewerActionDefinition pendingAction;

        public bool IsTargeting => pendingAction != null;

        private void Awake()
        {
            if (viewerWebSocketClient == null)
            {
                viewerWebSocketClient = FindFirstObjectByType<ViewerWebSocketClient>();
            }
        }

        private void OnEnable()
        {
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelTargeting);
            }

            RefreshVisuals();
        }

        private void OnDisable()
        {
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(CancelTargeting);
            }
        }

        public void BeginTargeting(ViewerActionDefinition action)
        {
            if (action == null || !action.RequiresWorldTarget)
            {
                return;
            }

            pendingAction = action;
            RefreshVisuals();
        }

        public void CancelTargeting()
        {
            pendingAction = null;
            RefreshVisuals();
        }

        public void SubmitViewportTarget(Vector2 viewportPoint)
        {
            if (pendingAction == null || viewerWebSocketClient == null)
            {
                return;
            }

            viewerWebSocketClient.SubmitAction(
                pendingAction.ActionId,
                pendingAction.TargetId,
                hasViewportTarget: true,
                viewportX: Mathf.Clamp01(viewportPoint.x),
                viewportY: Mathf.Clamp01(viewportPoint.y));

            pendingAction = null;
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (targetingOverlay != null)
            {
                targetingOverlay.enabled = pendingAction != null;
                targetingOverlay.raycastTarget = false;
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(pendingAction != null);
            }

            if (targetingHintText != null)
            {
                targetingHintText.text = pendingAction != null
                    ? $"Targeting: {pendingAction.DisplayName}. Click the live view."
                    : "Choose an action, then click the live view to place it.";
            }
        }
    }
}
