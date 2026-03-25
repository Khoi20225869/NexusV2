using SoulForge.Data;
using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerStorePresenter : MonoBehaviour
    {
        [SerializeField] private ViewerStoreCatalog storeCatalog;
        [SerializeField] private ViewerWebSocketClient remoteViewerClient;
        [SerializeField] private ViewerTargetingController targetingController;
        [SerializeField] private ViewerStoreButton[] actionButtons;
        [SerializeField] private TMP_Text headerText;

        private void Awake()
        {
            if (remoteViewerClient == null)
            {
                remoteViewerClient = FindFirstObjectByType<ViewerWebSocketClient>();
            }
        }

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (headerText != null)
            {
                headerText.text = "Viewer Store";
            }

            if (actionButtons == null)
            {
                return;
            }

            int actionCount = storeCatalog != null && storeCatalog.Actions != null ? storeCatalog.Actions.Count : 0;

            for (int i = 0; i < actionButtons.Length; i++)
            {
                ViewerStoreButton button = actionButtons[i];
                if (button == null)
                {
                    continue;
                }

                bool hasAction = i < actionCount && storeCatalog.Actions[i] != null;
                button.gameObject.SetActive(hasAction);

                if (!hasAction)
                {
                    continue;
                }

                button.Setup(
                    storeCatalog.Actions[i],
                    remoteViewerClient,
                    targetingController);
            }
        }
    }
}
