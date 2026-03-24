using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerStateFeed : MonoBehaviour
    {
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private TMP_Text snapshotText;
        [SerializeField] private TMP_Text deltaText;
        [SerializeField] private TMP_Text actionResultText;
        [SerializeField] private TMP_Text rawJsonText;

        private void Awake()
        {
            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            }
        }

        private void OnEnable()
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            stateBroadcaster.SnapshotBroadcast += ShowSnapshot;
            stateBroadcaster.DeltaBroadcast += ShowDelta;
            stateBroadcaster.ActionResultBroadcast += ShowActionResult;
            stateBroadcaster.JsonBroadcast += ShowRawJson;
        }

        private void OnDisable()
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            stateBroadcaster.SnapshotBroadcast -= ShowSnapshot;
            stateBroadcaster.DeltaBroadcast -= ShowDelta;
            stateBroadcaster.ActionResultBroadcast -= ShowActionResult;
            stateBroadcaster.JsonBroadcast -= ShowRawJson;
        }

        public void ShowSnapshot(ViewerSessionSnapshot snapshot)
        {
            if (snapshotText == null)
            {
                return;
            }

            snapshotText.text =
                $"Session: {snapshot.SessionId}\n" +
                $"Room: {snapshot.RoomIndex}\n" +
                $"Phase: {snapshot.RoomPhase}\n" +
                $"HP: {snapshot.HostHp:0.0}\n" +
                $"Shield: {snapshot.HostShield:0.0}\n" +
                $"Enemies: {snapshot.AliveEnemyCount}\n" +
                $"Queue: {snapshot.QueueCount}\n" +
                $"Budget: {snapshot.RoomBudget}";
        }

        public void ShowDelta(ViewerSessionDelta delta)
        {
            if (deltaText == null)
            {
                return;
            }

            deltaText.text =
                $"Event: {delta.EventType}\n" +
                $"Phase: {delta.RoomPhase}\n" +
                $"HP: {delta.HostHp:0.0}\n" +
                $"Enemies: {delta.AliveEnemyCount}\n" +
                $"Queue: {delta.QueueCount}\n" +
                $"Budget: {delta.RoomBudget}";
        }

        public void ShowActionResult(ViewerActionResult result)
        {
            if (actionResultText == null)
            {
                return;
            }

            actionResultText.text =
                $"Command: {result.CommandId}\n" +
                $"Action: {result.ActionId}\n" +
                $"Success: {result.Success}\n" +
                $"Reason: {result.Reason}";
        }

        public void ShowRawJson(string payload)
        {
            if (rawJsonText == null)
            {
                return;
            }

            rawJsonText.text = payload;
        }
    }
}
