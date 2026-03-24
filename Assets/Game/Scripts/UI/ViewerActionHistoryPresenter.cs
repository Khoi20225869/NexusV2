using System.Collections.Generic;
using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerActionHistoryPresenter : MonoBehaviour
    {
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private ViewerWebSocketClient remoteViewerClient;
        [SerializeField] private TMP_Text historyText;
        [SerializeField] private int maxEntries = 6;

        private readonly Queue<string> entries = new();

        private void Awake()
        {
            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            }

            if (remoteViewerClient == null)
            {
                remoteViewerClient = FindFirstObjectByType<ViewerWebSocketClient>();
            }
        }

        private void OnEnable()
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            stateBroadcaster.ActionResultBroadcast += AppendEntry;
        }

        private void OnDisable()
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            stateBroadcaster.ActionResultBroadcast -= AppendEntry;
        }

        private void AppendEntry(ViewerActionResult result)
        {
            bool isLocalViewer = remoteViewerClient != null && string.Equals(result.ViewerId, remoteViewerClient.ViewerId);
            string owner = string.IsNullOrWhiteSpace(result.ViewerId) ? "SYSTEM" : isLocalViewer ? "YOU" : result.ViewerId;
            string line = $"[{System.DateTime.Now:HH:mm:ss}] {owner} | {result.ActionId} | {(result.Success ? "OK" : "FAIL")} | {result.Reason}";
            entries.Enqueue(line);

            while (entries.Count > maxEntries)
            {
                entries.Dequeue();
            }

            if (historyText == null)
            {
                return;
            }

            historyText.text = string.Join("\n", entries);
        }
    }
}
