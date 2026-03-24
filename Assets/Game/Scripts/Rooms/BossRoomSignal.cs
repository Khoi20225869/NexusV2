using SoulForge.Bootstrap;
using SoulForge.Player;
using SoulForge.Viewer;
using UnityEngine;

namespace SoulForge.Rooms
{
    public sealed class BossRoomSignal : MonoBehaviour
    {
        [SerializeField] private RunController runController;
        [SerializeField] private RoomController ownerRoom;
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ViewerActionQueue viewerActionQueue;
        [SerializeField] private ViewerRoomBudgetService viewerRoomBudgetService;
        [SerializeField] private string bossName = "Forge Sentinel";

        private bool announced;

        private void Awake()
        {
            if (runController == null)
            {
                runController = FindFirstObjectByType<RunController>();
            }

            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            }

            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (viewerActionQueue == null)
            {
                viewerActionQueue = FindFirstObjectByType<ViewerActionQueue>();
            }

            if (viewerRoomBudgetService == null)
            {
                viewerRoomBudgetService = FindFirstObjectByType<ViewerRoomBudgetService>();
            }
        }

        private void Update()
        {
            if (announced || runController == null || ownerRoom == null)
            {
                return;
            }

            if (runController.CurrentRoom != ownerRoom)
            {
                return;
            }

            announced = true;
            stateBroadcaster?.BroadcastDelta(new ViewerSessionDelta
            {
                EventType = $"boss_room_entered:{bossName}",
                RoomPhase = ownerRoom.IsLocked ? "boss_combat" : "boss_reward",
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                AliveEnemyCount = ownerRoom.ActiveEnemyCount,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = viewerRoomBudgetService != null ? viewerRoomBudgetService.CurrentBudget : 0
            });
        }
    }
}
