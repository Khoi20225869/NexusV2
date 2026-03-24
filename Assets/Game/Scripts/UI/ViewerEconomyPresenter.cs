using SoulForge.Economy;
using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerEconomyPresenter : MonoBehaviour
    {
        [SerializeField] private ViewerEconomyService economyService;
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private ViewerRoomBudgetService roomBudgetService;
        [SerializeField] private LocalViewerCommandTester localViewerCommandTester;
        [SerializeField] private ViewerWebSocketClient remoteViewerClient;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text budgetText;

        private int remoteBalance;
        private int remoteBudget;

        private void Awake()
        {
            if (economyService == null)
            {
                economyService = FindFirstObjectByType<ViewerEconomyService>();
            }

            if (roomBudgetService == null)
            {
                roomBudgetService = FindFirstObjectByType<ViewerRoomBudgetService>();
            }

            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
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

        private void OnEnable()
        {
            if (economyService != null)
            {
                economyService.BalanceChanged += OnBalanceChanged;
            }

            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast += OnSnapshot;
                stateBroadcaster.DeltaBroadcast += OnDelta;
            }
        }

        private void OnDisable()
        {
            if (economyService != null)
            {
                economyService.BalanceChanged -= OnBalanceChanged;
            }

            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast -= OnSnapshot;
                stateBroadcaster.DeltaBroadcast -= OnDelta;
            }
        }

        private void Update()
        {
            bool isRemoteMode = localViewerCommandTester == null && remoteViewerClient != null;

            if (budgetText != null)
            {
                budgetText.text = isRemoteMode
                    ? $"Budget: {remoteBudget}"
                    : roomBudgetService != null ? $"Budget: {roomBudgetService.CurrentBudget}" : "Budget: 0";
            }

            if (balanceText == null)
            {
                return;
            }

            if (isRemoteMode)
            {
                balanceText.text = $"Crowns: {remoteBalance}";
                return;
            }

            if (economyService == null)
            {
                return;
            }

            string viewerId = localViewerCommandTester != null ? localViewerCommandTester.ViewerId : string.Empty;
            if (!string.IsNullOrWhiteSpace(viewerId))
            {
                balanceText.text = $"Crowns: {economyService.GetBalance(viewerId)}";
            }
        }

        public void OnBalanceChanged(string viewerId, int balance)
        {
            string activeViewerId = localViewerCommandTester != null ? localViewerCommandTester.ViewerId : remoteViewerClient != null ? remoteViewerClient.ViewerId : string.Empty;
            if (balanceText == null || string.IsNullOrWhiteSpace(activeViewerId) || viewerId != activeViewerId)
            {
                return;
            }

            balanceText.text = $"Crowns: {balance}";
        }

        private void OnSnapshot(ViewerSessionSnapshot snapshot)
        {
            remoteBalance = snapshot != null ? snapshot.ViewerBalance : 0;
            remoteBudget = snapshot != null ? snapshot.RoomBudget : 0;
        }

        private void OnDelta(ViewerSessionDelta delta)
        {
            remoteBalance = delta != null ? delta.ViewerBalance : remoteBalance;
            remoteBudget = delta != null ? delta.RoomBudget : remoteBudget;
        }
    }
}
