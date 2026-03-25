using SoulForge.Data;
using SoulForge.Viewer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ViewerStoreButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image background;
        [SerializeField] private Color availableColor = new(0.18f, 0.45f, 0.24f, 1f);
        [SerializeField] private Color unavailableColor = new(0.33f, 0.18f, 0.18f, 1f);

        private Button button;
        private ViewerActionDefinition actionDefinition;
        private ViewerWebSocketClient remoteViewerClient;
        private ViewerTargetingController targetingController;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Submit);
        }

        private void Update()
        {
            RefreshState();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(Submit);
            }
        }

        public void Setup(
            ViewerActionDefinition action,
            ViewerWebSocketClient targetRemoteViewerClient,
            ViewerTargetingController targetTargetingController)
        {
            actionDefinition = action;
            remoteViewerClient = targetRemoteViewerClient;
            targetingController = targetTargetingController;

            if (labelText != null)
            {
                labelText.text = action != null && !string.IsNullOrWhiteSpace(action.DisplayName) ? action.DisplayName : action != null ? action.ActionId : "missing_action";
            }

            if (priceText != null)
            {
                priceText.text = action != null ? $"{action.Price} C" : "-";
            }

            if (descriptionText != null)
            {
                descriptionText.text = action != null ? action.Description : string.Empty;
            }

            if (background != null && action != null)
            {
                background.color = action.AccentColor;
            }

            RefreshState();
        }

        private void Submit()
        {
            if (actionDefinition == null)
            {
                return;
            }

            if (actionDefinition.RequiresWorldTarget && targetingController != null)
            {
                targetingController.BeginTargeting(actionDefinition);
                return;
            }

            remoteViewerClient?.SubmitAction(actionDefinition.ActionId, actionDefinition.TargetId);
        }

        private void RefreshState()
        {
            if (button == null || actionDefinition == null)
            {
                return;
            }

            bool ready = remoteViewerClient != null && remoteViewerClient.IsConnected;
            button.interactable = ready;

            if (background != null)
            {
                background.color = ready ? Color.Lerp(actionDefinition.AccentColor, availableColor, 0.55f) : unavailableColor;
            }

            if (stateText != null)
            {
                stateText.text = !ready ? "Offline" : actionDefinition.RequiresWorldTarget ? "Target" : "Send";
            }
        }
    }
}
