using SoulForge.Viewer;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerHudPresenter : MonoBehaviour
    {
        [SerializeField] private ViewerSessionService sessionService;
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private ViewerStateFeed stateFeed;

        private void Awake()
        {
            if (sessionService == null)
            {
                sessionService = FindFirstObjectByType<ViewerSessionService>();
            }

            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            }

            if (stateFeed == null)
            {
                stateFeed = GetComponent<ViewerStateFeed>();
            }
        }

        public ViewerSessionService SessionService => sessionService;

        public StateBroadcaster StateBroadcaster => stateBroadcaster;
        public ViewerStateFeed StateFeed => stateFeed;
    }
}
