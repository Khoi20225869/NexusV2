using System.Collections.Generic;
using SoulForge.Rooms;
using SoulForge.Player;
using SoulForge.Viewer;
using SoulForge.Enemies;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SoulForge.Bootstrap
{
    public sealed class RunController : MonoBehaviour
    {
        [SerializeField] private List<RoomController> roomSequence = new();
        [SerializeField] private int floorCount = 3;
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ViewerActionQueue viewerActionQueue;
        [SerializeField] private ViewerRoomBudgetService viewerRoomBudgetService;
        [SerializeField] private float spectatorRefreshInterval = 0.1f;
        [SerializeField] private UnityEvent onRunStarted;
        [SerializeField] private UnityEvent onRunCompleted;

        public event Action RunStarted;
        public event Action RunCompleted;
        public event Action<RoomController> RoomChanged;

        public RoomController CurrentRoom { get; private set; }
        public int RoomIndex => Mathf.Max(0, FloorIndex);
        public int RoomCount => floorCount;
        public int FloorIndex { get; private set; }
        public int FloorCount => floorCount;
        public int TotalAliveEnemyCount { get; private set; }
        public string CurrentPhase => TotalAliveEnemyCount > 0 ? "combat" : "explore";

        private float spectatorRefreshTimer;

        private void Start()
        {
            if (roomSequence.Count == 0)
            {
                roomSequence.AddRange(FindObjectsByType<RoomController>(FindObjectsSortMode.None));
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
            FloorIndex = ResolveFloorIndex();
            RefreshEnemyCount();
            onRunStarted?.Invoke();
            RunStarted?.Invoke();
            BroadcastSnapshot();
        }

        private void Update()
        {
            spectatorRefreshTimer -= Time.deltaTime;
            if (spectatorRefreshTimer > 0f)
            {
                return;
            }

            spectatorRefreshTimer = Mathf.Max(0.05f, spectatorRefreshInterval);
            BroadcastDelta("spectator_refresh");
        }

        public void SetCurrentRoom(RoomController room)
        {
            if (room == null)
            {
                return;
            }

            CurrentRoom = room;
            RoomChanged?.Invoke(CurrentRoom);
        }

        public void CompleteRun()
        {
            onRunCompleted?.Invoke();
            RunCompleted?.Invoke();
            BroadcastDelta("run_completed");
        }

        public void AdvanceToNextRoom()
        {
            CompleteRun();
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
            _ = room;
            return true;
        }

        private void BroadcastSnapshot()
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            RefreshEnemyCount();
            ViewerSessionSnapshot snapshot = new()
            {
                SessionId = "local-session",
                RoomIndex = FloorIndex,
                RoomPhase = CurrentPhase,
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                HostShield = playerHealth != null ? playerHealth.MaxShield : 0f,
                AliveEnemyCount = TotalAliveEnemyCount,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = viewerRoomBudgetService != null ? viewerRoomBudgetService.CurrentBudget : 0,
                PlayerMarker = BuildMarker("player", "player", playerHealth != null ? playerHealth.transform : null),
                TargetMarker = BuildMarker("target", "target", FindFirstObjectByType<RunFinishGate>()?.transform),
                EnemyMarkers = BuildEnemyMarkers()
            };

            stateBroadcaster.BroadcastSnapshot(snapshot);
        }

        private void BroadcastDelta(string reason)
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            RefreshEnemyCount();
            ViewerSessionDelta delta = new()
            {
                EventType = reason,
                RoomPhase = CurrentPhase,
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                AliveEnemyCount = TotalAliveEnemyCount,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = viewerRoomBudgetService != null ? viewerRoomBudgetService.CurrentBudget : 0,
                PlayerMarker = BuildMarker("player", "player", playerHealth != null ? playerHealth.transform : null),
                TargetMarker = BuildMarker("target", "target", FindFirstObjectByType<RunFinishGate>()?.transform),
                EnemyMarkers = BuildEnemyMarkers()
            };

            stateBroadcaster.BroadcastDelta(delta);
        }

        private void RefreshEnemyCount()
        {
            int total = 0;
            for (int i = 0; i < roomSequence.Count; i++)
            {
                if (roomSequence[i] != null)
                {
                    total += roomSequence[i].ActiveEnemyCount;
                }
            }

            TotalAliveEnemyCount = total;
        }

        private static int ResolveFloorIndex()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Floor_", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(sceneName.Substring("Floor_".Length), out int parsed))
            {
                return parsed;
            }

            return 1;
        }

        private static ViewerViewportMarkerState BuildMarker(string id, string kind, Transform target)
        {
            Camera camera = Camera.main;
            if (camera == null || target == null)
            {
                return new ViewerViewportMarkerState { Id = id, Kind = kind, Visible = false };
            }

            Vector3 viewport = camera.WorldToViewportPoint(target.position);
            return new ViewerViewportMarkerState
            {
                Id = id,
                Kind = kind,
                ViewportX = Mathf.Clamp01(viewport.x),
                ViewportY = Mathf.Clamp01(viewport.y),
                Visible = viewport.z >= 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f
            };
        }

        private static ViewerViewportMarkerState[] BuildEnemyMarkers()
        {
            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            if (enemies == null || enemies.Length == 0)
            {
                return Array.Empty<ViewerViewportMarkerState>();
            }

            ViewerViewportMarkerState[] markers = new ViewerViewportMarkerState[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                markers[i] = BuildMarker($"enemy_{i:00}", "enemy", enemies[i] != null ? enemies[i].transform : null);
            }

            return markers;
        }
    }
}
