using System.Collections.Generic;
using SoulForge.Rooms;
using SoulForge.Player;
using SoulForge.Viewer;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SoulForge.Bootstrap
{
    public sealed class RunController : MonoBehaviour
    {
        [SerializeField] private List<RoomController> roomSequence = new();
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ViewerActionQueue viewerActionQueue;
        [SerializeField] private ViewerRoomBudgetService viewerRoomBudgetService;
        [SerializeField] private UnityEvent onRunStarted;
        [SerializeField] private UnityEvent onRunCompleted;

        private int roomIndex;

        public event Action RunStarted;
        public event Action RunCompleted;
        public event Action<RoomController> RoomChanged;

        public RoomController CurrentRoom { get; private set; }
        public int RoomIndex => roomIndex;
        public int RoomCount => roomSequence.Count;

        private void Start()
        {
            if (roomSequence.Count == 0)
            {
                RoomController firstRoom = FindFirstObjectByType<RoomController>();
                if (firstRoom != null)
                {
                    roomSequence.Add(firstRoom);
                }
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

            CurrentRoom = roomSequence.Count > 0 ? roomSequence[0] : null;
            roomIndex = 0;
            onRunStarted?.Invoke();
            RunStarted?.Invoke();
            BroadcastSnapshot();
        }

        public void SetCurrentRoom(RoomController room)
        {
            if (room == null)
            {
                return;
            }

            CurrentRoom = room;
            roomIndex = roomSequence.IndexOf(room);
            if (roomIndex < 0)
            {
                roomSequence.Add(room);
                roomIndex = roomSequence.Count - 1;
            }

            viewerRoomBudgetService?.ResetBudget();
            RoomChanged?.Invoke(CurrentRoom);
            BroadcastDelta("room_changed");
        }

        public void CompleteRun()
        {
            onRunCompleted?.Invoke();
            RunCompleted?.Invoke();
            BroadcastDelta("run_completed");
        }

        public void AdvanceToNextRoom()
        {
            int nextIndex = roomIndex + 1;
            if (nextIndex >= roomSequence.Count)
            {
                CompleteRun();
                return;
            }

            SetCurrentRoom(roomSequence[nextIndex]);
        }

        public void EnterRoom(RoomController room)
        {
            if (room == null || room == CurrentRoom)
            {
                return;
            }

            SetCurrentRoom(room);
        }

        public bool IsLastRoom(RoomController room)
        {
            if (room == null || roomSequence.Count == 0)
            {
                return false;
            }

            return roomSequence[roomSequence.Count - 1] == room;
        }

        private void BroadcastSnapshot()
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            ViewerSessionSnapshot snapshot = new()
            {
                SessionId = "local-session",
                RoomIndex = roomIndex,
                RoomPhase = CurrentRoom != null && CurrentRoom.IsLocked ? "combat" : "reward",
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                HostShield = playerHealth != null ? playerHealth.MaxShield : 0f,
                AliveEnemyCount = CurrentRoom != null ? CurrentRoom.ActiveEnemyCount : 0,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = viewerRoomBudgetService != null ? viewerRoomBudgetService.CurrentBudget : 0
            };

            stateBroadcaster.BroadcastSnapshot(snapshot);
        }

        private void BroadcastDelta(string reason)
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            ViewerSessionDelta delta = new()
            {
                EventType = reason,
                RoomPhase = CurrentRoom != null && CurrentRoom.IsLocked ? "combat" : "reward",
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                AliveEnemyCount = CurrentRoom != null ? CurrentRoom.ActiveEnemyCount : 0,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = viewerRoomBudgetService != null ? viewerRoomBudgetService.CurrentBudget : 0
            };

            stateBroadcaster.BroadcastDelta(delta);
        }
    }
}
