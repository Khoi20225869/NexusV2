using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SoulForge.Economy
{
    public sealed class ViewerEconomyService : MonoBehaviour
    {
        [System.Serializable]
        public sealed class BalanceChangedEvent : UnityEvent<string, int> { }

        [SerializeField] private ViewerEconomyConfig config;
        [SerializeField] private BalanceChangedEvent onBalanceChanged;

        private readonly Dictionary<string, int> balances = new();

        public event System.Action<string, int> BalanceChanged;

        public int GetBalance(string viewerId)
        {
            return balances.TryGetValue(viewerId, out int balance) ? balance : 0;
        }

        public void GrantJoinReward(string viewerId)
        {
            AddCurrency(viewerId, config != null ? config.JoinReward : 100);
        }

        public bool TrySpend(string viewerId, int amount)
        {
            int currentBalance = GetBalance(viewerId);
            if (currentBalance < amount)
            {
                return false;
            }

            balances[viewerId] = currentBalance - amount;
            BalanceChanged?.Invoke(viewerId, balances[viewerId]);
            onBalanceChanged?.Invoke(viewerId, balances[viewerId]);
            return true;
        }

        public void AddCurrency(string viewerId, int amount)
        {
            balances[viewerId] = GetBalance(viewerId) + Mathf.Max(0, amount);
            BalanceChanged?.Invoke(viewerId, balances[viewerId]);
            onBalanceChanged?.Invoke(viewerId, balances[viewerId]);
        }
    }
}
