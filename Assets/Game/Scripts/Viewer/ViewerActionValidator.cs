using SoulForge.Data;
using SoulForge.Economy;
using System.Collections.Generic;
using UnityEngine;

namespace SoulForge.Viewer
{
    public sealed class ViewerActionValidator : MonoBehaviour
    {
        [SerializeField] private ViewerStoreCatalog storeCatalog;
        [SerializeField] private ViewerEconomyService economyService;
        [SerializeField] private ViewerRoomBudgetService roomBudgetService;

        private readonly Dictionary<string, float> cooldowns = new();

        private void Update()
        {
            if (cooldowns.Count == 0)
            {
                return;
            }

            List<string> keys = ListPool<string>.Get();
            keys.AddRange(cooldowns.Keys);

            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                cooldowns[key] -= Time.deltaTime;
                if (cooldowns[key] <= 0f)
                {
                    cooldowns.Remove(key);
                }
            }

            ListPool<string>.Release(keys);
        }

        public ViewerCommandValidationResult Validate(in ViewerCommand command)
        {
            ViewerActionDefinition action = FindAction(command.ActionId);
            if (action == null)
            {
                return ViewerCommandValidationResult.Fail("action_disabled");
            }

            if (economyService == null)
            {
                return ViewerCommandValidationResult.Fail("economy_missing");
            }

            if (economyService.GetBalance(command.ViewerId) < action.Price)
            {
                return ViewerCommandValidationResult.Fail("insufficient_currency");
            }

            if (cooldowns.TryGetValue(command.ActionId, out float remainingCooldown) && remainingCooldown > 0f)
            {
                return ViewerCommandValidationResult.Fail("cooldown_active");
            }

            if (roomBudgetService != null && !roomBudgetService.CanSpend(action.BudgetCost))
            {
                return ViewerCommandValidationResult.Fail("budget_exceeded");
            }

            if (action.RequiresWorldTarget && !command.HasViewportTarget)
            {
                return ViewerCommandValidationResult.Fail("invalid_target");
            }

            return ViewerCommandValidationResult.Ok();
        }

        public void Commit(ViewerActionDefinition action)
        {
            if (action == null)
            {
                return;
            }

            cooldowns[action.ActionId] = Mathf.Max(0f, action.Cooldown);
            roomBudgetService?.TrySpend(action.BudgetCost);
        }

        public float GetCooldownRemaining(string actionId)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                return 0f;
            }

            return cooldowns.TryGetValue(actionId, out float remainingCooldown) ? Mathf.Max(0f, remainingCooldown) : 0f;
        }

        public bool CanPurchase(string viewerId, ViewerActionDefinition action)
        {
            if (action == null)
            {
                return false;
            }

            ViewerCommand command = new("ui_preview", viewerId, action.ActionId, action.TargetId, true);
            return Validate(command).IsValid;
        }

        public ViewerActionDefinition FindAction(string actionId)
        {
            if (storeCatalog == null || storeCatalog.Actions == null)
            {
                return null;
            }

            for (int i = 0; i < storeCatalog.Actions.Count; i++)
            {
                ViewerActionDefinition action = storeCatalog.Actions[i];
                if (action != null && action.ActionId == actionId)
                {
                    return action;
                }
            }

            return null;
        }

        private static class ListPool<T>
        {
            private static readonly Stack<List<T>> Pool = new();

            public static List<T> Get()
            {
                return Pool.Count > 0 ? Pool.Pop() : new List<T>();
            }

            public static void Release(List<T> list)
            {
                list.Clear();
                Pool.Push(list);
            }
        }
    }
}
