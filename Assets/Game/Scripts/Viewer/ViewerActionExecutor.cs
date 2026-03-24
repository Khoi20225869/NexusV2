using SoulForge.Data;
using SoulForge.Economy;
using SoulForge.Enemies;
using SoulForge.Player;
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
        [SerializeField] private SoulForge.Rooms.RoomController roomController;
        [SerializeField] private float resolveInterval = 3f;
        [SerializeField] private WeaponDefinition fallbackViewerWeapon;

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

            if (roomController == null)
            {
                roomController = FindFirstObjectByType<SoulForge.Rooms.RoomController>();
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
                BroadcastResult(command.CommandId, false, validation.Reason, command.ActionId);
                return false;
            }

            ViewerActionDefinition action = validator.FindAction(command.ActionId);
            if (action == null)
            {
                BroadcastResult(command.CommandId, false, "action_disabled", command.ActionId);
                return false;
            }

            if (!economyService.TrySpend(command.ViewerId, action.Price))
            {
                BroadcastResult(command.CommandId, false, "insufficient_currency", command.ActionId);
                return false;
            }

            if (!queue.TryEnqueue(command))
            {
                economyService.AddCurrency(command.ViewerId, action.Price);
                BroadcastResult(command.CommandId, false, "queue_full", command.ActionId);
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
                    bool spawned = enemySpawner != null && action != null && enemySpawner.SpawnById(action.TargetId);
                    BroadcastResult(command.CommandId, spawned, spawned ? "ok" : "spawn_failed", command.ActionId);
                    BroadcastDelta("action_resolved");
                    break;

                case "drop_heal":
                    if (playerHealth != null && action != null)
                    {
                        playerHealth.Restore(action.HealAmount);
                    }

                    BroadcastResult(command.CommandId, true, "ok", command.ActionId);
                    BroadcastDelta("action_resolved");
                    break;

                case "drop_random_weapon":
                    if (playerInventory != null && fallbackViewerWeapon != null)
                    {
                        playerInventory.AddWeapon(fallbackViewerWeapon, true);
                    }
                    else if (playerWeaponController != null && fallbackViewerWeapon != null)
                    {
                        playerWeaponController.Equip(fallbackViewerWeapon);
                    }

                    BroadcastResult(command.CommandId, true, "ok", command.ActionId);
                    BroadcastDelta("action_resolved");
                    break;

                default:
                    BroadcastResult(command.CommandId, false, "action_disabled", command.ActionId);
                    BroadcastDelta("action_resolved");
                    break;
            }
        }

        private void BroadcastResult(string commandId, bool success, string reason, string actionId)
        {
            broadcaster?.BroadcastActionResolved(new ViewerActionResult
            {
                CommandId = commandId,
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
                RoomPhase = roomController != null && roomController.IsLocked ? "combat" : "reward",
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                AliveEnemyCount = roomController != null ? roomController.ActiveEnemyCount : 0,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = roomBudgetService != null ? roomBudgetService.CurrentBudget : 0
            });
        }
    }
}
