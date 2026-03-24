using SoulForge.Data;
using SoulForge.Economy;
using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerStorePresenter : MonoBehaviour
    {
        [SerializeField] private ViewerStoreCatalog storeCatalog;
        [SerializeField] private ViewerActionExecutor actionExecutor;
        [SerializeField] private ViewerActionValidator actionValidator;
        [SerializeField] private ViewerEconomyService economyService;
        [SerializeField] private ViewerRoomBudgetService roomBudgetService;
        [SerializeField] private LocalViewerCommandTester localViewerCommandTester;
        [SerializeField] private ViewerWebSocketClient remoteViewerClient;
        [SerializeField] private ViewerStoreButton[] actionButtons;
        [SerializeField] private TMP_Text headerText;

        private void Awake()
        {
            if (actionExecutor == null)
            {
                actionExecutor = FindFirstObjectByType<ViewerActionExecutor>();
            }

            if (actionValidator == null)
            {
                actionValidator = FindFirstObjectByType<ViewerActionValidator>();
            }

            if (economyService == null)
            {
                economyService = FindFirstObjectByType<ViewerEconomyService>();
            }

            if (roomBudgetService == null)
            {
                roomBudgetService = FindFirstObjectByType<ViewerRoomBudgetService>();
            }

            if (localViewerCommandTester == null)
            {
                localViewerCommandTester = FindFirstObjectByType<LocalViewerCommandTester>();
            }

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
                    actionExecutor,
                    actionValidator,
                    economyService,
                    roomBudgetService,
                    localViewerCommandTester,
                    remoteViewerClient);
            }
        }
    }
}
