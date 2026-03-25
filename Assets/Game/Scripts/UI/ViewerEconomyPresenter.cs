using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerEconomyPresenter : MonoBehaviour
    {
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text budgetText;

        private int remoteBalance;
        private int remoteBudget;

        private void Awake()
        {
            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            }
        }

        private void OnEnable()
        {
            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast += OnSnapshot;
                stateBroadcaster.DeltaBroadcast += OnDelta;
            }
        }

        private void OnDisable()
        {
            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast -= OnSnapshot;
                stateBroadcaster.DeltaBroadcast -= OnDelta;
            }
        }

        private void Update()
        {
            if (budgetText != null)
            {
                budgetText.text = $"Budget: {remoteBudget}";
            }

            if (balanceText != null)
            {
                balanceText.text = $"Crowns: {remoteBalance}";
            }
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
