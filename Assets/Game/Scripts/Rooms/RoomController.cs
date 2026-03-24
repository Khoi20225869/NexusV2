using SoulForge.Data;
using SoulForge.Enemies;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SoulForge.Rooms
{
    public sealed class RoomController : MonoBehaviour
    {
        [SerializeField] private string roomId = "room_00";
        [SerializeField] private RoomTier roomTier = RoomTier.Tier1;
        [SerializeField] private bool lockedOnStart = true;
        [SerializeField] private bool autoUnlockWhenEnemiesDefeated = true;
        [SerializeField] private DoorController[] connectedDoors;
        [SerializeField] private RewardSpawner rewardSpawner;
        [SerializeField] private UnityEvent onRoomCleared;

        private readonly HashSet<EnemyController> activeEnemies = new();
        private bool cleared;

        public bool IsLocked { get; private set; }
        public bool IsCleared => cleared;
        public int ActiveEnemyCount => activeEnemies.Count;
        public string RoomId => roomId;
        public RoomTier Tier => roomTier;

        private void Start()
        {
            roomTier = InferTier(roomId, roomTier);
            IsLocked = lockedOnStart;
            ApplyDoorState();
        }

        private void Update()
        {
            if (!autoUnlockWhenEnemiesDefeated || !IsLocked || cleared)
            {
                return;
            }

            if (activeEnemies.Count == 0)
            {
                UnlockRoom();
            }
        }

        public void LockRoom()
        {
            IsLocked = true;
            cleared = false;
            ApplyDoorState();
        }

        public void UnlockRoom()
        {
            if (!IsLocked)
            {
                return;
            }

            IsLocked = false;
            cleared = true;
            ApplyDoorState();
            rewardSpawner?.ShowReward();
            onRoomCleared?.Invoke();
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy == null)
            {
                return;
            }

            activeEnemies.Add(enemy);
        }

        public void UnregisterEnemy(EnemyController enemy)
        {
            if (enemy == null)
            {
                return;
            }

            activeEnemies.Remove(enemy);
        }

        private void ApplyDoorState()
        {
            if (connectedDoors == null)
            {
                return;
            }

            for (int i = 0; i < connectedDoors.Length; i++)
            {
                if (connectedDoors[i] == null)
                {
                    continue;
                }

                if (IsLocked)
                {
                    connectedDoors[i].CloseDoor();
                }
                else
                {
                    connectedDoors[i].OpenDoor();
                }
            }
        }

        private static RoomTier InferTier(string id, RoomTier currentTier)
        {
            if (currentTier != RoomTier.Tier1)
            {
                return currentTier;
            }

            string normalized = (id ?? string.Empty).ToLowerInvariant();
            if (normalized.Contains("room_b") || normalized.Contains("tier2"))
            {
                return RoomTier.Tier2;
            }

            if (normalized.Contains("room_c") || normalized.Contains("boss") || normalized.Contains("tier3"))
            {
                return RoomTier.Tier3;
            }

            return RoomTier.Tier1;
        }
    }
}
