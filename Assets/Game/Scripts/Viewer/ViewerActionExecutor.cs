using SoulForge.Data;
using SoulForge.Economy;
using SoulForge.Enemies;
using SoulForge.Player;
using SoulForge.Rooms;
using UnityEngine;

namespace SoulForge.Viewer
{
    public sealed class ViewerActionExecutor : MonoBehaviour
    {
        [SerializeField] private ViewerActionValidator validator;
        [SerializeField] private ViewerActionQueue queue;
        [SerializeField] private ViewerEconomyService economyService;
        [SerializeField] private StateBroadcaster broadcaster;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerWeaponController playerWeaponController;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private ViewerActionQueue viewerActionQueue;
        [SerializeField] private ViewerRoomBudgetService roomBudgetService;
        [SerializeField] private SoulForge.Bootstrap.RunController runController;
        [SerializeField] private float resolveInterval = 3f;
        [SerializeField] private WeaponDefinition fallbackViewerWeapon;
        [SerializeField] private WeaponPickup viewerWeaponPickupPrefab;

        private float resolveCooldown;

        private void Awake()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawner>();
            }

            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (playerWeaponController == null)
            {
                playerWeaponController = FindFirstObjectByType<PlayerWeaponController>();
            }

            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<PlayerInventory>();
            }

            if (viewerActionQueue == null)
            {
                viewerActionQueue = queue != null ? queue : FindFirstObjectByType<ViewerActionQueue>();
            }

            if (roomBudgetService == null)
            {
                roomBudgetService = FindFirstObjectByType<ViewerRoomBudgetService>();
            }

            if (runController == null)
            {
                runController = FindFirstObjectByType<SoulForge.Bootstrap.RunController>();
            }
        }

        private void Update()
        {
            resolveCooldown -= Time.deltaTime;
            if (resolveCooldown > 0f || queue == null)
            {
                return;
            }

            if (!queue.TryDequeue(out ViewerCommand command))
            {
                return;
            }

            Execute(command);
            resolveCooldown = resolveInterval;
        }

        public bool TrySubmit(in ViewerCommand command)
        {
            if (validator == null || queue == null || economyService == null)
            {
                return false;
            }

            ViewerCommandValidationResult validation = validator.Validate(command);
            if (!validation.IsValid)
            {
                BroadcastResult(command.CommandId, command.ViewerId, false, validation.Reason, command.ActionId);
                return false;
            }

            ViewerActionDefinition action = validator.FindAction(command.ActionId);
            if (action == null)
            {
                BroadcastResult(command.CommandId, command.ViewerId, false, "action_disabled", command.ActionId);
                return false;
            }

            if (!economyService.TrySpend(command.ViewerId, action.Price))
            {
                BroadcastResult(command.CommandId, command.ViewerId, false, "insufficient_currency", command.ActionId);
                return false;
            }

            if (!queue.TryEnqueue(command))
            {
                economyService.AddCurrency(command.ViewerId, action.Price);
                BroadcastResult(command.CommandId, command.ViewerId, false, "queue_full", command.ActionId);
                return false;
            }

            validator.Commit(action);
            BroadcastDelta("action_queued");
            return true;
        }

        private void Execute(in ViewerCommand command)
        {
            ViewerActionDefinition action = validator != null ? validator.FindAction(command.ActionId) : null;

            switch (command.ActionId)
            {
                case "spawn_weak_enemy":
                case "spawn_elite_enemy":
                    bool spawned = TryResolveWorldTarget(command, out Vector3 spawnPosition) &&
                        enemySpawner != null &&
                        action != null &&
                        enemySpawner.SpawnByIdAt(action.TargetId, spawnPosition);
                    BroadcastResult(command.CommandId, command.ViewerId, spawned, spawned ? "ok" : "spawn_failed", command.ActionId);
                    BroadcastDelta("action_resolved");
                    break;

                case "drop_heal":
                    if (playerHealth != null && action != null)
                    {
                        playerHealth.Restore(action.HealAmount);
                    }

                    BroadcastResult(command.CommandId, command.ViewerId, true, "ok", command.ActionId);
                    BroadcastDelta("action_resolved");
                    break;

                case "drop_random_weapon":
                    bool droppedWeapon = TryResolveWorldTarget(command, out Vector3 dropPosition) &&
                        TryDropViewerWeapon(dropPosition);

                    if (!droppedWeapon && playerInventory != null && fallbackViewerWeapon != null)
                    {
                        playerInventory.AddWeapon(fallbackViewerWeapon, true);
                        droppedWeapon = true;
                    }
                    else if (!droppedWeapon && playerWeaponController != null && fallbackViewerWeapon != null)
                    {
                        playerWeaponController.Equip(fallbackViewerWeapon);
                        droppedWeapon = true;
                    }

                    BroadcastResult(command.CommandId, command.ViewerId, droppedWeapon, droppedWeapon ? "ok" : "drop_failed", command.ActionId);
                    BroadcastDelta("action_resolved");
                    break;

                default:
                    BroadcastResult(command.CommandId, command.ViewerId, false, "action_disabled", command.ActionId);
                    BroadcastDelta("action_resolved");
                    break;
            }
        }

        private void BroadcastResult(string commandId, string viewerId, bool success, string reason, string actionId)
        {
            broadcaster?.BroadcastActionResolved(new ViewerActionResult
            {
                CommandId = commandId,
                ViewerId = viewerId,
                Success = success,
                Reason = reason,
                ActionId = actionId
            });
        }

        private void BroadcastDelta(string eventType)
        {
            broadcaster?.BroadcastDelta(new ViewerSessionDelta
            {
                EventType = eventType,
                RoomPhase = runController != null ? runController.CurrentPhase : "explore",
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                AliveEnemyCount = runController != null ? runController.TotalAliveEnemyCount : 0,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = roomBudgetService != null ? roomBudgetService.CurrentBudget : 0,
                PlayerMarker = BuildMarker("player", "player", playerHealth != null ? playerHealth.transform : null),
                TargetMarker = BuildMarker("target", "target", FindFirstObjectByType<RunFinishGate>()?.transform),
                EnemyMarkers = BuildEnemyMarkers()
            });
        }

        private bool TryResolveWorldTarget(in ViewerCommand command, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            if (!command.HasViewportTarget || Camera.main == null)
            {
                return false;
            }

            Vector3 viewport = new(command.ViewportX, command.ViewportY, Mathf.Abs(Camera.main.transform.position.z));
            worldPosition = Camera.main.ViewportToWorldPoint(viewport);
            worldPosition.z = 0f;
            return true;
        }

        private bool TryDropViewerWeapon(Vector3 worldPosition)
        {
            if (viewerWeaponPickupPrefab == null || fallbackViewerWeapon == null)
            {
                return false;
            }

            WeaponPickup pickup = Instantiate(viewerWeaponPickupPrefab, worldPosition, Quaternion.identity);
            pickup.Configure(fallbackViewerWeapon);
            return true;
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
                return System.Array.Empty<ViewerViewportMarkerState>();
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
